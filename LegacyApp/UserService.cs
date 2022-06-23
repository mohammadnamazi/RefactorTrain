using System;

namespace LegacyApp
{
    public class UserService
    {
        public bool AddUser(string firstName, string surName, string email, DateTime dateOfBirth, int clientId)
        {
            int age = CalculateAge(dateOfBirth);
            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);
            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                Firstname = firstName,
                Surname = surName
            };

            if (IsUserValid(age, client, user))
            {
                UserDataAccess.AddUser(user);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void CalculateCreaditLimit(Client client, User user)
        {
            switch (client.Name)
            {
                // Skip credit chek
                case "VeryImportantClient":
                    user.HasCreditLimit = false;
                    break;
                case "ImportantClient":
                    {
                        // Do credit check and double credit limit
                        user.HasCreditLimit = true;
                        using (var userCreditService = new UserCreditServiceClient())
                        {
                            var creditLimit = userCreditService.GetCreditLimit(user.Firstname, user.Surname, user.DateOfBirth);
                            creditLimit = creditLimit * 2;
                            user.CreditLimit = creditLimit;
                        }
                    }
                    break;
                default:
                    {
                        // Do credit check
                        user.HasCreditLimit = true;
                        using (var userCreditService = new UserCreditServiceClient())
                        {
                            var creditLimit = userCreditService.GetCreditLimit(user.Firstname, user.Surname, user.DateOfBirth);
                            user.CreditLimit = creditLimit;
                        }
                    }
                    break;
            }
        }

        private bool IsUserValid(int age, Client client, User user)
        {
            CalculateCreaditLimit(client, user);

            if (string.IsNullOrWhiteSpace(user.Firstname) || string.IsNullOrWhiteSpace(user.Surname))
            {
                return false;
            }
            else if (!user.EmailAddress.Contains("@") && !user.EmailAddress.Contains("."))
            {
                return false;
            }
            else if (age < 21)
            {
                return false;
            }
            else if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private int CalculateAge(DateTime dateOfBirth)
        {
            var currentDate = DateTime.Now;
            var age = currentDate.Year - dateOfBirth.Year;
            if (currentDate.Month < dateOfBirth.Month || (currentDate.Month == dateOfBirth.Month && currentDate.Day < dateOfBirth.Day))
            {
                age--;
            }
            return age;
        }
    }
}
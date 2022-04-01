using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.Users.Validators
{
    /**
     * <summary>
     * validates a password before it is saved
     * </summary>
     */
    public class PasswordIsValid : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            char[] specialChars = {
                '@','_','!','#','$','%','^','&','*','(',')',
                '<','>','?','/','|','}','{','~',':'
            };
            char[] nums = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

            if (value == null)
                return new ValidationResult("Password cannot be null");

            string password = (string)value;
            if (password.Length < 8)
                return new ValidationResult("Minimum password length is 8");

            string invalidPasswordMsg = "Password requirements: at least 8 characters" +
            " long containing at least one number(zero through nine)" +
            " and one special character(@, _, !, #, $, %, ^, &, *, (, )," +
            " <, >, ?, /, |, }, {, ~, :)";
            bool containsSpecial = false;
            foreach (char c in specialChars)
            {
                if (password.IndexOf(c) != -1)
                {
                    containsSpecial = true;
                    break;
                }
            }

            if (!containsSpecial)
                return new ValidationResult(invalidPasswordMsg);

            bool containsNum = false;
            foreach (char num in nums)
            {
                if (password.IndexOf(num) != -1)
                {
                    containsNum = true;
                    break;
                }
            }
            if (!containsNum)
                return new ValidationResult(invalidPasswordMsg);

            return ValidationResult.Success;
        }
    }
}

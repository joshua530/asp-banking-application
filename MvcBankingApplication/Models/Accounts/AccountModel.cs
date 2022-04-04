using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.Accounts
{
    abstract public class AccountModel
    {
        public int ID { get; set; }
        [Required]
        public int AccountNumber { get; set; }

        [Required]
        [EnumDataType(typeof(AccountType))]
        public int Type { get; set; } = 1;

        public double Balance { get; set; } = 0;
    }

    public enum AccountType
    {
        CustomerAccount = 1,
        BankCashAccount = 2,
        BankOverdraftAccount = 3
    }
}

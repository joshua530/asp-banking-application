using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcBankingApplication.Models.Accounts
{
    abstract public class AccountModel
    {
        private double _balance = 0;
        private AccountType _accountType = AccountType.CustomerAccount;

        [Key]
        [Required]
        public int Id
        { get; set; }

        [Required]
        public AccountType Type
        {
            get
            {
                return _accountType;
            }
            set
            {
                _accountType = value;
            }
        }

        [Required]
        [DataType(DataType.Currency)]
        public double Balance
        {
            get { return _balance; }
            set { _balance = value; }
        }

        public override string ToString()
        {
            return $"{Id}-{Balance}-{Type}";
        }
    }

    public enum AccountType
    {
        CustomerAccount = 1,
        BankCashAccount = 2,
        BankOverdraftAccount = 3
    }
}

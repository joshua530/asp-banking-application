using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MvcBankingApplication.Models.Transactions;

namespace MvcBankingApplication.Models.Accounts
{
    abstract public class AccountModel
    {
        private double _balance = 0;
        private AccountType _accountType = AccountType.CustomerAccount;

        [Key]
        [Required]
        [DisplayFormat(DataFormatString = "{0:D5}")]
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

        public virtual IEnumerable<Transaction> CreditTransactions { get; set; }
        public virtual IEnumerable<Transaction> DebitTransactions { get; set; }

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

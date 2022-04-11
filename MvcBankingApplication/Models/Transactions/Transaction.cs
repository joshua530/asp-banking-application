using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MvcBankingApplication.Models.Users;
using MvcBankingApplication.Models.Accounts;

namespace MvcBankingApplication.Models.Transactions
{
#pragma warning disable CS8632
    public class Transaction
    {
        private DateTime _timeOfTransaction = DateTime.Now;
        [DisplayFormat(DataFormatString = "{0:D3}")]
        public int ID { get; set; }
        [Required]
        [DataType(DataType.DateTime), DisplayFormat(
            DataFormatString = "{0:dd MMMM, yyyy}"
        )]
        public DateTime TimeOfTransaction
        {
            get
            {
                // create datetime for new object
                return _timeOfTransaction;
            }
            set
            {
                _timeOfTransaction = value;
            }
        }

        [Required]
        [DataType(DataType.Currency)]
        public double Amount { get; set; }

        [Required]
        public TransactionTypes TransactionType { get; set; }

        public string CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public string CashierId { get; set; }
        public virtual Cashier Cashier { get; set; }

        public string ApproverId { get; set; }
        public virtual Admin Approver { get; set; }

        [DisplayFormat(DataFormatString = "{0:D5}")]
        public int AccountDebitedId { get; set; }
        public virtual AccountModel AccountDebited { get; set; }

        [DisplayFormat(DataFormatString = "{0:D5}")]
        public int AccountCreditedId { get; set; }
        public virtual AccountModel AccountCredited { get; set; }
    }

    public class TransactionWithTypeStr : Transaction
    {
        public string TransactionTypeStr { get; set; }
    }
#pragma warning restore CS8632
}

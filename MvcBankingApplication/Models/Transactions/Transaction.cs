using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MvcBankingApplication.Models.Users;
using MvcBankingApplication.Models.Accounts;

namespace MvcBankingApplication.Models.Transactions
{
    public class Transaction
    {
#pragma warning disable CS8632
        public int ID { get; set; }
        [Required]
        [DataType(DataType.DateTime), DisplayFormat(
            DataFormatString = "H:mm:ss dd-MM-yyyy"
        )]
        public DateTime TimeOfTransaction { get; set; }

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

        public int AccountDebitedId { get; set; }
        public virtual AccountModel AccountDebited { get; set; }

        public int AccountCreditedId { get; set; }
        public virtual AccountModel AccountCredited { get; set; }
    }
#pragma warning restore CS8632
}

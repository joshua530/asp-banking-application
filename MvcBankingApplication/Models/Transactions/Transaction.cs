using System.ComponentModel.DataAnnotations;
using MvcBankingApplication.Models.Users;

namespace MvcBankingApplication.Models.Transactions
{
    public class Transaction
    {
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
        public int AccountDebitedId { get; set; }
        [Required]
        public int AccountCreditedId { get; set; }
        [Required]
        public TransactionTypes TransactionType { get; set; }

        public string CustomerId { get; set; }
        public string CashierId { get; set; }
        public string ApproverId { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Cashier Cashier { get; set; }
        public virtual Admin Approver { get; set; }
    }
}

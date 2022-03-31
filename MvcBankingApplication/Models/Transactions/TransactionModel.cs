namespace MvcBankingApplication.Models.Transactions
{
    public class TransactionModel
    {
        public int Id { get; }
        public DateTime TimeOfTransaction { get; set; }
        public int CustomerId { get; set; }
        public int? CashierId { get; set; }
        public int? ApproverId { get; set; }
        public double Amount { get; set; }
        public int AccountDebitedId { get; set; }
        public int AccountCreditedId { get; set; }
        public String TransactionType { get; set; } = String.Empty;
    }
}

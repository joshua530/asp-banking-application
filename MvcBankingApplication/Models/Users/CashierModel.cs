namespace MvcBankingApplication.Models.Users
{
    public class CashierModel : BankUserModel
    {
        public double TransactionLimit { get; set; }
    }
}

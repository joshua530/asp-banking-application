namespace MvcBankingApplication.Models.Accounts
{
    abstract public class AccountModel
    {
        public int Id { get; }
        public string Type { get; set; } = String.Empty;
        public double Balance { get; set; } = 0;
    }
}

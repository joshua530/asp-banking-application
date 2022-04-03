namespace MvcBankingApplication.Models.Accounts
{
    public class CustomerAccountModel : AccountModel
    {
        public double OverdraftLimit { get; set; } = 0;
        public int UserId { get; set; } //TODO FK
    }
}

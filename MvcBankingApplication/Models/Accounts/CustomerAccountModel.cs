namespace MvcBankingApplication.Models.Accounts
{
    class CustomerAccountModel : AccountModel
    {
        public double OverdraftLimit { get; set; } = 0;
        public int UserId { get; set; } //TODO FK
    }
}

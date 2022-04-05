using MvcBankingApplication.Models.Accounts;

namespace MvcBankingApplication.Models.Users
{
    public class Customer : ApplicationUser
    {
        public int CustomerAccountId { get; set; }
        public virtual CustomerAccount Account { get; set; }
    }
}

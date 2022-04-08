using MvcBankingApplication.Models.Accounts;

namespace MvcBankingApplication.Models.Users
{
    public class Customer : ApplicationUser
    {
        public virtual CustomerAccount Account { get; set; }
    }
}

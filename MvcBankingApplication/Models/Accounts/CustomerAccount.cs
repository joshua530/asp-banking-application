using MvcBankingApplication.Models.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcBankingApplication.Models.Accounts
{
    public class CustomerAccount : AccountModel
    {
        public double OverdraftLimit { get; set; } = 0;

        [ForeignKey("Customer")]
        public string CustomerId { get; set; }

        public virtual Customer Customer { get; set; }
    }
}

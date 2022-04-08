using MvcBankingApplication.Models.Users;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.Accounts
{
    public class CustomerAccount : AccountModel
    {
        [DataType(DataType.Currency)]
        public double OverdraftLimit { get; set; } = 1000;

        [ForeignKey("Customer")]
        public string CustomerId { get; set; }

        public virtual Customer Customer { get; set; }
    }
}

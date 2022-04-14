using MvcBankingApplication.Models.Users;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.Accounts
{
    public class CustomerAccount : AccountModel
    {
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:0.00}")]
        public double OverdraftLimit { get; set; } = 1000;

        [DataType(DataType.Currency)]
        public double OverdrawnAmount { get; set; } = 0;

        [ForeignKey("Customer")]
        public string CustomerId { get; set; }

        public virtual Customer Customer { get; set; }
    }
}

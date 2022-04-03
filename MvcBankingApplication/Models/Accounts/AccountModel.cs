using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.Accounts
{
    abstract public class AccountModel
    {
        public int ID { get; set; }
        [Required]
        public int AccountNumber { get; set; }
        public string Type { get; set; } = String.Empty;
        public double Balance { get; set; } = 0;
    }
}

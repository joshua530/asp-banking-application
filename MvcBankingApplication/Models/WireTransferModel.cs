using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models;

public class WireTransferModel
{
    public WireTransferModel()
    {
        Amount = 1;
    }

    [Required]
    [Range(1.0, 999999.99, ErrorMessage = "Amount must be between 1 and 999999.99")]
    public double Amount { get; set; }

    [Required]
    [Display(Name = "Transfer to")]
    public int AccountNumber { get; set; }
}

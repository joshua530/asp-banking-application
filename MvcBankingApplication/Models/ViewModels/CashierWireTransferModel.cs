using System.ComponentModel.DataAnnotations;


namespace MvcBankingApplication.Models.ViewModels;

public class CashierWireTransferModel
{
    [Required]
    public int From { get; set; }

    [Required]
    public int To { get; set; }

    [DataType(DataType.Currency)]
    [Required]
    [Range(1, 999999.99, ErrorMessage = "amount should be between 1 and 999999.99")]
    public double Amount { get; set; }
}

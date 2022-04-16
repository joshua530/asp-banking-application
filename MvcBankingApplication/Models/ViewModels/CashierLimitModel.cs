using System.ComponentModel.DataAnnotations;


namespace MvcBankingApplication.Models.ViewModels;

public class CashierLimitModel
{
    [Required(ErrorMessage = "cashier id is required")]
    [Display(Name = "cashier id")]
    [DisplayFormat]
    public string CashierId { get; set; }

    [DataType(DataType.Currency)]
    [Required(ErrorMessage = "new cashier limit is required")]
    [Range(10000, 499999.99, ErrorMessage = "cashier limit should be between 10000 and 499999.99")]
    [Display(Name = "new limit")]
    public double NewLimit { get; set; }
}

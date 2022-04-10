using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models;

public class OverdraftModel
{
    private double _amount = 1;

    [Required]
    [Range(1, 99999.99, ErrorMessage = "Overdraft amount can only be between 1 and 99999.99")]
    public double Amount
    {
        get { return _amount; }
        set { _amount = value; }
    }

    [DataType(DataType.Currency)]
    public double OverdraftLimit
    {
        get; set;
    }

    [DataType(DataType.Currency)]
    public double OverdrawnAmount { get; set; }
}

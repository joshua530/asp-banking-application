using MvcBankingApplication.Models.Transactions;
using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.ViewModels;

public class CustomerTransactionModel
{
    [DataType(DataType.Currency)]
    public double Amount { get; set; }
    public string Type { get; set; }
    [DisplayFormat(DataFormatString = "{0:dd MMMM, yyyy}")]
    public string Date { get; set; }
}

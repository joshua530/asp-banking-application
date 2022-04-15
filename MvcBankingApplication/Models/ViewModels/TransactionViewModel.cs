using MvcBankingApplication.Models.Transactions;
using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.ViewModels;

abstract public class TransactionViewModel
{
    public int AccountCreditedId { get; set; }
    public int AccountDebitedId { get; set; }
    [DisplayFormat(DataFormatString = "{0:D2}")]
    public int Id { get; set; }
    public double Amount { get; set; }
    public string TransactionTypeStr { get; set; }
    public string Date { get; set; }
}

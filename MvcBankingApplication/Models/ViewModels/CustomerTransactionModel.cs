using MvcBankingApplication.Models.Transactions;
using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.ViewModels;

public class CustomerTransactionModel
{
    public string TimeString { get; set; }
    public int AccountCreditedId { get; set; }
    public int AccountDebitedId { get; set; }
    public int Id { get; set; }
    public double Amount { get; set; }
    public string TransactionTypeStr { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.ViewModels;

public enum TransactionType
{
    [Display(Name = "deposit")]
    Deposit,
    [Display(Name = "overdraft")]
    Overdraft,
    [Display(Name = "withdrawal")]
    Widthdraw
}

public class WithdrawDepositOverdraftModel
{
    [Display(Name = "Select transaction type")]
    public TransactionType TransactionType { get; set; }
    public double Amount { get; set; }
    [Display(Name = "Account number")]
    [Range(1, 999999.99)]
    public int AccountNumber { get; set; }
}

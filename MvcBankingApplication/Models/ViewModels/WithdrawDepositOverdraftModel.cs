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

    [Range(1, 9999999.99)]
    public double Amount { get; set; }

    [Display(Name = "Account number")]
    public int AccountNumber { get; set; }
}

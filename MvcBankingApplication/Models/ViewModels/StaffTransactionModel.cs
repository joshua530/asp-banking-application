using MvcBankingApplication.Models.Transactions;
using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.ViewModels;

public class StaffTransactionModel : TransactionViewModel
{
    public string ApprovedBy { get; set; }
}

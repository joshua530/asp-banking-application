using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.Transactions
{
  public class TransactionModel
  {
    public int Id { get; }
    [Required]
    [DataType(DataType.DateTime), DisplayFormat(
        DataFormatString = "H:mm:ss dd-MM-yyyy"
    )]
    public DateTime TimeOfTransaction { get; set; }
    [Required]
    public int CustomerId { get; set; }
    public int? CashierId { get; set; }
    public int? ApproverId { get; set; }
    [Required]
    [DataType(DataType.Currency)]
    public double Amount { get; set; }
    [Required]
    public int AccountDebitedId { get; set; }
    [Required]
    public int AccountCreditedId { get; set; }
    [Required]
    [EnumDataType(typeof(TransactionTypes))]
    public string TransactionType { get; set; }
  }
}

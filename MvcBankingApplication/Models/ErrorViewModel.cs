namespace MvcBankingApplication.Models;

public class ErrorViewModel
{
#pragma warning disable CS8632
    public string? RequestId { get; set; }
#pragma warning restore CS8632

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

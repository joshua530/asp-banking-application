namespace MvcBankingApplication.Models.Notifications;

/// <summary>
/// notification meant to be seen by all admins
/// </summary>
public class AdminNotification : Notification
{
    public new string ApplicationUserId { get; set; }
}

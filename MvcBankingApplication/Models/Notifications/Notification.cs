using System.ComponentModel.DataAnnotations;
using MvcBankingApplication.Models.Users;

namespace MvcBankingApplication.Models.Notifications;

public class Notification
{
    public int Id { get; set; }

    [Required]
    public string Message { get; set; }

    [Required]
    public NotificationTypes Type { get; set; }

    public string ApplicationUserId { get; set; }

    public virtual ApplicationUser Owner { get; set; }
}

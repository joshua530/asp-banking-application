using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.Notifications;

public class NotificationModel
{
    public int Id { get; set; }
    [Required]
    public string Message { get; set; }
    [Required]
    public int UserId { get; set; }

    [Required]
    [EnumDataType(typeof(NotificationTypes))]
    public int Type { get; set; }
}

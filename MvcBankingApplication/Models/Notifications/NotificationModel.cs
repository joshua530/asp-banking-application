using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.Notifications;

public class NotificationModel
{
    public int Id { get; set; }
    [Required]
    public string Message { get; set; }
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// The types are:
    ///    0 - success;
    ///    1 - info;
    ///    2 - warning;
    ///    3 - danger
    /// </summary>
    public int Type { get; set; }
}

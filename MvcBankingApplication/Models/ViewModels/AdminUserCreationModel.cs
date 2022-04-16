using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.ViewModels;

/// <summary>
/// used when admin is creating a user(admin or cashier)
/// </summary>
public class AdminUserCreationModel
{
    [Required]
    [MinLength(3), MaxLength(20)]
    public string Username { get; set; }
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
}

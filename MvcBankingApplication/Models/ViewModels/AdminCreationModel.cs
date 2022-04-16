using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.ViewModels;

public class AdminCreationModel
{
    [Required]
    [MinLength(3), MaxLength(20)]
    public string Username { get; set; }
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
}

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace MvcBankingApplication.Models.Users;

abstract public class ApplicationUser : IdentityUser
{
    [Required]
    [RegularExpression(@"[a-zA-Z]{2,10}")]
    public string FirstName { get; set; } = String.Empty;

    [Required]
    [RegularExpression(@"[a-zA-Z]{2,10}")]
    public string LastName { get; set; } = String.Empty;

    public string FullName
    {
        get
        {
            return $"{FirstName} {LastName}";
        }
    }

    public override string ToString()
    {
        return $"Username<{UserName}>; FirstName<{FirstName}>;" +
        $" LastName<{LastName}>; Email<{Email}>";
    }
}

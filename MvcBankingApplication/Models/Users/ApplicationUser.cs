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

    [Required]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:MMMM dd, yyyy}")] // January 12, 2001
    public DateTime DateCreated { get; set; }

    public string ImageUrl { get; set; }

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

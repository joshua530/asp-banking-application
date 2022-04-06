using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MvcBankingApplication.Models.Users;

abstract public class ApplicationUser : IdentityUser
{
    [Required]
    [RegularExpression(@"[a-zA-Z]{2,10}")]
    public string FirstName { get; set; }

    [Required]
    [RegularExpression(@"[a-zA-Z]{2,10}")]
    public string LastName { get; set; }

    [DataType(DataType.Date)]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [DisplayFormat(DataFormatString = "{0:MMMM dd, yyyy}")] // January 12, 2001
    public DateTime? DateCreated { get; set; }

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

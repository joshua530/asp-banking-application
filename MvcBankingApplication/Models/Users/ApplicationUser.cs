using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.Users;

abstract public class ApplicationUser : IdentityUser
{
    private DateTime _dateCreated = DateTime.Now;

    [Required]
    [RegularExpression(@"[a-zA-Z]{2,10}")]
    public string FirstName { get; set; }

    [Required]
    [RegularExpression(@"[a-zA-Z]{2,10}")]
    public string LastName { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:MMMM dd, yyyy}")] // January 12, 2001
    public DateTime DateCreated
    {
        get
        {
            return _dateCreated;
        }
        set
        {
            _dateCreated = value;
        }
    }

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

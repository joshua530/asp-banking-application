using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.ViewModels;

public class Profile
{
    private bool _isProfileOwner = false;

    public string FullName { get; set; }

    public string ImageUrl { get; set; }

    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [DisplayFormat(DataFormatString = "{0:MMMM dd, yyyy}")]
    public DateTime DateCreated { get; set; }

    public string UserName { get; set; }

    public bool IsProfileOwner
    {
        get { return _isProfileOwner; }
        set { _isProfileOwner = value; }
    }
}

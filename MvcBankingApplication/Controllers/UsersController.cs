using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MvcBankingApplication.Models.Users;
using MvcBankingApplication.Models.ViewModels;


namespace MvcBankingApplication.Controllers;

#pragma warning disable CS8632
public class UsersController : Controller
{
    public UserManager<ApplicationUser> _userManager;
    public SignInManager<ApplicationUser> _signInManager;

    public UsersController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // GET: /Users/Profile/<id>
    [Authorize]
    public async Task<IActionResult> Profile(string? userId)
    {
        if (userId == null)
        {
            return View("NotFound404");
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return View("NotFound404");
        }

        var profile = new Profile
        {
            FullName = user.FullName,
            ImageUrl = user.ImageUrl,
            DateCreated = user.DateCreated,
            Email = user.Email,
            UserName = user.UserName
        };

        var userObj = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
        if (userId == userObj.Id)
        {
            profile.IsProfileOwner = true;
        }

        return View(profile);
    }

    [Authorize(Roles = "customer")]
    public IActionResult CustomerHome()
    {
        return View();
    }

    public IActionResult CashierHome()
    {
        return View();
    }

    public IActionResult AdminHome()
    {
        return View();
    }
}
#pragma warning restore CS8632

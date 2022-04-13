using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MvcBankingApplication.Models;
using Microsoft.AspNetCore.Identity;
using MvcBankingApplication.Models.Users;

namespace MvcBankingApplication.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public IActionResult Index()
    {
        if (_signInManager.IsSignedIn(User))
        {
            if (User.IsInRole("customer"))
            {
                return RedirectToAction("", "Customers");
            }
            else if (User.IsInRole("admin"))
            {
                return RedirectToAction("", "Admin");
            }
            else if (User.IsInRole("cashier"))
            {
                return RedirectToAction("", "Cashiers");
            }
        }
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult StaffLogin()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult BranchesNearUser()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

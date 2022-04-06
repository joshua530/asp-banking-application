using Microsoft.AspNetCore.Mvc;


namespace MvcBankingApplication.Controllers;

public class ErrorsController : Controller
{
    [Route("/404")]
    public IActionResult NotFound404()
    {
        return View();
    }

    [Route("/403")]
    public IActionResult Forbidden403()
    {
        return View();
    }
}

#nullable disable
using Microsoft.AspNetCore.Mvc;


namespace MvcBankingApplication.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationContext _context;

        public AdminController(ApplicationContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: Admin/Create
        public IActionResult CreateAdmin()
        {
            return View();
        }

        public IActionResult CreateCashier()
        {
            return View();
        }

        public IActionResult UpdateCashierLimit()
        {
            return View();
        }
    }
}

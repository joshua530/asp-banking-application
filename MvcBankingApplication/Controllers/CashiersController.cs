#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcBankingApplication.Models.Users;
using MvcBankingApplication.Models.ViewModels;


namespace MvcBankingApplication.Controllers
{
    public class CashiersController : Controller
    {
        private readonly ApplicationContext _context;

        public CashiersController(ApplicationContext context)
        {
            _context = context;
        }

        public IActionResult GeneralTransactions()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult WireTransfer()
        {
            return View();
        }

        public IActionResult WithdrawDepositOverdraft()
        {
            var model = new WithdrawDepositOverdraftModel();
            return View(model);
        }
    }
}

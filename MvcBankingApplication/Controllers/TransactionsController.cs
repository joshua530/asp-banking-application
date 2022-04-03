#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcBankingApplication.Models.Transactions;

namespace MvcBankingApplication.Controllers
{
  public class TransactionsController : Controller
  {
    private readonly ApplicationContext _context;

    public TransactionsController(ApplicationContext context)
    {
      _context = context;
    }

    // GET: Transactions
    // GET: Transactions/<user id>
    // public async Task<IActionResult> Index()
    // {
    //   return View(await _context.TransactionModel.ToListAsync());
    // }
    public IActionResult CustomerTransactions()
    {
      return View();
    }

    public IActionResult StaffTransactions()
    {
      return View();
    }

    // GET: Transactions/Details/5
    // public async Task<IActionResult> Details(int? id)
    // {
    //   if (id == null)
    //   {
    //     return NotFound();
    //   }

    //   var transactionModel = await _context.TransactionModel
    //       .FirstOrDefaultAsync(m => m.ID == id);
    //   if (transactionModel == null)
    //   {
    //     return NotFound();
    //   }

    //   return View(transactionModel);
    // }
    public IActionResult Details()
    {
      return View();
    }
  }
}

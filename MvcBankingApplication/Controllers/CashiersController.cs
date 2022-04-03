#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcBankingApplication.Models.Users;

namespace MvcBankingApplication.Controllers
{
    public class CashiersController : Controller
    {
        private readonly ApplicationContext _context;

        public CashiersController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: Cashiers/Details/5
        // public async Task<IActionResult> Details(int? id)
        // {
        //   if (id == null)
        //   {
        //     return NotFound();
        //   }

        //   var cashierModel = await _context.CashierModel
        //       .FirstOrDefaultAsync(m => m.ID == id);
        //   if (cashierModel == null)
        //   {
        //     return NotFound();
        //   }

        //   return View(cashierModel);
        // }
        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult UpdateProfile()
        {
            return View();
        }

        public IActionResult UpdatePassword()
        {
            return View();
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

        // GET: Cashiers/Edit/5
        // public async Task<IActionResult> Edit(int? id)
        // {
        //   if (id == null)
        //   {
        //     return NotFound();
        //   }

        //   var cashierModel = await _context.CashierModel.FindAsync(id);
        //   if (cashierModel == null)
        //   {
        //     return NotFound();
        //   }
        //   return View(cashierModel);
        // }

        // POST: Cashiers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(int id, [Bind("TransactionLimit,StaffId,IsAdmin,ID,ImageUrl,Password,Username,FirstName,LastName")] CashierModel cashierModel)
        {
            if (id != cashierModel.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cashierModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CashierModelExists(cashierModel.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cashierModel);
        }

        private bool CashierModelExists(int id)
        {
            return _context.CashierModel.Any(e => e.ID == id);
        }
    }
}

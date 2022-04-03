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
    public class AdminController : Controller
    {
        private readonly ApplicationContext _context;

        public AdminController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: Admin
        // public async Task<IActionResult> Index()
        // {
        //     return View(await _context.AdminModel.ToListAsync());
        // }
        public IActionResult Index()
        {
            return View();
        }

        // GET: Admin/Profile/5
        // public async Task<IActionResult> Profile(int? id)
        // {
        //     if (id == null)
        //     {
        //         return NotFound();
        //     }

        //     var adminModel = await _context.AdminModel
        //         .FirstOrDefaultAsync(m => m.ID == id);
        //     if (adminModel == null)
        //     {
        //         return NotFound();
        //     }

        //     return View(adminModel);
        // }
        public IActionResult Profile(int? id)
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

        // POST: Admin/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Create([Bind("StaffId,IsAdmin,ID,ImageUrl,Password,Username,FirstName,LastName")] AdminModel adminModel)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         _context.Add(adminModel);
        //         await _context.SaveChangesAsync();
        //         return RedirectToAction(nameof(Index));
        //     }
        //     return View(adminModel);
        // }

        // GET: Admin/Edit/5
        // public async Task<IActionResult> UpdateProfile(int? id)
        // {
        //     if (id == null)
        //     {
        //         return NotFound();
        //     }

        //     var adminModel = await _context.AdminModel.FindAsync(id);
        //     if (adminModel == null)
        //     {
        //         return NotFound();
        //     }
        //     return View(adminModel);
        // }
        public IActionResult UpdateProfile()
        {
            return View();
        }

        public IActionResult UpdatePassword()
        {
            return View();
        }

        public IActionResult UpdateCashierLimit()
        {
            return View();
        }

        // POST: Admin/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Edit(int id, [Bind("StaffId,IsAdmin,ID,ImageUrl,Password,Username,FirstName,LastName")] AdminModel adminModel)
        // {
        //     if (id != adminModel.ID)
        //     {
        //         return NotFound();
        //     }

        //     if (ModelState.IsValid)
        //     {
        //         try
        //         {
        //             _context.Update(adminModel);
        //             await _context.SaveChangesAsync();
        //         }
        //         catch (DbUpdateConcurrencyException)
        //         {
        //             if (!AdminModelExists(adminModel.ID))
        //             {
        //                 return NotFound();
        //             }
        //             else
        //             {
        //                 throw;
        //             }
        //         }
        //         return RedirectToAction(nameof(Index));
        //     }
        //     return View(adminModel);
        // }

        private bool AdminModelExists(int id)
        {
            return _context.AdminModel.Any(e => e.ID == id);
        }
    }
}

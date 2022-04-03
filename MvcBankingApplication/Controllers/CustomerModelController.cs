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
  public class CustomerModelController : Controller
  {
    private readonly ApplicationContext _context;

    public CustomerModelController(ApplicationContext context)
    {
      _context = context;
    }

    // GET: CustomerModel
    // public async Task<IActionResult> Index()
    // {
    //     return View(await _context.CustomerModel.ToListAsync());
    // }
    public IActionResult Index()
    {
      return View();
    }

    // GET: CustomerModel/Details/5
    // public async Task<IActionResult> Details(int? id)
    // {
    //     if (id == null)
    //     {
    //         return NotFound();
    //     }

    //     var customerModel = await _context.CustomerModel
    //         .FirstOrDefaultAsync(m => m.ID == id);
    //     if (customerModel == null)
    //     {
    //         return NotFound();
    //     }

    //     return View(customerModel);
    // }
    public IActionResult Details()
    {
      return View();
    }

    // GET: CustomerModel/EditPassword
    public IActionResult EditPassword()
    {
      return View();
    }

    // GET: CustomerModel/Create
    public IActionResult Create()
    {
      return View();
    }

    // POST: CustomerModel/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ID,ImageUrl,Password,Username,FirstName,LastName")] CustomerModel customerModel)
    {
      if (ModelState.IsValid)
      {
        _context.Add(customerModel);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
      }
      return View(customerModel);
    }

    // GET: CustomerModel/Edit/5
    // public async Task<IActionResult> Edit(int? id)
    // {
    //   if (id == null)
    //   {
    //     return NotFound();
    //   }

    //   var customerModel = await _context.CustomerModel.FindAsync(id);
    //   if (customerModel == null)
    //   {
    //     return NotFound();
    //   }
    //   return View(customerModel);
    // }
    public IActionResult Edit()
    {
      return View();
    }

    // POST: CustomerModel/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ID,ImageUrl,Password,Username,FirstName,LastName")] CustomerModel customerModel)
    {
      if (id != customerModel.ID)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          _context.Update(customerModel);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!CustomerModelExists(customerModel.ID))
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
      return View(customerModel);
    }

    // GET: CustomerModel/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var customerModel = await _context.CustomerModel
          .FirstOrDefaultAsync(m => m.ID == id);
      if (customerModel == null)
      {
        return NotFound();
      }

      return View(customerModel);
    }

    // POST: CustomerModel/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var customerModel = await _context.CustomerModel.FindAsync(id);
      _context.CustomerModel.Remove(customerModel);
      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }

    private bool CustomerModelExists(int id)
    {
      return _context.CustomerModel.Any(e => e.ID == id);
    }
  }
}

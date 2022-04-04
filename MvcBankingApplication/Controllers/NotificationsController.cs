#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcBankingApplication.Models.Notifications;

namespace MvcBankingApplication.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationContext _context;

        public NotificationsController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: Notification
        // public async Task<IActionResult> Index()
        // {
        //     return View(await _context.NotificationModel.ToListAsync());
        // }
        public IActionResult Index()
        {
            return View();
        }

        // POST: Notification/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var notificationModel = await _context.NotificationModel.FindAsync(id);
            _context.NotificationModel.Remove(notificationModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotificationModelExists(int id)
        {
            return _context.NotificationModel.Any(e => e.Id == id);
        }
    }
}

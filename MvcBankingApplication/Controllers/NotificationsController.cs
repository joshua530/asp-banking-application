#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcBankingApplication.Models.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MvcBankingApplication.Models.Users;

namespace MvcBankingApplication.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(ApplicationContext context, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            bool isAdmin = User.IsInRole("admin");
            string userId = _userManager.GetUserAsync(User).GetAwaiter().GetResult().Id;
            List<Notification> allUserNotifications = _context.Notifications
                                                .Where(
                                                    n => n.ApplicationUserId == userId)
                                                .ToList();

            if (isAdmin)
            {
                List<AdminNotification> adminNotifications = _context.AdminNotifications.Where(n => true).ToList();
                adminNotifications.ForEach(n => allUserNotifications.Add((Notification)n));
            }
            return View(allUserNotifications);
        }

        // POST: Notification/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var canDeleteNotification = NotificationExistsForUser(id);
            if (canDeleteNotification)
            {
                var notificationModel = await _context.Notifications.FindAsync(id);
                _context.Notifications.Remove(notificationModel);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public bool NotificationExistsForUser(int id)
        {
            var userId = _userManager.GetUserAsync(User)
                            .GetAwaiter()
                            .GetResult()
                            .Id;
            // does the notification belong to the user specifically?
            var ownedByUser = _context.Notifications.Any(
                n => n.Id == id && n.ApplicationUserId == userId);

            // is the user an admin and was the notification intended for all
            // admins?
            var isAdmin = User.IsInRole("admin");
            var ownedByAdmins = _context.AdminNotifications.Any(n => n.Id == id);
            if (isAdmin && ownedByAdmins)
                ownedByUser = true;

            return ownedByUser;
        }

        private bool NotificationModelExists(int id)
        {
            return _context.Notifications.Any(e => e.Id == id);
        }
    }
}

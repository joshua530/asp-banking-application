#nullable disable
using Microsoft.AspNetCore.Mvc;
using MvcBankingApplication.Models.Transactions;
using MvcBankingApplication.Models.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MvcBankingApplication.Models.Users;
using MvcBankingApplication.Models.Accounts;


namespace MvcBankingApplication.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationContext context,
            UserManager<ApplicationUser> manager,
            ILogger<AdminController> logger)
        {
            _logger = logger;
            _context = context;
            _userManager = manager;
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

        public IActionResult PendingTransactions()
        {
            var pendingTrxQuery = from p_trx in _context.PendingTransactions
                                  select p_trx;
            var pendingTransactions = pendingTrxQuery
                                      .AsEnumerable()
                                      .ToArray();
            return View(pendingTransactions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PendingTransactions(int id = -1, string action = null)
        {
            if (!ModelState.IsValid || id == -1 || action == null)
            {
                return RedirectToAction("PendingTransactions", "Admin");
            }
            if (action != "decline" && action != "approve")
            {
                return RedirectToAction("PendingTransactions", "Admin");
            }

            // is pending transaction valid?
            var pendingTrxQuery = from pending_trx in _context.PendingTransactions
                                  where pending_trx.ID == id
                                  select pending_trx;
            PendingTransaction pendingTrx = pendingTrxQuery.FirstOrDefault();
            if (pendingTrx == null)
            {
                return RedirectToAction("PendingTransactions", "Admin");
            }
            int acDebitId = pendingTrx.AccountDebitedId;
            int acCreditId = pendingTrx.AccountCreditedId;
            double amount = pendingTrx.Amount;
            string userId = _userManager.GetUserAsync(User).GetAwaiter().GetResult().Id;

            using (var transaction = _context.Database.BeginTransaction())
            {
                if (action == "approve")
                {
                    var creditAc = FindAccount(pendingTrx.AccountCreditedId);
                    if (creditAc == null)
                        return RedirectToAction("PendingTransactions", "Admin");
                    var debitAc = FindAccount(pendingTrx.AccountDebitedId);
                    if (debitAc == null)
                        return RedirectToAction("PendingTransactions", "Admin");

                    var trx = CopyTransaction(pendingTrx);
                    trx.ApproverId = userId;

                    _context.Transactions.Add(trx);
                    _context.PendingTransactions.Remove(pendingTrx);

                    if (creditAc.Balance < amount)
                    {
                        _logger.LogCritical($"account balance of account number {creditAc.Id} is less than {amount}.");
                        TempData["Error"] = $"account balance of account number {creditAc.Id.ToString("D5")} is less than {amount}.";
                        return RedirectToAction("PendingTransactions", "Admin");
                    }
                    creditAc.Balance -= amount;
                    debitAc.Balance += amount;
                    _context.Update(creditAc);
                    _context.Update(debitAc);

                    var cashierNotification = new Notification
                    {
                        Message = "your transaction has been approved",
                        Type = NotificationTypes.SUCCESS,
                        ApplicationUserId = pendingTrx.CashierId
                    };
                    var customerNotification = new Notification
                    {
                        Message = "your transaction has been completed. Thank you for banking with us",
                        Type = NotificationTypes.SUCCESS,
                        ApplicationUserId = pendingTrx.CustomerId
                    };
                    _context.Notifications.Add(cashierNotification);
                    _context.Notifications.Add(customerNotification);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                else if (action == "decline")
                {
                    var cashierNotification = new Notification
                    {
                        Message = "your transaction approval request has been denied",
                        Type = NotificationTypes.SUCCESS,
                        ApplicationUserId = pendingTrx.CashierId
                    };
                    var customerNotification = new Notification
                    {
                        Message = "your transaction has not been completed. Contact us for more information",
                        Type = NotificationTypes.SUCCESS,
                        ApplicationUserId = pendingTrx.CustomerId
                    };
                    _context.Notifications.Add(cashierNotification);
                    _context.Notifications.Add(customerNotification);
                    _context.PendingTransactions.Remove(pendingTrx);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                return RedirectToAction("PendingTransactions", "Admin");
            }
        }

        private Transaction CopyTransaction(Transaction t)
        {
            var trx = new Transaction
            {
                Amount = t.Amount,
                TransactionType = t.TransactionType,
                CustomerId = t.CustomerId,
                CashierId = t.CashierId,
                ApproverId = t.ApproverId,
                AccountCreditedId = t.AccountCreditedId,
                AccountDebitedId = t.AccountDebitedId
            };
            return trx;
        }

        private AccountModel FindAccount(int id)
        {
            var customerAccount = _context
                                .CustomerAccounts
                                .Where(ca => ca.Id == id)
                                .FirstOrDefault();
            if (customerAccount != null)
                return customerAccount;
            var bankCashAccount = _context.BankCashAccount.FirstOrDefault();
            if (bankCashAccount != null)
                return bankCashAccount;
            var bankOverdraftAccount = _context.BankOverdraftAccount.FirstOrDefault();
            if (bankOverdraftAccount != null)
                return bankOverdraftAccount;
            return null;
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

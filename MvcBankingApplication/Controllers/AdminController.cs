#nullable disable
using Microsoft.AspNetCore.Mvc;
using MvcBankingApplication.Models.Transactions;
using MvcBankingApplication.Models.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MvcBankingApplication.Models.Users;
using MvcBankingApplication.Models.Accounts;
using MvcBankingApplication.Models.ViewModels;


namespace MvcBankingApplication.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;
        private readonly IUserStore<ApplicationUser> _userStore;

        public AdminController(
            ApplicationContext context,
            UserManager<ApplicationUser> manager,
            ILogger<AdminController> logger,
            IUserStore<ApplicationUser> userStore)
        {
            _logger = logger;
            _context = context;
            _userManager = manager;
            _userStore = userStore;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateCashier()
        {
            var model = new AdminUserCreationModel { };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCashier(AdminUserCreationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userWithEmail = UserWithEmail(model.Email);
            if (userWithEmail != null)
            {
                ModelState.AddModelError("Email", "that email has already been taken. Try another one");
            }
            var userWithUsername = UserWithUsername(model.Username);
            if (userWithUsername != null)
            {
                ModelState.AddModelError("Username", "that username has already been taken. Try another one");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Cashier cashier = InstantiateCashier();
            cashier.UserName = model.Username;
            cashier.Email = model.Email;

            cashier.FirstName = model.Username;
            cashier.LastName = model.Username;
            cashier.EmailConfirmed = true;
            cashier.TransactionLimit = 10000;

            string password = GeneratePassword();
            var userCreation = await _userManager.CreateAsync(cashier, password);

            if (userCreation.Succeeded)
            {
                _logger.LogInformation(String.Format($"Created cashier id {0}", cashier.Id));
                await _userManager.AddToRoleAsync(cashier, "cashier");
                // user should reset password after this to gain access to their account
            }
            else
            {
                ModelState.AddModelError("", "User creation failed");
                return View(model);
            }
            return RedirectToAction("", "Admin");
        }

        public IActionResult CreateAdmin()
        {
            var model = new AdminUserCreationModel { };
            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(AdminUserCreationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userWithEmail = UserWithEmail(model.Email);
            if (userWithEmail != null)
            {
                ModelState.AddModelError("Email", "that email has already been taken. Try another one");
            }
            var userWithUsername = UserWithUsername(model.Username);
            if (userWithUsername != null)
            {
                ModelState.AddModelError("Username", "that username has already been taken. Try another one");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Admin admin = InstantiateAdmin();
            admin.UserName = model.Username;
            admin.Email = model.Email;

            admin.FirstName = model.Username;
            admin.LastName = model.Username;
            admin.EmailConfirmed = true;

            string password = GeneratePassword();
            var userCreation = await _userManager.CreateAsync(admin, password);

            if (userCreation.Succeeded)
            {
                _logger.LogInformation(String.Format($"Created admin id {0}", admin.Id));
                await _userManager.AddToRoleAsync(admin, "admin");
                // user should reset password after this to gain access to their account
            }
            else
            {
                ModelState.AddModelError("", "User creation failed");
                return View(model);
            }
            return RedirectToAction("", "Admin");
        }

        private Admin InstantiateAdmin()
        {
            try
            {
                return Activator.CreateInstance<Admin>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Admin)}'. " +
                    $"Ensure that '{nameof(Admin)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private Cashier InstantiateCashier()
        {
            try
            {
                return Activator.CreateInstance<Cashier>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Cashier)}'. " +
                    $"Ensure that '{nameof(Cashier)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        /// <summary>
        /// generates a random password that conforms to password policies
        /// each time it is called
        /// </summary>
        private string GeneratePassword()
        {
            IDictionary<string, char[]> charClasses = GeneratePasswordCharacters();
            string[] types = { "lower", "upper", "int", "special" };

            // the password is divided into four regions. each of the regions
            // should contain at least two characters with the second class
            // containing only two characters
            // the maximum number of characters that each region can contain is
            // four
            // | 0 | 1 | 2 | 3 |
            string password = "";
            // ensure the character types occupying each region are selected randomly
            int[] charClassIndices = GenerateRandomIntArray(0, 3, 4);
            for (int i = 0; i < 4; ++i)
            {
                int currentPosIndex = charClassIndices[i];
                // get the type of character that will occupy the current region
                string charClass = types[currentPosIndex];
                char[] chars = charClasses[charClass];
                int[] randomIndices;
                int lastIndex = chars.Length - 1;

                // indices for second region
                if (i == 1)
                {
                    randomIndices = GenerateRandomIntArray(0, lastIndex, 2);
                }
                else
                {
                    int numCharsForRegion = new Random().Next(2, 4);
                    randomIndices = GenerateRandomIntArray(0, 4, numCharsForRegion);
                }
                foreach (int j in randomIndices)
                {
                    password += chars[j];
                }
            }
            return password;
        }

        private Dictionary<string, char[]> GeneratePasswordCharacters()
        {
            Dictionary<string, char[]> passwordChars = new Dictionary<string, char[]>();
            passwordChars.Add(
                "lower",
                new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' }
            );
            passwordChars.Add(
                "upper",
                new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' }
            );
            passwordChars.Add(
                "int",
                new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' }
            );
            passwordChars.Add("special", new char[] { '@', '#', '$', '-', '=', '/', '*', '!', '?', '~' });
            return passwordChars;
        }

        /// <summary>
        /// generates a list of random numbers without repeating
        /// </summary>
        /// <param name="start">minimum value to include in list</param>
        /// <param name="end">maximum value to include in list</param>
        private int[] GenerateRandomIntArray(int start, int end, int count)
        {
            int possibleListLen = (end - start) + 1;
            // if possibly generated list length is less than the required number
            // of items, return empty array since the required list will never
            // be generated. the while loops below will run forever if this is
            // not done
            if (possibleListLen < count)
            {
                return new int[] { };
            }

            List<int> generated = new List<int>();
            Random rand = new Random();

            while (generated.Count() < count)
            {
                int genInt = rand.Next(start, end + 1);
                // is generated number in the list of already generated numbers?
                while (generated.Contains(genInt))
                {
                    genInt = rand.Next(start, end);
                }
                generated.Add(genInt);
            }
            return generated.ToArray();
        }

        private ApplicationUser UserWithEmail(string email)
        {
            var admin = _context.Admins
                        .Where(user => user.Email == email)
                        .FirstOrDefault();
            if (admin != null)
                return admin;
            var cashier = _context.Cashiers
                            .Where(user => user.Email == email)
                            .FirstOrDefault();
            if (cashier != null)
                return cashier;
            var customer = _context.Customers
                            .Where(user => user.Email == email)
                            .FirstOrDefault();
            if (customer != null)
                return customer;
            return null;
        }

        private ApplicationUser UserWithUsername(string username)
        {
            var admin = _context.Admins
                        .Where(user => user.UserName == username)
                        .FirstOrDefault();
            if (admin != null)
                return admin;
            var cashier = _context.Cashiers
                            .Where(user => user.UserName == username)
                            .FirstOrDefault();
            if (cashier != null)
                return cashier;
            var customer = _context.Customers
                            .Where(user => user.UserName == username)
                            .FirstOrDefault();
            if (customer != null)
                return customer;
            return null;
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
            if (!ModelState.IsValid || id < 1 || action == null)
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
                    if (pendingTrx.TransactionType == TransactionTypes.OVERDRAFT)
                    {
                        var customerAccount = (CustomerAccount)debitAc;
                        var overdrawableAmount = customerAccount.OverdraftLimit - customerAccount.OverdrawnAmount;
                        if (pendingTrx.Amount > overdrawableAmount)
                        {
                            TempData["Error"] = $"amount provided({pendingTrx.Amount.ToString("0.00")}) exceeds overdrawable amount({overdrawableAmount.ToString("0.00")}).";
                            return RedirectToAction("PendingTransactions", "Admin");
                        }
                        customerAccount.OverdrawnAmount += amount;
                        _context.Update(customerAccount);
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

        public IActionResult UpdateCashierLimit()
        {
            var model = new CashierLimitModel { };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCashierLimit(CashierLimitModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // cashier exists?
            var cashier = _context.Cashiers
                            .Where(c => c.Id == model.CashierId)
                            .FirstOrDefault();
            if (cashier == null)
            {
                ModelState.AddModelError("", "invalid cashier id provided");
                return View(model);
            }
            cashier.TransactionLimit = model.NewLimit;
            _context.Update(cashier);
            await _context.SaveChangesAsync();
            TempData["Message"] = "cashier limit updated successfully";
            return RedirectToAction("", "Admin");
        }
    }
}

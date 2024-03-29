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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MvcBankingApplication.Models.Transactions;
using MvcBankingApplication.Models.Notifications;


namespace MvcBankingApplication.Controllers
{
    [Authorize(Roles = "cashier")]
    public class CashiersController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CashiersController> _logger;

        public CashiersController(ApplicationContext context, UserManager<ApplicationUser> userManager, ILogger<CashiersController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
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

        private string GetCashierId()
        {
            return _userManager.GetUserAsync(User).GetAwaiter().GetResult().Id;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult WireTransfer(CashierWireTransferModel model)
        {
            var fromAccount = _context.CustomerAccounts
                                  .Where(c_a => c_a.Id == model.From)
                                  .FirstOrDefault();
            if (fromAccount == null)
            {
                ModelState.AddModelError("From", "invalid account number");
            }
            var toAccount = _context.CustomerAccounts
                            .Where(c_a => c_a.Id == model.To)
                            .FirstOrDefault();
            if (toAccount == null)
            {
                ModelState.AddModelError("To", "invalid account number");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var fatalErrors = new List<string>();
            ViewData["FatalErrors"] = fatalErrors;
            if (fromAccount.Balance < model.Amount)
            {
                fatalErrors.Add($"Amount requested({model.Amount.ToString("0.00")}) exceeds account balance({fromAccount.Balance.ToString("0.00")})");
                return View(model);
            }

            // all cashier wire transfers require validation
            using (var transaction = _context.Database.BeginTransaction())
            {
                var cashierId = GetCashierId();
                var pendingTrx = new PendingTransaction
                {
                    Amount = model.Amount,
                    TransactionType = TransactionTypes.WIRE_TRANSFER,
                    CustomerId = fromAccount.CustomerId,
                    CashierId = cashierId,
                    AccountDebitedId = toAccount.Id,
                    AccountCreditedId = fromAccount.Id
                };

                var cashierNotification = new Notification
                {
                    Message = "Transaction approval request has been successfully submitted. Check your notifications periodically for an approval notification",
                    Type = NotificationTypes.INFO,
                    ApplicationUserId = cashierId
                };
                var customerNotification = new Notification
                {
                    Message = "Your withdrawal request is being processed. Check your notifications periodically for completion of the transaction",
                    Type = NotificationTypes.INFO,
                    ApplicationUserId = fromAccount.CustomerId
                };
                var adminNotification = new AdminNotification
                {
                    Message = "A transaction requires your approval. Check the transactions page for the transaction",
                    Type = NotificationTypes.INFO
                };

                _context.Notifications.Add(cashierNotification);
                _context.Notifications.Add(customerNotification);
                _context.Notifications.Add(adminNotification);
                _context.PendingTransactions.Add(pendingTrx);

                _context.SaveChanges();
                transaction.Commit();
                TempData["Message"] = "Transaction approval request has been successfully submitted. Check your notifications periodically for an approval notification";
                return RedirectToAction("", "Cashiers");
            }
        }

        public IActionResult WithdrawDepositOverdraft()
        {
            var model = new WithdrawDepositOverdraftModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WithdrawDepositOverdraft(WithdrawDepositOverdraftModel model)
        {
            if (ModelState.IsValid)
            {
                var errors = new List<string>();
                ViewData["FatalErrors"] = errors;
                // ensure account number is valid and belongs to a customer
                var accountQuery = from c_a in _context.CustomerAccounts
                                   where c_a.Id == model.AccountNumber
                                   select c_a;
                var customerAccount = accountQuery.FirstOrDefault();
                if (customerAccount == null)
                {
                    ModelState.AddModelError("AccountNumber", "Invalid account number");
                    return View(model);
                }

                var cashier = (Cashier)_userManager.GetUserAsync(User).GetAwaiter().GetResult();
                bool shouldBeApproved = false;
                if (cashier.TransactionLimit < model.Amount)
                {
                    shouldBeApproved = true;
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    // deposits need no approval
                    if (model.TransactionType == TransactionType.Deposit)
                    {
                        var bankCashAccount = _context.BankCashAccount.FirstOrDefault();
                        if (bankCashAccount == null)
                        {
                            errors.Add("Your transaction cannot be processed at this time. If the error persists, contact us");
                            _logger.LogCritical("bank cash account does not exist");
                            return View();
                        }

                        if (bankCashAccount.Balance < model.Amount)
                        {
                            // bank does not have enough cash reserves
                            errors.Add("The transaction cannot be processed at this time. We have informed our technical team on it. If the error persists, please contact us.");
                            _logger.LogCritical("Bank cash account does not have enough reserves");
                            return View(model);
                        }
                        // add amount to customer account
                        customerAccount.Balance += model.Amount;
                        bankCashAccount.Balance -= model.Amount;
                        var trx1 = new Transaction
                        {
                            Amount = model.Amount,
                            TransactionType = TransactionTypes.DEBIT,
                            CustomerId = customerAccount.CustomerId,
                            AccountDebitedId = customerAccount.Id,
                            AccountCreditedId = bankCashAccount.Id,
                            CashierId = cashier.Id
                        };
                        _context.Transactions.Add(trx1);

                        // repay overdraft
                        if (customerAccount.OverdrawnAmount > 0)
                        {
                            var bankOverdraftAccount = _context.BankOverdraftAccount.FirstOrDefault();
                            var overdrawnAmount = customerAccount.OverdrawnAmount;
                            // amount in customer's account is sufficient to fully pay
                            // out overdraft balance
                            if (customerAccount.Balance >= overdrawnAmount)
                            {
                                customerAccount.Balance -= overdrawnAmount;
                                customerAccount.OverdrawnAmount -= overdrawnAmount;
                                // since the balance is fully paid, increment
                                // their overdraft limit
                                customerAccount.OverdraftLimit *= 1.1;
                                bankOverdraftAccount.Balance += overdrawnAmount;
                                var trx2 = new Transaction
                                {
                                    Amount = overdrawnAmount,
                                    TransactionType = TransactionTypes.CREDIT,
                                    CustomerId = customerAccount.CustomerId,
                                    AccountDebitedId = bankOverdraftAccount.Id,
                                    AccountCreditedId = customerAccount.Id,
                                    CashierId = cashier.Id
                                };
                                _context.Transactions.Add(trx2);
                            }
                            // amount in customer's account is not sufficient enough
                            // to fully pay out the overdraft balance
                            else
                            {
                                var paidAmount = customerAccount.Balance;
                                bankOverdraftAccount.Balance += paidAmount;
                                customerAccount.Balance = 0;
                                customerAccount.OverdrawnAmount -= paidAmount;
                                var trx3 = new Transaction
                                {
                                    Amount = paidAmount,
                                    TransactionType = TransactionTypes.CREDIT,
                                    CustomerId = customerAccount.CustomerId,
                                    AccountDebitedId = bankOverdraftAccount.Id,
                                    AccountCreditedId = customerAccount.Id,
                                    CashierId = cashier.Id
                                };
                                _context.Transactions.Add(trx3);
                            }
                            _context.BankOverdraftAccount.Update(bankOverdraftAccount);
                        }
                        _context.CustomerAccounts.Update(customerAccount);
                        _context.BankCashAccount.Update(bankCashAccount);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                        return RedirectToAction("", "Cashiers");
                    }
                    else if (model.TransactionType == TransactionType.Widthdraw)
                    {
                        var bankCashAccount = _context.BankCashAccount.FirstOrDefault();
                        if (bankCashAccount == null)
                        {
                            errors.Add("Your transaction cannot be processed at this time. If the error persists, contact us");
                            _logger.LogCritical("bank cash account does not exist");
                            return View();
                        }

                        if (customerAccount.Balance < model.Amount)
                        {
                            errors.Add($"Amount given exceeds account balance. Balance is {customerAccount.Balance}");
                            return View(model);
                        }

                        if (!shouldBeApproved)
                        {

                            customerAccount.Balance -= model.Amount;
                            bankCashAccount.Balance += model.Amount;
                            var trx = new Transaction
                            {
                                CashierId = cashier.Id,
                                CustomerId = customerAccount.CustomerId,
                                AccountDebitedId = bankCashAccount.Id,
                                AccountCreditedId = customerAccount.Id,
                                Amount = model.Amount,
                                TransactionType = TransactionTypes.CREDIT
                            };
                            _context.Transactions.Add(trx);
                            _context.CustomerAccounts.Update(customerAccount);
                            _context.BankCashAccount.Update(bankCashAccount);
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                        }
                        else
                        {
                            var pendingTrx = new PendingTransaction
                            {
                                CustomerId = customerAccount.CustomerId,
                                Amount = model.Amount,
                                AccountCreditedId = customerAccount.Id,
                                AccountDebitedId = bankCashAccount.Id,
                                CashierId = cashier.Id,
                                TransactionType = TransactionTypes.CREDIT
                            };
                            var cashierNotification = new Notification
                            {
                                Message = "Transaction approval request has been successfully submitted. Check your notifications periodically for an approval notification",
                                Type = NotificationTypes.INFO,
                                ApplicationUserId = cashier.Id
                            };
                            var customerNotification = new Notification
                            {
                                Message = "Your withdrawal request is being processed. Check your notifications periodically for completion of the transaction",
                                Type = NotificationTypes.INFO,
                                ApplicationUserId = customerAccount.CustomerId
                            };
                            var adminNotification = new AdminNotification
                            {
                                Message = "A transaction requires your approval. Check the transactions page for the transaction",
                                Type = NotificationTypes.INFO
                            };

                            _context.PendingTransactions.Add(pendingTrx);
                            _context.Notifications.Add(cashierNotification);
                            _context.Notifications.Add(customerNotification);
                            _context.AdminNotifications.Add(adminNotification);
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            // notify cashier
                            TempData["Message"] = "Approval request has been submitted. Check your notifications for an approval";
                        }

                        return RedirectToAction("", "Cashiers");
                    }
                    else if (model.TransactionType == TransactionType.Overdraft)
                    {
                        var bankOverdraftAccount = _context.BankOverdraftAccount.FirstOrDefault();
                        if (bankOverdraftAccount == null)
                        {
                            errors.Add("Your transaction cannot be processed at this time. If the error persists, contact us");
                            _logger.LogCritical("bank overdraft account does not exist");
                            return View();
                        }
                        if (!shouldBeApproved)
                        {
                            if (bankOverdraftAccount.Balance < model.Amount)
                            {
                                _logger.LogCritical("Bank overdraft account does not have enough reserves for this transaction");
                                errors.Add("Your transaction cannot be processed at this time. If the error persists, contact us");
                                return View();
                            }
                            if (model.Amount > customerAccount.OverdraftLimit)
                            {
                                errors.Add($"Amount requested exceeds the overdraft limit of the account {customerAccount.Id.ToString("D5")}. Current overdraft limit is {customerAccount.OverdraftLimit.ToString("0.00")}");
                                return View();
                            }
                            bankOverdraftAccount.Balance -= model.Amount;
                            customerAccount.Balance += model.Amount;
                            customerAccount.OverdrawnAmount += model.Amount;
                            var trx = new Transaction
                            {
                                Amount = model.Amount,
                                AccountDebitedId = customerAccount.Id,
                                AccountCreditedId = bankOverdraftAccount.Id,
                                TransactionType = TransactionTypes.DEBIT,
                                CustomerId = customerAccount.CustomerId,
                                CashierId = cashier.Id
                            };
                            _context.Transactions.Add(trx);
                            _context.CustomerAccounts.Update(customerAccount);
                            _context.BankOverdraftAccount.Update(bankOverdraftAccount);
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                        }
                        else
                        {
                            var pendingTrx = new PendingTransaction
                            {
                                CustomerId = customerAccount.CustomerId,
                                Amount = model.Amount,
                                AccountDebitedId = customerAccount.Id,
                                AccountCreditedId = bankOverdraftAccount.Id,
                                CashierId = cashier.Id,
                                TransactionType = TransactionTypes.OVERDRAFT
                            };
                            var cashierNotification = new Notification
                            {
                                Message = "Transaction approval request has been successfully submitted. Check your notifications periodically for an approval notification",
                                Type = NotificationTypes.INFO,
                                ApplicationUserId = cashier.Id
                            };
                            var customerNotification = new Notification
                            {
                                Message = "Your withdrawal request is being processed. Check your notifications periodically for completion of the transaction",
                                Type = NotificationTypes.INFO,
                                ApplicationUserId = customerAccount.CustomerId
                            };
                            var adminNotification = new AdminNotification
                            {
                                Message = "A transaction requires your approval. Check the transactions page for the transaction",
                                Type = NotificationTypes.INFO
                            };

                            _context.PendingTransactions.Add(pendingTrx);
                            _context.Notifications.Add(cashierNotification);
                            _context.Notifications.Add(customerNotification);
                            _context.AdminNotifications.Add(adminNotification);
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            // notify cashier
                            TempData["Message"] = "Approval request has been submitted. Check your notifications for an approval";
                        }
                        return RedirectToAction("", "Cashiers");
                    }
                }
            }
            return View(model);
        }
    }
}

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
                    var bankCashAccount = _context.BankCashAccount.FirstOrDefault();
                    if (bankCashAccount == null)
                    {
                        errors.Add("Your transaction cannot be processed at this time. If the error persists, contact us");
                        _logger.LogCritical("bank cash account does not exist");
                        return View();
                    }

                    // deposits need no approval
                    if (model.TransactionType == TransactionType.Deposit)
                    {
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
                        // without approval
                        if (!shouldBeApproved)
                        {
                            if (customerAccount.Balance < model.Amount)
                            {
                                errors.Add($"Amount given exceeds account balance. Balance is {customerAccount.Balance}");
                                return View(model);
                            }

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
                            return RedirectToAction("", "Cashiers");
                        }

                        // initiate transaction
                        //   {pending transaction}
                        //   - acc to credit
                        //   - acc to debit
                        //   - amount
                        //   - time
                        // add notification to cashier & customer that the transaction is
                        // still being processed
                        // add notification to admin to approve the transaction
                        // let admin approve the transaction
                        // once transaction is approved
                        //   - post the transaction
                        //   - delete pending transaction
                        //   - create notifications for cashier and user
                        // once the transaction goes through, inform both the cashier and
                        // customer that the transaction went through
                    }
                }
            }
            return View(model);
        }
    }
}

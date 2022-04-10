#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcBankingApplication.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using MvcBankingApplication.Models.Users;
using MvcBankingApplication.Models.Accounts;
using Microsoft.AspNetCore.Authorization;
using MvcBankingApplication.Models;
using MvcBankingApplication.Models.Transactions;


namespace MvcBankingApplication.Controllers
{
    [Authorize(Roles = "customer")]
    public class CustomersController : Controller
    {
        private readonly ApplicationContext _context;
        UserManager<ApplicationUser> _userManager;
        ILogger<CustomersController> _logger;

        public CustomersController(ApplicationContext context, UserManager<ApplicationUser> userManager, ILogger<CustomersController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = default)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = (Customer)user;
            IQueryable<CustomerAccount> query = from c_a in _context.CustomerAccounts
                                                where c_a.CustomerId == customer.Id
                                                select c_a;
            CustomerAccount account = await query.FirstAsync();
            if (page < 1)
            {
                page = 1;
            }
            int pageSize = 5;
            IQueryable<Transaction> trxQuery = from tr in _context.Transactions.Skip((page - 1) * pageSize).Take(pageSize)
                                               where tr.AccountCreditedId == account.Id ||
                                               tr.AccountDebitedId == account.Id
                                               select tr;
            var stockApi = new StockApiModel();

            var homeModel = new CustomerHomeModel
            {
                User = customer,
                Account = account,
                Stocks = stockApi.GetStocks(),
                Transactions = await trxQuery.ToListAsync()
            };

            return View(homeModel);
        }

        public IActionResult WireTransfer()
        {
            var model = new WireTransferModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WireTransfer(WireTransferModel model)
        {
            var fatalErrors = new List<string>();
            ViewData["FatalErrors"] = fatalErrors;
            CustomerAccount toSendTo = FindCustomerAccountById(model.AccountNumber);

            if (toSendTo == null)
            {
                ModelState.AddModelError("AccountNumber", "no account with that account number exists");
            }

            if (!ModelState.IsValid || fatalErrors.Count > 0)
            {
                return View(model);
            }

            var userId = GetUserId();
            CustomerAccount toWithdrawFrom = FindCustomerAccountByUserId(userId);
            // customer lacks account
            if (toWithdrawFrom == null)
            {
                fatalErrors.Add("Error occured. Our technical team has been notified. If this error persists, contact customer service.");
                _logger.LogCritical("Customer has no account");
                return View(model);
            }

            if (toWithdrawFrom.Id == toSendTo.Id)
            {
                fatalErrors.Add("Invalid transaction. You cannot transfer money to yourself");
                return View(model);
            }

            // perform the transaction
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // credit
                    if (toWithdrawFrom.Balance < model.Amount)
                    {
                        fatalErrors.Add("The amount requested exceeds your account balance");
                        return View(model);
                    }
                    toWithdrawFrom.Balance -= model.Amount;
                    // debit
                    toSendTo.Balance += model.Amount;
                    // record transaction
                    Transaction tr = new Transaction
                    {
                        Amount = model.Amount,
                        TransactionType = TransactionTypes.WIRE_TRANSFER,
                        CustomerId = userId,
                        AccountDebitedId = toSendTo.Id,
                        AccountCreditedId = toWithdrawFrom.Id
                    };
                    // commit
                    _context.CustomerAccounts.Update(toWithdrawFrom);
                    _context.CustomerAccounts.Update(toSendTo);
                    _context.Transactions.Add(tr);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (OperationCanceledException)
                {
                    fatalErrors.Add("Transaction failed. Refresh page and try again. If the error persists, contact us.");
                    _logger.LogError("Transaction failed");
                    return View(model);
                }
            }

            // transaction succeeded
            TempData["Message"] = "Transaction posted successfully";
            return RedirectToAction("", "Customers");
        }

        public IActionResult Overdraft()
        {
            var userId = GetUserId();
            var userAccount = FindCustomerAccountByUserId(userId);
            var model = new OverdraftModel
            {
                OverdraftLimit = userAccount.OverdraftLimit,
                OverdrawnAmount = userAccount.OverdrawnAmount
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Overdraft(OverdraftModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var fatalErrors = new List<string>();
            ViewData["FatalErrors"] = fatalErrors;

            var userId = GetUserId();
            CustomerAccount customerAccount = FindCustomerAccountByUserId(userId);
            // customer has no account
            if (customerAccount == null)
            {
                fatalErrors.Add("Error occured. Our technical team has been notified. If this error persists, contact customer service.");
                _logger.LogCritical("Customer has no account");
                return View(model);
            }

            var allowedAmount = customerAccount.OverdraftLimit - customerAccount.OverdrawnAmount;
            // customer has already reached overdraw limit
            if (allowedAmount <= 0)
            {
                fatalErrors.Add("You have already reached your overdraft limit. Pay off the overdrawn amount first in order to overdraw again");
                return View(model);
            }
            var overdraftDifference = allowedAmount - model.Amount;
            // requested amount will exceed allowed amount
            if (overdraftDifference < 0)
            {
                fatalErrors.Add("Requested amount exceeds your overdraft limit");
                return View(model);
            }

            BankOverdraftAccount bankOverdraftAccount = GetBankOverdraftAccount();
            if (bankOverdraftAccount == null)
            {
                fatalErrors.Add("Error occured. Our technical team has been notified. If this error persists, contact customer service.");
                _logger.LogCritical("Bank overdraft account does not exist");
                return View(model);
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // credit
                    bankOverdraftAccount.Balance -= model.Amount;
                    // debit
                    customerAccount.Balance += model.Amount;
                    customerAccount.OverdrawnAmount += model.Amount;
                    // save transaction
                    Transaction tr = new Transaction
                    {
                        Amount = model.Amount,
                        TransactionType = TransactionTypes.OVERDRAFT,
                        CustomerId = userId,
                        AccountCreditedId = bankOverdraftAccount.Id,
                        AccountDebitedId = customerAccount.Id
                    };
                    // post transaction
                    _context.CustomerAccounts.Update(customerAccount);
                    _context.BankOverdraftAccount.Update(bankOverdraftAccount);
                    _context.Transactions.Add(tr);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (OperationCanceledException)
                {
                    fatalErrors.Add("Transaction failed. Refresh page and try again. If the error persists, contact us.");
                    _logger.LogError("Transaction failed");
                    return View(model);
                }
            }

            // transaction succeeded
            TempData["Message"] = "Transaction posted successfully. Your overdraft balance will be recovered on the next deposit into your account";
            return RedirectToAction("", "Customers");
        }

        private string GetUserId()
        {
            return _userManager.GetUserAsync(User).GetAwaiter().GetResult().Id;
        }

        private CustomerAccount FindCustomerAccountByUserId(string userId)
        {
            var customerAccountQuery = from cust_ac in _context.CustomerAccounts
                                       where cust_ac.CustomerId == userId
                                       select cust_ac;

            var customerAccount = customerAccountQuery.FirstOrDefault();
            return customerAccount;
        }

        private BankOverdraftAccount GetBankOverdraftAccount()
        {
            var bankOverdraftQuery = from b_overdraft in _context.BankOverdraftAccount
                                     select b_overdraft;
            var bankOverdraftAccount = bankOverdraftQuery.FirstOrDefault();
            return bankOverdraftAccount;
        }

        private CustomerAccount FindCustomerAccountById(int id)
        {
            var accountQuery = from cust_ac in _context.CustomerAccounts
                               where cust_ac.Id == id
                               select cust_ac;
            var account = accountQuery.FirstOrDefault();
            return account;
        }
    }
}

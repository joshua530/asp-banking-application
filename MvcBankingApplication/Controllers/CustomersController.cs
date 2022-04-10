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

        public CustomersController(ApplicationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
            var accountQuery = from cust_ac in _context.CustomerAccounts
                               where cust_ac.Id == model.AccountNumber
                               select cust_ac;
            var accountToSendToList = await accountQuery.ToArrayAsync();

            var fatalErrors = new List<string>();
            ViewData["FatalErrors"] = fatalErrors;

            if (accountToSendToList.Length == 0)
            {
                ModelState.AddModelError("AccountNumber", "no account with that account number exists");
            }

            if (!ModelState.IsValid || fatalErrors.Count > 0)
            {
                return View(model);
            }

            var userId = _userManager.GetUserAsync(User).GetAwaiter().GetResult().Id;
            var toWithdrawFromQuery = from cust_ac in _context.CustomerAccounts
                                      where cust_ac.CustomerId == userId
                                      select cust_ac;
            var toWithdrawFromList = await toWithdrawFromQuery.ToArrayAsync();
            if (toWithdrawFromList.Length == 0 || toWithdrawFromList.Length > 1)
            {
                fatalErrors.Add("Error occured. Our technical team has been notified. If this error persists, contact customer service.");
                return View(model);
            }

            CustomerAccount toWithdrawFrom = toWithdrawFromList[0];
            CustomerAccount toSendTo = accountToSendToList[0];

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
                    return View(model);
                }
            }

            // transaction succeeded
            TempData["Message"] = "Transaction posted successfully";
            return RedirectToAction("", "Customers");
        }
    }
}

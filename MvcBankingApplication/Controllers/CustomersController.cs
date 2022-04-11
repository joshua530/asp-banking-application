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
            CustomerAccount account = FindCustomerAccountByUserId(user.Id);
            if (page < 1)
            {
                page = 1;
            }
            int pageSize = 5;
            var stockApi = new StockApiModel();
            IEnumerable<TransactionWithTypeStr> transactions = FindTransactions(account.Id, page, pageSize);

            var homeModel = new CustomerHomeModel
            {
                User = customer,
                Account = account,
                Stocks = stockApi.GetStocks(),
                Transactions = transactions
            };

            return View(homeModel);
        }

        public async Task<IActionResult> Transactions()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = (Customer)user;
            CustomerAccount account = FindCustomerAccountByUserId(user.Id);
            IEnumerable<TransactionWithTypeStr> transactions = FindTransactions(account.Id, 1, 5);
            return View(transactions);
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
                    double amountBeingTransferred = model.Amount;
                    toWithdrawFrom.Balance -= amountBeingTransferred;
                    // debit
                    toSendTo.Balance += amountBeingTransferred;
                    // record transaction
                    Transaction tr = new Transaction
                    {
                        Amount = amountBeingTransferred,
                        TransactionType = TransactionTypes.WIRE_TRANSFER,
                        CustomerId = userId,
                        AccountDebitedId = toSendTo.Id,
                        AccountCreditedId = toWithdrawFrom.Id
                    };
                    _context.CustomerAccounts.Update(toWithdrawFrom);
                    _context.CustomerAccounts.Update(toSendTo);
                    _context.Transactions.Add(tr);

                    // if there is an overdraft, recover it
                    var overdrawnAmount = toSendTo.OverdrawnAmount;
                    if (overdrawnAmount > 0)
                    {
                        double balanceOfOverdrawnAccount = toSendTo.Balance;
                        double recoveredAmount;
                        // balance of account can fully pay off overdraft
                        if (balanceOfOverdrawnAccount >= overdrawnAmount)
                        {
                            recoveredAmount = overdrawnAmount;
                        }
                        else
                        {
                            recoveredAmount = balanceOfOverdrawnAccount;
                        }

                        // record overdraft recovery
                        var bankOverdraftAccount = GetBankOverdraftAccount();
                        bankOverdraftAccount.Balance += recoveredAmount;
                        toSendTo.Balance -= recoveredAmount;
                        toSendTo.OverdrawnAmount -= recoveredAmount;

                        // increment overdraft limit by 10% if fully paid off
                        if (overdrawnAmount == recoveredAmount)
                        {
                            toSendTo.OverdraftLimit *= 1.1;
                        }
                        // record transaction
                        Transaction recovTransaction = new Transaction
                        {
                            Amount = recoveredAmount,
                            TransactionType = TransactionTypes.CREDIT,
                            AccountDebitedId = bankOverdraftAccount.Id,
                            AccountCreditedId = toSendTo.Id
                        };
                        _context.Update(bankOverdraftAccount);
                        _context.Update(toSendTo);
                        _context.Add(recovTransaction);
                    }

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
            TempData["Message"] = "Overdraft request was successful. Your overdraft balance will be recovered on the next deposit into your account. The amount already in your account will be part of the recovery";
            return RedirectToAction("", "Customers");
        }

        //################ refactored methods ###################3
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

        private IEnumerable<TransactionWithTypeStr> FindTransactions(int accountId, int page, int pageSize)
        {
            IQueryable<Transaction> trxQuery = from tr in _context.Transactions
                                               where tr.AccountCreditedId == accountId ||
                                               tr.AccountDebitedId == accountId
                                               select tr;
            var stockApi = new StockApiModel();

            List<TransactionWithTypeStr> transactions = new List<TransactionWithTypeStr>();
            var fetchedTransactions = trxQuery
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .OrderByDescending(t => t.TimeOfTransaction)
                                    .ToListAsync().GetAwaiter().GetResult();
            foreach (var trx in fetchedTransactions)
            {
                string type = "DR";
                if (trx.AccountCreditedId == accountId)
                    type = "CR";
                transactions.Add(new TransactionWithTypeStr
                {
                    TimeOfTransaction = trx.TimeOfTransaction,
                    Amount = trx.Amount,
                    TransactionTypeStr = type,
                    ID = trx.ID,
                    AccountDebitedId = trx.AccountDebitedId,
                    AccountCreditedId = trx.AccountCreditedId
                });
            }
            return transactions;
        }
    }
}

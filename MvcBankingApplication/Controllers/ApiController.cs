using Microsoft.AspNetCore.Mvc;
using MvcBankingApplication.Models.ViewModels;
using MvcBankingApplication.Models.Accounts;
using MvcBankingApplication.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MvcBankingApplication.Models.Transactions;
using System.Net;
using System.Linq;
using System.Collections.Generic;

namespace MvcBankingApplication.Controllers;

[ApiController]
public class ApiController : ControllerBase
{
    private ApplicationContext _context;
    private UserManager<ApplicationUser> _userManager;
    private ILogger<ApiController> _logger;

    public ApiController(ApplicationContext context, UserManager<ApplicationUser> manager, ILogger<ApiController> logger)
    {
        _context = context;
        _userManager = manager;
        _logger = logger;
    }

    public class PaginationLink
    {
        public string Href { get; set; }
        public bool IsActive { get; set; }
    }

    public class PaginationModel
    {
        public IEnumerable<CustomerTransactionModel> Transactions { get; set; }
        public PaginationLink PreviousLink { get; set; }
        public PaginationLink NextLink { get; set; }
    }

    [Authorize(Roles = "customer")]
    [HttpGet("api/transactions/customers/{id?}")]
    public ActionResult<PaginationModel>
    CustomerTransactions(
        int id = int.MinValue, int page = 1,
        int key = int.MinValue,
        int minAmount = 1,
        int maxAmount = int.MaxValue,
        string dr = null, string cr = null)
    {
        // ensure user is account owner
        string userId = _userManager.GetUserAsync(User).GetAwaiter().GetResult().Id;
        IQueryable<CustomerAccount> query = from c_a in _context.CustomerAccounts
                                            where c_a.CustomerId == userId
                                            select c_a;
        CustomerAccount account = query.FirstOrDefault();
        // customer has no account, report it
        if (account == null)
        {
            _logger.LogCritical($"Customer with id {userId} does not have an account");
            return NotFound();
        }
        // account id was provided in url, ensure current user owns it
        if (id != int.MinValue)
        {
            if (account.Id != id)
            {
                return Forbid();
            }
        }
        else
        {
            id = account.Id;
        }

        // normalize max and min amounts
        if (maxAmount < minAmount)
        {
            maxAmount = minAmount;
        }

        // find transaction type
        string trxType = "all";
        if (dr == "on" && cr == null)
        {
            trxType = "debit";
        }
        else if (dr == null && cr == "on")
        {
            trxType = "credit";
        }

        if (page < 1)
            page = 1;
        int pageSize = 5;

        // fetch transactions
        var trxQuery = _context
                       .Transactions
                       .Where(
                            trx => trx.AccountCreditedId == account.Id
                            || trx.AccountDebitedId == account.Id)
                       .Where(
                            trx => trx.Amount >= minAmount
                            && trx.Amount <= maxAmount);
        // if key is set, use it
        if (key >= 1)
        {
            trxQuery = trxQuery.Where(
                trx => trx.ID == key
                || trx.AccountCreditedId == key
                || trx.AccountDebitedId == key);
        }
        if (trxType == "debit")
        {
            trxQuery = trxQuery.Where(trx => trx.AccountDebitedId == account.Id);
        }
        else if (trxType == "credit")
        {
            trxQuery = trxQuery.Where(trx => trx.AccountCreditedId == account.Id);
        }

        // pagination queries
        var prevPage = trxQuery
                    .Skip((page - 2) * pageSize)
                    .Take(pageSize);
        var nextPage = trxQuery
                    .Skip((page) * pageSize)
                    .Take(pageSize);

        // paginate transactions
        List<Transaction> transactionList = trxQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .OrderByDescending(trx => trx.TimeOfTransaction)
                    .ToList();

        // add transaction type string to each of the transactions
        List<CustomerTransactionModel> toReturn = new List<CustomerTransactionModel>();
        foreach (var trx in transactionList)
        {
            string transactionType = "DR";
            switch (trx.TransactionType)
            {
                case TransactionTypes.CREDIT:
                    transactionType = "CR";
                    break;
                case TransactionTypes.WIRE_TRANSFER:
                    if (trx.AccountCreditedId == account.Id)
                        transactionType = "CR";
                    break;
            }

            toReturn.Add(new CustomerTransactionModel
            {
                Amount = trx.Amount,
                TimeString = FormatDate(trx.TimeOfTransaction),
                Id = trx.ID,
                TransactionTypeStr = transactionType,
                AccountCreditedId = trx.AccountCreditedId,
                AccountDebitedId = trx.AccountDebitedId
            });
        }


        // attempt to construct pagination links
        var prevLink = new PaginationLink
        {
            Href = "",
            IsActive = false
        };
        var nextLink = new PaginationLink
        {
            Href = "",
            IsActive = false
        };

        // 
        if (key < 1)
            key = 0;
        // previous page
        // there will be no previous page when we are on the first page
        if (page > 1)
        {
            if (prevPage.Count() != 0)
            {
                prevLink.Href = $"/api/transactions/customers/{id}" +
                            $"?page={page - 1}&key={key}" +
                            $"&minAmount={minAmount}&maxAmount={maxAmount}" +
                            $"&dr={dr}&cr={cr}";
                prevLink.IsActive = true;
            }
        }
        // next page
        if (nextPage.Count() != 0)
        {
            nextLink.Href = $"/api/transactions/customers/{id}" +
                        $"?page={page + 1}&key={key}" +
                        $"&minAmount={minAmount}&maxAmount={maxAmount}" +
                        $"&dr={dr}&cr={cr}";
            nextLink.IsActive = true;
        }

        var paginationModel = new PaginationModel
        {
            Transactions = toReturn,
            NextLink = nextLink,
            PreviousLink = prevLink
        };
        return paginationModel;
    }

    private string FormatDate(DateTime date)
    {
        return date.ToString("dd MMMM, yyyy");
    }
}

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
        public IEnumerable<TransactionViewModel> Transactions { get; set; }
        public PaginationLink PreviousLink { get; set; }
        public PaginationLink NextLink { get; set; }
    }

    public class StaffPaginationModel : PaginationModel
    {
        public new IEnumerable<StaffTransactionModel> Transactions { get; set; }
    }

    [Authorize(Roles = "admin, cashier")]
    [HttpGet("api/transactions/staff")]
    public ActionResult<StaffPaginationModel>
    StaffTransactions(
        int key = int.MinValue,
        int minAmount = int.MinValue,
        int maxAmount = int.MaxValue,
        int accountCredited = int.MinValue,
        int accountDebited = int.MinValue,
        string dr = null, string cr = null,
        string approverId = null, int page = 1,
        string over_d = null, string wire_t = null)
    {

        // normalize max and min amounts
        if (minAmount < 0)
        {
            minAmount = 0;
        }
        if (maxAmount < minAmount)
        {
            maxAmount = minAmount;
        }


        Func<Transaction, bool> matchesTransactionType = (t) =>
        {
            // there is no explicit transaction type chosen
            if (dr == null && cr == null && over_d == null && wire_t == null)
                return true;

            if (dr == "on" && t.TransactionType == TransactionTypes.DEBIT)
                return true;
            if (cr == "on" && t.TransactionType == TransactionTypes.CREDIT)
                return true;
            if (over_d == "on" && t.TransactionType == TransactionTypes.OVERDRAFT)
                return true;
            if (wire_t == "on" && t.TransactionType == TransactionTypes.WIRE_TRANSFER)
                return true;

            return false;
        };

        var query = _context.Transactions
                        .Where(matchesTransactionType)
                        .Where(
                            trx => trx.Amount >= minAmount
                            && trx.Amount <= maxAmount);
        // if key is set, use it
        if (key >= 1)
        {
            query = query.Where(
                trx => trx.ID == key
                || trx.AccountCreditedId == key
                || trx.AccountDebitedId == key);
        }
        // credit and debit accounts
        if (accountDebited > 0)
        {
            query = query.Where(trx => trx.AccountDebitedId == accountDebited);
        }
        if (accountCredited > 0)
        {
            query = query.Where(trx => trx.AccountCreditedId == accountCredited);
        }

        // approver
        if (approverId != null)
        {
            query = query.Where(trx => trx.ApproverId == approverId);
        }

        if (page < 1)
            page = 1;
        int pageSize = 5;

        var transactions = query
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .OrderByDescending(tr => tr.TimeOfTransaction)
                        .ToArray();
        var toReturn = GetStaffTransactions(transactions);

        // pagination queries
        var prevPage = query
                    .Skip((page - 2) * pageSize)
                    .Take(pageSize);
        var nextPage = query
                    .Skip((page) * pageSize)
                    .Take(pageSize);

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
                prevLink.Href = $"/api/transactions/staff" +
                            $"?page={page - 1}&key={key}" +
                            $"&minAmount={minAmount}&maxAmount={maxAmount}" +
                            $"&dr={dr}&cr={cr}&approverId={approverId}" +
                            $"&over_d={over_d}&wire_t={wire_t}" +
                            $"&accountCredited={accountCredited}" +
                            $"&accountDebited={accountDebited}";
                prevLink.IsActive = true;
            }
        }
        // next page
        if (nextPage.Count() != 0)
        {
            nextLink.Href = $"/api/transactions/staff" +
                        $"?page={page + 1}&key={key}" +
                        $"&minAmount={minAmount}&maxAmount={maxAmount}" +
                        $"&dr={dr}&cr={cr}&approverId={approverId}" +
                        $"&over_d={over_d}&wire_t={wire_t}" +
                        $"&accountCredited={accountCredited}" +
                        $"&accountDebited={accountDebited}";
            nextLink.IsActive = true;
        }

        var paginationModel = new StaffPaginationModel
        {
            Transactions = toReturn,
            NextLink = nextLink,
            PreviousLink = prevLink
        };
        return paginationModel;
    }

    private List<StaffTransactionModel> GetStaffTransactions(
        IEnumerable<Transaction> transactions)
    {
        List<StaffTransactionModel> staffTransactions = new List<StaffTransactionModel>();
        foreach (var trx in transactions)
        {
            string approver = trx.ApproverId ?? "N/A";
            staffTransactions.Add(
                new StaffTransactionModel
                {
                    Id = trx.ID,
                    Date = trx.TimeOfTransaction.ToString("dd MMMM, yyyy"),
                    Amount = trx.Amount,
                    TransactionTypeStr = GetTransactionTypeString(trx.TransactionType),
                    AccountCreditedId = trx.AccountCreditedId,
                    AccountDebitedId = trx.AccountDebitedId,
                    ApprovedBy = approver
                }
            );
        }
        return staffTransactions;
    }

    private string GetTransactionTypeString(TransactionTypes type)
    {
        string trxType = "DR";
        switch (type)
        {
            case (TransactionTypes.CREDIT):
                trxType = "CR";
                break;
            case (TransactionTypes.WIRE_TRANSFER):
                trxType = "WIRE_T";
                break;
            case (TransactionTypes.OVERDRAFT):
                trxType = "OVER_D";
                break;
        }
        return trxType;
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
            if (trx.AccountCreditedId == account.Id)
                transactionType = "CR";

            toReturn.Add(new CustomerTransactionModel
            {
                Amount = trx.Amount,
                Date = FormatDate(trx.TimeOfTransaction),
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

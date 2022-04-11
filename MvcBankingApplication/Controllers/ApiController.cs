using Microsoft.AspNetCore.Mvc;
using MvcBankingApplication.Models.ViewModels;
using MvcBankingApplication.Models.Accounts;
using MvcBankingApplication.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MvcBankingApplication.Models.Transactions;
using System.Net;

namespace MvcBankingApplication.Controllers;

[ApiController]
public class ApiController : ControllerBase
{
    private ApplicationContext _context;
    private UserManager<ApplicationUser> _userManager;

    public ApiController(ApplicationContext context, UserManager<ApplicationUser> manager)
    {
        _context = context;
        _userManager = manager;
    }

    [Authorize(Roles = "customer")]
    [HttpGet("api/transactions/customers/{id}")]
    public ActionResult<IEnumerable<CustomerTransactionModel>>
    CustomerTransactions(int id = int.MinValue, int page = 1)
    {
        if (page < 1)
            page = 1;
        int pageSize = 5;

        // ensure user is account owner
        string userId = _userManager.GetUserAsync(User).GetAwaiter().GetResult().Id;
        IQueryable<CustomerAccount> query = from c_a in _context.CustomerAccounts
                                            where c_a.Id == id
                                            select c_a;
        CustomerAccount account = query.FirstOrDefault();
        if (account == null)
        {
            return NotFound();
        }
        if (account.CustomerId != userId)
        {
            return Forbid();
        }

        // fetch transactions
        var transactions = from trx in
                            _context.Transactions
                           where trx.AccountCreditedId == account.Id ||
                           trx.AccountDebitedId == account.Id
                           select trx;
        var transactionList = transactions.Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToArray();

        // format transactions
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
                Date = FormatDate(trx.TimeOfTransaction),
                Type = transactionType
            });
        }
        return toReturn;
    }

    private string FormatDate(DateTime date)
    {
        return date.ToString("dd MMMM, yyyy");
    }
}

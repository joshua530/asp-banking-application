using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MvcBankingApplication.Models.Transactions;
using MvcBankingApplication.Models.ViewModels;
using System.Linq;
using System;
using System.Data;
// using System.Data.Objects;  
using System.Data.Entity;
using System.Linq;
// using System.Web.Mvc;

namespace MvcBankingApplication.Controllers;

[Authorize(Roles = "admin, cashier")]
public class StaffController : Controller
{
    private readonly ApplicationContext _context;

    public StaffController(ApplicationContext dbContext)
    {
        _context = dbContext;
    }

    /// <summary>
    /// enables staff to view all transactions
    /// </summary>
    public IActionResult Transactions(
            int key = int.MinValue,
            int minAmount = int.MinValue,
            int maxAmount = int.MaxValue,
            int accountCredited = int.MinValue,
            int accountDebited = int.MinValue,
            string dr = null, string cr = null,
            string approverId = null, string over_d = null,
            string wire_t = null)
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

        Func<Transaction, bool> meetsTransactionCriteria = (tr) =>
        {
            // nothing is selected, return all transaction types
            if (dr == null && cr == null && over_d == null && wire_t == null)
                return true;

            TransactionTypes trxType = tr.TransactionType;
            if (dr == "on" && trxType == TransactionTypes.DEBIT)
                return true;
            if (cr == "on" && trxType == TransactionTypes.CREDIT)
                return true;
            if (over_d == "on" && trxType == TransactionTypes.OVERDRAFT)
                return true;
            if (wire_t == "on" && trxType == TransactionTypes.WIRE_TRANSFER)
                return true;

            return false;
        };

        var query = _context.Transactions
                        .Where(meetsTransactionCriteria)
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

        int pageSize = 5;
        int page = 1;
        var transactions = query
                        .Skip((page - 1) * pageSize)
                        .OrderByDescending(tr => tr.TimeOfTransaction)
                        .Take(pageSize)
                        .ToArray();
        var toReturn = GetStaffTransactions(transactions);
        // persist input from the user to search form
        if (key == int.MinValue)
            ViewData["key"] = "";
        else
            ViewData["key"] = key;

        if (minAmount == int.MinValue)
            ViewData["minAmount"] = "";
        else
            ViewData["minAmount"] = minAmount;
        if (maxAmount == int.MaxValue)
            ViewData["maxAmount"] = "";
        else
            ViewData["maxAmount"] = maxAmount;

        if (accountCredited == int.MinValue)
            ViewData["accountCredited"] = "";
        else
            ViewData["accountCredited"] = accountCredited;
        if (accountDebited == int.MinValue)
            ViewData["accountDebited"] = "";
        else
            ViewData["accountDebited"] = accountDebited;

        // transaction types
        if (dr == "on")
            ViewData["dr"] = "checked";
        else
            ViewData["dr"] = "";
        if (cr == "on")
            ViewData["cr"] = "checked";
        else
            ViewData["cr"] = "";
        if (over_d == "on")
            ViewData["over_d"] = "checked";
        else
            ViewData["over_d"] = "";
        if (wire_t == "on")
            ViewData["wire_t"] = "checked";
        else
            ViewData["wire_t"] = "";

        // approver
        if (approverId != null)
            ViewData["approverId"] = approverId;
        else
            ViewData["approverId"] = "";
        return View(toReturn);
    }

    public List<StaffTransactionModel> GetStaffTransactions(
        IEnumerable<Transaction> transactions)
    {
        List<StaffTransactionModel> staffTransactions = new List<StaffTransactionModel>();
        foreach (var trx in transactions)
        {
            staffTransactions.Add(
                new StaffTransactionModel
                {
                    Id = trx.ID,
                    Date = trx.TimeOfTransaction.ToString("dd MMMM, yyyy"),
                    Amount = trx.Amount,
                    TransactionTypeStr = GetTransactionTypeString(trx.TransactionType),
                    AccountCreditedId = trx.AccountCreditedId,
                    AccountDebitedId = trx.AccountDebitedId,
                    ApprovedBy = trx.ApproverId
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
            case (TransactionTypes.OVERDRAFT):
                trxType = "OVER_D";
                break;
            case (TransactionTypes.WIRE_TRANSFER):
                trxType = "WIRE_T";
                break;
        }
        return trxType;
    }
}

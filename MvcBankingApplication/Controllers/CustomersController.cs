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
    public class CustomersController : Controller
    {
        private readonly ApplicationContext _context;
        UserManager<ApplicationUser> _userManager;

        public CustomersController(ApplicationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "customer")]
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
    }
}

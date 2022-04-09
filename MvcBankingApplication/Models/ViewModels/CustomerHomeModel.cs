using MvcBankingApplication.Models.Accounts;
using MvcBankingApplication.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace MvcBankingApplication.Models.ViewModels;

public class CustomerHomeModel
{
    public Customer User { get; set; }
    public CustomerAccount Account { get; set; }
    // account
    // account transactions -> separate model
    // stock quotes
    public StockModel[] Stocks { get; set; }
    public string StockMarketLink
    {
        get { return "https://www.nasdaq.com/market-activity/stocks"; }
    }
}

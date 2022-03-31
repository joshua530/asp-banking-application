using MvcBankingApplication.Services.Stocks;
using MvcBankingApplication.Utils;

namespace MvcBankingApplication.Models.StockApiModel
{

    public class StockApiModel
    {
        public StockApiService StockApi = new StockApiService();

        public StockObj[] GetStocks()
        {
            return StockApi.Stocks;
        }
    }
}

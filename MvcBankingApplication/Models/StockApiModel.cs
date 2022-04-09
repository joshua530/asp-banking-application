using MvcBankingApplication.Services.Stocks;

namespace MvcBankingApplication.Models
{

    public class StockApiModel
    {
        public StockApiService StockApi = new StockApiService();

        public StockModel[] GetStocks()
        {
            return StockApi.Stocks;
        }
    }
}

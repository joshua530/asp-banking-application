namespace MvcBankingApplication.Services.Stocks
{
    abstract public class StockNamesService
    {
        public static string[] Stocks
        {
            get
            {
                return new string[]
                {
                    "AAPL","MSFT","GOOGL","AMZN","TSLA","TSLA","FB","TSM","TSM","BABA","PFE","ABBV","ASML","LLY","KO","AVGO","DIS","NTES","COST","TM","NVO","PEP","CSCO","TMO","ORCL","VZ","CMCSA","NKE","INTC","SHEL","ABT","CRM","ADBE","MA","XOM","MCD","PYPL","IBM","UL","GS","BA","GSK","GE","SBUX","ABNB","SHOP","MUFG","UBER","F","HPQ","DELL"
                };
            }
        }
    }
}

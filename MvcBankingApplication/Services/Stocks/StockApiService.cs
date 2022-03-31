using StackExchange.Redis;
using MvcBankingApplication.Utils;
using System.Globalization;
using System.Xml.Serialization;

namespace MvcBankingApplication.Services.Stocks
{


    class StockApiService
    {
        public String[] StockNames = StockNamesService.Stocks;
        IDatabase Cache = RedisConnectorHelper.Connection.GetDatabase();

        public StockObj[] Stocks
        {
            get
            {
                RedisValue cachedStocks = Cache.StringGet("stocks");
                RedisValue expirationTime = Cache.StringGet("expiration_time");
                StockObj[] stocks;
                if (cachedStocks == RedisValue.Null || ExpirationTimePassed(expirationTime))
                {
                    stocks = ApiRequests.GetDataForStocks(StockNames);
                    // serialize the stocks
                    string serializedStocks = SerializeStocks(stocks);
                    Cache.StringSet("stocks", serializedStocks);
                    // set the timestamp at which the data will become stale
                    Cache.StringSet("expiration_time", CurrentTime());
                    return stocks;
                }
                string stockStr = Cache.StringGet("expiration_time");
                return DeserializeStocksStr(stockStr);
            }
            set { }
        }

        private string SerializeStocks(StockObj[] stocks)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(StockObj));
            using (StringWriter sw = new StringWriter())
            {
                serializer.Serialize(sw, stocks);
                return sw.ToString();
            }
        }

        private StockObj[] DeserializeStocksStr(string stocksStr)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(StockObj));
            using (StringReader sr = new StringReader(stocksStr))
            {
                return (StockObj[])serializer.Deserialize(sr);
            }
        }

        private string CurrentTime()
        {
            return DateTime.Now.ToString("H:mm:ss dd-MM-yyyy");
        }

        private bool ExpirationTimePassed(RedisValue time)
        {
            if (time == RedisValue.Null) return true;
            DateTime expirationTime = DateTime.ParseExact((string)time, "H:mm:ss dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime now = DateTime.Now;

            if (expirationTime < now)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

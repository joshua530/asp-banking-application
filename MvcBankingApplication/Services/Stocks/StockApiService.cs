using StackExchange.Redis;
using MvcBankingApplication.Models;
using MvcBankingApplication.Utils;
using System.Globalization;
using System.Xml.Serialization;

namespace MvcBankingApplication.Services.Stocks
{
    public class StockApiService
    {
        public String[] StockNames = StockNamesService.Stocks;
        IDatabase Cache = RedisConnectorHelper.Connection.GetDatabase();

        public StockModel[] Stocks
        {
            get
            {
                RedisValue cachedStocks = Cache.StringGet("stocks");
                RedisValue expirationTime = Cache.StringGet("expiration_time");
                StockModel[] stocks;
                if (cachedStocks == RedisValue.Null || ExpirationTimePassed((string)expirationTime))
                {
                    try
                    {
                        stocks = ApiRequests.GetDataForStocks(StockNames);
                        // serialize the stocks
                        string serializedStocks = SerializeStocks(stocks);
                        Cache.StringSet("stocks", serializedStocks);
                        // set the timestamp at which the data will become stale
                        Cache.StringSet("expiration_time", GenerateExpirationTime(5));
                        return stocks;
                    }
                    catch (HttpRequestException)
                    {
                        // if network is unreachable, we'll just return the cached stocks
                        // and attempt a re-fetch during the next request
                    }

                }
                string stockStr = Cache.StringGet("stocks");
                return DeserializeStocksStr(stockStr);
            }
        }

        public string SerializeStocks(StockModel[] stocks)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(StockModel[]));
            using (StringWriter sw = new StringWriter())
            {
                serializer.Serialize(sw, stocks);
                return sw.ToString();
            }
        }

        public StockModel[] DeserializeStocksStr(string stocksStr)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(StockModel[]));
            using (StringReader sr = new StringReader(stocksStr))
            {
                return (StockModel[])serializer.Deserialize(sr);
            }
        }

        private string GenerateExpirationTime(int numMinutes)
        {
            return DateTime.Now.AddMinutes(numMinutes).ToString("H:mm:ss dd-MM-yyyy");
        }

        public bool ExpirationTimePassed(string time)
        {
            if (time == RedisValue.Null) return true;
            DateTime expirationTime = DateTime.ParseExact(time, "H:mm:ss dd-MM-yyyy", CultureInfo.InvariantCulture);
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

using Newtonsoft.Json.Linq;

namespace MvcBankingApplication.Utils
{

    class ApiRequests
    {
        public static HttpClient client = new HttpClient();
        public static async Task<StockObj> GetStockData(string symbol)
        {
            String uri = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey=JZNAORST2Q2AQGQ9";


            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri)
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                String result = await response.Content.ReadAsStringAsync();
                JObject jsonObj = JObject.Parse(result);
                StockObj stock = new StockObj();
                if (jsonObj.ContainsKey("Global Quote"))
                {
                    stock.Symbol = jsonObj["Global Quote"]["01. symbol"].ToString();
                    stock.Price = jsonObj["Global Quote"]["05. price"].ToString();
                    stock.PercentChange = jsonObj["Global Quote"]["10. change percent"].ToString();
                }
                return stock;
            }
        }

        public static StockObj[] GetDataForStocks(string[] stockSymbols)
        {
            List<StockObj> stocks = new List<StockObj>();
            int[] randIndices = RandomNums(0, stockSymbols.Length - 1, 5);

            foreach (int s in randIndices)
            {
                stocks.Add(
                  GetStockData(
                    stockSymbols[s]
                  ).GetAwaiter().GetResult());
            }

            return stocks.ToArray();
        }

        public static int[] RandomNums(int start, int end, int count)
        {
            int[] nums = new int[count];
            HashSet<int> tmp = new HashSet<int>();
            Random r = new Random();
            while (tmp.Count < count)
            {
                tmp.Add(r.Next(start, end));
            }
            tmp.CopyTo(nums);
            return nums;
        }
    }
}

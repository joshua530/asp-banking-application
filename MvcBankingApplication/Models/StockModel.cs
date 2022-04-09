namespace MvcBankingApplication.Models
{
#pragma warning disable CS8632
    public class StockModel
    {
        private string _price = null;

        public string Symbol { get; set; }
        public string Price
        {
            get
            {
                string dollars, cents;
                if (_price != null)
                {
                    // separate dollars and cents
                    string[] segs = _price.Split(".");
                    dollars = segs[0];
                    cents = segs[1];
                    // keep track of zeros so that we can trim the excess
                    // ones from the end of the cents
                    int numZerosAtEnd = 0;
                    // skip if decimal section comprises of only two digits
                    // since our end goal is to ensure the cents section
                    // comprises of only two digits
                    if (cents.Length > 2)
                    {
                        for (int i = cents.Length - 1; i >= 0; --i)
                        {
                            if (cents[i] == '0')
                                ++numZerosAtEnd;
                        }
                    }
                    // trim zeros from the end
                    cents = cents.Substring(0, cents.Length - numZerosAtEnd);
                    // if the cents segment was composed of only zeros, give it a
                    // default value to ensure it is not empty
                    if (cents.Length == 0)
                        cents = "00";
                    // combine the dollars and cents
                    return $"{dollars}.{cents}";
                }
                return _price;
            }
            set { _price = value; }
        }
        public string PercentChange { get; set; }

        public string PriceChange
        {
            get
            {
                if (PercentChange != null && PercentChange != String.Empty)
                {
                    // percent format = <sign><value>%
                    // sign is only included for negative changes
                    string percent = PercentChange.Split("%")[0];
                    double percentDouble = Convert.ToDouble(percent);

                    if (percentDouble > 0)
                        return "up";
                    else if (percentDouble < 0)
                        return "down";
                }
                return "same";
            }
        }

        public override string ToString()
        {
            return $"{Symbol} | {Price} | {PercentChange}";
        }

        public override int GetHashCode()
        {
            return Symbol.GetHashCode();
        }

        public override bool Equals(Object? compare)
        {
            if (compare == null || !this.GetType().Equals(compare.GetType()))
                return false;
            StockModel tmp = (StockModel)compare;
            if (tmp.Symbol != Symbol)
                return false;
            if (tmp.Price != Price)
                return false;
            if (tmp.PercentChange != PercentChange)
                return false;
            return true;
        }
    }
#pragma warning restore CS8632
}

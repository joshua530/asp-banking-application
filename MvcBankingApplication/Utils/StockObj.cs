namespace MvcBankingApplication.Utils
{
#pragma warning disable CS8632
    public class StockObj
    {
        public string Symbol { get; set; } = String.Empty;
        public string Price { get; set; } = String.Empty;
        public string PercentChange { get; set; } = String.Empty;

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
            StockObj tmp = (StockObj)compare;
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

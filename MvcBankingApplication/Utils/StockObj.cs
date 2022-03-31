namespace MvcBankingApplication.Utils
{
    class StockObj
    {
        public string Symbol { get; set; } = String.Empty;
        public string Price { get; set; } = String.Empty;
        public string PercentChange { get; set; } = String.Empty;

        public override string ToString()
        {
            return $"{Symbol} | {Price} | {PercentChange}";
        }
    }
}

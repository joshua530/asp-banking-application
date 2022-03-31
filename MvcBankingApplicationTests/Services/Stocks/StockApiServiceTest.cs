using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcBankingApplication.Services.Stocks;
using MvcBankingApplication.Utils;
using System;

namespace MvcBankingApplicationTests;

[TestClass]
public class StockApiServiceTest
{
    public StockApiService StockService = new StockApiService();
    [TestMethod]
    public void TestStockSerialization()
    {
        StockObj[] stocks = GenerateStocks();
        string expected = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfStockObj xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><StockObj><Symbol>ABC</Symbol><Price>1.22</Price><PercentChange>+3%</PercentChange></StockObj><StockObj><Symbol>DEF</Symbol><Price>2.34</Price><PercentChange>+4%</PercentChange></StockObj></ArrayOfStockObj>";
        string actual = StockService.SerializeStocks(stocks);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestStockDeserialization()
    {
        string stockString = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfStockObj xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><StockObj><Symbol>ABC</Symbol><Price>1.22</Price><PercentChange>+3%</PercentChange></StockObj><StockObj><Symbol>DEF</Symbol><Price>2.34</Price><PercentChange>+4%</PercentChange></StockObj></ArrayOfStockObj>";
        StockObj[] expected = GenerateStocks();
        StockObj[] actual = StockService.DeserializeStocksStr(stockString);
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestEmptyStockListSerialization()
    {
        string expected = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfStockObj xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" />";
        StockObj[] emptyStocks = { };
        string actual = StockService.SerializeStocks(emptyStocks);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestEmptyStockListDeserialization()
    {
        string emptyStocksXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfStockObj xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" />";
        StockObj[] expected = { };
        StockObj[] actual = StockService.DeserializeStocksStr(emptyStocksXml);
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestNullStockListSerialization()
    {
        string expected = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfStockObj xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xsi:nil=\"true\" />";
        StockObj?[] nullStocks = null;
        string actual = StockService.SerializeStocks(nullStocks);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestNullStockListDeserialization()
    {
        string nullStocksXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfStockObj xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xsi:nil=\"true\" />";
        StockObj?[] actual = StockService.DeserializeStocksStr(nullStocksXml);
        Assert.IsNull(actual);
    }

    public StockObj[] GenerateStocks()
    {
        return new StockObj[]
        {
            new StockObj
            {
                Symbol = "ABC",
                Price = "1.22",
                PercentChange = "+3%"
            },
            new StockObj
            {
                Symbol = "DEF",
                Price = "2.34",
                PercentChange = "+4%"
            }
        };
    }
}

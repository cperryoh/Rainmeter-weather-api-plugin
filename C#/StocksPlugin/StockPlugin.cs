using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using HtmlAgilityPack;
using Rainmeter;

// Overview: This is a blank canvas on which to build your plugin.

// Note: GetString, ExecuteBang and an unnamed function for use as a section variable
// have been commented out. If you need GetString, ExecuteBang, and/or section variables 
// and you have read what they are used for from the SDK docs, uncomment the function(s)
// and/or add a function name to use for the section variable function(s). 
// Otherwise leave them commented out (or get rid of them)!

namespace PluginEmpty
{
    public class Stock
    {
        public String ticker;
        public double price;
        public double change;
        public Stock(String ticker)
        {
            this.ticker = ticker;
        }
    }
    public class Measure
    {
        static public implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
        }

        public String exchange;
        public API api;
        public String data;
        public Dictionary<String,Stock> stocks;
        public String type;
        public IntPtr buffer = IntPtr.Zero;
    }
    public class Plugin
    {

        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
            Rainmeter.API api = (Rainmeter.API)rm;
            Measure measure = (Measure)data;
            measure.api = api;
            measure.stocks = new Dictionary<string, Stock>();
            measure.stocks.Add(api.ReadString("ticker1", "AMD").ToUpper(), new Stock(api.ReadString("ticker1", "AMD").ToUpper()));
            measure.stocks.Add(api.ReadString("ticker2", "MSFT").ToUpper(), new Stock(api.ReadString("ticker2", "MSFT").ToUpper()));
            measure.stocks.Add(api.ReadString("ticker3", "MSFT").ToUpper(), new Stock(api.ReadString("ticker3", "GME").ToUpper()));
            measure.type = api.ReadString("type", "price").ToLower();
        }
        public static Stock updatePrice(Stock stock) 
        {
            HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            String ticker = "AMD";
            HtmlAgilityPack.HtmlDocument doc = web.Load("https:" + $"//www.marketwatch.com/investing/stock/{ticker}");
            HtmlNode node = doc.DocumentNode;
            HtmlNode pricenode = node.SelectSingleNode("//*[@id='maincontent']/div[2]/div[3]/div/div[2]/h2/bg-quote");
            double price = Double.Parse(pricenode.InnerText);
            String change = node.SelectSingleNode("//*[@id='maincontent']/div[2]/div[3]/div/div[2]/bg-quote/span[2]/bg-quote").InnerText;
            change = change.Replace("(", "").Replace(")", "").Replace("+", "").Replace("%", "");
            stock.change = Double.Parse(change);
            stock.price = price;
            return stock;
        }
        [DllExport]
        public static void Finalize(IntPtr data)
        {
            Measure measure = (Measure)data;
            if (measure.buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(measure.buffer);
            }
            GCHandle.FromIntPtr(data).Free();
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = (Measure)data;
            Rainmeter.API api = (Rainmeter.API)rm;
            measure.stocks.Add(api.ReadString("ticker1", "AMD").ToUpper(), new Stock(api.ReadString("ticker1", "AMD").ToUpper()));
            measure.stocks.Add(api.ReadString("ticker2", "MSFT").ToUpper(), new Stock(api.ReadString("ticker2", "MSFT").ToUpper()));
            measure.stocks.Add(api.ReadString("ticker3", "MSFT").ToUpper(), new Stock(api.ReadString("ticker3", "GME").ToUpper()));
            measure.type = api.ReadString("type", "price").ToLower();
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)data;
            try
            {
                foreach (String key in measure.stocks.Keys)
                {
                    Stock s = measure.stocks[key];
                    measure.stocks[key] = updatePrice(s);
                }
            }catch(Exception e)
            {
                measure.api.Log(API.LogType.Error, e.Message);
            }
            return -1;
        }
        [DllExport]
        public static IntPtr getValue(IntPtr data, int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2)] string[] argv)
        {
            Measure measure = (Measure)data;
            double dataOut = -1.0;
            Stock s = measure.stocks[argv[0].ToUpper()];
            if (argv[1].Equals("price"))
            {
                return Marshal.StringToHGlobalUni(s.price.ToString());
            }else if (argv[1].ToLower().Equals("percenthange"))
            {
                return Marshal.StringToHGlobalUni(s.change.ToString());
            }
            return Marshal.StringToHGlobalUni("no value");
        }

    }
}

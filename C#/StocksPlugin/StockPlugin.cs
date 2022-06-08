using System;
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

    public class Measure
    {
        static public implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
        }

        public double percentChange;
        public double price;
        public String exchange;
        public String data;
        public API api;
        public String type;
        public String ticker;
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
            measure.ticker = api.ReadString("ticker", "AMD").ToUpper();
            measure.type = api.ReadString("type", "price").ToLower();
            measure.api = api;
        }
        public static Measure updatePrice(Measure measure)
        {
            try
            {
                HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = web.Load("https" + $"://www.marketwatch.com/investing/stock/{measure.ticker}");
                HtmlNode node = doc.DocumentNode;
                node = node.SelectSingleNode("//div[@class='intraday__data']");
                String price = node.SelectSingleNode("//h2[@class='intraday__price ']").InnerText;
                String change = node.SelectSingleNode("//span[@class='change--percent--q']").InnerText;
                double priceNum = Double.Parse(price.Replace("\n", "").Replace("\r", "").Replace(" ", "").Replace("$", ""));
                double changeNum = Double.Parse(change.Replace("%", ""));
                measure.price = priceNum;
                measure.percentChange = changeNum;

            }catch(Exception e)
            {
                measure.api.Log(API.LogType.Error, e.Message);
            }
            return measure;
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
            measure.ticker = api.ReadString("ticker", "AMD").ToUpper();
            measure.exchange = api.ReadString("exchange", "NASDAQ").ToUpper();
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)data;
            measure = updatePrice(measure);
            if (measure.type.Equals("price"))
            {
                return measure.price;
            }
            else if (measure.type.Equals("percentchange"))
            {
                return measure.percentChange;
            }
            return -1.0;
        }
        [DllExport]
        public static IntPtr getValue(IntPtr data, int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)] string[] argv)
        {
            Measure measure = (Measure)data;
            double dataOut = -1.0;
            if (argv[0].Equals("price"))
            {
                dataOut = measure.price;
            }
            else if (argv[0].ToLower().Equals("percentchange"))
            {
                dataOut = measure.percentChange;
            }
            return Marshal.StringToHGlobalUni(dataOut.ToString());
        }

    }
}

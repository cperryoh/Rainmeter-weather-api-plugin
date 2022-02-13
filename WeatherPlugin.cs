using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using Rainmeter;

// Overview: This is a blank canvas on which to build your plugin.

// Note: GetString, ExecuteBang and an unnamed function for use as a section variable
// have been commented out. If you need GetString, ExecuteBang, and/or section variables 
// and you have read what they are used for from the SDK docs, uncomment the function(s)
// and/or add a function name to use for the section variable function(s). 
// Otherwise leave them commented out (or get rid of them)!

namespace PluginEmpty
{
    public class Main
    {
        public double temp { get; set; }
        public double temp_max { get; set; }
        public double temp_min { get; set; }
        public double humidity { get; set; }
    }
    public class Weather
    {
        public String main { get; set; }
        public String description { get; set; }
        public String icon { get; set; }
    }
    public class MainJson
    {
        public Weather[] weather { get; set; }
        public Main main { get; set; }
    }
    class Measure
    {
        static public implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
        }

        public IntPtr buffer = IntPtr.Zero;
        public MainJson mainJson;
        public String apiKey;
        public double lon;
        public double lat;
        public String type;
        public String units;
        public String data;
    }
    public class Plugin
    {
        
        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
            Rainmeter.API api = (Rainmeter.API)rm;
            Measure measure = (Measure)data;

            measure.apiKey = api.ReadString("key", "");
            measure.lon = api.ReadDouble("longitude", 0.0);
            measure.lat = api.ReadDouble("latitude", 0.0);
            measure.type = api.ReadString("type", "");
            measure.units = api.ReadString("units", "");
            try
            {
                String json = new WebClient().DownloadString("https://" + $"api.openweathermap.org/data/2.5/weather?lat={measure.lat}&lon={measure.lon}&appid={measure.apiKey}");
                MainJson weather = (new JavaScriptSerializer()).Deserialize<MainJson>(json);
                measure.mainJson = weather;
                if (measure.type.Equals("temp") || measure.type.Equals(""))
                {
                    measure.data = convert(weather.main.temp, measure.units).ToString();
                }
                if (measure.type.Equals("temp_min"))
                {
                    measure.data = convert(weather.main.temp_min, measure.units).ToString();
                }
                if (measure.type.Equals("temp_max"))
                {
                    measure.data = convert(weather.main.temp_max, measure.units).ToString();
                }
                if (measure.type.Equals("humidity"))
                {
                    measure.data = weather.main.humidity.ToString();
                }
                if (measure.type.Equals("condition"))
                {
                    measure.data = weather.weather[0].main;
                }
                if (measure.type.Equals("description"))
                {
                    measure.data = weather.weather[0].description;
                }
            }
            catch (Exception e)
            {
            }
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
            measure.apiKey = api.ReadString("key", "");
            measure.lon = api.ReadDouble("longitude", 0.0);
            measure.lat = Double.Parse(api.ReadString("latitude", "0"));
            measure.type = api.ReadString("type", "");
            measure.units = api.ReadString("units", "f");
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)data;

            try
            {
                String json = new WebClient().DownloadString("https://" + $"api.openweathermap.org/data/2.5/weather?lat={measure.lat}&lon={measure.lon}&appid={measure.apiKey}");
                MainJson weather = (new JavaScriptSerializer()).Deserialize<MainJson>(json);
                measure.mainJson = weather;
                if (measure.type.Equals("temp") || measure.type.Equals(""))
                {
                    measure.data = convert(weather.main.temp, measure.units).ToString();
                    return convert(weather.main.temp, measure.units);
                }
                if (measure.type.Equals("temp_min"))
                {
                    measure.data = convert(weather.main.temp_min, measure.units).ToString();
                    return convert(weather.main.temp_min, measure.units);
                }
                if (measure.type.Equals("temp_max"))
                {
                    measure.data = convert(weather.main.temp_max, measure.units).ToString();
                    return convert(weather.main.temp_max, measure.units);
                }
                if (measure.type.Equals("humidity"))
                {
                    measure.data = weather.main.humidity.ToString();
                    return weather.main.humidity;
                }
                if (measure.type.Equals("condition"))
                {
                    measure.data = weather.weather[0].main;
                }
                if (measure.type.Equals("description"))
                {
                    measure.data = weather.weather[0].description;
                }
                return convert(weather.main.temp, measure.units);
            }
            catch (Exception e)
            {
                return -1;
            }
        }
        public static double convert(double val, String units)
        {
            if (units.ToLower().Equals("c"))
            {
                return Math.Round(toC(val));
            }
            else
            {
                return Math.Round(toF(val));
            }
        }
        public static double toF(double val)
        {
            return (toC(val) * (9.0 / 5.0)) + 32.0;
        }
        public static double toC(double val)
        {
            return val - 273.15;
        }
        [DllExport]
        public static IntPtr GetString(IntPtr data)
        {
            Measure measure = (Measure)data;
            if (measure.buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(measure.buffer);
                measure.buffer = IntPtr.Zero;
            }

            return Marshal.StringToHGlobalUni(measure.data);
        }

        //[DllExport]
        //public static void ExecuteBang(IntPtr data, [MarshalAs(UnmanagedType.LPWStr)]String args)
        //{
        //    Measure measure = (Measure)data;
        //}

        [DllExport]
        public static IntPtr func(IntPtr data, int argc,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)] string[] argv)
        {
            Measure measure = (Measure)data;
            string outVal = "";
            if (argv[0].Equals("temp") || argv[0].Equals(""))
            {
                outVal = convert(measure.mainJson.main.temp, measure.units).ToString();
            }
            if (argv[0].Equals("temp_min"))
            {
                outVal = convert(measure.mainJson.main.temp_min, measure.units).ToString();
            }
            if (argv[0].Equals("temp_max"))
            {
                outVal = convert(measure.mainJson.main.temp_max, measure.units).ToString();
            }
            if (argv[0].Equals("humidity"))
            {
                outVal = measure.mainJson.main.humidity.ToString();
            }
            if (argv[0].Equals("condition"))
            {
                outVal = measure.mainJson.weather[0].main;
            }
            if (argv[0].Equals("description"))
            {
                outVal = measure.mainJson.weather[0].description;
            }
            if (argv[0].Equals("iconUrl"))
            {
                outVal = "http://" + $"openweathermap.org/img/wn/{measure.mainJson.weather[0].icon}@2x.png";
            }
            if (argv[0].Equals("icon"))
            {
                outVal = measure.mainJson.weather[0].icon;
            }
            return Marshal.StringToHGlobalUni(outVal);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using OpenHardwareMonitor.Hardware;
using Rainmeter;

// Overview: This is a blank canvas on which to build your plugin.

// Note: GetString, ExecuteBang and an unnamed function for use as a section variable
// have been commented out. If you need GetString, ExecuteBang, and/or section variables 
// and you have read what they are used for from the SDK docs, uncomment the function(s)
// and/or add a function name to use for the section variable function(s). 
// Otherwise leave them commented out (or get rid of them)!

namespace HomeAssitantIntegration
{
    class Measure
    {
        static public implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
        }
        public IHardware gpu;
        public ISensor gpuSens;
        public IntPtr buffer;
        public Computer computer;
    }
    public class Plugin
    {
        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
            Rainmeter.API api = (Rainmeter.API)rm;
            Measure measure = (Measure)data;
            measure.computer = new Computer();
            measure.computer.CPUEnabled = true;
            measure.computer.GPUEnabled = true;
            measure.computer.Open();
            foreach (IHardware hardware in measure.computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.GpuNvidia)
                {
                    measure.gpu = hardware;
                    api.Log(API.LogType.Debug, "Loaded gpu hardware");
                    foreach (ISensor s in hardware.Sensors)
                    {
                        if (s.SensorType == SensorType.Temperature)
                        {
                            api.Log(API.LogType.Debug, "Loaded gpu sens");
                            measure.gpuSens = s;
                        }
                    }
                }
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
            measure.computer.Close();
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = (Measure)data;
            Rainmeter.API api = (Rainmeter.API)rm;
            measure.computer = new Computer();
            measure.computer.CPUEnabled = true;
            measure.computer.GPUEnabled = true;
            measure.computer.Open();
            foreach (IHardware hardware in measure.computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.GpuNvidia)
                {
                    measure.gpu = hardware;
                    api.Log(API.LogType.Debug, "Loaded gpu hardware");
                    foreach (ISensor s in hardware.Sensors)
                    {
                        if (s.SensorType == SensorType.Temperature)
                        {
                            api.Log(API.LogType.Debug, "Loaded gpu sens");
                            measure.gpuSens = s;
                        }
                    }
                }
            }
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)data;
            measure.gpu.Update();
            return (double)measure.gpuSens.Value;

        }
        /*[DllExport]
        public static IntPtr GetString(IntPtr data)
        {
            Measure measure = (Measure)data;
            if (measure.buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(measure.buffer);
                measure.buffer = IntPtr.Zero;
            }

            return Marshal.StringToHGlobalUni(measure.data);
        }*/

        //[DllExport]
        //public static void ExecuteBang(IntPtr data, [MarshalAs(UnmanagedType.LPWStr)]String args)
        //{
        //    Measure measure = (Measure)data;
        //}

        
    }
}

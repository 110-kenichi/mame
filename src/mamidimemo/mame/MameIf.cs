// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Mame
{
    public static class MameIF
    {
        /// <summary>
        /// 
        /// </summary>
        public static IntPtr ParentModule
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hModule"></param>
        /// <param name="procName"></param>
        /// <returns></returns>
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiName"></param>
        /// <returns></returns>
        public static IntPtr GetProcAddress(string apiName)
        {
            return GetProcAddress(ParentModule, apiName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentModule"></param>
        internal static void Initialize(IntPtr parentModule)
        {
            ParentModule = parentModule;
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int delg_sample_rate();

        private static delg_sample_rate sample_rate;

        /// <summary>
        /// 
        /// </summary>
        public static delg_sample_rate SampleRate
        {
            get
            {
                if (sample_rate == null)
                {
                    IntPtr funcPtr = MameIF.GetProcAddress("sample_rate");
                    if (funcPtr != IntPtr.Zero)
                        sample_rate = (delg_sample_rate)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delg_sample_rate));
                }
                return sample_rate;
            }
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void delg_start_recording_to(string wavfile);

        private static delg_start_recording_to start_recording_to;

        /// <summary>
        /// 
        /// </summary>
        public static delg_start_recording_to StartRecordingTo
        {
            get
            {
                if (start_recording_to == null)
                {
                    IntPtr funcPtr = MameIF.GetProcAddress("start_recording_to");
                    if (funcPtr != IntPtr.Zero)
                        start_recording_to = (delg_start_recording_to)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delg_start_recording_to));
                }
                return start_recording_to;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void delg_stop_recording();

        private static delg_stop_recording stop_recording;

        /// <summary>
        /// 
        /// </summary>
        public static delg_stop_recording StopRecording
        {
            get
            {
                if (stop_recording == null)
                {
                    IntPtr funcPtr = MameIF.GetProcAddress("stop_recording");
                    if (funcPtr != IntPtr.Zero)
                        stop_recording = (delg_stop_recording)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delg_stop_recording));
                }
                return stop_recording;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void delg_parameter_automated();

        private static delg_parameter_automated parameter_automated;

        /// <summary>
        /// 
        /// </summary>
        public static delg_parameter_automated ParameterAutomated
        {
            get
            {
                if (parameter_automated == null)
                {
                    IntPtr funcPtr = MameIF.GetProcAddress("parameter_automated");
                    if (funcPtr != IntPtr.Zero)
                        parameter_automated = (delg_parameter_automated)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delg_parameter_automated));
                }
                return parameter_automated;
            }
        }

    }
}

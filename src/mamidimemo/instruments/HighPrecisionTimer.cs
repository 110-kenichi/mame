using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments
{
    public static class HighPrecisionTimer
    {

        private const int WAIT_TIMEOUT = 120 * 1000;

        /// <summary>
        /// Periodic Action Timer Interval[ms]
        /// </summary>
        public const uint TIMER_BASIC_INTERVAL = 5;

        /// <summary>
        /// Periodic Action Timer Hz
        /// </summary>
        public const double TIMER_BASIC_HZ = 1000d / TIMER_BASIC_INTERVAL;

        private static MultiMediaTimerComponent multiMediaTimerComponent;

        private static Dictionary<Func<object, double>, object> fixedTimerSounds = new Dictionary<Func<object, double>, object>();

        static HighPrecisionTimer()
        {
            Program.ShuttingDown += Program_ShuttingDown;

            multiMediaTimerComponent = new MultiMediaTimerComponent();
            multiMediaTimerComponent.Interval = TIMER_BASIC_INTERVAL;
            multiMediaTimerComponent.Resolution = 1;
            multiMediaTimerComponent.OnTimer += MultiMediaTimerComponent_OnTimer;
            multiMediaTimerComponent.Enabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="periodMs"></param>
        /// <param name="state"></param>    
        public static void SetPeriodicCallbackAsReader(Func<object, double> action, double periodMs, object state)
        {
            long lpSystemTimeAsFileTime;
            periodMs = action(state);
            GetSystemTimeAsFileTime(out lpSystemTimeAsFileTime);
            double nextTime = lpSystemTimeAsFileTime;
            Thread th = new Thread((object obj) =>
            //Action<object> ta = (obj) =>
            {
                using (SafeWaitHandle handle = CreateWaitableTimer(IntPtr.Zero, false, null))
                {
                    periodMs *= 1000 * 10;
                    while (true)
                    {
                        nextTime += periodMs;
                        long dueTime = (long)Math.Round(nextTime);
                        SetWaitableTimer(handle, ref dueTime, 0, IntPtr.Zero, IntPtr.Zero, false);
                        WaitForSingleObject(handle, WAIT_TIMEOUT);
                        try
                        {
                            InstrumentManager.ExclusiveLockObject.EnterReadLock();
                            periodMs = action(obj);
                        }
                        finally
                        {
                            InstrumentManager.ExclusiveLockObject.ExitReadLock();
                        }
                        if (periodMs < 0 || shutDown)
                            break;
                        periodMs *= 1000 * 10;
                        // Next time is past time?
                        GetSystemTimeAsFileTime(out lpSystemTimeAsFileTime);
                        if (lpSystemTimeAsFileTime > nextTime + periodMs)
                            nextTime = lpSystemTimeAsFileTime;  // adjust to current time
                    }
                }
            })
            //Task.Factory.StartNew(ta, state);
            { Priority = ThreadPriority.AboveNormal };
            th.Start(state);
        }

        private static bool shutDown;

        private static void Program_ShuttingDown(object sender, EventArgs e)
        {
            shutDown = true;
            multiMediaTimerComponent?.Dispose();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        public static void SetFixedPeriodicCallbackAsReader(Func<object, double> action, object state)
        {
            action(state);
            lock (fixedTimerSounds)
                fixedTimerSounds.Add(action, state);
        }

        /// <s
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        private static void MultiMediaTimerComponent_OnTimer(object sender)
        {
            List<KeyValuePair<Func<object, double>, object>> list = null;
            lock (fixedTimerSounds)
                list = fixedTimerSounds.ToList();
            Parallel.ForEach(list, (snd) =>
            {
                double ret = -1;
                try
                {
                    InstrumentManager.ExclusiveLockObject.EnterReadLock();

                    ret = snd.Key(snd.Value);
                }
                finally
                {
                    InstrumentManager.ExclusiveLockObject.ExitReadLock();
                }
                if (ret >= 0)
                    return;
                lock (fixedTimerSounds)
                    fixedTimerSounds.Remove(snd.Key);
            });
        }


        [DllImport("kernel32.dll")]
        public static extern SafeWaitHandle CreateWaitableTimer(IntPtr lpTimerAttributes, bool bManualReset, string lpTimerName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWaitableTimer(SafeWaitHandle hTimer,
            [In] ref long pDueTime, int lPeriod,
            IntPtr pfnCompletionRoutine, IntPtr lpArgToCompletionRoutine, bool fResume);

        [DllImport("kernel32.dll")]
        internal static extern uint WaitForSingleObject(SafeWaitHandle hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll")]
        public static extern void GetSystemTimeAsFileTime(out long lpSystemTimeAsFileTime);

    }
}

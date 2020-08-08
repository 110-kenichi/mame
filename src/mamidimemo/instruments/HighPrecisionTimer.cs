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
using zanac.MAmidiMEmo.Gui;

namespace zanac.MAmidiMEmo.Instruments
{
    public static class HighPrecisionTimer
    {

        private const int WAIT_TIMEOUT = 120 * 1000;

        /// <summary>
        /// Periodic Action Timer Interval[ms]
        /// </summary>
        public const uint TIMER_BASIC_INTERVAL = 1;

        /// <summary>
        /// Periodic Action Timer Hz
        /// </summary>
        public const double TIMER_BASIC_HZ = 1000d / TIMER_BASIC_INTERVAL;

        private static List<PeriodicAction> periodicTimerSounds = new List<PeriodicAction>();

        static HighPrecisionTimer()
        {
            Program.ShuttingDown += Program_ShuttingDown;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="periodMs"></param>
        /// <param name="state"></param>    
        public static void SetPeriodicCallback(Func<object, double> action, double periodMs, object state)
        {
            action(state);
            lock (periodicTimerSounds)
                periodicTimerSounds.Add(new PeriodicAction(action, periodMs, state));
        }

        private static bool shutDown;

        private static void Program_ShuttingDown(object sender, EventArgs e)
        {
            shutDown = true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        public static void SetFixedPeriodicCallback(Func<object, double> action, object state)
        {
            action(state);
            lock (periodicTimerSounds)
                periodicTimerSounds.Add(new PeriodicAction(action, TIMER_BASIC_INTERVAL, state));
        }

        /// <summary>
        /// 
        /// </summary>
        public static void SoundTimerCallback()
        {
            if (shutDown)
                return;

            List<PeriodicAction> plist = null;
            lock (periodicTimerSounds)
                plist = new List<PeriodicAction>(periodicTimerSounds);
            foreach (var snd in plist)
            {
                snd.CurrentPeriodMs -= TIMER_BASIC_INTERVAL;
                if (snd.CurrentPeriodMs > 0)
                    continue;

                double ret = -1;
                try
                {
                    InstrumentManager.ExclusiveLockObject.EnterWriteLock();
                    //process action
                    ret = snd.Action(snd.State);
                }
                finally
                {
                    InstrumentManager.ExclusiveLockObject.ExitWriteLock();
                }
                if (ret < 0)
                {
                    //end action
                    lock (periodicTimerSounds)
                        periodicTimerSounds.Remove(snd);
                }
                else
                {
                    //continue action
                    snd.CurrentPeriodMs = ret;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class PeriodicAction
        {
            public Func<object, double> Action
            {
                get;
                private set;
            }

            public double CurrentPeriodMs
            {
                get;
                set;
            }

            public object State
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="periodMs"></param>
            /// <param name="state"></param>
            public PeriodicAction(Func<object, double> action, double periodMs, object state)
            {
                Action = action;
                CurrentPeriodMs = periodMs;
                State = state;
            }
        }


    }
}

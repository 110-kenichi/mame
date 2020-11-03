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
using zanac.MAmidiMEmo.Midi;

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 高精度タイマー
    /// </summary>
    public static class HighPrecisionTimer
    {

        /// <summary>
        /// Periodic Action Timer Interval[1 ms]
        /// </summary>
        public const uint TIMER_BASIC_1MS_COUNT = 100;

        /// <summary>
        /// Periodic Action Timer Hz()
        /// </summary>
        public const double TIMER_BASIC_1KHZ = 1000d;

        private static List<PeriodicAction> periodicTimerSounds = new List<PeriodicAction>();

        static HighPrecisionTimer()
        {
            Program.ShuttingDown += Program_ShuttingDown;
        }

        private static bool shutDown;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Program_ShuttingDown(object sender, EventArgs e)
        {
            shutDown = true;
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="periodMs"></param>
        /// <param name="state"></param>    
        public static void SetPeriodicCallback(Func<object, double> action, double periodMs, object state, bool skipFirstAction)
        {
            if (!skipFirstAction)
                action(state);
            lock (periodicTimerSounds)
                periodicTimerSounds.Add(new PeriodicAction(action, periodMs, state));
        }

        /// <summary>
        /// MAMEから呼ばれる
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
                snd.CurrentPeriodMs -= (double)1 / (double)TIMER_BASIC_1MS_COUNT;
                if (snd.CurrentPeriodMs > 0)
                    continue;

                double ret = -1;
                try
                {
                    //InstrumentManager.ExclusiveLockObject.EnterWriteLock();
                    //process action
                    lock (MidiManager.SoundExclusiveLockObject)
                        ret = snd.Action(snd.State);
                }
                finally
                {
                    //InstrumentManager.ExclusiveLockObject.ExitWriteLock();
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

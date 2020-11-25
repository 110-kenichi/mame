// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.ComponentModel
{
    /// <summary>
    /// 
    /// </summary>
    [Obsolete]
    public class MultiMediaTimerComponent : Component
    {
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern UInt32 timeSetEvent(UInt32 msDelay, UInt32 msResolution,
          TimerEventHandler handler, UIntPtr userCtx, UInt32 eventType);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern UInt32 timeKillEvent(UInt32 timerEventId);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern UInt32 timeBeginPeriod(UInt32 uMilliseconds);
        //public static extern uint timeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint timeEndPeriod(UInt32 uMilliseconds);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern UInt32 timeGetDevCaps(ref TimeCaps timeCaps, UInt32 sizeTimeCaps);

        [StructLayout(LayoutKind.Sequential)]
        private struct TimeCaps
        {
            public UInt32 wPeriodMin;
            public UInt32 wPeriodMax;
        };

        private const int TIMERR_NOERROR = 0;

        private delegate void TimerEventHandler(UInt32 id, UInt32 msg, UIntPtr userCtx, UIntPtr rsv1, UIntPtr rsv2);

        private UInt32 f_TimerID;

        private TimerEventHandler TimerHandler;

        private bool f_Enabled;

        /// <summary>
        /// 
        /// </summary>
        public bool Enabled
        {
            get
            {
                return f_Enabled;
            }
            set
            {
                //値が異なる場合のみ設定
                if (f_Enabled != value)
                {
                    f_Enabled = value;
                    updateTimeEvent();
                }
            }
        }

        private uint f_Interval = 1000;

        /// <summary>
        /// Gets or sets the time between timer events in milliseconds.
        /// </summary>
        public uint Interval
        {
            get
            {
                return f_Interval;
            }
            set
            {
                if (f_Resolution > value)
                    f_Resolution = value;

                //値が異なる場合のみ設定
                if (f_Interval != value)
                {
                    f_Interval = value;
                    updateTimeEvent();
                }
            }
        }


        private uint f_Resolution = 1000;

        /// <summary>
        ///  Gets or sets the timer resolution in milliseconds.
        /// </summary>
        public uint Resolution
        {
            get
            {
                return f_Resolution;
            }
            set
            {
                TimeCaps timeCaps = new TimeCaps();
                UInt32 DevCaps = timeGetDevCaps(ref timeCaps, (uint)Marshal.SizeOf(timeCaps));
                if (value > timeCaps.wPeriodMax)
                    f_Resolution = timeCaps.wPeriodMax;

                if (value < timeCaps.wPeriodMin)
                    f_Resolution = timeCaps.wPeriodMin;

                if (value > f_Interval)
                    f_Resolution = f_Interval;

                //値が異なる場合のみ設定
                if (f_Resolution != value)
                {
                    f_Resolution = value;
                    updateTimeEvent();
                }
            }
        }

        private TimerDelegate onTimer;

        /// <summary>
        /// 
        /// </summary>
        public event TimerDelegate OnTimer
        {
            add
            {
                onTimer += value;
            }
            remove
            {
                onTimer -= value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public MultiMediaTimerComponent()
        {
            TimerHandler = timerProc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        public MultiMediaTimerComponent(IContainer container)
        {
            container.Add(this);

            TimerHandler = timerProc;
        }

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、
        ///  破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            Enabled = false;

            base.Dispose(disposing);
        }

        /// <summary>
        /// 
        /// </summary>
        private void updateTimeEvent()
        {
            UInt32 msDelay;
            UInt32 msResolution;
            uint uKillTimer;
            uint uEndPeriod;
            UIntPtr userCtx = UIntPtr.Zero;

            if (f_Enabled == true && f_Interval > 0)
            {
                if (f_TimerID != 0)
                {
                    uKillTimer = timeKillEvent(f_TimerID);
                }
                msDelay = (uint)f_Interval;
                msResolution = (uint)f_Resolution;

                if (timeBeginPeriod(msResolution) == TIMERR_NOERROR)
                {
                    f_TimerID = timeSetEvent(msDelay, msResolution, TimerHandler, userCtx, 1);
                }
            }
            else
            {
                if (f_TimerID != 0)
                {
                    uKillTimer = timeKillEvent(f_TimerID);

                    msResolution = (uint)f_Resolution;
                    uEndPeriod = timeEndPeriod(msResolution);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="msg"></param>
        /// <param name="userCtx"></param>
        /// <param name="rsv1"></param>
        /// <param name="rsv2"></param>
        private void timerProc(UInt32 id, UInt32 msg, UIntPtr userCtx, UIntPtr rsv1, UIntPtr rsv2)
        {
            onTimer?.Invoke(this);

            /*
            //クリティカルセクションでイベントハンドラを実行する場合
            object syncObject = new object();
            bool lockTaken = false;

            try {
              Monitor.Enter(syncObject, ref lockTaken);
              if (onTimer != null) {

                //onTimer(this);
              }
            }
            finally {
              if (lockTaken ==true) Monitor.Exit(syncObject);
            }
            */
        }
    }

    public delegate void TimerDelegate(object sender);

}

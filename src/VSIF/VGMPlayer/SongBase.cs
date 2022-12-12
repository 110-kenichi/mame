using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace zanac.VGMPlayer
{
    public abstract class SongBase : IDisposable
    {
        public event EventHandler ProcessLoadOccurred;

        public event EventHandler SpeedChanged;

        public event EventHandler PlayStatusChanged;

        public event EventHandler Finished;

        protected void NotifyProcessLoadOccurred()
        {
            HighLoad = true;
            ProcessLoadOccurred?.Invoke(this, EventArgs.Empty);
        }


        protected void NotifyFinished()
        {
            Finished?.Invoke(this, EventArgs.Empty);
        }

        private SoundState state = SoundState.Stopped;

        /// <summary>
        /// 
        /// </summary>
        public virtual SoundState State
        {
            get
            {
                return state;
            }
            protected set
            {
                if (state != value)
                {
                    state = value;
                    PlayStatusChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        protected SoundState RequestedStat
        {
            set;
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void SetStateRequest(SoundState state)
        {
            RequestedStat = state;
            while (State != state)
                Thread.Sleep(1);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool LoopByCount
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int LoopedCount
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int CurrentLoopedCount
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool LoopByElapsed
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TimeSpan LoopTimes
        {
            get;
            set;
        }

        private double _playbackSpeed = 1.0f;

        /// <summary>
        /// 
        /// </summary>
        public virtual double PlaybackSpeed
        {
            get
            {
                return _playbackSpeed;
            }
            set
            {
                _playbackSpeed = Math.Min(Math.Max(value, 0.1), 4);
                SpeedChanged.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string FileName
        {
            get;
            private set;
        }

        public bool Accepted
        {
            get;
            protected set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        protected SongBase(string fileName)
        {
            FileName = fileName;
        }

        private Timer playTicTimer;

        private Stopwatch stopwatch;

        /// <summary>
        /// 
        /// </summary>
        public void Play()
        {
            if (!Accepted)
            {
                FormMain.TopForm.SetStatusText($"The {Path.GetFileName(FileName)} could not be played due to the lack of a compatible chip.");
                NotifyFinished();
                return;
            }

            if (State == SoundState.Playing)
                return;

            playTicTimer = new Timer(250);
            playTicTimer.Elapsed += LoopTimer_Elapsed;
            //playTicTimer.Enabled = true;

            stopwatch = new Stopwatch();

            Thread t = new Thread(new ThreadStart(StreamSong));
            t.Priority = ThreadPriority.Highest;
            t.Start();
            playTicTimer.Start();
            stopwatch.Start();

            FormMain.TopForm.SetElapsedTime(new TimeSpan(0));
            FormMain.TopForm.SetStatusText("Playing");
        }

        private void LoopTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            FormMain.TopForm.SetElapsedTime(stopwatch.Elapsed);
            if (stopwatch.Elapsed > LoopTimes && LoopByElapsed)
            {
                if (!LoopByCount || (LoopByCount && LoopedCount == 0))
                {
                    Stop();
                    NotifyFinished();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Pause()
        {
            if (State == SoundState.Paused)
                return;

            SetStateRequest(SoundState.Paused);
            playTicTimer?.Stop();
            stopwatch?.Stop();
            FormMain.TopForm.SetStatusText("Paused");
        }

        /// <summary>
        /// 
        /// </summary>
        public void Freeze()
        {
            if (State == SoundState.Freezed)
                return;

            SetStateRequest(SoundState.Freezed);
            playTicTimer?.Stop();
            stopwatch?.Stop();
            FormMain.TopForm.SetStatusText("Freezed");
        }

        /// <summary>
        /// 
        /// </summary>
        public void Resume()
        {
            if (State == SoundState.Playing)
                return;

            SetStateRequest(SoundState.Playing);
            playTicTimer?.Start();
            stopwatch?.Start();
            FormMain.TopForm.SetStatusText("Resumed");
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (State == SoundState.Stopped)
                return;

            SetStateRequest(SoundState.Stopped);
            playTicTimer?.Stop();
            stopwatch?.Stop();
            FormMain.TopForm.SetStatusText("Stopped");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="volumeOff"></param>
        abstract protected void StopAllSounds(bool volumeOff);

        abstract protected void StreamSong();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                playTicTimer?.Dispose();
                playTicTimer = null;
            }
        }

        public virtual void Dispose()
        {
        }

        public bool HighLoad
        {
            get;
            set;
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryPerformanceFrequency(out long frequency);

    }

    /// <summary>
    /// 
    /// </summary>
    public enum SoundState
    {
        Playing,
        Stopped,
        Paused,
        Freezed,
    }
}

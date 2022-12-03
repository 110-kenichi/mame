using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            set
            {
                if (state != value)
                {
                    state = value;
                    PlayStatusChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool Looped
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int LoopCount
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        protected SongBase(string fileName)
        {
            FileName = fileName;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Play()
        {
            if (State == SoundState.Playing)
                return;

            FormMain.TopForm.SetStatusText("Playing");
            State = SoundState.Playing;

            Thread t = new Thread(new ThreadStart(StreamSong));
            t.Priority = ThreadPriority.AboveNormal;
            t.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Pause()
        {
            FormMain.TopForm.SetStatusText("Paused");
            State = SoundState.Paused;
            StopAllSounds(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Freeze()
        {
            FormMain.TopForm.SetStatusText("Freezed");
            State = SoundState.Paused;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Resume()
        {
            FormMain.TopForm.SetStatusText("Resumed");
            State = SoundState.Playing;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Stop()
        {
            FormMain.TopForm.SetStatusText("Stopped");
            State = SoundState.Stopped;
            StopAllSounds(true);
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
    }
}

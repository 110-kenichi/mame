using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            ProcessLoadOccurred?.Invoke(this, EventArgs.Empty);
        }


        protected void NotifyFinished()
        {
            Finished?.Invoke(this, EventArgs.Empty);
        }

        private SoundState state = SoundState.Stopped;

        public SoundState State
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

        public bool Looped
        {
            get;
            set;
        }


        public int LoopCount
        {
            get;
            set;
        }

        private double _playbackSpeed = 1.0f;

        public double PlaybackSpeed
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


        public virtual void Play()
        {
            if (State == SoundState.Playing)
                return;

            State = SoundState.Playing;
            Task.Run(StreamSong);
        }

        public void Pause()
        {
            State = SoundState.Paused;
            StopAllSounds(false);
        }

        public void Freeze()
        {
            State = SoundState.Paused;
        }

        public void Resume()
        {
            State = SoundState.Playing;
        }

        public void Stop()
        {
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

        public double Wait
        {
            get;
            set;
        }

    }

    public enum SoundState
    {
        Playing,
        Stopped,
        Paused,
    }
}

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
using zanac.VGMPlayer.Properties;
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
        public virtual bool ConvertChipClock
        {
            get;
            set;
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
            FormMain.TopForm.SetElapsedTime(new TimeSpan(0));
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="frequency_as_hz"></param>
        /// <returns></returns>
        protected static double calcNoteNumberFromFrequency(double frequency)
        {
            return 12.0 * Math.Log(frequency / 440.0, 2) + 69.0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected static double calcFrequencyFromNoteNumber(double noteNum)
        {
            double nn = Math.Pow(2.0, (noteNum - 69.0) / 12.0);
            double freq = 440.0 * nn;
            return freq;
        }

        protected byte convertFromOpmNoteNum(int noteNum)
        {
            byte nn = 0;
            switch (noteNum)
            {
                case 14:
                    nn = 12;
                    break;
                case 0:
                    nn = 1;
                    break;
                case 1:
                    nn = 2;
                    break;
                case 2:
                    nn = 3;
                    break;
                case 4:
                    nn = 4;
                    break;
                case 5:
                    nn = 5;
                    break;
                case 6:
                    nn = 6;
                    break;
                case 8:
                    nn = 7;
                    break;
                case 9:
                    nn = 8;
                    break;
                case 10:
                    nn = 9;
                    break;
                case 12:
                    nn = 10;
                    break;
                case 13:
                    nn = 11;
                    break;
                default:
                    break;
            }

            return nn;
        }

        protected byte convertToOpmNoteNum(int noteNum)
        {
            byte nn = 0;
            switch (noteNum)
            {
                case 0:
                    nn = 14;
                    break;
                case 1:
                    nn = 0;
                    break;
                case 2:
                    nn = 1;
                    break;
                case 3:
                    nn = 2;
                    break;
                case 4:
                    nn = 4;
                    break;
                case 5:
                    nn = 5;
                    break;
                case 6:
                    nn = 6;
                    break;
                case 7:
                    nn = 8;
                    break;
                case 8:
                    nn = 9;
                    break;
                case 9:
                    nn = 10;
                    break;
                case 10:
                    nn = 12;
                    break;
                case 11:
                    nn = 13;
                    break;
            }

            return nn;
        }

        protected (byte Hi, byte Lo, bool noConverted) convertAy8910Frequency(int freqValueHi, int freqValueLo, double chipClock, double dataClock)
        {
            var freq = (int)Math.Round((freqValueHi << 8 | freqValueLo) * chipClock / dataClock);

            if (freq > 0xfff)
                freq = 0xfff;

            var ret = ((byte)(freq >> 8), (byte)(freq & 0xff), false);
            if (ret.Item1 == freqValueHi && ret.Item2 == freqValueLo)
                ret.Item3 = true;
            return ret;
        }

        protected (byte Hi, byte Lo, bool noConverted) convertOpmFrequency(int KF, int KC, double chipClock, double dataClock)
        {
            var oct = (KC >> 4) & 0x7;
            var note = KC & 0xf;
            var kf = KF >> 2;

            var fout = calcFrequencyFromNoteNumber((oct * 12) + convertFromOpmNoteNum(note));
            fout += (calcFrequencyFromNoteNumber((oct * 12) + convertFromOpmNoteNum(note) + 1) - fout) * kf / 64d;

            //convert
            fout *= dataClock / chipClock;

            var noted = calcNoteNumberFromFrequency(fout);

            oct = (int)noted / 12;
            note = convertToOpmNoteNum((int)noted % 12);
            if (note == 14)
                oct -= 1;
            if (oct < 0)
            {
                oct = 0;
                note = 0;
            }
            if (oct > 7)
            {
                oct = 7;
                note = 14;
            }

            kf = (int)Math.Round((noted - (int)noted) * 64d);

            if (oct > 7)
            {
                oct = 7;
                note = 14;
            }

            var ret = ((byte)(kf << 2), (byte)((oct << 4) | note), false);
            if (ret.Item1 == KF && ret.Item2 == KC)
                ret.Item3 = true;
            return ret;
        }

        protected (byte Hi, byte Lo, bool noConverted) convertOpnFrequency(int freqValueHi, int freqValueLo, double chipClock, double dataClock)
        {
            var freq = (freqValueHi << 8) | freqValueLo;
            var block = (freq >> 11) & 0x7;
            var fnum = freq & 0x7ff;

            fnum = (int)Math.Round(fnum * dataClock / chipClock);
            if (fnum > 0x7ff)
            {
                block++;
                fnum = (int)Math.Round(fnum / 2d);
            }
            if (block > 7)
            {
                block = 7;
                fnum = 0x7ff;
            }
            freq = (block << 11) | fnum;

            var ret = ((byte)(freq >> 8), (byte)(freq & 0xff), false);
            if (ret.Item1 == freqValueHi && ret.Item2 == freqValueLo)
                ret.Item3 = true;
            return ret;
        }

        protected (byte Hi, byte Lo, bool noConverted) convertDcsgFrequency(int freqValueHi, int freqValueLo, double chipClock, double dataClock)
        {
            var freq = (int)Math.Round((freqValueHi << 4 | freqValueLo) * chipClock / dataClock);
            if (freq > 0x3ff)
                freq = 0x3ff;

            var ret = ((byte)(freq >> 4), (byte)(freq & 0xf), false);
            if (ret.Item1 == freqValueHi && ret.Item2 == freqValueLo)
                ret.Item3 = true;
            return ret;
        }

        protected (byte Hi, byte Lo, bool noConverted) convertOpllFrequency(int freqValueHi, int freqValueLo, double chipClock, double dataClock)
        {
            var freq = (freqValueHi << 8) | freqValueLo;
            var block = (freq >> 9) & 0x7;
            var fnum = freq & 0x1ff;
            var skon = freqValueHi & 0x30;

            fnum = (int)Math.Round(fnum * dataClock / chipClock);
            if (fnum > 0x1ff)
            {
                block++;
                fnum = (int)Math.Round(fnum / 2d);
            }
            if (block > 7)
            {
                block = 7;
                fnum = 0x1ff;
            }
            freq = (block << 9) | fnum;

            var ret = ((byte)((freq >> 8) | skon), (byte)(freq & 0xff), false);
            if (ret.Item1 == freqValueHi && ret.Item2 == freqValueLo)
                ret.Item3 = true;
            return ret;
        }


        protected (byte Hi, byte Lo, bool noConverted) convertOplFrequency(int freqValueHi, int freqValueLo, double chipClock, double dataClock)
        {
            var freq = (freqValueHi << 8) | freqValueLo;
            var block = (freq >> 10) & 0x7;
            var fnum = freq & 0x3ff;
            var kon = freqValueHi & 0x20;

            fnum = (int)Math.Round(fnum * dataClock / chipClock);
            if (fnum > 0x3ff)
            {
                block++;
                fnum = (int)Math.Round(fnum / 2d);
            }
            if (block > 7)
            {
                block = 7;
                fnum = 0x3ff;
            }
            freq = (block << 10) | fnum;

            var ret = ((byte)((freq >> 8) | kon), (byte)(freq & 0xff), false);
            if (ret.Item1 == freqValueHi && ret.Item2 == freqValueLo)
                ret.Item3 = true;
            return ret;
        }

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

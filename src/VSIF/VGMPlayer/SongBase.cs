using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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

        public String SongChipInformation
        {
            get;
            protected set;
        }

        public String UseChipInformation
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
            t.Priority = ThreadPriority.AboveNormal;
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

        public virtual bool HighLoad
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

        protected (byte Hi, byte Lo, bool noConverted) convertAy8910EnvFrequency(int freqValueHi, int freqValueLo, double chipClock, double dataClock)
        {
            var freq = (int)Math.Round((freqValueHi << 8 | freqValueLo) * chipClock / dataClock);

            if (freq > 0xffff)
                freq = 0xffff;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transferData"></param>
        /// <param name="saddr"></param>
        /// <param name="fp"></param>
        protected void EnablePseudoDacYM2608(VsifClient comPort, bool enable)
        {
            deferredWriteOPNA_P1(comPort, 0x00, 3, 0x01);  //RESET
            if (enable)
            {
                //https://www.piece-me.org/piece-lab/adpcm/adpcm2.html
                //MAX Attenuation
                byte[] adpcmdata = new byte[32];
                for (int i = 0; i < adpcmdata.Length; i++)
                    adpcmdata[i] = 0x80;
                sendAdpcmDataYM2608(comPort, true, adpcmdata, 0, null);

                //*
                //ADPCM mode
                deferredWriteOPNA_P1(comPort, 0x10, 0x17);   //ENA FLAG BRDY
                deferredWriteOPNA_P1(comPort, 0x10, 0x80);   //RESET FLAGS
                deferredWriteOPNA_P1(comPort, 0x00, 0x80);   //CPU->OPNA
                deferredWriteOPNA_P1(comPort, 0x01, 0xC0);   //LR

                // (f / 55.5) * 65536
                // 8KHz = 9447
                //int f = (int)Math.Round((44.1 / 55.5) * 65536);
                //YM2608WriteData(comPort, 0x09, 0, 3, (byte)(f & 0xff), false);   //14KHz
                //YM2608WriteData(comPort, 0x0A, 0, 3, (byte)((f >> 8) & 0xff), false);   //14KHz

                deferredWriteOPNA_P1(comPort, 0x09, 0xff);   //55.5KHz
                deferredWriteOPNA_P1(comPort, 0x0A, 0xff);   //55.5KHz

                deferredWriteOPNA_P1(comPort, 0x0B, 0x00);   // Volume 0

                //playAdpcmDataYM2608(comPort, 0, 0, false);
                //Thread.Sleep(1);
                playAdpcmDataYM2608(comPort, 0, 0, false);

                /*
                deferredWriteOPNA_P1(comPort, 0x08, 0xff);  //255
                deferredWriteOPNA_P1(comPort, 0x08, 0x77);  //119
                deferredWriteOPNA_P1(comPort, 0x08, 0x77);  //119
                deferredWriteOPNA_P1(comPort, 0x08, 0x77);  //119
                deferredWriteOPNA_P1(comPort, 0x08, 0xff);  //255
                deferredWriteOPNA_P1(comPort, 0x08, 0x70);  //112
                deferredWriteOPNA_P1(comPort, 0x08, 0x80);  //128
                //*/

                /*/
                //* DAC mode
                //flag
                deferredWriteOPNA_P1(comPort, 0x10, 0x1B);   //ENA FLAG EOS
                deferredWriteOPNA_P1(comPort, 0x10, 0x80);   //RESET FLAGS
                deferredWriteOPNA_P1(comPort, 0x06, 0xF4);   //16KHz
                deferredWriteOPNA_P1(comPort, 0x07, 0x01);   //16KHz
                deferredWriteOPNA_P1(comPort, 0x01, 0xCC);   //Sart
                deferredWriteOPNA_P1(comPort, 0x0B, 0xff);   // Volume 

                //*/
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transferData"></param>
        /// <param name="saddr"></param>
        /// <param name="fp"></param>
        protected void EnableDacYM2608(VsifClient comPort, bool enable)
        {
            deferredWriteOPNA_P1(comPort, 0x00, 3, 0x01);  //RESET
            if (enable)
            {
                deferredWriteOPNA_P1(comPort, 0x10, 0x1B);   //ENA FLAG EOS
                deferredWriteOPNA_P1(comPort, 0x10, 0x80);   //RESET FLAGS
                deferredWriteOPNA_P1(comPort, 0x06, 0xF4);   //16KHz
                deferredWriteOPNA_P1(comPort, 0x07, 0x01);   //16KHz
                deferredWriteOPNA_P1(comPort, 0x01, 0xCC);   //Sart
                deferredWriteOPNA_P1(comPort, 0x0B, 0x1f);   // Volume 
            }
        }

        protected void playAdpcmDataYM2608(VsifClient comPortOPNA, uint saddr, uint eaddr, bool loop)
        {
            deferredWriteOPNA_P1(comPortOPNA, 0x10, 0x1B); //CLEAR FLAGS
            deferredWriteOPNA_P1(comPortOPNA, 0x10, 0x80); //IRQ RESET

            deferredWriteOPNA_P1(comPortOPNA, 0x00, 0x20); //ACCESS TO MEM

            //pcm start
            deferredWriteOPNA_P1(comPortOPNA, 0x02, (byte)((saddr >> 5) & 0xff));
            deferredWriteOPNA_P1(comPortOPNA, 0x03, (byte)((saddr >> (5 + 8) & 0xff)));
            //pcm end
            deferredWriteOPNA_P1(comPortOPNA, 0x04, (byte)((eaddr >> 5) & 0xff));
            deferredWriteOPNA_P1(comPortOPNA, 0x05, (byte)((eaddr >> (5 + 8)) & 0xff));
            //limit
            deferredWriteOPNA_P1(comPortOPNA, 0x0C, (byte)(0xff));
            deferredWriteOPNA_P1(comPortOPNA, 0x0D, (byte)(0xff));

            //KeyOn
            deferredWriteOPNA_P1(comPortOPNA, 0x00, (byte)(0x80 | 0x20 | (loop ? (byte)0x10 : (byte)0x00)));   //PLAY, ACCESS TO MEM, LOOP
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transferData"></param>
        /// <param name="saddr"></param>
        /// <param name="fp"></param>
        protected void sendAdpcmDataYM2608(VsifClient comPortOPNA, bool ym2608_adpcmbit8, byte[] transferData, int saddr, FormProgress fp)
        {
            //File.WriteAllBytes(transferData.Length.ToString(), transferData);

            deferredWriteOPNA_P1(comPortOPNA, 0x00, 0x01);  //RESET

            //flag
            deferredWriteOPNA_P1(comPortOPNA, 0x10, 0x13);   //CLEAR MASK
            deferredWriteOPNA_P1(comPortOPNA, 0x10, 0x80);   //IRQ RESET
                                                             //Ctrl1
            deferredWriteOPNA_P1(comPortOPNA, 0x00, 0x60);   //REC, EXTMEM
            //Ctrl2
            //START
            if (ym2608_adpcmbit8)
            {
                deferredWriteOPNA_P1(comPortOPNA, 0x01, 0x02);   //LR, 8bit DRAM
                deferredWriteOPNA_P1(comPortOPNA, 0x02, (byte)((saddr >> 5) & 0xff));
                deferredWriteOPNA_P1(comPortOPNA, 0x03, (byte)((saddr >> (5 + 8)) & 0xff));
            }
            else
            {
                deferredWriteOPNA_P1(comPortOPNA, 0x01, 0x00);   //LR, 1bit DRAM
                deferredWriteOPNA_P1(comPortOPNA, 0x02, (byte)((saddr >> 2) & 0xff));
                deferredWriteOPNA_P1(comPortOPNA, 0x03, (byte)((saddr >> (2 + 8)) & 0xff));
            }
            //STOP
            deferredWriteOPNA_P1(comPortOPNA, 0x04, 0xff);
            deferredWriteOPNA_P1(comPortOPNA, 0x05, 0xff);
            //LIMIT
            deferredWriteOPNA_P1(comPortOPNA, 0x0C, 0xff);
            deferredWriteOPNA_P1(comPortOPNA, 0x0D, 0xff);

            //Transfer
            int len = transferData.Length;
            int index = 0;
            int percentage = 0;
            int lastPercentage = 0;
            for (int i = 0; i < len; i++)
            {
                deferredWriteOPNA_P1(comPortOPNA, 0x08, transferData[i]);

                //HACK: WAIT
                switch (comPortOPNA?.SoundModuleType)
                {
                    case VsifSoundModuleType.Spfm:
                    case VsifSoundModuleType.SpfmLight:
                    case VsifSoundModuleType.Gimic:
                        comPortOPNA?.FlushDeferredWriteDataAndWait();
                        break;
                }

                percentage = (100 * index) / len;
                if (percentage != lastPercentage)
                {
                    FormMain.TopForm.SetStatusText("YM2608: Transferring ADPCM(" + percentage + "%)");
                    //fp.Percentage = percentage;
                    comPortOPNA?.FlushDeferredWriteDataAndWait();
                }
                lastPercentage = percentage;
                index++;
                if (RequestedStat == SoundState.Stopped)
                    break;
                else updateStatusForDataTransfer();
            }
            FormMain.TopForm.SetStatusText("YM2608: Transferred ADPCM");

            // Finish
            deferredWriteOPNA_P1(comPortOPNA, 0x10, 0x80);
            deferredWriteOPNA_P1(comPortOPNA, 0x00, 0x01);  //RESET
        }

        /// <summary>
        /// 
        /// </summary>
        protected void updateStatusForDataTransfer()
        {
            if (RequestedStat == SoundState.Paused)
            {
                if (State != SoundState.Paused)
                    State = SoundState.Paused;
            }
            else if (RequestedStat == SoundState.Freezed)
            {
                if (State != SoundState.Freezed)
                    State = SoundState.Freezed;
            }
            else if (RequestedStat == SoundState.Playing)
            {
                if (State != SoundState.Playing)
                    State = SoundState.Freezed;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="comPortOPNA"></param>
        /// <param name="adrs"></param>
        /// <param name="dt"></param>
        /// <param name="dclk"></param>
        protected void deferredWriteOPNA_P0(VsifClient comPortOPNA, int adrs, int dt, uint dclk)
        {
            if (adrs == 7)
                dt &= 0x3f;
            comPortOPNA.RegTable[adrs] = dt;

            switch (adrs)
            {
                case 0:
                case 2:
                case 4:
                    if (!ConvertChipClock || (double)comPortOPNA.ChipClockHz["OPNA_SSG"] == (double)dclk)
                        goto default;
                    {
                        //LO
                        var ret = convertAy8910Frequency(comPortOPNA.RegTable[adrs + 1], dt, comPortOPNA.ChipClockHz["OPNA_SSG"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Lo;
                        deferredWriteOPNA_P0(comPortOPNA, adrs, dt);
                        deferredWriteOPNA_P0(comPortOPNA, adrs + 1, ret.Hi);
                    }
                    break;
                case 1:
                case 3:
                case 5:
                    if (!ConvertChipClock || (double)comPortOPNA.ChipClockHz["OPNA_SSG"] == (double)dclk)
                        goto default;
                    {
                        //HI
                        var ret = convertAy8910Frequency(dt, comPortOPNA.RegTable[adrs - 1], comPortOPNA.ChipClockHz["OPNA_SSG"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Hi;
                        deferredWriteOPNA_P0(comPortOPNA, adrs - 1, ret.Lo);
                        deferredWriteOPNA_P0(comPortOPNA, adrs, dt);
                    }
                    break;
                case 6:
                    if (!ConvertChipClock || (double)comPortOPNA.ChipClockHz["OPNA_SSG"] == (double)dclk)
                        goto default;
                    {
                        var data = (int)Math.Round(dt * (dclk) / (double)comPortOPNA.ChipClockHz["OPNA_SSG"]);
                        if (data > 32)
                            data = 32;
                        deferredWriteOPNA_P0(comPortOPNA, adrs, (byte)data);
                    }
                    break;
                case 0xB:
                    if (!ConvertChipClock || (double)comPortOPNA.ChipClockHz["OPNA_SSG"] == (double)dclk)
                        goto default;
                    {
                        //LO
                        var ret = convertAy8910EnvFrequency(comPortOPNA.RegTable[adrs + 1], dt, comPortOPNA.ChipClockHz["OPNA_SSG"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Lo;
                        deferredWriteOPNA_P0(comPortOPNA, adrs, dt);
                        deferredWriteOPNA_P0(comPortOPNA, adrs + 1, ret.Hi);
                    }
                    break;
                case 0xC:
                    if (!ConvertChipClock || (double)comPortOPNA.ChipClockHz["OPNA_SSG"] == (double)dclk)
                        goto default;
                    {
                        //HI
                        var ret = convertAy8910EnvFrequency(dt, comPortOPNA.RegTable[adrs - 1], comPortOPNA.ChipClockHz["OPNA_SSG"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Hi;
                        deferredWriteOPNA_P0(comPortOPNA, adrs - 1, ret.Lo);
                        deferredWriteOPNA_P0(comPortOPNA, adrs, dt);
                    }
                    break;

                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa8:
                case 0xa9:
                case 0xaa:
                    if (!ConvertChipClock || (double)comPortOPNA.ChipClockHz["OPNA"] == (double)dclk)
                        goto default;
                    {
                        //LO
                        var ret = convertOpnFrequency(comPortOPNA.RegTable[adrs + 4], dt, comPortOPNA.ChipClockHz["OPNA"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Lo;
                        deferredWriteOPNA_P0(comPortOPNA, adrs + 4, ret.Hi);
                        deferredWriteOPNA_P0(comPortOPNA, adrs, dt);
                    }
                    break;
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xac:
                case 0xad:
                case 0xae:
                    if (!ConvertChipClock || (double)comPortOPNA.ChipClockHz["OPNA"] == (double)dclk)
                        goto default;
                    {
                        //HI
                        var ret = convertOpnFrequency(dt, comPortOPNA.RegTable[adrs - 4], comPortOPNA.ChipClockHz["OPNA"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Hi;
                        deferredWriteOPNA_P0(comPortOPNA, adrs, dt);
                        deferredWriteOPNA_P0(comPortOPNA, adrs - 4, ret.Lo);
                    }
                    break;
                default:
                    deferredWriteOPNA_P0(comPortOPNA, adrs, dt);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adrs"></param>
        /// <param name="dt"></param>
        protected void deferredWriteOPNA_P0(VsifClient comPortOPNA, int adrs, int dt)
        {
            switch (comPortOPNA.SoundModuleType)
            {
                case VsifSoundModuleType.MSX_FTDI:
                    comPortOPNA.DeferredWriteData(0x10, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitOPNA);
                    break;
                case VsifSoundModuleType.SpfmLight:
                case VsifSoundModuleType.Spfm:
                    comPortOPNA.DeferredWriteData(0x00, (byte)adrs, (byte)dt, 0);
                    break;
                case VsifSoundModuleType.Gimic:
                    if (!comPortOPNA.Tag.ContainsKey("OPN3L"))
                        comPortOPNA.DeferredWriteData(0, (byte)adrs, (byte)dt, 0);
                    else
                        comPortOPNA.DeferredWriteData(3, (byte)adrs, (byte)dt, 0);
                    break;
            }
        }


        protected void deferredWriteOPNA_P1(VsifClient comPortOPNA, int adrs, int dt, uint dclk)
        {
            comPortOPNA.RegTable[adrs + 0x100] = dt;

            switch (adrs)
            {
                case 0x0e:

                    break;
                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa8:
                case 0xa9:
                case 0xaa:
                    if (!ConvertChipClock || (double)comPortOPNA.ChipClockHz["OPNA"] == (double)dclk)
                        goto default;
                    {
                        //LO
                        var ret = convertOpnFrequency(comPortOPNA.RegTable[adrs + 4 + 0x100], dt, comPortOPNA.ChipClockHz["OPNA"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Lo;
                        deferredWriteOPNA_P1(comPortOPNA, adrs + 4, ret.Hi);
                        deferredWriteOPNA_P1(comPortOPNA, adrs, dt);
                    }
                    break;
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xac:
                case 0xad:
                case 0xae:
                    if (!ConvertChipClock || (double)comPortOPNA.ChipClockHz["OPNA"] == (double)dclk)
                        goto default;
                    {
                        //HI
                        var ret = convertOpnFrequency(dt, comPortOPNA.RegTable[adrs - 4 + 0x100], comPortOPNA.ChipClockHz["OPNA"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Hi;
                        deferredWriteOPNA_P1(comPortOPNA, adrs, dt);
                        deferredWriteOPNA_P1(comPortOPNA, adrs - 4, ret.Lo);
                    }
                    break;
                default:
                    deferredWriteOPNA_P1(comPortOPNA, adrs, dt);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adrs"></param>
        /// <param name="dt"></param>
        protected void deferredWriteOPNA_P1(VsifClient comPortOPNA, int adrs, int dt)
        {
            switch (comPortOPNA.SoundModuleType)
            {
                case VsifSoundModuleType.MSX_FTDI:
                    comPortOPNA.DeferredWriteData(0x11, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitOPNA);
                    break;
                case VsifSoundModuleType.SpfmLight:
                case VsifSoundModuleType.Spfm:
                    comPortOPNA.DeferredWriteData(0x01, (byte)adrs, (byte)dt, 0);
                    break;
                case VsifSoundModuleType.Gimic:
                    if (!comPortOPNA.Tag.ContainsKey("OPN3L"))
                        comPortOPNA.DeferredWriteData(1, (byte)adrs, (byte)dt, 0);
                    else
                        comPortOPNA.DeferredWriteData(4, (byte)adrs, (byte)dt, 0);
                    break;
            }
        }

        /// <summary>
        /// PCM値
        /// </summary>
        int pcmValue = 0;
        /// <summary>
        /// 予測変化量
        /// </summary>
        int predictValue = 127;
        /// <summary>
        /// 8bitデータの1つ目(上位4bit)かどうか
        /// </summary>
        bool firstData = true;
        /// <summary>
        /// 実際のADPCM出力値
        /// </summary>
        byte outputValue = 0;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="adrs"></param>
        /// <param name="dt"></param>
        public void DeferredWriteOPN2_DAC(VsifClient comPortOPN2, int inputValue)
        {
            if (Settings.Default.DisableDAC)
                return;

            if (comPortOPN2.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
            {
                comPortOPN2.DeferredWriteData(0x14, (byte)0x2a, (byte)inputValue, (int)Settings.Default.BitBangWaitOPN2);
            }
            else //Genesis
            {
                comPortOPN2.DeferredWriteDataPrior(
                    new byte[] { 0, 0 },
                    new byte[] { 0x04, 0x8 },
                    new byte[] { (byte)0x2a, (byte)inputValue },
                    (int)Settings.Default.BitBangWaitOPN2);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adrs"></param>
        /// <param name="inputValue"></param>
        public void DeferredWriteOPNA_PseudoDAC(VsifClient comPortOPNA, int inputValue)
        {
            if (Settings.Default.DisableDAC)
                return;

            switch (comPortOPNA.SoundModuleType)
            {
                case VsifSoundModuleType.MSX_FTDI:
                    //Set volume for pseudo DAC
                    comPortOPNA.DeferredWriteData(0x13, (byte)0xb, (byte)inputValue, (int)Settings.Default.BitBangWaitOPNA);
                    //outputAdpcm(comPort, lastWriteDacValue);
                    break;
                case VsifSoundModuleType.SpfmLight:
                case VsifSoundModuleType.Spfm:
                    comPortOPNA.DeferredWriteData(0x02, 0x0b, (byte)inputValue, 0);
                    break;
                case VsifSoundModuleType.Gimic:
                    if (!comPortOPNA.Tag.ContainsKey("OPN3L"))
                        comPortOPNA.DeferredWriteData(1, 0x0b, (byte)inputValue, 0);
                    else
                        comPortOPNA.DeferredWriteData(4, 0x0b, (byte)inputValue, 0);
                    break;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="adrs"></param>
        /// <param name="inputValue"></param>
        public void DeferredWriteOPNA_DAC(VsifClient comPortOPNA, int inputValue)
        {
            if (Settings.Default.DisableDAC)
                return;

            switch (comPortOPNA.SoundModuleType)
            {
                case VsifSoundModuleType.MSX_FTDI:
                    deferredWriteOPNA_P1(comPortOPNA, 0x0e, (byte)inputValue);
                    //TODO: comPortOPNA.DeferredWriteData(0x13, (byte)0xb, (byte)lastWriteDacValue, (int)Settings.Default.BitBangWaitOPNA);
                    break;
                case VsifSoundModuleType.SpfmLight:
                case VsifSoundModuleType.Spfm:
                    comPortOPNA.DeferredWriteData(0x01, 0x0e, (byte)inputValue, 0);
                    break;
                case VsifSoundModuleType.Gimic:
                    if (!comPortOPNA.Tag.ContainsKey("OPN3L"))
                        comPortOPNA.DeferredWriteData(1, 0x0e, (byte)inputValue, 0);
                    break;
            }
        }

        protected void deferredWriteOPN2_P0(VsifClient comPortOPN2, int adrs, int dt, uint dclk)
        {
            //ignore test and unknown registers
            if (adrs < 0x22 || adrs == 0x23 || adrs == 0x29 || (0x2c < adrs && adrs < 0x30))
                return;
            if (adrs > 0xb6)
                return;

            comPortOPN2.RegTable[adrs] = dt;

            switch (adrs)
            {
                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa8:
                case 0xa9:
                case 0xaa:
                    if (!ConvertChipClock || (double)comPortOPN2.ChipClockHz["OPN2"] == (double)dclk)
                        goto default;
                    {
                        //LO
                        var ret = convertOpnFrequency(comPortOPN2.RegTable[adrs + 4], dt, comPortOPN2.ChipClockHz["OPN2"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Lo;
                        deferredWriteOPN2_P0(comPortOPN2, adrs + 4, ret.Hi);
                        deferredWriteOPN2_P0(comPortOPN2, adrs, dt);
                    }
                    break;
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xac:
                case 0xad:
                case 0xae:
                    if (!ConvertChipClock || (double)comPortOPN2.ChipClockHz["OPN2"] == (double)dclk)
                        goto default;
                    {
                        //HI
                        var ret = convertOpnFrequency(dt, comPortOPN2.RegTable[adrs - 4], comPortOPN2.ChipClockHz["OPN2"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Hi;
                        deferredWriteOPN2_P0(comPortOPN2, adrs, dt);
                        deferredWriteOPN2_P0(comPortOPN2, adrs - 4, ret.Lo);
                    }
                    break;
                default:
                    deferredWriteOPN2_P0(comPortOPN2, adrs, dt);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adrs"></param>
        /// <param name="dt"></param>
        protected void deferredWriteOPN2_P0(VsifClient comPortOPN2, int adrs, int dt)
        {
            if (comPortOPN2.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
            {
                comPortOPN2.DeferredWriteData(0x10, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitOPN2);
            }
            else //Genesis
            {
                comPortOPN2.DeferredWriteData(
                    new byte[] { 0, 0 },
                    new byte[] { 0x04, 0x8 },
                    new byte[] { (byte)adrs, (byte)dt },
                    (int)Settings.Default.BitBangWaitOPN2);
            }
        }

        protected void deferredWriteOPN2_P1(VsifClient comPortOPN2, int adrs, int dt, uint dclk)
        {
            //ignore test and unknown registers
            if (adrs < 0x22 || adrs == 0x23 || adrs == 0x29 || (0x2c < adrs && adrs < 0x30))
                return;
            if (adrs > 0xb6)
                return;

            comPortOPN2.RegTable[adrs + 0x100] = dt;

            switch (adrs)
            {
                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa8:
                case 0xa9:
                case 0xaa:
                    if (!ConvertChipClock || (double)comPortOPN2.ChipClockHz["OPN2"] == (double)dclk)
                        goto default;
                    {
                        //LO
                        var ret = convertOpnFrequency(comPortOPN2.RegTable[adrs + 4 + 0x100], dt, comPortOPN2.ChipClockHz["OPN2"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Lo;
                        deferredWriteOPN2_P1(comPortOPN2, adrs + 4, ret.Hi);
                        deferredWriteOPN2_P1(comPortOPN2, adrs, dt);
                    }
                    break;
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xac:
                case 0xad:
                case 0xae:
                    if (!ConvertChipClock || (double)comPortOPN2.ChipClockHz["OPN2"] == (double)dclk)
                        goto default;
                    {
                        //HI
                        var ret = convertOpnFrequency(dt, comPortOPN2.RegTable[adrs - 4 + 0x100], comPortOPN2.ChipClockHz["OPN2"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Hi;
                        deferredWriteOPN2_P1(comPortOPN2, adrs, dt);
                        deferredWriteOPN2_P1(comPortOPN2, adrs - 4, ret.Lo);
                    }
                    break;
                default:
                    deferredWriteOPN2_P1(comPortOPN2, adrs, dt);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adrs"></param>
        /// <param name="dt"></param>
        protected void deferredWriteOPN2_P1(VsifClient comPortOPN2, int adrs, int dt)
        {
            if (comPortOPN2.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
            {
                comPortOPN2.DeferredWriteData(0x11, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitOPN2);
            }
            else
            {
                comPortOPN2.DeferredWriteData(
                    new byte[] { 0, 0 },
                    new byte[] { 0x0C, 0x10 },
                    new byte[] { (byte)adrs, (byte)dt },
                    (int)Settings.Default.BitBangWaitOPN2);
            }
        }

        private void outputAdpcm(VsifClient comPortOPNA, int inputValue)
        {
            //https://www.piece-me.org/piece-lab/adpcm/adpcm2.html
            //変化量 <- PCM入力値 - PCM値
            int deltaValue = (inputValue * 256) - pcmValue;
            //ADPCM値
            int adpcmData = 0;
            //予測変化値に対する変化量の比率によってADPCM値を決定
            if (predictValue * 14 / 8 <= deltaValue)
                adpcmData = 7;
            else if (predictValue * 12 / 8 <= deltaValue && deltaValue < predictValue * 14 / 8)
                adpcmData = 6;
            else if (predictValue * 10 / 8 <= deltaValue && deltaValue < predictValue * 12 / 8)
                adpcmData = 5;
            else if (predictValue * 8 / 8 <= deltaValue && deltaValue < predictValue * 10 / 8)
                adpcmData = 4;
            else if (predictValue * 6 / 8 <= deltaValue && deltaValue < predictValue * 8 / 8)
                adpcmData = 3;
            else if (predictValue * 4 / 8 <= deltaValue && deltaValue < predictValue * 6 / 8)
                adpcmData = 2;
            else if (predictValue * 2 / 8 <= deltaValue && deltaValue < predictValue * 4 / 8)
                adpcmData = 1;
            else if (predictValue * 0 / 8 <= deltaValue && deltaValue < predictValue * 2 / 8)
                adpcmData = 0;
            else if (predictValue * -2 / 8 <= deltaValue && deltaValue < predictValue * 0 / 8)
                adpcmData = 8;
            else if (predictValue * -4 / 8 <= deltaValue && deltaValue < predictValue * -2 / 8)
                adpcmData = 9;
            else if (predictValue * -6 / 8 <= deltaValue && deltaValue < predictValue * -4 / 8)
                adpcmData = 10;
            else if (predictValue * -8 / 8 <= deltaValue && deltaValue < predictValue * -6 / 8)
                adpcmData = 11;
            else if (predictValue * -10 / 8 <= deltaValue && deltaValue < predictValue * -8 / 8)
                adpcmData = 12;
            else if (predictValue * -12 / 8 <= deltaValue && deltaValue < predictValue * -10 / 8)
                adpcmData = 13;
            else if (predictValue * -14 / 8 <= deltaValue && deltaValue < predictValue * -12 / 8)
                adpcmData = 14;
            else if (deltaValue < predictValue * -14 / 8)
                adpcmData = 15;

            //出力
            if (firstData)
            {
                outputValue = (byte)(adpcmData << 4);
                firstData = false;
            }
            else
            {
                outputValue |= (byte)adpcmData;
                comPortOPNA.DeferredWriteData(0x13, (byte)0x8, outputValue, (int)Settings.Default.BitBangWaitOPNA);
                outputValue = 0;
                firstData = true;
            }

            //変化率 = ADPCM値のbit2-0
            int factor = adpcmData & 0x7;
            //変化量 <- 予測変化量 x (変化率 x 2 + 1) / 8
            deltaValue = predictValue * (factor * 2 + 1) / 8;
            if ((adpcmData & 0x8) == 0)
            {
                //増加
                pcmValue = pcmValue + deltaValue;
            }
            else
            {
                //減少
                pcmValue = pcmValue - deltaValue;
            }
            //変化率によって次回の予測変化量を更新
            switch (factor)
            {
                case 0:
                    predictValue = predictValue * 57 / 64;
                    break;
                case 1:
                    predictValue = predictValue * 57 / 64;
                    break;
                case 2:
                    predictValue = predictValue * 57 / 64;
                    break;
                case 3:
                    predictValue = predictValue * 57 / 64;
                    break;
                case 4:
                    predictValue = predictValue * 77 / 64;
                    break;
                case 5:
                    predictValue = predictValue * 102 / 64;
                    break;
                case 6:
                    predictValue = predictValue * 128 / 64;
                    break;
                case 7:
                    predictValue = predictValue * 153 / 64;
                    break;
            }
            //
            if (predictValue < 127)
                predictValue = 127;
            else if (predictValue > 24576)
                predictValue = 24576;
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

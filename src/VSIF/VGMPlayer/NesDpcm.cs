using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using zanac.VGMPlayer.Properties;

namespace zanac.VGMPlayer
{

    /// <summary>
    /// 
    /// </summary>
    public class NesDpcm : IDisposable
    {
        private object engineLockObject;

        private bool stopEngineFlag;

        private bool disposedValue;

        private SampleData currentSampleData;

        private uint clock;

        private VGMSong vgmSong;

        /// <summary>
        /// 
        /// </summary>
        public NesDpcm(uint clock, VGMSong vgmSong)
        {
            this.clock = clock;
            this.vgmSong = vgmSong;
            engineLockObject = new object();
            stopEngineFlag = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartEngine()
        {
            if (stopEngineFlag)
            {
                stopEngineFlag = false;
                Thread t = new Thread(processDac);
                t.Priority = ThreadPriority.AboveNormal;
                t.Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopEngine()
        {
            stopEngineFlag = true;
        }

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcmData"></param>
        public void SetSampleRate(int sampleRate)
        {
            lock (engineLockObject)
            {
                multiply = 1 << Program.Default.NesDpcmRate;
                this.sampleRate = sampleRate / multiply;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcmData"></param>
        public void SetLoopEnable(bool enable)
        {
            lock (engineLockObject)
            {
                lock (engineLockObject)
                {
                    if (currentSampleData != null)
                    {
                        currentSampleData.LoopEnabled = enable;
                    }
                }
            }
        }

        public void SetLoopStart(int start)
        {
            lock (engineLockObject)
            {
                lock (engineLockObject)
                {
                    if (currentSampleData != null)
                    {
                        currentSampleData.LoopStart = start;
                    }
                }
            }
        }

        public void SetLoopLength(int length)
        {
            lock (engineLockObject)
            {
                lock (engineLockObject)
                {
                    if (currentSampleData != null)
                    {
                        currentSampleData.LoopLength = length;
                    }
                }
            }
        }
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcmData"></param>
        public void Play(int sampleRate, byte[] dpcmData, int index, int length, bool loopEnable)
        {
            lock (engineLockObject)
            {
                var sd = new SampleData(dpcmData, index);
                sd.LoopEnabled = loopEnable;
                sd.LoopStart = index;
                sd.LoopLength = length;
                sd.Multiply = 1 << Program.Default.NesDpcmRate;
                sd.SampleRate = sampleRate / sd.Multiply;
                currentSampleData = sd;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcmData"></param>
        public bool IsPlaying()
        {
            lock (engineLockObject)
            {
                return currentSampleData != null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcmData"></param>
        public void Stop()
        {
            lock (engineLockObject)
            {
                currentSampleData = null;
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryPerformanceFrequency(out long frequency);

        /// <summary>
        /// 
        /// </summary>
        private void processDac()
        {
            long freq, before, after;
            double dbefore;
            QueryPerformanceFrequency(out freq);
            QueryPerformanceCounter(out before);
            dbefore = before;
            while (!stopEngineFlag)
            {
                if (disposedValue)
                    break;

                byte dacData = 0;
                int sampleRate = 8000;
                bool playDac = false;
                {
                    lock (engineLockObject)
                    {
                        var sd = currentSampleData;
                        if (sd != null)
                        {
                            var d = sd.GetDacData();
                            if (d != null)
                            {
                                sampleRate = sd.SampleRate;
                                dacData = d.Value;
                                playDac = true;
                            }
                        }
                    }

                    if (playDac)
                    {
                        vgmSong.DeferredWriteNESDAC(dacData);
                    }
                }

                QueryPerformanceCounter(out after);
                double nextTime = dbefore + ((double)freq / (double)sampleRate);
                while (after < nextTime)
                    QueryPerformanceCounter(out after);
                dbefore = nextTime;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    stopEngineFlag = true;
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~PcmEngine()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SampleData
    {
        public byte[] DpcmData
        {
            get;
            private set;
        }

        public bool LoopEnabled
        {
            get;
            set;
        }

        public int LoopStart
        {
            get;
            set;
        }

        public int LoopLength
        {
            get;
            set;
        }

        public int SampleRate
        {
            get;
            set;
        }

        public int Multiply
        {
            get;
            set;
        }

        private int index;

        private int bitIndex;

        private int dpacmValue = 0x40;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adress"></param>
        /// <param name="size"></param>
        public SampleData(byte[] dpcmData, int index)
        {
            DpcmData = (byte[])dpcmData.Clone();
            this.index = index;
        }

        public byte? GetDacData()
        {
            if (index >= DpcmData.Length || index >= LoopStart + LoopLength)
            {
                if (LoopEnabled)
                {
                    dpacmValue = 0;
                    index = LoopStart;
                    bitIndex = 0;
                }
                else
                {
                    return null;
                }
            }

            int dv = 0;
            for (int i = 0; i < Multiply; i++)
            {
                var dd = DpcmData[index];
                if ((dd & (1 << bitIndex++)) != 0)
                    dpacmValue += 2;
                else
                    dpacmValue -= 2;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    index++;
                }
                dv += dpacmValue;
            }
            dpacmValue = dv / Multiply;

            if (dpacmValue > 127)
                dpacmValue = 127;
            else if (dpacmValue < 0)
                dpacmValue = 0;

            return (byte)dpacmValue;
        }
    }
}

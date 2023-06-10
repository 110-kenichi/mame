//https://github.com/kuma4649/MDPlayer
//kumatan
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using zanac.VGMPlayer.Properties;
using static System.Windows.Forms.AxHost;

namespace zanac.VGMPlayer
{
    internal class DacStream : IDisposable
    {
        private bool disposedValue;

        private SongBase parentSong;

        private VsifClient comPortOPN2 = null;
        private VsifClient comPortOPNA = null;
        private OKIM6258 okim6258 = null;
        private VsifClient comPortNES = null;

        public DacStream(SongBase parentSong, VsifClient comPortOPN2, VsifClient comPortOPNA, OKIM6258 okim6258, VsifClient comPortNES)
        {
            this.parentSong = parentSong;

            this.comPortOPN2 = comPortOPN2;
            this.comPortOPNA = comPortOPNA;
            this.okim6258 = okim6258;
            this.comPortNES = comPortNES;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool QueryPerformanceFrequency(out long frequency);


        public object lockObject = new object();

        bool streaming;

        private PlayDataInfo playData;


        /// <summary>
        /// 
        /// </summary>
        public void StreamSong()
        {
            double sampleRate = 15626;
            int multiply = 1;

            int[][] outputs = new int[2][];

            long freq, before, after;
            QueryPerformanceFrequency(out freq);

            while (true)
            {
                if (disposedValue)
                    break;

                if (parentSong.State == SoundState.Stopped)
                {
                    break;
                }
                else if (parentSong.State == SoundState.Paused)
                {
                    Thread.Sleep(1);
                    QueryPerformanceCounter(out before);
                    if (playData != null)
                        playData.BeforeTime = before;
                    continue;
                }
                else if (parentSong.State == SoundState.Freezed)
                {
                    Thread.Sleep(1);
                    QueryPerformanceCounter(out before);
                    if (playData != null)
                        playData.BeforeTime = before;
                    continue;
                }

                if (!streaming)
                    continue;

                PlayDataInfo pd = this.playData;

                if (pd.StreamIdxDir > 0)
                {
                    if (pd.StreamIdx >= pd.CurrentStreamParam.Offset + pd.CurrentStreamParam.Length)
                    {
                        if ((pd.CurrentStreamParam.Mode & StreamModes.Loop) != StreamModes.Loop)
                            streaming = false;
                        else
                            pd.StreamIdx = pd.CurrentStreamParam.Offset;
                    }
                }
                else
                {
                    if (pd.StreamIdx < pd.CurrentStreamParam.Offset)
                    {
                        if ((pd.CurrentStreamParam.Mode & StreamModes.Loop) != StreamModes.Loop)
                            streaming = false;
                        else
                            pd.StreamIdx = pd.CurrentStreamParam.Offset + pd.CurrentStreamParam.Length - 1;
                    }
                }

                if (!streaming)
                    continue;

                byte data = pd.DacData[pd.StreamIdx];
                pd.StreamIdx += pd.StreamIdxDir;

                switch (pd.ChipType)
                {
                    case 0x2:
                        if (comPortOPN2 != null)
                        {
                            parentSong.DeferredWriteOPN2_DAC(comPortOPN2, data);
                        }
                        else if (comPortOPNA != null && comPortOPNA.Tag.ContainsKey("ProxyOPN2"))
                        {
                            data = (byte)Math.Round((double)data * (double)Settings.Default.DacVolume / 100d);
                            parentSong.DeferredWriteOPNA_PseudoDAC(comPortOPNA, data);
                        }
                        sampleRate = pd.CurrentStreamData.Frequency;
                        break;
                    case 20:
                        if (comPortNES != null)
                        {
                            ((VGMSong)parentSong).DeferredWriteNES(0x11, data, (uint)Settings.Default.BitBangWaitNES);
                        }
                        sampleRate = pd.CurrentStreamData.Frequency;
                        break;
                    case 23:
                        {
                            if (okim6258 != null)
                            {
                                if (!pd.Oki6285Adpcm2ndNibble)
                                {
                                    var ddata = okim6258.decode(data & 0xf);
                                    ddata = (int)Math.Round((double)ddata * (double)Settings.Default.DacVolume / 100d);

                                    if (comPortOPN2 != null && comPortOPN2.Tag.ContainsKey("ProxyOKIM6258"))
                                    {
                                        byte bdata = (byte)((ddata >> 8) + 128);
                                        parentSong.DeferredWriteOPN2_DAC(comPortOPN2, bdata);
                                    }
                                    else if (comPortOPNA != null && comPortOPNA.Tag.ContainsKey("ProxyOKIM6258"))
                                    {
                                        byte bdata = (byte)((ddata >> 8));
                                        parentSong.DeferredWriteOPNA_DAC(comPortOPNA, bdata);
                                    }
                                    pd.StreamIdx -= pd.StreamIdxDir;
                                    pd.Oki6285Adpcm2ndNibble = true;
                                }
                                else
                                {
                                    var ddata = okim6258.decode(data >> 4);
                                    //var ddata = decodeOpnaAdpcm(data >> 4);

                                    ddata = (int)Math.Round((double)ddata * (double)Settings.Default.DacVolume / 100d);

                                    if (comPortOPN2 != null && comPortOPN2.Tag.ContainsKey("ProxyOKIM6258"))
                                    {
                                        byte bdata = (byte)((ddata >> 8) + 128);
                                        parentSong.DeferredWriteOPN2_DAC(comPortOPN2, bdata);
                                    }
                                    else if (comPortOPNA != null && comPortOPNA.Tag.ContainsKey("ProxyOKIM6258"))
                                    {
                                        byte bdata = (byte)((ddata >> 8));
                                        parentSong.DeferredWriteOPNA_DAC(comPortOPNA, bdata);
                                    }
                                    pd.Oki6285Adpcm2ndNibble = false;
                                }
                            }
                        }
                        sampleRate = pd.CurrentStreamData.Frequency << 1;
                        break;
                }

                QueryPerformanceCounter(out after);
                double nextTime = pd.BeforeTime + ((double)freq / (sampleRate / (double)multiply));
                while (after < nextTime)
                    QueryPerformanceCounter(out after);
                pd.BeforeTime = nextTime;
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
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~SegaPcm()
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="comPortOPN2"></param>
        /// <param name="comPortOPNA"></param>
        /// <param name="okim6258"></param>
        /// <param name="currentStreamParam"></param>
        /// <param name="currentStreamData"></param>
        /// <param name="streamChipType"></param>
        /// <param name="dacData"></param>
        public void Play(StreamParam currentStreamParam, StreamData currentStreamData, int streamChipType, List<byte> dacData)
        {
            PlayDataInfo pd = new PlayDataInfo();
            pd.CurrentStreamParam = currentStreamParam;
            pd.CurrentStreamData = currentStreamData;

            pd.ChipType = streamChipType;
            pd.DacData = dacData;

            if ((currentStreamParam.Mode & StreamModes.Reverse) != StreamModes.Reverse)
            {
                pd.StreamIdx = currentStreamParam.Offset;
                pd.StreamIdxDir = 1;
            }
            else
            {
                pd.StreamIdx = currentStreamParam.Offset + currentStreamParam.Length - 1;
                pd.StreamIdxDir = -1;
            }
            long before;
            QueryPerformanceCounter(out before);
            pd.BeforeTime = before;

            this.playData = pd;
            streaming = true;
        }

        public void Stop()
        {
            streaming = false;
        }

        /// <summary>
        /// 
        /// </summary>
        private class PlayDataInfo
        {
            public int ChipType;

            public StreamData CurrentStreamData = null;
            public StreamParam CurrentStreamParam = null;

            public int StreamIdxDir;
            public int StreamIdx;

            public List<byte> DacData;

            public double BeforeTime;

            public bool Oki6285Adpcm2ndNibble;
        }

    }


    [Flags]
    internal enum StreamModes
    {
        Loop = 0x01,
        Reverse = 0x02,
    }

    internal class StreamParam
    {
        public int StreamID;
        public int BlockID;
        public int Offset;
        public int Length;
        public StreamModes Mode;
    }
}

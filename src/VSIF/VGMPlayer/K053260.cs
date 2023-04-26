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
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.AxHost;

namespace zanac.VGMPlayer
{
    internal class K053260 : IDisposable
    {
        private bool disposedValue;

        private SongBase parentSong;

        public K053260(SongBase parentSong)
        {
            //0..Main

            this.parentSong = parentSong;
        }

        private VsifClient vsifClient;

        private int drive_clock;


        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool QueryPerformanceFrequency(out long frequency);

        int sampleRate = 31250;


        /// <summary>
        /// 
        /// </summary>
        public void StreamSong()
        {
            int multiply = 1 + Settings.Default.K053260Rate;
            int[][] outputs = new int[2][];

            long freq, before, after;
            double dbefore;
            QueryPerformanceFrequency(out freq);
            QueryPerformanceCounter(out before);
            dbefore = before;
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
                    dbefore = before;
                    continue;
                }
                else if (parentSong.State == SoundState.Freezed)
                {
                    Thread.Sleep(1);
                    QueryPerformanceCounter(out before);
                    dbefore = before;
                    continue;
                }

                outputs[0] = new int[multiply];
                outputs[1] = new int[multiply];
                k053260_update(0, outputs, multiply);

                int dtL = 0;
                int dtR = 0;
                for (int i = 0; i < multiply; i++)
                {
                    dtL += outputs[0][i];
                    dtR += outputs[1][i];
                }
                dtL /= multiply;
                dtR /= multiply;
                dtL = (int)Math.Round((double)dtL * (double)Settings.Default.DacVolume / 100d);
                dtR = (int)Math.Round((double)dtR * (double)Settings.Default.DacVolume / 100d);

                if (vsifClient != null)
                {
                    if (vsifClient.ChipClockHz.ContainsKey("OPN2") && vsifClient.Tag.ContainsKey("ProxyK053260"))
                    {
                        int dt = ((dtL + dtR) / 2) >> 8;

                        if (dt > sbyte.MaxValue)
                            dt = sbyte.MaxValue;
                        else if (dt < sbyte.MinValue)
                            dt = sbyte.MinValue;

                        parentSong.DeferredWriteOPN2_DAC(vsifClient, (byte)(dt + 128));
                    }
                    else if (vsifClient.ChipClockHz.ContainsKey("OPNA") && vsifClient.Tag.ContainsKey("ProxyK053260"))
                    {
                        int dt = ((dtL + dtR) / 2) >> 8;

                        if (dt > sbyte.MaxValue)
                            dt = sbyte.MaxValue;
                        else if (dt < sbyte.MinValue)
                            dt = sbyte.MinValue;

                        parentSong.DeferredWriteOPNA_DAC(vsifClient, dt);
                    }
                }

                QueryPerformanceCounter(out after);
                double nextTime = dbefore + ((double)freq / (sampleRate / (double)multiply));
                while (after < nextTime)
                    QueryPerformanceCounter(out after);
                dbefore = nextTime ;
            }
        }

        //static DEVICE_START( segapcm )
        public int device_start_k053260(byte ChipID, int clock, VsifClient vsifClient)
        {
            this.vsifClient = vsifClient;
            this.drive_clock = clock;
            return device_start_k053260(ChipID, clock);
        }

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

        public class k053260_channel
        {
            public uint rate;

            public uint size;

            public uint start;

            public uint bank;

            public uint volume;

            public int play;

            public uint pan;

            public uint pos;

            public int loop;

            public int ppcm;

            public int ppcm_data;

            public byte Muted;
        }

        public class k053260_state
        {
            public int mode;

            public int[] regs = new int[48];

            public byte[] rom;

            public uint rom_size;

            public uint[] delta_table;

            public k053260_channel[] channels = new k053260_channel[4]
            {
                new k053260_channel(),
                new k053260_channel(),
                new k053260_channel(),
                new k053260_channel()
            };
        }

        private const int BASE_SHIFT = 16;

        private const int MAX_CHIPS = 2;

        private k053260_state[] K053260Data = new k053260_state[2]
        {
            new k053260_state(),
            new k053260_state()
        };

        private const int MAXOUT = 32768;

        private const int MINOUT = -32768;

        private sbyte[] dpcmcnv = new sbyte[16]
        {
            0, 1, 2, 4, 8, 16, 32, 64, -128, -64,
            -32, -16, -8, -4, -2, -1
        };

        private int[] lvol = new int[4];

        private int[] rvol = new int[4];

        private int[] play = new int[4];

        private int[] loop = new int[4];

        private int[] ppcm = new int[4];

        private uint[] ptrRom = new uint[4];

        private uint[] delta = new uint[4];

        private uint[] end = new uint[4];

        private uint[] pos = new uint[4];

        private sbyte[] ppcm_data = new sbyte[4];


        private void InitDeltaTable(k053260_state ic, int rate, int clock)
        {
            double num = rate;
            double num2 = clock;
            for (int i = 0; i < 4096; i++)
            {
                double num3 = 4096 - i;
                double num4 = num2 / num3;
                double num5 = 65536.0;
                uint num6;
                if (num4 != 0.0 && num != 0.0)
                {
                    num4 = num5 / (num / num4);
                    num6 = (uint)num4;
                    if (num6 == 0)
                    {
                        num6 = 1u;
                    }
                }
                else
                {
                    num6 = 1u;
                }

                ic.delta_table[i] = num6;
            }
        }

        public void device_reset_k053260(byte ChipID)
        {
            k053260_state k053260_state = K053260Data[ChipID];
            for (int i = 0; i < 4; i++)
            {
                k053260_state.channels[i].rate = 0u;
                k053260_state.channels[i].size = 0u;
                k053260_state.channels[i].start = 0u;
                k053260_state.channels[i].bank = 0u;
                k053260_state.channels[i].volume = 0u;
                k053260_state.channels[i].play = 0;
                k053260_state.channels[i].pan = 0u;
                k053260_state.channels[i].pos = 0u;
                k053260_state.channels[i].loop = 0;
                k053260_state.channels[i].ppcm = 0;
                k053260_state.channels[i].ppcm_data = 0;
            }
        }

        private int limit(int val, int max, int min)
        {
            if (val > max)
            {
                val = max;
            }
            else if (val < min)
            {
                val = min;
            }

            return val;
        }

        public void k053260_update(byte ChipID, int[][] outputs, int samples)
        {
            k053260_state k053260_state = K053260Data[ChipID];
            for (int i = 0; i < 4; i++)
            {
                if (k053260_state.channels[i].Muted != 0)
                {
                    play[i] = 0;
                    continue;
                }

                ptrRom[i] = k053260_state.channels[i].start + (k053260_state.channels[i].bank << 16);
                delta[i] = k053260_state.delta_table[k053260_state.channels[i].rate];
                lvol[i] = (int)(k053260_state.channels[i].volume * k053260_state.channels[i].pan);
                rvol[i] = (int)(k053260_state.channels[i].volume * (8 - k053260_state.channels[i].pan));
                end[i] = k053260_state.channels[i].size;
                pos[i] = k053260_state.channels[i].pos;
                play[i] = k053260_state.channels[i].play;
                loop[i] = k053260_state.channels[i].loop;
                ppcm[i] = k053260_state.channels[i].ppcm;
                ppcm_data[i] = (sbyte)k053260_state.channels[i].ppcm_data;
                if (ppcm[i] != 0)
                {
                    delta[i] /= 2u;
                }
            }

            for (int j = 0; j < samples; j++)
            {
                int num;
                int num2 = (num = 0);
                for (int i = 0; i < 4; i++)
                {
                    if (play[i] == 0)
                    {
                        continue;
                    }

                    if (pos[i] >> 16 >= end[i])
                    {
                        ppcm_data[i] = 0;
                        if (loop[i] == 0)
                        {
                            play[i] = 0;
                            continue;
                        }

                        pos[i] = 0u;
                    }

                    sbyte b;
                    if (ppcm[i] != 0)
                    {
                        if (pos[i] == 0 || ((pos[i] ^ (pos[i] - delta[i])) & 0x8000) == 32768)
                        {
                            int num3 = (((pos[i] & 0x8000) == 0) ? (k053260_state.rom[ptrRom[i] + (pos[i] >> 16)] & 0xF) : ((k053260_state.rom[ptrRom[i] + (pos[i] >> 16)] >> 4) & 0xF));
                            ppcm_data[i] += dpcmcnv[num3];
                        }

                        b = ppcm_data[i];
                        pos[i] += delta[i];
                    }
                    else
                    {
                        b = (sbyte)k053260_state.rom[ptrRom[i] + (pos[i] >> 16)];
                        pos[i] += delta[i];
                    }

                    if (((uint)k053260_state.mode & 2u) != 0)
                    {
                        num2 += b * lvol[i] >> 2;
                        num += b * rvol[i] >> 2;
                    }
                }

                outputs[1][j] = limit(num2, 32768, -32768);
                outputs[0][j] = limit(num, 32768, -32768);
            }

            for (int i = 0; i < 4; i++)
            {
                if (k053260_state.channels[i].Muted == 0)
                {
                    k053260_state.channels[i].pos = pos[i];
                    k053260_state.channels[i].play = play[i];
                    k053260_state.channels[i].ppcm_data = ppcm_data[i];
                }
            }
        }

        public int device_start_k053260(byte ChipID, int clock)
        {
            sampleRate = clock / 32;
            if (ChipID >= 2)
            {
                return 0;
            }

            k053260_state k053260_state = K053260Data[ChipID];
            k053260_state.mode = 0;
            k053260_state.rom = null;
            k053260_state.rom_size = 0u;
            for (int i = 0; i < 48; i++)
            {
                k053260_state.regs[i] = 0;
            }

            k053260_state.delta_table = new uint[4096];
            InitDeltaTable(k053260_state, sampleRate, clock);
            for (int i = 0; i < 4; i++)
            {
                k053260_state.channels[i].Muted = 0;
            }

            return sampleRate;
        }

        public void device_stop_k053260(byte ChipID)
        {
            K053260Data[ChipID].rom = null;
        }

        private void check_bounds(k053260_state ic, int channel)
        {
            int num = (int)((ic.channels[channel].bank << 16) + ic.channels[channel].start);
            int num2 = (int)(num + ic.channels[channel].size - 1);
            if (num > ic.rom_size)
            {
                ic.channels[channel].play = 0;
            }
            else if (num2 > ic.rom_size)
            {
                ic.channels[channel].size = (uint)(ic.rom_size - num);
            }
        }

        public void k053260_w(byte ChipID, int offset, byte data)
        {
            k053260_state k053260_state = K053260Data[ChipID];
            if (offset > 47)
            {
                return;
            }

            if (offset == 40)
            {
                int num = k053260_state.regs[offset] ^ data;
                for (int i = 0; i < 4; i++)
                {
                    if ((num & (1 << i)) != 0)
                    {
                        if ((data & (1 << i)) != 0)
                        {
                            k053260_state.channels[i].play = 1;
                            k053260_state.channels[i].pos = 0u;
                            k053260_state.channels[i].ppcm_data = 0;
                            check_bounds(k053260_state, i);
                        }
                        else
                        {
                            k053260_state.channels[i].play = 0;
                        }
                    }
                }

                k053260_state.regs[offset] = data;
                return;
            }

            k053260_state.regs[offset] = data;
            switch (offset)
            {
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                case 29:
                case 30:
                case 31:
                case 32:
                case 33:
                case 34:
                case 35:
                case 36:
                case 37:
                case 38:
                case 39:
                    {
                        int num2 = (offset - 8) / 8;
                        switch ((offset - 8) & 7)
                        {
                            case 0:
                                k053260_state.channels[num2].rate &= 3840u;
                                k053260_state.channels[num2].rate |= data;
                                break;
                            case 1:
                                k053260_state.channels[num2].rate &= 255u;
                                k053260_state.channels[num2].rate |= (uint)((data & 0xF) << 8);
                                break;
                            case 2:
                                k053260_state.channels[num2].size &= 65280u;
                                k053260_state.channels[num2].size |= data;
                                break;
                            case 3:
                                k053260_state.channels[num2].size &= 255u;
                                k053260_state.channels[num2].size |= (uint)(data << 8);
                                break;
                            case 4:
                                k053260_state.channels[num2].start &= 65280u;
                                k053260_state.channels[num2].start |= data;
                                break;
                            case 5:
                                k053260_state.channels[num2].start &= 255u;
                                k053260_state.channels[num2].start |= (uint)(data << 8);
                                break;
                            case 6:
                                k053260_state.channels[num2].bank = data & 0xFFu;
                                break;
                            case 7:
                                k053260_state.channels[num2].volume = (uint)((data & 0x7F) << 1) | (data & 1u);
                                break;
                        }

                        break;
                    }
                case 42:
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            k053260_state.channels[i].loop = (((data & (1 << i)) != 0) ? 1 : 0);
                        }

                        for (int i = 4; i < 8; i++)
                        {
                            k053260_state.channels[i - 4].ppcm = (((data & (1 << i)) != 0) ? 1 : 0);
                        }

                        break;
                    }
                case 44:
                    k053260_state.channels[0].pan = data & 7u;
                    k053260_state.channels[1].pan = (uint)(data >> 3) & 7u;
                    break;
                case 45:
                    k053260_state.channels[2].pan = data & 7u;
                    k053260_state.channels[3].pan = (uint)(data >> 3) & 7u;
                    break;
                case 47:
                    k053260_state.mode = data & 7;
                    break;
            }
        }

        public byte k053260_r(byte ChipID, int offset)
        {
            k053260_state k053260_state = K053260Data[ChipID];
            switch (offset)
            {
                case 41:
                    {
                        int num2 = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            num2 |= k053260_state.channels[i].play << i;
                        }

                        return (byte)num2;
                    }
                case 46:
                    if (((uint)k053260_state.mode & (true ? 1u : 0u)) != 0)
                    {
                        uint num = k053260_state.channels[0].start + (k053260_state.channels[0].pos >> 16) + (k053260_state.channels[0].bank << 16);
                        k053260_state.channels[0].pos += 65536u;
                        if (num > k053260_state.rom_size)
                        {
                            return 0;
                        }

                        return k053260_state.rom[num];
                    }

                    break;
            }

            return (byte)k053260_state.regs[offset];
        }

        public void k053260_write_rom(byte ChipID, int ROMSize, int DataStart, int DataLength, byte[] ROMData)
        {
            k053260_state k053260_state = K053260Data[ChipID];
            if (k053260_state.rom_size != ROMSize)
            {
                k053260_state.rom = new byte[ROMSize];
                k053260_state.rom_size = (uint)ROMSize;
                for (int i = 0; i < ROMSize; i++)
                {
                    k053260_state.rom[i] = byte.MaxValue;
                }
            }

            if (DataStart <= ROMSize)
            {
                if (DataStart + DataLength > ROMSize)
                {
                    DataLength = ROMSize - DataStart;
                }

                for (int j = 0; j < DataLength; j++)
                {
                    k053260_state.rom[j + DataStart] = ROMData[j];
                }
            }
        }

        public void k053260_write_rom(byte ChipID, int ROMSize, int DataStart, int DataLength, byte[] ROMData, int SrcStartAdr)
        {
            k053260_state k053260_state = K053260Data[ChipID];
            if (k053260_state.rom_size != ROMSize)
            {
                k053260_state.rom = new byte[ROMSize];
                k053260_state.rom_size = (uint)ROMSize;
                for (int i = 0; i < ROMSize; i++)
                {
                    k053260_state.rom[i] = byte.MaxValue;
                }
            }

            if (DataStart <= ROMSize)
            {
                if (DataStart + DataLength > ROMSize)
                {
                    DataLength = ROMSize - DataStart;
                }

                for (int j = 0; j < DataLength; j++)
                {
                    k053260_state.rom[j + DataStart] = ROMData[j + SrcStartAdr];
                }
            }
        }

        public void k053260_set_mute_mask(byte ChipID, uint MuteMask)
        {
            k053260_state k053260_state = K053260Data[ChipID];
            for (byte b = 0; b < 4; b = (byte)(b + 1))
            {
                k053260_state.channels[b].Muted = (byte)((MuteMask >> (int)b) & 1u);
            }
        }
    }
}

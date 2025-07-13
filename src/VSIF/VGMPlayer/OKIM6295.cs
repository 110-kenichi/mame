//https://github.com/kuma4649/MDPlayer
//kumatan

// license:BSD-3-Clause
// copyright-holders:Mirko Buffoni,Aaron Giles
/***************************************************************************
    okim6295.cpp
    OKIM 6295 ADCPM sound chip.
****************************************************************************
    Library to transcode from an ADPCM source to raw PCM.
    Written by Buffoni Mirko in 08/06/97
    References: various sources and documents.
    R. Belmont 31/10/2003
    Updated to allow a driver to use both MSM6295s and "raw" ADPCM voices
    (gcpinbal). Also added some error trapping for MAME_DEBUG builds
****************************************************************************
    OKIM 6295 ADPCM chip:
    Command bytes are sent:
        1xxx xxxx = start of 2-byte command sequence, xxxxxxx is the sample
                    number to trigger
        abcd vvvv = second half of command; one of the abcd bits is set to
                    indicate which voice the v bits seem to be volumed
        0abc d000 = stop playing; one or more of the abcd bits is set to
                    indicate which voice(s)
    Status is read:
        ???? abcd = one bit per voice, set to 0 if nothing is playing, or
                    1 if it is active
    OKI Semiconductor produced this chip in two package variants. The
    44-pin QFP version, MSM6295GS, is the original one and by far the more
    common of the two. The 42-pin DIP version, MSM6295VRS, omits A17 and
    RD, which limits its ROM addressing to one megabit instead of two.
***************************************************************************/

using System;
using System.Collections;
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
    internal class OKIM6295 : IDisposable
    {
        private bool disposedValue;

        private SongBase parentSong;

        public OKIM6295(SongBase parentSong)
        {
            //0..Main

            this.parentSong = parentSong;
        }

        private VsifClient vsifClient;

        private int drive_clock;

        private int sample_rate;

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool QueryPerformanceFrequency(out long frequency);

        /// <summary>
        /// 
        /// </summary>
        public void StreamSong()
        {
            int multiply = 1 + Program.Default.OKIM6295Rate;

            int[][] outputs = new int[2][];

            //int count = 0;
            long freq, before, after;
            double dbefore;
            QueryPerformanceFrequency(out freq);
            QueryPerformanceCounter(out before);
            dbefore = before;
            while (true)
            {
                double sampleRate = sample_rate;

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
                lock(lockObject)
                    okim6295_update(0, outputs, multiply);

                int dtL = 0;
                int dtR = 0;
                for (int i = 0; i < multiply; i++)
                {
                    dtL += outputs[0][i];
                    dtR += outputs[1][i];
                }
                dtL /= multiply;
                dtR /= multiply;
                //dtL = (int)Math.Round((double)dtL * (double)PcmMixer.DacVolume / 100d);
                //dtR = (int)Math.Round((double)dtR * (double)PcmMixer.DacVolume / 100d);

                if (vsifClient != null)
                {
                    if ((vsifClient.SoundModuleType == VsifSoundModuleType.TurboR_FTDI || vsifClient.SoundModuleType == VsifSoundModuleType.MSX_PiTR)
                        && vsifClient.Tag.ContainsKey("ProxyK053260"))
                    {
                        int dt = ((dtL + dtR) / 2) >> 8;

                        if (dt > sbyte.MaxValue)
                        {
                            dt = sbyte.MaxValue;
                            parentSong.NotifyDacClipOccurred();
                        }
                        else if (dt < sbyte.MinValue)
                        {
                            dt = sbyte.MinValue;
                            parentSong.NotifyDacClipOccurred();
                        }
                        parentSong.DeferredWriteTurboR_DAC(vsifClient, (byte)(dt + 128));
                    }
                    else if (vsifClient.ChipClockHz.ContainsKey("OPN2") && vsifClient.Tag.ContainsKey("ProxyOKIM6295"))
                    {
                        int dt = ((dtL + dtR) / 2) >> 7;

                        if (dt > sbyte.MaxValue)
                        {
                            dt = sbyte.MaxValue;
                            parentSong.NotifyDacClipOccurred();
                        }
                        else if (dt < sbyte.MinValue)
                        {
                            dt = sbyte.MinValue;
                            parentSong.NotifyDacClipOccurred();
                        }

                        parentSong.DeferredWriteOPN2_DAC(vsifClient, (byte)(dt + 128));
                    }
                    else if (vsifClient.ChipClockHz.ContainsKey("OPNA") && vsifClient.Tag.ContainsKey("ProxyOKIM6295"))
                    {
                        int dt = ((dtL + dtR) / 2) >> 7;

                        if (dt > sbyte.MaxValue)
                        {
                            dt = sbyte.MaxValue;
                            parentSong.NotifyDacClipOccurred();
                        }
                        else if (dt < sbyte.MinValue)
                        {
                            dt = sbyte.MinValue;
                            parentSong.NotifyDacClipOccurred();
                        }

                        parentSong.DeferredWriteOPNA_DAC(vsifClient, dt);
                    }
                }

                QueryPerformanceCounter(out after);
                double nextTime = dbefore + ((double)freq / (sampleRate / (double)multiply));
                while (after < nextTime)
                    QueryPerformanceCounter(out after);
                dbefore = nextTime;
            }
        }

        //static DEVICE_START( segapcm )
        public int device_start_okim6295(byte ChipID, int clock, VsifClient vsifClient)
        {
            this.vsifClient = vsifClient;
            drive_clock = clock;
            sample_rate = device_start_okim6295(ChipID, clock);
            return sample_rate;
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

        public class adpcm_state
        {
            public int signal;

            public int step;
        }

        public class ADPCMVoice
        {
            public byte playing;

            public uint base_offset;

            public uint sample;

            public uint count;

            public adpcm_state adpcm = new adpcm_state();

            public uint volume;

            public byte Muted;
        }

        public class okim6295_state
        {
            public const int OKIM6295_VOICES = 4;

            public ADPCMVoice[] voice = new ADPCMVoice[4]
            {
                new ADPCMVoice(),
                new ADPCMVoice(),
                new ADPCMVoice(),
                new ADPCMVoice()
            };

            public int command;

            //public byte bank_installed;

            public int bank_offs;

            public byte pin7_state;

            public byte nmk_mode;

            public byte[] nmk_bank = new byte[4];

            public uint master_clock;

            public uint initial_clock;

            public uint ROMSize;

            //public int ptrROM;

            public byte[] ROM;

        }

        public class okim6295ChInfo
        {
            public bool mask;

            public int stAdr;

            public int edAdr;
        }

        public class okim6295Info
        {
            public uint masterClock;

            public byte pin7State;

            public byte[] nmkBank = new byte[4];

            public bool[] keyon = new bool[4];

            public okim6295ChInfo[] chInfo = new okim6295ChInfo[4]
            {
                new okim6295ChInfo(),
                new okim6295ChInfo(),
                new okim6295ChInfo(),
                new okim6295ChInfo()
            };
        }

        private const int MAX_SAMPLE_CHUNK = 16;

        private static int[] index_shift = new int[8] { -1, -1, -1, -1, 2, 4, 6, 8 };

        private static int[] diff_lookup = new int[784];

        private static int[] volume_table = new int[16]
        {
            32, 22, 16, 11, 8, 6, 4, 3, 2, 0,
            0, 0, 0, 0, 0, 0
        };

        private static int tables_computed = 0;

        public okim6295_state[] OKIM6295Data = new okim6295_state[2]
        {
            new okim6295_state(),
            new okim6295_state()
        };

        private const uint NMK_BNKTBLBITS = 8u;

        private const uint NMK_BNKTBLSIZE = 256u;

        private const uint NMK_TABLESIZE = 1024u;

        private const uint NMK_TABLEMASK = 1023u;

        private const uint NMK_BANKBITS = 16u;

        private const uint NMK_BANKSIZE = 65536u;

        private const uint NMK_BANKMASK = 65535u;

        private const uint NMK_ROMBASE = 262144u;

        private okim6295Info[] infos = new okim6295Info[2]
        {
            new okim6295Info(),
            new okim6295Info()
        };

        private okim6295Info retInfo = new okim6295Info();

        private void compute_tables()
        {
            int[][] array = new int[16][]
            {
                new int[4] { 1, 0, 0, 0 },
                new int[4] { 1, 0, 0, 1 },
                new int[4] { 1, 0, 1, 0 },
                new int[4] { 1, 0, 1, 1 },
                new int[4] { 1, 1, 0, 0 },
                new int[4] { 1, 1, 0, 1 },
                new int[4] { 1, 1, 1, 0 },
                new int[4] { 1, 1, 1, 1 },
                new int[4] { -1, 0, 0, 0 },
                new int[4] { -1, 0, 0, 1 },
                new int[4] { -1, 0, 1, 0 },
                new int[4] { -1, 0, 1, 1 },
                new int[4] { -1, 1, 0, 0 },
                new int[4] { -1, 1, 0, 1 },
                new int[4] { -1, 1, 1, 0 },
                new int[4] { -1, 1, 1, 1 }
            };
            for (int i = 0; i <= 48; i++)
            {
                int num = (int)Math.Floor(16.0 * Math.Pow(1.1, i));
                for (int j = 0; j < 16; j++)
                {
                    diff_lookup[i * 16 + j] = array[j][0] * (num * array[j][1] + num / 2 * array[j][2] + num / 4 * array[j][3] + num / 8);
                }
            }

            tables_computed = 1;
        }

        public void reset_adpcm(adpcm_state state)
        {
            if (tables_computed == 0)
            {
                compute_tables();
            }

            state.signal = -2;
            state.step = 0;
        }

        public short clock_adpcm(adpcm_state state, byte nibble)
        {
            state.signal += diff_lookup[state.step * 16 + (nibble & 0xF)];
            if (state.signal > 2047)
            {
                state.signal = 2047;
            }
            else if (state.signal < -2048)
            {
                state.signal = -2048;
            }

            state.step += index_shift[nibble & 7];
            if (state.step > 48)
            {
                state.step = 48;
            }
            else if (state.step < 0)
            {
                state.step = 0;
            }

            return (short)state.signal;
        }

        private static byte memory_raw_read_byte(okim6295_state chip, int offset)
        {
            int num;
            if (chip.nmk_mode == 0)
            {
                num = chip.bank_offs | offset;
            }
            else
            {
                byte b;
                if ((long)offset < 1024L && (chip.nmk_mode & 0x80u) != 0)
                {
                    b = (byte)(offset >> 8);
                    num = (int)((long)offset & 0x3FFL);
                }
                else
                {
                    b = (byte)(offset >> 16);
                    num = (int)((long)offset & 0xFFFFL);
                }

                num |= chip.nmk_bank[b & 3] << 16;
            }

            if (num < chip.ROMSize)
            {
                return chip.ROM[num];
            }

            return 0;
        }

        private void generate_adpcm(okim6295_state chip, ADPCMVoice voice, short[] buffer, int samples)
        {
            int num = 0;
            if (voice.playing != 0)
            {
                int base_offset = (int)voice.base_offset;
                int num2 = (int)voice.sample;
                int count = (int)voice.count;
                while (samples != 0)
                {
                    byte nibble = (byte)(memory_raw_read_byte(chip, base_offset + num2 / 2) >> (((num2 & 1) << 2) ^ 4));
                    buffer[num++] = (short)(clock_adpcm(voice.adpcm, nibble) * voice.volume / 2);
                    samples--;
                    if (++num2 >= count)
                    {
                        voice.playing = 0;
                        break;
                    }
                }

                voice.sample = (uint)num2;
            }

            while (samples-- != 0)
            {
                buffer[num++] = 0;
            }
        }

        private Object lockObject = new object();

        private void okim6295_update(byte ChipID, int[][] outputs, int samples)
        {
            okim6295_state okim6295_state = OKIM6295Data[ChipID];
            for (int i = 0; i < samples; i++)
            {
                outputs[0][i] = 0;
            }

            List<short[]> data = new List<short[]>();
            for (int i = 0; i < 4; i++)
            {
                data.Add(new short[samples]);

                ADPCMVoice aDPCMVoice = okim6295_state.voice[i];
                infos[ChipID].chInfo[i].mask = aDPCMVoice.Muted == 0;
                if (aDPCMVoice.Muted != 0)
                {
                    continue;
                }

                //int num = 0;
                short[] array = new short[16];
                for (int num2 = samples; num2 != 0; num2 -= samples)
                {
                    int num3 = ((num2 > 16) ? 16 : num2);
                    generate_adpcm(okim6295_state, aDPCMVoice, array, num3);
                    for (int j = 0; j < num3; j++)
                    {
                        data[i][j] = array[j];
                        //outputs[0][num++] += array[j];
                    }
                }
            }
            var mix = PcmMixer.Mix(data, PcmMixer.DacClipping);
            for (int i = 0; i < samples; i++)
                outputs[1][i] = outputs[0][i] = (int)Math.Round(mix[i]);
        }

        private int device_start_okim6295(byte ChipID, int clock)
        {
            if (ChipID >= 2)
            {
                return 0;
            }

            okim6295_state okim6295_state = OKIM6295Data[ChipID];
            compute_tables();
            okim6295_state.command = -1;
            okim6295_state.bank_offs = 0;
            okim6295_state.nmk_mode = 0;
            for (int i = 0; i < 4; i++)
            {
                okim6295_state.nmk_bank[i] = 0;
            }

            okim6295_state.initial_clock = (uint)clock;
            okim6295_state.master_clock = (uint)clock & 0x7FFFFFFFu;
            okim6295_state.pin7_state = (byte)((uint)(clock & int.MinValue) >> 31);
            infos[ChipID].masterClock = okim6295_state.master_clock;
            infos[ChipID].pin7State = okim6295_state.pin7_state;
            int num = ((okim6295_state.pin7_state != 0) ? 132 : 165);
            return (int)((long)okim6295_state.master_clock / (long)num);
        }

        private void device_stop_okim6295(byte ChipID)
        {
            okim6295_state obj = OKIM6295Data[ChipID];
            obj.ROM = null;
            obj.ROMSize = 0u;
        }

        private void device_reset_okim6295(byte ChipID)
        {
            okim6295_state okim6295_state = OKIM6295Data[ChipID];
            okim6295_state.command = -1;
            okim6295_state.bank_offs = 0;
            okim6295_state.nmk_mode = 0;
            for (int i = 0; i < 4; i++)
            {
                okim6295_state.nmk_bank[i] = 0;
            }

            okim6295_state.master_clock = okim6295_state.initial_clock & 0x7FFFFFFFu;
            infos[ChipID].masterClock = okim6295_state.master_clock;
            okim6295_state.pin7_state = (byte)((okim6295_state.initial_clock & 0x80000000u) >> 31);
            infos[ChipID].pin7State = (byte)((okim6295_state.initial_clock & 0x80000000u) >> 31);
            for (int j = 0; j < 4; j++)
            {
                okim6295_state.voice[j].volume = 0u;
                reset_adpcm(okim6295_state.voice[j].adpcm);
                okim6295_state.voice[j].playing = 0;
            }
        }

        private void okim6295_set_bank_base(okim6295_state info, int iBase)
        {
            info.bank_offs = iBase;
        }

        private void okim6295_clock_changed(okim6295_state info)
        {
            int num = ((info.pin7_state != 0) ? 132 : 165);
            sample_rate = (int)info.master_clock / num;
        }

        private void okim6295_set_pin7(okim6295_state info, int pin7, okim6295Info Info)
        {
            Info.pin7State = (byte)pin7;
            info.pin7_state = (byte)pin7;
            okim6295_clock_changed(info);
        }

        private byte okim6295_r(byte ChipID, int offset)
        {
            okim6295_state okim6295_state = OKIM6295Data[ChipID];
            int num = 240;
            for (int i = 0; i < 4; i++)
            {
                if (okim6295_state.voice[i].playing != 0)
                {
                    num |= 1 << i;
                }
            }

            return (byte)num;
        }

        private void okim6295_write_command(okim6295_state info, byte data, okim6295Info Info)
        {
            if (info.command != -1)
            {
                int num = data >> 4;
                int num2 = 0;
                while (num2 < 4)
                {
                    if (((uint)num & (true ? 1u : 0u)) != 0)
                    {
                        ADPCMVoice aDPCMVoice = info.voice[num2];
                        int num3 = info.command * 8;
                        int num4 = memory_raw_read_byte(info, num3) << 16;
                        num4 |= memory_raw_read_byte(info, num3 + 1) << 8;
                        num4 |= memory_raw_read_byte(info, num3 + 2);
                        num4 &= 0x3FFFF;
                        Info.chInfo[num2].stAdr = num4;
                        int num5 = memory_raw_read_byte(info, num3 + 3) << 16;
                        num5 |= memory_raw_read_byte(info, num3 + 4) << 8;
                        num5 |= memory_raw_read_byte(info, num3 + 5);
                        num5 &= 0x3FFFF;
                        Info.chInfo[num2].edAdr = num5;
                        if (num4 < num5)
                        {
                            if (aDPCMVoice.playing == 0)
                            {
                                aDPCMVoice.playing = 1;
                                aDPCMVoice.base_offset = (uint)num4;
                                aDPCMVoice.sample = 0u;
                                aDPCMVoice.count = (uint)(2 * (num5 - num4 + 1));
                                reset_adpcm(aDPCMVoice.adpcm);
                                aDPCMVoice.volume = (uint)volume_table[data & 0xF];
                                Info.keyon[num2] = true;
                            }
                        }
                        else
                        {
                            aDPCMVoice.playing = 0;
                        }
                    }

                    num2++;
                    num >>= 1;
                }

                info.command = -1;
                return;
            }

            if ((data & 0x80u) != 0)
            {
                info.command = data & 0x7F;
                return;
            }

            int num6 = data >> 3;
            int num7 = 0;
            while (num7 < 4)
            {
                if (((uint)num6 & (true ? 1u : 0u)) != 0)
                {
                    info.voice[num7].playing = 0;
                }

                num7++;
                num6 >>= 1;
            }
        }

        public void okim6295_w(byte ChipID, int offset, byte data)
        {
            lock(lockObject)
                okim6295_w_internal(ChipID, offset, data);
        }

        private void okim6295_w_internal(byte ChipID, int offset, byte data)
        {
            okim6295_state okim6295_state = OKIM6295Data[ChipID];
            switch (offset)
            {
                case 0:
                    okim6295_write_command(okim6295_state, data, infos[ChipID]);
                    break;
                case 8:
                    okim6295_state.master_clock &= 4294967040u;
                    okim6295_state.master_clock |= data;
                    infos[ChipID].masterClock = okim6295_state.master_clock;
                    break;
                case 9:
                    okim6295_state.master_clock &= 4294902015u;
                    okim6295_state.master_clock |= (uint)(data << 8);
                    infos[ChipID].masterClock = okim6295_state.master_clock;
                    break;
                case 10:
                    okim6295_state.master_clock &= 4278255615u;
                    okim6295_state.master_clock |= (uint)(data << 16);
                    infos[ChipID].masterClock = okim6295_state.master_clock;
                    break;
                case 11:
                    data = (byte)(data & 0x7Fu);
                    okim6295_state.master_clock &= 16777215u;
                    okim6295_state.master_clock |= (uint)(data << 24);
                    okim6295_clock_changed(okim6295_state);
                    infos[ChipID].masterClock = okim6295_state.master_clock;
                    break;
                case 12:
                    okim6295_set_pin7(okim6295_state, data, infos[ChipID]);
                    break;
                case 14:
                    okim6295_state.nmk_mode = data;
                    break;
                case 15:
                    okim6295_set_bank_base(okim6295_state, data << 18);
                    break;
                case 16:
                case 17:
                case 18:
                case 19:
                    okim6295_state.nmk_bank[offset & 3] = data;
                    infos[ChipID].nmkBank[offset & 3] = data;
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 13:
                    break;
            }
        }

        public void okim6295_write_rom(byte ChipID, int ROMSize, int DataStart, int DataLength, byte[] ROMData)
        {
            okim6295_state okim6295_state = OKIM6295Data[ChipID];
            if (okim6295_state.ROMSize != ROMSize)
            {
                okim6295_state.ROM = new byte[ROMSize];
                okim6295_state.ROMSize = (uint)ROMSize;
                for (int i = 0; i < ROMSize; i++)
                {
                    okim6295_state.ROM[i] = byte.MaxValue;
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
                    okim6295_state.ROM[j + DataStart] = ROMData[j];
                }
            }
        }

        public void okim6295_write_rom2(byte ChipID, int ROMSize, int DataStart, int DataLength, byte[] ROMData, uint srcStartAddr)
        {
            okim6295_state okim6295_state = OKIM6295Data[ChipID];
            if (okim6295_state.ROMSize != ROMSize)
            {
                okim6295_state.ROM = new byte[ROMSize];
                okim6295_state.ROMSize = (uint)ROMSize;
                for (int i = 0; i < ROMSize; i++)
                {
                    okim6295_state.ROM[i] = byte.MaxValue;
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
                    okim6295_state.ROM[j + DataStart] = ROMData[j + srcStartAddr];
                }
            }
        }

        public void okim6295_set_mute_mask(byte ChipID, uint MuteMask)
        {
            okim6295_state okim6295_state = OKIM6295Data[ChipID];
            for (byte b = 0; b < 4; b = (byte)(b + 1))
            {
                okim6295_state.voice[b].Muted = (byte)((MuteMask >> (int)b) & 1u);
            }
        }

    }
}

//https://github.com/kuma4649/MDPlayer
//kumatan

// license:BSD-3-Clause
// copyright-holders:Hiromitsu Shioya, Olivier Galibert
/*********************************************************/
/*    SEGA 16ch 8bit PCM                                 */
/*********************************************************/

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
    internal class SegaPcm : IDisposable
    {
        private bool disposedValue;

        private SongBase parentSong;

        public SegaPcm(SongBase parentSong)
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

        /// <summary>
        /// 
        /// </summary>
        public void StreamSong()
        {
            double sampleRate = 31250;
            int multiply = 1 + Settings.Default.SegaPcmRate;

            int[][] outputs = new int[2][];

            //int count = 0;
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
                SEGAPCM_update(0, outputs, multiply);

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
                    if (vsifClient.SoundModuleType == VsifSoundModuleType.TurboR_FTDI && vsifClient.Tag.ContainsKey("ProxySegaPcm"))
                    {
                        int dt = ((dtL + dtR) / 2) >> 7;

                        if (dt > sbyte.MaxValue)
                            dt = sbyte.MaxValue;
                        else if (dt < sbyte.MinValue)
                            dt = sbyte.MinValue;

                        parentSong.DeferredWriteTurboR_DAC(vsifClient, (byte)(dt + 128));
                    }
                    else if (vsifClient.ChipClockHz.ContainsKey("OPN2") && vsifClient.Tag.ContainsKey("ProxySegaPcm"))
                    {
                        int dt = ((dtL + dtR) / 2) >> 7;

                        if (dt > sbyte.MaxValue)
                            dt = sbyte.MaxValue;
                        else if (dt < sbyte.MinValue)
                            dt = sbyte.MinValue;

                        parentSong.DeferredWriteOPN2_DAC(vsifClient, (byte)(dt + 128));
                    }
                    else if (vsifClient.ChipClockHz.ContainsKey("OPNA") && vsifClient.Tag.ContainsKey("ProxySegaPcm"))
                    {
                        int dt = ((dtL + dtR) / 2) >> 7;

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
                dbefore = nextTime;
            }
        }

        //static DEVICE_START( segapcm )
        public int device_start_segapcm(byte ChipID, int clock, int intf_bank, VsifClient vsifClient)
        {
            this.vsifClient = vsifClient;
            this.drive_clock = clock;
            return device_start_segapcm(ChipID, clock, intf_bank);
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


        public class sega_pcm_interface
        {
            public int bank;
        }

        public class segapcm_state
        {
            public byte[] ram;

            public int ptrRam;

            public byte[] low = new byte[16];

            public uint ROMSize;

            public byte[] rom;

            public int ptrRom;

            public int bankshift;

            public int bankmask;

            public int rgnmask;

            public sega_pcm_interface intf = new sega_pcm_interface();

            public byte[] Muted = new byte[16];
        }

        public const int BANK_256 = 11;

        public const int BANK_512 = 12;

        public const int BANK_12M = 13;

        public const int BANK_MASK7 = 7340032;

        public const int BANK_MASKF = 15728640;

        public const int BANK_MASKF8 = 16252928;

        public segapcm_state[] SPCMData = new segapcm_state[2]
        {
            new segapcm_state(),
            new segapcm_state()
        };

        // reg      function
        // ------------------------------------------------
        // 0x00     ?
        // 0x01     ?
        // 0x02     volume left
        // 0x03     volume right
        // 0x04     loop address (08-15)
        // 0x05     loop address (16-23)
        // 0x06     end address
        // 0x07     address delta
        // 0x80     ?
        // 0x81     ?
        // 0x82     ?
        // 0x83     ?
        // 0x84     current address (08-15), 00-07 is internal?
        // 0x85     current address (16-23)
        // 0x86     bit 0: channel disable?
        //          bit 1: loop disable
        //          other bits: bank
        // 0x87     ?

        public void SEGAPCM_update(byte ChipID, int[][] outputs, int samples)
        {
            segapcm_state segapcm_state = SPCMData[ChipID];
            int rgnmask = segapcm_state.rgnmask;
            for (int i = 0; i < samples; i++)
            {
                outputs[0][i] = 0;
                outputs[1][i] = 0;
            }

            for (int j = 0; j < 16; j++)
            {
                int num = segapcm_state.ptrRam + 8 * j;
                if (((uint)segapcm_state.ram[num + 0x86] & (true ? 1u : 0u)) != 0 || segapcm_state.Muted[j] != 0)
                {
                    continue;
                }

                int num2 = segapcm_state.ptrRom + ((segapcm_state.ram[num + 0x86] & segapcm_state.bankmask) << segapcm_state.bankshift);
                uint num3 = (uint)((segapcm_state.ram[num + 0x85] << 16) | (segapcm_state.ram[num + 0x84] << 8) | segapcm_state.low[j]);
                uint num4 = (uint)((segapcm_state.ram[num + 5] << 16) | (segapcm_state.ram[num + 4] << 8));
                byte b = (byte)(segapcm_state.ram[num + 6] + 1);
                for (int k = 0; k < samples; k++)
                {
                    sbyte b2 = 0;
                    if (num3 >> 16 == b)
                    {
                        if ((segapcm_state.ram[num + 0x86] & 2u) != 0)
                        {
                            segapcm_state.ram[num + 0x86] |= 1;
                            break;
                        }

                        num3 = num4;
                    }

                    if (num2 + ((num3 >> 8) & rgnmask) < segapcm_state.rom.Length)
                    {
                        b2 = (sbyte)(segapcm_state.rom[num2 + ((num3 >> 8) & rgnmask)] - 128);
                    }

                    outputs[0][k] += b2 * (segapcm_state.ram[num + 2] & 0x7F);
                    outputs[1][k] += b2 * (segapcm_state.ram[num + 3] & 0x7F);
                    num3 = (num3 + segapcm_state.ram[num + 7]) & 0xFFFFFFu;
                    //Debug.WriteLine($"ch {j} dat {b2}");
                }

                segapcm_state.ram[num + 0x84] = (byte)(num3 >> 8);
                segapcm_state.ram[num + 0x85] = (byte)(num3 >> 16);
                segapcm_state.low[j] = (byte)(((segapcm_state.ram[num + 0x86] & 1) == 0) ? num3 : 0u);
            }
        }

        public int device_start_segapcm(byte ChipID, int clock, int intf_bank)
        {
            uint num = 524288u;
            if (ChipID >= 2)
            {
                return 0;
            }

            segapcm_state segapcm_state = SPCMData[ChipID];
            sega_pcm_interface intf = segapcm_state.intf;
            intf.bank = intf_bank;
            segapcm_state.ROMSize = num;
            segapcm_state.rom = new byte[num];
            segapcm_state.ptrRom = 0;
            segapcm_state.ram = new byte[2048];
            for (int i = 0; i < num; i++)
            {
                segapcm_state.rom[i] = 128;
            }

            segapcm_state.bankshift = (byte)intf.bank;
            int num2 = intf.bank >> 16;
            if (num2 == 0)
            {
                num2 = 112;
            }

            int num3 = (int)num;
            segapcm_state.rgnmask = num3 - 1;
            int num4;
            for (num4 = 1; num4 < num3; num4 *= 2)
            {
            }

            num4--;
            segapcm_state.bankmask = num2 & (num4 >> segapcm_state.bankshift);
            for (num2 = 0; num2 < 16; num2++)
            {
                segapcm_state.Muted[num2] = 0;
            }

            return clock / 128;
        }

        public void device_stop_segapcm(byte ChipID)
        {
            segapcm_state obj = SPCMData[ChipID];
            obj.rom = null;
            obj.ram = null;
        }

        public void device_reset_segapcm(byte ChipID)
        {
            segapcm_state segapcm_state = SPCMData[ChipID];
            for (int i = 0; i < 2048; i++)
            {
                segapcm_state.ram[i] = byte.MaxValue;
            }
        }


        // reg      function
        // ------------------------------------------------
        // 0x00     ?
        // 0x01     ?
        // 0x02     volume left
        // 0x03     volume right
        // 0x04     loop address (08-15)
        // 0x05     loop address (16-23)
        // 0x06     end address
        // 0x07     address delta
        // 0x80     ?
        // 0x81     ?
        // 0x82     ?
        // 0x83     ?
        // 0x84     current address (08-15), 00-07 is internal?
        // 0x85     current address (16-23)
        // 0x86     bit 0: channel disable?
        //          bit 1: loop disable
        //          other bits: bank
        // 0x87     ?

        public void sega_pcm_w(byte ChipID, int offset, byte data)
        {
            if (SPCMData != null && SPCMData.Length >= ChipID + 1)
            {
                segapcm_state segapcm_state = SPCMData[ChipID];
                if (segapcm_state.ram != null)
                {
                    segapcm_state.ram[offset & 0x7FF] = data;

#if DEBUG
                    /*
                    if ((offset & 0x80) == 0x80)
                    {
                        Console.WriteLine("SEGAPCM W: " + ((offset / 8) & 0xf) + "ch, " + (0x80 + (offset & 7)).ToString("X2") + ", " + data.ToString("X2"));
                    }
                    else
                    {
                        Console.WriteLine("SEGAPCM W: " + ((offset / 8) & 0xf) + "ch, " + (offset & 7).ToString("X2") + ", " + data.ToString("X2"));
                    }
                    */
#endif
                }
            }
        }

        public byte sega_pcm_r(byte ChipID, int offset)
        {
            return SPCMData[ChipID].ram[offset & 0x7FF];
        }

        public void sega_pcm_write_rom(byte ChipID, int ROMSize, int DataStart, int DataLength, byte[] ROMData)
        {
            segapcm_state segapcm_state = SPCMData[ChipID];
            if (segapcm_state.ROMSize != ROMSize)
            {
                segapcm_state.rom = new byte[ROMSize];
                segapcm_state.ROMSize = (uint)ROMSize;
                for (int i = 0; i < ROMSize; i++)
                {
                    segapcm_state.rom[i] = 128;
                }

                ulong num = (ulong)(segapcm_state.intf.bank >> 16);
                if (num == 0L)
                {
                    num = 112uL;
                }

                ulong num2;
                for (num2 = 1uL; num2 < (ulong)ROMSize; num2 *= 2)
                {
                }

                num2--;
                segapcm_state.rgnmask = (int)num2;
                segapcm_state.bankmask = (int)(num & (num2 >> segapcm_state.bankshift));
            }

            if (DataStart <= ROMSize)
            {
                if (DataStart + DataLength > ROMSize)
                {
                    DataLength = ROMSize - DataStart;
                }

                for (int j = 0; j < DataLength; j++)
                {
                    segapcm_state.rom[j + DataStart] = ROMData[j];
                }
            }
        }

        public void sega_pcm_write_rom2(byte ChipID, int ROMSize, int DataStart, int DataLength, byte[] ROMData, uint SrcStartAdr)
        {
            segapcm_state segapcm_state = SPCMData[ChipID];
            if (segapcm_state.ROMSize != ROMSize)
            {
                segapcm_state.rom = new byte[ROMSize];
                segapcm_state.ROMSize = (uint)ROMSize;
                for (int i = 0; i < ROMSize; i++)
                {
                    segapcm_state.rom[i] = 128;
                }

                ulong num = (ulong)(segapcm_state.intf.bank >> 16);
                if (num == 0L)
                {
                    num = 112uL;
                }

                ulong num2;
                for (num2 = 1uL; num2 < (ulong)ROMSize; num2 *= 2)
                {
                }

                num2--;
                segapcm_state.rgnmask = (int)num2;
                segapcm_state.bankmask = (int)(num & (num2 >> segapcm_state.bankshift));
            }

            if (DataStart <= ROMSize)
            {
                if (DataStart + DataLength > ROMSize)
                {
                    DataLength = ROMSize - DataStart;
                }

                for (int j = 0; j < DataLength; j++)
                {
                    segapcm_state.rom[j + DataStart] = ROMData[j + SrcStartAdr];
                }
            }
        }

        public void segapcm_set_mute_mask(byte ChipID, uint MuteMask)
        {
            segapcm_state segapcm_state = SPCMData[ChipID];
            for (byte b = 0; b < 16; b = (byte)(b + 1))
            {
                segapcm_state.Muted[b] = (byte)((MuteMask >> (int)b) & 1u);
            }
        }


    }
}

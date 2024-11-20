using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using zanac.VGMPlayer;
using zanac.VGMPlayer.Properties;
using static System.Windows.Forms.AxHost;

namespace zanac.VGMPlayer
{
    // BSD 3-Clause License
    //
    // Copyright (c) 2021, Aaron Giles
    // All rights reserved.
    //
    // Redistribution and use in source and binary forms, with or without
    // modification, are permitted provided that the following conditions are met:
    //
    // 1. Redistributions of source code must retain the above copyright notice, this
    //    list of conditions and the following disclaimer.
    //
    // 2. Redistributions in binary form must reproduce the above copyright notice,
    //    this list of conditions and the following disclaimer in the documentation
    //    and/or other materials provided with the distribution.
    //
    // 3. Neither the name of the copyright holder nor the names of its
    //    contributors may be used to endorse or promote products derived from
    //    this software without specific prior written permission.
    //
    // THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
    // AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
    // IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
    // DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
    // FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
    // DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
    // SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
    // CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
    // OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
    // OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

    internal class OpnbPcm : IDisposable
    {
        private bool disposedValue;

        private SongBase parentSong;

        public adpcm_a_engine adpcm_a_engine
        {
            get;
            private set;
        }
        public adpcm_b_engine adpcm_b_engine
        {
            get;
            private set;
        }

        private ProxyOPNType opnType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentSong"></param>
        /// <param name="opnType"></param>
        public OpnbPcm(SongBase parentSong, ProxyOPNType opnType)
        {
            var inf = new ymfm_interface();
            adpcm_a_engine = new adpcm_a_engine(inf, 8);
            adpcm_b_engine = new adpcm_b_engine(inf, 8);

            this.opnType = opnType;
            this.parentSong = parentSong;
        }

        private VsifClient vsifClient;

        private int drive_clock;

        private int sample_rate;

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool QueryPerformanceFrequency(out long frequency);

        private Object lockObject = new object();

        /// <summary>
        /// 
        /// </summary>
        public void StreamSong()
        {
            int multiply = 1 + Program.Default.OPNBRate;

            long freq, before, after;
            double dbefore;
            QueryPerformanceFrequency(out freq);
            QueryPerformanceCounter(out before);
            dbefore = before;
            uint clock = 0;

            List<ymfm_output> pcmouts = new List<ymfm_output>();
            var po_pcm = new ymfm_output();
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

                {
                    po_pcm = new ymfm_output();
                    var po_pcmb = new ymfm_output();
                    var po_pcma = new ymfm_output();
                    lock (lockObject)
                    {
                        adpcm_b_engine.clock();
                        adpcm_b_engine.output(po_pcmb, 0, 2);
                        if (clock % 3 == 0)
                            adpcm_a_engine.clock(0xff);
                        adpcm_a_engine.output(po_pcma, 0xff, 2);
                    }
                    po_pcm.data[0] = (int)Math.Round(PcmMixer.Mix(new short[] { (short)po_pcma.data[0], (short)po_pcmb.data[0] }, Program.Default.DACClipping));
                    po_pcm.data[1] = (int)Math.Round(PcmMixer.Mix(new short[] { (short)po_pcma.data[1], (short)po_pcmb.data[1] }, Program.Default.DACClipping));
                    pcmouts.Add(po_pcm);
                }

                if (clock % multiply == 0)
                {
                    int dtL = 0;
                    int dtR = 0;
                    for (int i = 0; i < pcmouts.Count; i++)
                    {
                        dtL += pcmouts[i].data[0];
                        dtR += pcmouts[i].data[1];
                    }
                    dtL /= pcmouts.Count;
                    dtR /= pcmouts.Count;

                    pcmouts.Clear();

                    if (vsifClient != null)
                    {
                        if (vsifClient.Tag.ContainsKey("ProxyOPNB_ADPCM"))
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

                            switch(opnType)
                            {
                                case ProxyOPNType.OPNA:
                                    {
                                        if (vsifClient.SoundModuleType == VsifSoundModuleType.TurboR_FTDI)
                                            parentSong.DeferredWriteTurboR_DAC(vsifClient, (byte)(dt + 128));
                                        else
                                            parentSong.DeferredWriteOPNA_PseudoDAC(vsifClient, (byte)(dt + 128));
                                    }
                                    break;
                                case ProxyOPNType.OPN2:
                                    {
                                        if (vsifClient.SoundModuleType == VsifSoundModuleType.TurboR_FTDI)
                                            parentSong.DeferredWriteTurboR_DAC(vsifClient, (byte)(dt + 128));
                                        else
                                            parentSong.DeferredWriteOPN2_DAC(vsifClient, (byte)(dt + 128));
                                    }
                                    break;
                            }
                        }
                    }

                    QueryPerformanceCounter(out after);
                    double nextTime = dbefore + ((double)freq / (sampleRate / (double)multiply));
                    while (after < nextTime)
                        QueryPerformanceCounter(out after);
                    dbefore = nextTime;
                }
                clock++;
            }
        }

        public void WritRegisterADPCM_A(byte regnum, byte data)
        {
            lock (lockObject)
                this.adpcm_a_engine.write(regnum, data);
        }

        public void WritRegisterADPCM_B(byte regnum, byte data)
        {
            lock (lockObject)
                this.adpcm_b_engine.write(regnum, data);
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

        //static DEVICE_START( opnb )
        public int device_start_opnb(byte ChipID, int clock, VsifClient vsifClient)
        {
            this.vsifClient = vsifClient;
            drive_clock = clock;
            sample_rate = clock / 12 / 6 / 2;
            return sample_rate;
        }
    }

    public class adpcm_a_registers
    {
        // constants
        public const uint OUTPUTS = 2;
        public const uint CHANNELS = 6;
        public const uint REGISTERS = 0x30;
        public static uint ALL_CHANNELS = (uint)(1 << (int)CHANNELS - 1);

        // constructor
        public adpcm_a_registers()
        {
        }

        // reset to initial state
        public void reset()
        {
            for (int i = 0; i < CHANNELS; i++)
                m_regdata[i] = 0;

            // initialize the pans to on by default, and max instrument volume;
            // some neogeo homebrews (for example ffeast) rely on this
            m_regdata[0x08] = m_regdata[0x09] = m_regdata[0x0a] =
            m_regdata[0x0b] = m_regdata[0x0c] = m_regdata[0x0d] = 0xdf;
        }

        // map channel number to register offset
        public static uint channel_offset(uint chnum)
        {
            return chnum;
        }

        // direct read/write access
        public void write(uint index, byte data)
        {
            m_regdata[index] = data;
        }

        uint bitfield(uint value, int start, int length = 1)
        {
            return (uint)((value >> start) & ((1 << length) - 1));
        }

        //// system-wide registers
        public uint dump()
        {
            return bitfield(m_regdata[0x00], 7);
        }
        public uint dump_mask()
        {
            return bitfield(m_regdata[0x00], 0, 6);
        }

        public uint total_level()
        {
            return bitfield(m_regdata[0x01], 0, 6);
        }
        public uint test()
        {
            return m_regdata[0x02];
        }

        //// per-channel registers
        public uint ch_pan_left(uint choffs)
        {
            return bitfield(m_regdata[choffs + 0x08], 7);
        }
        public uint ch_pan_right(uint choffs)
        {
            return bitfield(m_regdata[choffs + 0x08], 6);
        }
        public uint ch_instrument_level(uint choffs)
        {
            return bitfield(m_regdata[choffs + 0x08], 0, 5);
        }
        public uint ch_start(uint choffs)
        {
            return (uint)(m_regdata[choffs + 0x10] | (m_regdata[choffs + 0x18] << 8));
        }
        public uint ch_end(uint choffs)
        {
            return (uint)(m_regdata[choffs + 0x20] | (m_regdata[choffs + 0x28] << 8));
        }

        // per-channel writes
        public void write_start(uint choffs, uint address)
        {
            write(choffs + 0x10, (byte)address);
            write(choffs + 0x18, (byte)(address >> 8));
        }

        public void write_end(uint choffs, uint address)
        {
            write(choffs + 0x20, (byte)address);
            write(choffs + 0x28, (byte)(address >> 8));
        }

        // internal state
        private byte[] m_regdata = new byte[REGISTERS];         // register data
    }

    public enum access_class
    {
        ACCESS_IO = 0,
        ACCESS_ADPCM_A,
        ACCESS_ADPCM_B,
        ACCESS_PCM,
        ACCESS_CLASSES
    };

    public class adpcm_a_channel
    {
        // constructor
        public adpcm_a_channel(adpcm_a_engine owner, uint choffs, uint addrshift)
        {
            m_choffs = choffs;
            m_address_shift = addrshift;
            m_regs = owner.regs();
            m_owner = owner;
        }

        // reset the channel state
        public void reset()
        {
            m_playing = false;
            m_curnibble = 0;
            m_curbyte = 0;
            m_curaddress = 0;
            m_accumulator = 0;
            m_step_index = 0;
        }

        // signal key on/off
        public void keyonoff(bool on)
        {
            // QUESTION: repeated key ons restart the sample?
            m_playing = on;
            if (m_playing)
            {
                m_curaddress = m_regs.ch_start(m_choffs) << (int)m_address_shift;
                m_curnibble = 0;
                m_curbyte = 0;
                m_accumulator = 0;
                m_step_index = 0;

                //// don't log masked channels
                //if (((debug::GLOBAL_ADPCM_A_CHANNEL_MASK >> m_choffs) & 1) != 0)
                //    debug::log_keyon("KeyOn ADPCM-A%d: pan=%d%d start=%04X end=%04X level=%02X\n",
                //        m_choffs,
                //        m_regs.ch_pan_left(m_choffs),
                //        m_regs.ch_pan_right(m_choffs),
                //        m_regs.ch_start(m_choffs),
                //        m_regs.ch_end(m_choffs),
                //        m_regs.ch_instrument_level(m_choffs));
            }
        }

        uint bitfield(uint value, int start, int length = 1)
        {
            return (uint)((value >> start) & ((1 << length) - 1));
        }

        int clamp(int value, int minval, int maxval)
        {
            if (value < minval)
                return minval;
            if (value > maxval)
                return maxval;
            return value;
        }

        // master clockingfunction
        public bool clock()
        {
            // if not playing, just output 0
            if (!m_playing)
            {
                m_accumulator = 0;
                return false;
            }

            // if we're about to read nibble 0, fetch the data
            byte data;
            if (m_curnibble == 0)
            {
                // stop when we hit the end address; apparently only low 20 bits are used for
                // comparison on the YM2610: this affects sample playback in some games, for
                // example twinspri character select screen music will skip some samples if
                // this is not correct
                //
                // note also: end address is inclusive, so wait until we are about to fetch
                // the sample just after the end before stopping; this is needed for nitd's
                // jump sound, for example
                int end = (int)((m_regs.ch_end(m_choffs) + 1) << (int)m_address_shift);
                if (((m_curaddress ^ end) & 0xfffff) == 0)
                {
                    m_playing = false;
                    m_accumulator = 0;
                    return true;
                }

                m_curbyte = m_owner.intf().ymfm_external_read(access_class.ACCESS_ADPCM_A, m_curaddress++);
                data = (byte)(m_curbyte >> 4);
                m_curnibble = 1;
            }

            // otherwise just extract from the previosuly-fetched byte
            else
            {
                data = (byte)(m_curbyte & 0xf);
                m_curnibble = 0;
            }

            // compute the ADPCM delta
            ushort[] s_steps =
            {
                 16,  17,   19,   21,   23,   25,   28,
                 31,  34,   37,   41,   45,   50,   55,
                 60,  66,   73,   80,   88,   97,  107,
                118, 130,  143,  157,  173,  190,  209,
                230, 253,  279,  307,  337,  371,  408,
                449, 494,  544,  598,  658,  724,  796,
                876, 963, 1060, 1166, 1282, 1411, 1552
            };
            int delta = (int)((2 * bitfield(data, 0, 3) + 1) * s_steps[m_step_index] / 8);
            if (bitfield(data, 3) != 0)
                delta = -delta;

            // the 12-bit accumulator wraps on the ym2610 and ym2608 (like the msm5205)
            m_accumulator = (m_accumulator + delta) & 0xfff;

            // adjust ADPCM step
            sbyte[] s_step_inc = { -1, -1, -1, -1, 2, 5, 7, 9 };
            m_step_index = clamp(m_step_index + s_step_inc[bitfield(data, 0, 3)], 0, 48);

            return false;
        }

        // return the computed output value, with panning applied
        //template<int NumOutputs>
        public void output(ymfm_output output, int NumOutputs)
        {
            // volume combines instrument and total levels
            int vol = (int)((m_regs.ch_instrument_level(m_choffs) ^ 0x1f) + (m_regs.total_level() ^ 0x3f));

            // if combined is maximum, don't add to outputs
            if (vol >= 63)
                return;

            // convert into a shift and a multiplier
            // QUESTION: verify this from other sources
            sbyte mul = (sbyte)(15 - (vol & 7));
            byte shift = (byte)(4 + 1 + (vol >> 3));

            // m_accumulator is a 12-bit value; shift up to sign-extend;
            // the downshift is incorporated into 'shift'
            short value = (short)((((short)(m_accumulator << 4) * mul) >> shift) & ~3);

            // apply to left/right as appropriate
            if (NumOutputs == 1 || m_regs.ch_pan_left(m_choffs) != 0)
                output.data[0] += value;
            if (NumOutputs > 1 && m_regs.ch_pan_right(m_choffs) != 0)
                output.data[1] += value;
        }

        // internal state
        private uint m_choffs;              // channel offset
        private uint m_address_shift;       // address bits shift-left
        private bool m_playing;                   // currently playing?
        private uint m_curnibble;                 // index of the current nibble
        private uint m_curbyte;                   // current byte of data
        private uint m_curaddress;                // current address
        private int m_accumulator;                // accumulator
        private int m_step_index;                 // index in the stepping table
        private adpcm_a_registers m_regs;            // reference to registers
        private adpcm_a_engine m_owner;              // reference to our owner
    };


    public class adpcm_a_engine
    {
        public const uint CHANNELS = adpcm_a_registers.CHANNELS;

        // constructor
        public adpcm_a_engine(ymfm_interface intf, uint addrshift)
        {
            this.m_intf = intf;
            // create the channels
            for (int chnum = 0; chnum < CHANNELS; chnum++)
                m_channel[chnum] = new adpcm_a_channel(this, (uint)chnum, addrshift);
        }

        // reset our status
        public void reset()
        {
            // reset register state
            m_regs.reset();

            // reset each channel
            foreach (var chan in m_channel)
                chan.reset();
        }

        uint bitfield(uint value, int start, int length = 1)
        {
            return (uint)((value >> start) & ((1 << length) - 1));
        }

        // master clocking function
        public uint clock(uint chanmask)
        {
            // clock each channel, setting a bit in result if it finished
            uint result = 0;
            for (int chnum = 0; chnum < CHANNELS; chnum++)
            {
                if (bitfield(chanmask, chnum) != 0)
                {
                    if (m_channel[chnum].clock())
                        result |= (uint)(1 << chnum);
                }
            }

            // return the bitmask of completed samples
            return result;
        }

        // compute sum of channel outputs
        //template<int NumOutputs>
        public void output(ymfm_output output, uint chanmask, int NumOutputs)
        {
            const uint GLOBAL_ADPCM_A_CHANNEL_MASK = 0xffffffff;

            // mask out some channels for debug purposes
            chanmask &= GLOBAL_ADPCM_A_CHANNEL_MASK;

            // compute the output of each channel
            List<short> left = new List<short>();
            List<short> right = new List<short>();
            for (int chnum = 0; chnum < CHANNELS; chnum++)
            {
                if (bitfield(chanmask, chnum) != 0)
                {
                    //m_channel[chnum].output(output, ymfm_output.NumOutputs);

                    ymfm_output a = new ymfm_output();
                    m_channel[chnum].output(a, ymfm_output.NumOutputs);
                    left.Add((short)(a.data[0]));
                    right.Add((short)(a.data[1]));
                }
            }
            output.data[0] = (int)PcmMixer.Mix(left, Program.Default.DACClipping);
            output.data[1] = (int)PcmMixer.Mix(right, Program.Default.DACClipping);
        }

        // write to the ADPCM-A registers
        public void write(uint regnum, byte data)
        {
            // store the raw value to the register array;
            // most writes are passive, consumed only when needed
            m_regs.write(regnum, data);

            // actively handle writes to the control register
            if (regnum == 0x00)
            {
                for (int chnum = 0; chnum < CHANNELS; chnum++)
                {
                    if (bitfield(data, chnum) != 0)
                        m_channel[chnum].keyonoff(bitfield((uint)~data, 7) != 0);
                }
            }
        }

        // set the start/end address for a channel (for hardcoded YM2608 percussion)
        public void set_start_end(byte chnum, ushort start, ushort end)
        {
            uint choffs = adpcm_a_registers.channel_offset(chnum);
            m_regs.write_start(choffs, start);
            m_regs.write_end(choffs, end);
        }

        // return a reference to our interface
        public ymfm_interface intf()
        {
            return m_intf;
        }

        // return a reference to our registers
        public adpcm_a_registers regs()
        {
            return m_regs;
        }

        // internal state
        private ymfm_interface m_intf;                                 // reference to the interface
        private adpcm_a_channel[] m_channel = new adpcm_a_channel[CHANNELS]; // array of channels
        private adpcm_a_registers m_regs = new adpcm_a_registers();                             // registers
    };

    public class adpcm_b_registers
    {
        // constants
        public const uint REGISTERS = 0x11;

        // constructor
        public adpcm_b_registers() { }

        // reset to initial state
        public void reset()
        {
            for (int i = 0; i < REGISTERS; i++)
                m_regdata[i] = 0;

            // default limit to wide open
            m_regdata[0x0c] = m_regdata[0x0d] = 0xff;
        }

        // direct read/write access
        public void write(uint index, byte data)
        {
            m_regdata[index] = data;
        }

        uint bitfield(uint value, int start, int length = 1)
        {
            return (uint)((value >> start) & ((1 << length) - 1));
        }

        // system-wide registers
        public uint execute() { return bitfield(m_regdata[0x00], 7); }
        public uint record()
        {
            return 0;
            //return bitfield(m_regdata[0x00], 6);
        }
        public uint external()
        {
            return 1;
            //return bitfield(m_regdata[0x00], 5);
        }
        public uint repeat() { return bitfield(m_regdata[0x00], 4); }
        public uint speaker() { return bitfield(m_regdata[0x00], 3); }
        public uint resetflag() { return bitfield(m_regdata[0x00], 0); }
        public uint pan_left() { return bitfield(m_regdata[0x01], 7); }
        public uint pan_right() { return bitfield(m_regdata[0x01], 6); }
        public uint start_conversion() { return bitfield(m_regdata[0x01], 3); }
        public uint dac_enable() { return bitfield(m_regdata[0x01], 2); }
        public uint dram_8bit() { return bitfield(m_regdata[0x01], 1); }
        public uint rom_ram() { return bitfield(m_regdata[0x01], 0); }
        public uint start() { return (uint)(m_regdata[0x02] | (m_regdata[0x03] << 8)); }
        public uint end() { return (uint)(m_regdata[0x04] | (m_regdata[0x05] << 8)); }
        public uint prescale() { return m_regdata[0x06] | (bitfield(m_regdata[0x07], 0, 3) << 8); }
        public uint cpudata() { return m_regdata[0x08]; }
        public uint delta_n() { return (uint)(m_regdata[0x09] | (m_regdata[0x0a] << 8)); }
        public uint level() { return m_regdata[0x0b]; }
        public uint limit() { return (uint)(m_regdata[0x0c] | (m_regdata[0x0d] << 8)); }
        public uint dac() { return m_regdata[0x0e]; }
        public uint pcm() { return m_regdata[0x0f]; }

        // internal state
        private byte[] m_regdata = new byte[adpcm_b_registers.REGISTERS];         // register data
    };


    public class adpcm_b_channel
    {
        const uint GLOBAL_ADPCM_B_CHANNEL_MASK = 0xffffffff;

        public const int STEP_MIN = 127;
        public const int STEP_MAX = 24576;

        public const byte STATUS_EOS = 0x01;
        public const byte STATUS_BRDY = 0x02;
        public const byte STATUS_PLAYING = 0x04;

        // constructor
        public adpcm_b_channel(adpcm_b_engine owner, uint addrshift)
        {
            m_address_shift = addrshift;
            m_status = STATUS_BRDY;
            m_adpcm_step = STEP_MIN;
            m_regs = owner.regs();
            m_owner = owner;
        }

        // reset the channel state
        public void reset()
        {
            m_status = STATUS_BRDY;
            m_curnibble = 0;
            m_curbyte = 0;
            m_dummy_read = 0;
            m_position = 0;
            m_curaddress = 0;
            m_accumulator = 0;
            m_prev_accum = 0;
            m_adpcm_step = STEP_MIN;
        }

        // signal key on/off
        public void keyonoff(bool on) { }

        uint bitfield(uint value, int start, int length = 1)
        {
            return (uint)((value >> start) & ((1 << length) - 1));
        }

        int clamp(int value, int minval, int maxval)
        {
            if (value < minval)
                return minval;
            if (value > maxval)
                return maxval;
            return value;
        }

        // master clocking function
        public void clock()
        {
            // only process if active and not recording (which we don't support)
            if (m_regs.execute() == 0 || m_regs.record() != 0 || (m_status & STATUS_PLAYING) == 0)
            {
                unchecked
                {
                    m_status &= (uint)~STATUS_PLAYING;
                }
                return;
            }

            // otherwise, advance the step
            int position = (int)(m_position + m_regs.delta_n());
            m_position = (ushort)position;
            if (position < 0x10000)
                return;

            // if we're about to process nibble 0, fetch sample
            if (m_curnibble == 0)
            {
                // playing from RAM/ROM
                if (m_regs.external() != 0)
                    m_curbyte = m_owner.intf().ymfm_external_read(access_class.ACCESS_ADPCM_B, m_curaddress);
            }

            // extract the nibble from our current byte
            byte data = (byte)(((byte)(m_curbyte << (int)(4 * m_curnibble))) >> 4);
            m_curnibble ^= 1;

            // we just processed the last nibble
            if (m_curnibble == 0)
            {
                // if playing from RAM/ROM, check the end/limit address or advance
                if (m_regs.external() != 0)
                {
                    // handle the sample end, either repeating or stopping
                    if (at_end())
                    {
                        // if repeating, go back to the start
                        if (m_regs.repeat() != 0)
                            load_start();

                        // otherwise, done; set the EOS bit
                        else
                        {
                            m_accumulator = 0;
                            m_prev_accum = 0;
                            m_status = (uint)((m_status & ~STATUS_PLAYING) | STATUS_EOS);
                            //debug::log_keyon("%s\n", "ADPCM EOS");
                            return;
                        }
                    }
                    /*
                    // wrap at the limit address
                    else if (at_limit())
                        m_curaddress = 0;
                    */
                    // otherwise, advance the current address
                    else
                    {
                        m_curaddress++;
                        m_curaddress &= 0xffffff;
                    }
                }

                // if CPU-driven, copy the next byte and request more
                else
                {
                    m_curbyte = m_regs.cpudata();
                    m_status |= STATUS_BRDY;
                }
            }

            // remember previous value for interpolation
            m_prev_accum = m_accumulator;

            // forecast to next forecast: 1/8, 3/8, 5/8, 7/8, 9/8, 11/8, 13/8, 15/8
            int delta = (int)((2 * bitfield(data, 0, 3) + 1) * m_adpcm_step / 8);
            if (bitfield(data, 3) != 0)
                delta = -delta;

            // add and clamp to 16 bits
            m_accumulator = clamp(m_accumulator + delta, -32768, 32767);

            // scale the ADPCM step: 0.9, 0.9, 0.9, 0.9, 1.2, 1.6, 2.0, 2.4
            byte[] s_step_scale = { 57, 57, 57, 57, 77, 102, 128, 153 };
            m_adpcm_step = clamp((m_adpcm_step * s_step_scale[bitfield(data, 0, 3)]) / 64, STEP_MIN, STEP_MAX);
        }

        // return the computed output value, with panning applied
        //template<int NumOutputs>
        public void output(ymfm_output output, uint rshift, int NumOutputs)
        {
            // mask out some channels for debug purposes
            //if ((GLOBAL_ADPCM_B_CHANNEL_MASK & 1) == 0)
            //    return;

            // do a linear interpolation between samples
            int result = (m_prev_accum * (int)((m_position ^ 0xffff) + 1) + m_accumulator * (int)(m_position)) >> 16;

            // apply volume (level) in a linear fashion and reduce
            result = (int)((result * (int)(m_regs.level())) >> (int)(8 + rshift));

            // apply to left/right
            if (NumOutputs == 1 || m_regs.pan_left() != 0)
                output.data[0] += result;
            if (NumOutputs > 1 && m_regs.pan_right() != 0)
                output.data[1] += result;
        }

        // return the status register
        public byte status() { return (byte)m_status; }

        // handle special register reads
        public byte read(uint regnum)
        {
            byte result = 0;

            // register 8 reads over the bus under some conditions
            if (regnum == 0x08 && m_regs.execute() == 0 && m_regs.record() == 0 && m_regs.external() != 0)
            {
                // two dummy reads are consumed first
                if (m_dummy_read != 0)
                {
                    load_start();
                    m_dummy_read--;
                }

                // read the data
                else
                {
                    // read from outside of the chip
                    result = m_owner.intf().ymfm_external_read(access_class.ACCESS_ADPCM_B, m_curaddress++);

                    // did we hit the end? if so, signal EOS
                    if (at_end())
                    {
                        m_status = STATUS_EOS | STATUS_BRDY;
                        //debug::log_keyon("%s\n", "ADPCM EOS");
                    }
                    else
                    {
                        // signal ready
                        m_status = STATUS_BRDY;
                    }
                    /*
                    // wrap at the limit address
                    if (at_limit())
                        m_curaddress = 0;
                    */
                }
            }
            return result;
        }

        // handle special register writes
        public void write(uint regnum, byte value)
        {
            // register 0 can do a reset; also use writes here to reset the
            // dummy read counter
            if (regnum == 0x00)
            {
                if (m_regs.execute() != 0)
                {
                    load_start();

                    // don't log masked channels
                    //if ((GLOBAL_ADPCM_B_CHANNEL_MASK & 1) != 0)
                    //    debug::log_keyon("KeyOn ADPCM-B: rep=%d spk=%d pan=%d%d dac=%d 8b=%d rom=%d ext=%d rec=%d start=%04X end=%04X pre=%04X dn=%04X lvl=%02X lim=%04X\n",
                    //        m_regs.repeat(),
                    //        m_regs.speaker(),
                    //        m_regs.pan_left(),
                    //        m_regs.pan_right(),
                    //        m_regs.dac_enable(),
                    //        m_regs.dram_8bit(),
                    //        m_regs.rom_ram(),
                    //        m_regs.external(),
                    //        m_regs.record(),
                    //        m_regs.start(),
                    //        m_regs.end(),
                    //        m_regs.prescale(),
                    //        m_regs.delta_n(),
                    //        m_regs.level(),
                    //        m_regs.limit());
                }
                else
                {
                    unchecked
                    {
                        m_status &= (uint)~STATUS_EOS;
                    }
                }
                if (m_regs.resetflag() != 0)
                    reset();
                if (m_regs.external() != 0)
                    m_dummy_read = 2;
            }

            // register 8 writes over the bus under some conditions
            else if (regnum == 0x08)
            {
                // if writing from the CPU during execute, clear the ready flag
                if (m_regs.execute() != 0 && m_regs.record() == 0 && m_regs.external() == 0)
                {
                    unchecked
                    {
                        m_status &= (uint)~STATUS_BRDY;
                    }
                }
                /*
                // if writing during "record", pass through as data
                else if (m_regs.execute() == 0 && m_regs.record() != 0 && m_regs.external() != 0)
                {
                    // clear out dummy reads and set start address
                    if (m_dummy_read != 0)
                    {
                        load_start();
                        m_dummy_read = 0;
                    }

                    // did we hit the end? if so, signal EOS
                    if (at_end())
                    {
                        //debug::log_keyon("%s\n", "ADPCM EOS");
                        m_status = STATUS_EOS | STATUS_BRDY;
                    }

                    // otherwise, write the data and signal ready
                    else
                    {
                        m_owner.intf().ymfm_external_write(access_class.ACCESS_ADPCM_B, m_curaddress++, value);
                        m_status = STATUS_BRDY;
                    }
                }*/
            }
        }

        // helper - return the current address shift
        private uint address_shift()
        {
            // if a constant address shift, just provide that
            if (m_address_shift != 0)
                return m_address_shift;

            // if ROM or 8-bit DRAM, shift is 5 bits
            if (m_regs.rom_ram() != 0)
                return 5;
            if (m_regs.dram_8bit() != 0)
                return 5;

            // otherwise, shift is 2 bits
            return 2;
        }

        // load the start address
        private void load_start()
        {
            m_status = (uint)((m_status & ~STATUS_EOS) | STATUS_PLAYING);
            m_curaddress = m_regs.external() != 0 ? (m_regs.start() << (int)address_shift()) : 0;
            m_curnibble = 0;
            m_curbyte = 0;
            m_position = 0;
            m_accumulator = 0;
            m_prev_accum = 0;
            m_adpcm_step = STEP_MIN;
        }

        // limit checker; stops at the last byte of the chunk described by address_shift()
        private bool at_limit() { return (m_curaddress == (((m_regs.limit() + 1) << (int)address_shift()) - 1)); }

        // end checker; stops at the last byte of the chunk described by address_shift()
        private bool at_end() { return (m_curaddress == (((m_regs.end() + 1) << (int)address_shift()) - 1)); }

        // internal state
        private uint m_address_shift; // address bits shift-left
        private uint m_status;              // currently playing?
        private uint m_curnibble;           // index of the current nibble
        private uint m_curbyte;             // current byte of data
        private uint m_dummy_read;          // dummy read tracker
        private uint m_position;            // current fractional position
        private uint m_curaddress;          // current address
        private int m_accumulator;          // accumulator
        private int m_prev_accum;           // previous accumulator (for linear interp)
        private int m_adpcm_step;           // next forecast
        private adpcm_b_registers m_regs;      // reference to registers
        private adpcm_b_engine m_owner;        // reference to our owner
    };

    public class adpcm_b_engine
    {
        // constructor
        public adpcm_b_engine(ymfm_interface intf, int addrshift = 0)
        {
            m_intf = intf;
            // create the channel (only one supported for now, but leaving possibilities open)
            m_channel = new adpcm_b_channel(this, (uint)addrshift);
        }

        // reset our status
        public void reset()
        {
            // reset registers
            m_regs.reset();

            // reset each channel
            m_channel.reset();
        }

        // master clocking function
        public void clock()
        {
            // clock each channel, setting a bit in result if it finished
            m_channel.clock();
        }

        // compute sum of channel outputs
        //template<int NumOutputs>
        public void output(ymfm_output output, int rshift, int NumOutputs)
        {
            // compute the output of each channel
            m_channel.output(output, (uint)rshift, ymfm_output.NumOutputs);
        }

        // read from the ADPCM-B registers
        public int read(int regnum) { return m_channel.read((uint)regnum); }

        // write to the ADPCM-B registers
        public void write(int regnum, byte data)
        {
            // store the raw value to the register array;
            // most writes are passive, consumed only when needed
            m_regs.write((uint)regnum, data);

            // let the channel handle any special writes
            m_channel.write((uint)regnum, data);
        }

        // status
        public byte status()
        {
            return (byte)m_channel.status();
        }

        // return a reference to our interface
        public ymfm_interface intf() { return m_intf; }

        // return a reference to our registers
        public adpcm_b_registers regs() { return m_regs; }


        // internal state
        private ymfm_interface m_intf;                     // reference to our interface
        private adpcm_b_channel m_channel; // channel pointer
        private adpcm_b_registers m_regs = new adpcm_b_registers();                   // registers
    };

    public class ymfm_interface
    {
        Dictionary<access_class, Dictionary<uint, byte>> romdata;

        public ymfm_interface()
        {
            romdata = new Dictionary<access_class, Dictionary<uint, byte>>();
        }

        public byte ymfm_external_read(access_class type, uint address)
        {
            if (!romdata.ContainsKey(type))
                return 0;
            if (!romdata[type].ContainsKey(address))
                return 0;

            return romdata[type][address];
        }

        // the chip implementation calls this whenever data is written outside
        // of the chip; our responsibility is to pass the written data on to any consumers
        public void ymfm_external_write(access_class type, uint address, byte data)
        {
            if (!romdata.ContainsKey(type))
                romdata.Add(type, new Dictionary<uint, byte>());

            romdata[type][address] = data;
        }
    }

    public class ymfm_output
    {
        public const int NumOutputs = 2;

        // clear all outputs to 0
        public ymfm_output clear()
        {
            for (uint index = 0; index < NumOutputs; index++)
                data[index] = 0;
            return this;
        }

        int clamp(int value, int minval, int maxval)
        {
            if (value < minval)
                return minval;
            if (value > maxval)
                return maxval;
            return value;
        }

        // clamp all outputs to a 16-bit signed value
        public ymfm_output clamp16()
        {
            for (uint index = 0; index < NumOutputs; index++)
                data[index] = clamp(data[index], -32768, 32767);
            return this;
        }

        byte count_leading_zeros(uint x)
        {
            int n = 32;
            uint y;

            y = x >> 16; if (y != 0) { n = n - 16; x = y; }
            y = x >> 8; if (y != 0) { n = n - 8; x = y; }
            y = x >> 4; if (y != 0) { n = n - 4; x = y; }
            y = x >> 2; if (y != 0) { n = n - 2; x = y; }
            y = x >> 1; if (y != 0) return (byte)(n - 2);
            return (byte)(n - x);
        }

        short roundtrip_fp(int value)
        {
            // handle overflows first
            if (value < -32768)
                return -32768;
            if (value > 32767)
                return 32767;

            // we need to count the number of leading sign bits after the sign
            // we can use count_leading_zeros if we invert negative values
            int scanvalue = value ^ ((int)(value) >> 31);

            // exponent is related to the number of leading bits starting from bit 14
            int exponent = 7 - count_leading_zeros((uint)(scanvalue << 17));

            // smallest exponent value allowed is 1
            exponent = Math.Max(exponent, 1);

            // apply the shift back and forth to zero out bits that are lost
            exponent -= 1;
            return (short)((value >> exponent) << exponent);
        }


        // run each output value through the floating-point processor
        ymfm_output roundtrip_fp()
        {
            for (uint index = 0; index < NumOutputs; index++)
                data[index] = roundtrip_fp(data[index]);
            return this;
        }

        // internal state
        public int[] data = new int[NumOutputs];
    };

    public enum ProxyOPNType
    {
        OPNA,
        OPN2
    }
}

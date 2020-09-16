// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Properties;

//https://www.g200kg.com/jp/docs/tech/index.html
//http://lib.roland.co.jp/support/jp/manuals/res/1809744/MT-32_j2.pdf
//https://nickfever.com/Music/midi-cc-list

namespace zanac.MAmidiMEmo.Midi
{
    public static class MidiManager
    {
        private static InputDevice inputDevice;

        //
        // 概要:
        //     Occurs when a MIDI event is received.
        public static event EventHandler<MidiEvent> MidiEventReceived;

        /// <summary>
        /// 
        /// </summary>
        static MidiManager()
        {
            Program.ShuttingDown += Program_ShuttingDown;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Program_ShuttingDown(object sender, EventArgs e)
        {
            if (inputDevice != null)
                inputDevice.EventReceived -= midiEventReceived;

            //All Sounds Off
            SendMidiEvent(new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<InputDevice> GetInputMidiDevices()
        {
            return InputDevice.GetAll();
        }

        /// <summary>
        /// 
        /// </summary>
        public static void SetInputMidiDevice(string deviceName)
        {
            if (inputDevice != null && inputDevice.Name.Equals(deviceName))
                return;

            if (inputDevice != null)
            {
                try
                {
                    inputDevice.StopEventsListening();
                }
                catch
                {
                }
                finally
                {
                    inputDevice.Dispose();
                }
            }
            Settings.Default.MidiIF = deviceName;
            inputDevice = InputDevice.GetByName(deviceName);
            if (inputDevice != null)
            {
                inputDevice.EventReceived += midiEventReceived;
                inputDevice.StartEventsListening();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void SendMidiEvent(MidiEvent e)
        {
            MidiEventReceived?.Invoke(typeof(MidiManager), e);
        }

        private static BytesToMidiEventConverter midiConverter = new Melanchall.DryWetMidi.Core.BytesToMidiEventConverter();


        /// <summary>
        /// 
        /// </summary>
        public static void SendMidiEvent(byte data1, byte data2, byte data3)
        {
            var me = midiConverter.Convert(new byte[] { data1, data2, data3 });
            Midi.MidiManager.SendMidiEvent(me);
        }

        /// <summary>
        /// 
        /// </summary>
        unsafe public static void SendMidiSysEvent(byte *data, int length)
        {
            List<byte> buf = new List<byte>();
            for (int i = 0; i < length; i++)
                buf.Add(data[i]);
            var me = midiConverter.Convert(buf.ToArray());
            Midi.MidiManager.SendMidiEvent(me);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void midiEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            MidiEventReceived?.Invoke(sender, e.Event);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="structure"></param>
        /// <returns></returns>
        public static byte[] ToByteArray<T>(this T structure) where T : struct
        {
            byte[] bb = new byte[Marshal.SizeOf(typeof(T))];
            GCHandle gch = GCHandle.Alloc(bb, GCHandleType.Pinned);
            Marshal.StructureToPtr(structure, gch.AddrOfPinnedObject(), false);
            gch.Free();
            return bb;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T FromByteArray<T>(byte[] data) where T : struct
        {
            GCHandle? gch = null;
            T str;
            try
            {
                gch = GCHandle.Alloc(data, GCHandleType.Pinned);
                str = Marshal.PtrToStructure<T>(gch.Value.AddrOfPinnedObject());
            }
            finally
            {
                gch?.Free();
            }
            return str;
        }

        public static string GetNoteName(SevenBitNumber noteNumber)
        {
            var no = new TaggedNoteOnEvent(noteNumber, (SevenBitNumber)0);
            var nn = no.GetNoteName() + no.GetNoteOctave().ToString();

            return nn.Replace("Sharp", "#");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static double CalcCurrentFrequency(double noteNum)
        {
            double nn = Math.Pow(2.0, (noteNum - 69.0) / 12.0);
            double freq = 440.0 * nn;
            return freq;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frequency_as_hz"></param>
        /// <returns></returns>
        public static double CalcNoteNumberFromFrequency(double frequency)
        {
            return 12.0 * Math.Log(frequency / 440.0, 2) + 69.0;
        }
    }


}

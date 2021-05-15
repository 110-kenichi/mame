// copyright-holders:K.Ito
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Vsif
{

    public static class VsifManager
    {

        private static object lockObject = new object();

        private static List<VsifClient> vsifClients = new List<VsifClient>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static VsifClient TryToConnectVSIF(VsifSoundModuleType soundModule, string comPortName)
        {
            lock (lockObject)
            {
                foreach (var c in vsifClients)
                {
                    if (c.SerialPort.PortName.Equals(comPortName))
                    {
                        c.ReferencedCount++;
                        return c;
                    }
                }

                SerialPort sp = null;
                try
                {
                    sp = new SerialPort(comPortName);
                    sp.WriteTimeout = 100;
                    sp.ReadTimeout = 100;
                    switch (soundModule)
                    {
                        case VsifSoundModuleType.SMS:
                            sp.BaudRate = 115200;
                            sp.StopBits = StopBits.Two;
                            //sp.BaudRate = 57600;
                            //sp.StopBits = StopBits.One;
                            sp.Parity = Parity.None;
                            sp.DataBits = 8;
                            sp.Handshake = Handshake.None;
                            break;

                        case VsifSoundModuleType.Genesis:
                            //sp.BaudRate = 230400;
                            sp.BaudRate = 115200;
                            sp.StopBits = StopBits.One;
                            sp.Parity = Parity.None;
                            sp.DataBits = 8;
                            sp.Handshake = Handshake.None;
                            break;
                    }
                    sp.Open();

                    //sp.Write(new byte[] { (byte)'M', (byte)'a', (byte)'M', (byte)'i' }, 0, 4);
                    //sp.BaseStream.WriteByte((byte)soundModule);
                    //Thread.Sleep(100);
                    //var ret = sp.BaseStream.ReadByte();
                    //if (ret == 0x0F)   //OK
                    var client = new VsifClient(soundModule, sp);
                    client.Disposed += Client_Disposed;
                    vsifClients.Add(client);
                    return client;
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;
                }
                sp?.Dispose();
                return null;
            }
        }

        private static void Client_Disposed(object sender, EventArgs e)
        {
            lock (lockObject)
            {
                foreach (var c in vsifClients)
                {
                    if (c == sender)
                    {
                        vsifClients.Remove(c);
                        break;
                    }
                }
            }
        }
    }

    public enum VsifSoundModuleType
    {
        None,
        SMS,
        Genesis
    }

    /// <summary>
    /// 
    /// </summary>
    public class VsifClient : IDisposable
    {
        private bool disposedValue;

        private List<(byte address, byte data)> deferredWriteData;

        /// <summary>
        /// 
        /// </summary>
        public SerialPort SerialPort
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public VsifSoundModuleType SoundModuleType
        {
            get;
            private set;
        }

        public event EventHandler Disposed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public VsifClient(VsifSoundModuleType type, SerialPort serialPort)
        {
            SoundModuleType = type;
            SerialPort = serialPort;

            deferredWriteData = new List<(byte address, byte data)>();

            ReferencedCount = 1;
        }

        /// <summary>
        /// 
        /// </summary>
        public int ReferencedCount
        {
            get;
            set;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //WriteData(0xff, 0x00);

                    //マネージド状態を破棄します (マネージド オブジェクト)
                    if (SerialPort != null)
                        SerialPort.Dispose();
                    SerialPort = null;
                }

                // アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~VsifClient()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            ReferencedCount--;
            if (ReferencedCount != 0)
                return;

            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);

            Disposed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        public virtual void WriteData(byte address, byte data)
        {
            try
            {
                SerialPort.Write(new byte[] { address, data }, 0, 2);
                //Debug.WriteLine(address);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
        }
    }

}

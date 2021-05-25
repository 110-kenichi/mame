// copyright-holders:K.Ito
using FTD2XX_NET;
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
using System.Windows.Forms;

namespace zanac.VGMPlayer
{

    public static class VsifManager
    {
        //public const int FTDI_BAUDRATE = 163840;
        public const int FTDI_BAUDRATE = 10240;  //TARGET CLOCK is 115200
        public const int FTDI_BAUDRATE_MUL = 32;

        private static object lockObject = new object();

        private static List<VsifClient> vsifClients = new List<VsifClient>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static VsifClient TryToConnectVSIF(VsifSoundModuleType soundModule, ComPort comPort)
        {
            return TryToConnectVSIF(soundModule, comPort, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static VsifClient TryToConnectVSIF(VsifSoundModuleType soundModule, ComPort comPort, bool shareOnly)
        {
            lock (lockObject)
            {
                foreach (var c in vsifClients)
                {
                    if (c.SerialPort.PortName.Equals("COM" + (int)(comPort + 1)))
                    {
                        c.ReferencedCount++;
                        return c;
                    }
                    if (c.SerialPort.PortName.Equals("FTDI_COM" + (int)comPort))
                    {
                        c.ReferencedCount++;
                        return c;
                    }
                }
                if (shareOnly)
                    return null;

                try
                {
                    switch (soundModule)
                    {
                        case VsifSoundModuleType.SMS:
                            {
                                SerialPort sp = null;
                                sp = new SerialPort("COM" + ((int)comPort + 1));
                                sp.WriteTimeout = 100;
                                sp.ReadTimeout = 100;
                                sp.BaudRate = 115200;
                                sp.StopBits = StopBits.Two;
                                //sp.BaudRate = 57600;
                                //sp.StopBits = StopBits.One;
                                sp.Parity = Parity.None;
                                sp.DataBits = 8;
                                sp.Handshake = Handshake.None;
                                sp.Open();
                                var client = new VsifClient(soundModule, new PortWriter(sp));
                                client.Disposed += Client_Disposed;
                                vsifClients.Add(client);
                                return client;
                            }

                        case VsifSoundModuleType.Genesis:
                            {
                                SerialPort sp = null;
                                sp = new SerialPort("COM" + ((int)comPort + 1));
                                sp.WriteTimeout = 500;
                                sp.ReadTimeout = 500;
                                //sp.BaudRate = 230400;
                                sp.BaudRate = 163840;
                                //sp.BaudRate = 115200;
                                sp.StopBits = StopBits.One;
                                sp.Parity = Parity.None;
                                sp.DataBits = 8;
                                sp.Open();
                                var client = new VsifClient(soundModule, new PortWriter(sp));
                                client.Disposed += Client_Disposed;
                                vsifClients.Add(client);
                                return client;
                            }
                        case VsifSoundModuleType.Genesis_FTDI:
                            {
                                var ftdi = new FTD2XX_NET.FTDI();
                                var stat = ftdi.OpenByIndex((uint)comPort);
                                if (stat == FTDI.FT_STATUS.FT_OK)
                                {
                                    ftdi.SetBitMode(0x00, FTDI.FT_BIT_MODES.FT_BIT_MODE_RESET);
                                    ftdi.SetBitMode(0xFF, FTDI.FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG);
                                    ftdi.SetBaudRate(FTDI_BAUDRATE * FTDI_BAUDRATE_MUL);
                                    ftdi.SetTimeouts(500, 500);
                                    ftdi.SetLatency(0);
                                    byte ps = 0;
                                    ftdi.GetPinStates(ref ps);
                                    if ((ps & 0x40) == 0)
                                    {
                                        uint dummy = 0;
                                        ftdi.Write(new byte[] { 0x40 }, 1, ref dummy);
                                    }

                                    var client = new VsifClient(soundModule, new PortWriter(ftdi, comPort));

                                    //ftdi.Write(new byte[] { (byte)(((0x07 << 1) & 0xe) | 0) }, 1, ref dummy);
                                    //ftdi.Write(new byte[] { (byte)(((0x38 >> 2) & 0xe) | 1) }, 1, ref dummy);
                                    //ftdi.Write(new byte[] { (byte)(((0xC0 >> 5) & 0xe) | 0) }, 1, ref dummy);
                                    //ftdi.Write(new byte[] { 1 }, 1, ref dummy);

                                    client.Disposed += Client_Disposed;
                                    vsifClients.Add(client);
                                    return client;
                                }
                                else
                                {
                                    MessageBox.Show(stat.ToString());
                                }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;

                    MessageBox.Show(ex.ToString());
                }
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
        Genesis,
        Genesis_FTDI
    }

    /// <summary>
    /// 
    /// </summary>
    public class VsifClient : IDisposable
    {
        private object lockObject = new object();

        private List<byte> deferredWriteAdrAndData;

        private bool disposedValue;

        /// <summary>
        /// 
        /// </summary>
        public PortWriter SerialPort
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

        /// <summary>
        /// 
        /// </summary>
        public int ReferencedCount
        {
            get;
            set;
        }

        public event EventHandler Disposed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public VsifClient(VsifSoundModuleType type, PortWriter serialPort)
        {
            SoundModuleType = type;
            SerialPort = serialPort;

            ReferencedCount = 1;
            deferredWriteAdrAndData = new List<byte>();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //マネージド状態を破棄します (マネージド オブジェクト)
                }

                // アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        ~VsifClient()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            ReferencedCount--;
            if (ReferencedCount != 0)
                return;

            if (SerialPort != null)
                SerialPort.Dispose();
            SerialPort = null;

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
        public virtual void DeferredWriteData(byte address, byte data)
        {
            lock (lockObject)
            {
                deferredWriteAdrAndData.Add(address);
                deferredWriteAdrAndData.Add(data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        public virtual void FlushDeferredWriteData()
        {
            lock (lockObject)
            {
                if (deferredWriteAdrAndData.Count != 0)
                {
                    List<byte> tmpData = null;
                    lock (deferredWriteAdrAndData)
                    {
                        tmpData = new List<byte>(deferredWriteAdrAndData);
                        deferredWriteAdrAndData.Clear();
                    }
                    SerialPort?.Write(tmpData.ToArray());
                }
            }
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
                lock (lockObject)
                {
                    FlushDeferredWriteData();
                    SerialPort?.Write(address, data);
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
            /*
                      lock (deferredWriteData)
                      {
                          deferredWriteData.Add(address);
                          deferredWriteData.Add(data);
                          if (deferredWriteData.Count != 2)
                              return;
                      }
                      void act()
                      {
                          List<byte> tmpData = null;
                          lock (deferredWriteData)
                          {
                              tmpData = new List<byte>(deferredWriteData);
                              deferredWriteData.Clear();
                          }
                          SerialPort?.Write(tmpData.ToArray(), 0, tmpData.Count);
                      }
                      Task.Factory.StartNew(act);
            */
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public enum ComPort
    {
        No1,
        No2,
        No3,
        No4,
        No5,
        No6,
        No7,
        No8,
        No9,
        No10,
        No11,
        No12,
        No13,
        No14,
        No15,
        No16,
        No17,
        No18,
        No19,
        No20,
        No21,
        No22,
        No23,
        No24,
        No25,
        No26,
        No27,
        No28,
        No29,
        No30,
        No31,
        No32,
        No33,
        No34,
        No35,
        No36,
        No37,
        No38,
        No39,
        No40,
        No41,
        No42,
        No43,
        No44,
        No45,
        No46,
        No47,
        No48,
        No49,
        No50,
        No51,
        No52,
        No53,
        No54,
        No55,
        No56,
        No57,
        No58,
        No59,
        No60,
        No61,
        No62,
        No63,
        No64,
        No65,
        No66,
        No67,
        No68,
        No69,
        No70,
        No71,
        No72,
        No73,
        No74,
        No75,
        No76,
        No77,
        No78,
        No79,
        No80,
        No81,
        No82,
        No83,
        No84,
        No85,
        No86,
        No87,
        No88,
        No89,
        No90,
        No91,
        No92,
        No93,
        No94,
        No95,
        No96,
        No97,
        No98,
        No99,
        No100,
        No101,
        No102,
        No103,
        No104,
        No105,
        No106,
        No107,
        No108,
        No109,
        No110,
        No111,
        No112,
        No113,
        No114,
        No115,
        No116,
        No117,
        No118,
        No119,
        No120,
        No121,
        No122,
        No123,
        No124,
        No125,
        No126,
        No127,
        No128,
        No129,
        No130,
        No131,
        No132,
        No133,
        No134,
        No135,
        No136,
        No137,
        No138,
        No139,
        No140,
        No141,
        No142,
        No143,
        No144,
        No145,
        No146,
        No147,
        No148,
        No149,
        No150,
        No151,
        No152,
        No153,
        No154,
        No155,
        No156,
        No157,
        No158,
        No159,
        No160,
        No161,
        No162,
        No163,
        No164,
        No165,
        No166,
        No167,
        No168,
        No169,
        No170,
        No171,
        No172,
        No173,
        No174,
        No175,
        No176,
        No177,
        No178,
        No179,
        No180,
        No181,
        No182,
        No183,
        No184,
        No185,
        No186,
        No187,
        No188,
        No189,
        No190,
        No191,
        No192,
        No193,
        No194,
        No195,
        No196,
        No197,
        No198,
        No199,
        No200,
        No201,
        No202,
        No203,
        No204,
        No205,
        No206,
        No207,
        No208,
        No209,
        No210,
        No211,
        No212,
        No213,
        No214,
        No215,
        No216,
        No217,
        No218,
        No219,
        No220,
        No221,
        No222,
        No223,
        No224,
        No225,
        No226,
        No227,
        No228,
        No229,
        No230,
        No231,
        No232,
        No233,
        No234,
        No235,
        No236,
        No237,
        No238,
        No239,
        No240,
        No241,
        No242,
        No243,
        No244,
        No245,
        No246,
        No247,
        No248,
        No249,
        No250,
        No251,
        No252,
        No253,
        No254,
        No255,
        No256
    }
}

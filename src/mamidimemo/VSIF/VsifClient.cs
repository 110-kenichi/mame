// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.VSIF;

namespace zanac.MAmidiMEmo.VSIF
{
    /// <summary>
    /// 
    /// </summary>
    public class VsifClient : IDisposable
    {

        private object lockObject = new object();

        /// <summary>
        /// 
        /// </summary>
        public object LockObject
        {
            get
            {
                return lockObject;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        public void Sleep(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }

        private List<byte> deferredWriteData;

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
            deferredWriteData = new List<byte>();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;

                if (disposing)
                {
                    //マネージド状態を破棄します (マネージド オブジェクト)
                }

                // アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // 大きなフィールドを null に設定します
                if (SerialPort != null)
                    SerialPort.Dispose();
                SerialPort = null;
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
        public virtual void WriteData(byte type, byte address, byte data, int wait)
        {
            try
            {
                if (disposedValue)
                    return;
                lock (lockObject)
                    SerialPort?.Write(new PortWriteData[] { new PortWriteData() { Type = type, Address = address, Data = data, Wait = wait } });
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        public virtual void WriteData(byte type, byte address, byte[] data, int wait)
        {
            try
            {
                if (disposedValue)
                    return;
                lock (lockObject)
                {
                    PortWriteData[] pdata = new PortWriteData[data.Length];
                    for (int i = 0; i < data.Length; i++)
                    {
                        pdata[i] = new PortWriteData() { Type = type, Address = address++, Data = data[i], Wait = wait };
                    }
                    SerialPort?.Write(pdata);
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        public virtual void RawWriteData(byte[] data, int wait)
        {
            try
            {
                if (disposedValue)
                    return;
                lock (lockObject)
                    SerialPort?.RawWrite(data, wait);
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

    /// <summary>
    /// 
    /// </summary>
    public enum PortId
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

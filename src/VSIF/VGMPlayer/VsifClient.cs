// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace zanac.VGMPlayer
{

    /// <summary>
    /// 
    /// </summary>
    public class VsifClient : IDisposable
    {
        public Dictionary<string, double> ChipClockHz
        {
            get;
            set;
        } = new Dictionary<string, double>();

        public Dictionary<int, int> RegTable
        {
            get;
            set;
        } = new Dictionary<int, int>();

        public Dictionary<string, object> Tag
        {
            get;
            set;
        }

        private object lockObject = new object();
        private object lockObjectPrior = new object();

        private List<PortWriteData> deferredWriteAdrAndData;
        private List<PortWriteData> deferredWriteAdrAndDataPrior;

        private bool disposedValue;

        private Thread writeThread;
        private Thread writeThreadPrior;


        private AutoResetEvent autoResetEvent;
        private AutoResetEvent autoResetEventPrior;

        /// <summary>
        /// 
        /// </summary>
        public PortWriter DataWriter
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
        public VsifClient(VsifSoundModuleType type, PortWriter dataWriter)
        {
            Tag = new Dictionary<string, object>();
            SoundModuleType = type;
            DataWriter = dataWriter;

            ReferencedCount = 1;
            deferredWriteAdrAndData = new List<PortWriteData>();
            deferredWriteAdrAndDataPrior = new List<PortWriteData>();

            autoResetEvent = new AutoResetEvent(false);
            autoResetEventPrior = new AutoResetEvent(false);
            writeThread = new Thread(new ThreadStart(deferredWriteDataTask));
            writeThread.Priority = ThreadPriority.AboveNormal;
            writeThread.Start();
            writeThreadPrior = new Thread(new ThreadStart(deferredWriteDataTaskPrior));
            writeThreadPrior.Priority = ThreadPriority.Highest;
            writeThreadPrior.Start();

            for (int i = 0; i < 0x1ff; i++)
                RegTable.Add(i, 0);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;

                if (disposing)
                {
                    //マネージド状態を破棄します (マネージド オブジェクト)
                    autoResetEvent.Set();
                    while (writeThread.IsAlive)
                        Thread.Sleep(0);
                    autoResetEvent.Dispose();

                    autoResetEventPrior.Set();
                    while (writeThreadPrior.IsAlive)
                        Thread.Sleep(0);
                    autoResetEventPrior.Dispose();
                }

                // アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // 大きなフィールドを null に設定します
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

            if (DataWriter != null)
                DataWriter.Dispose();
            DataWriter = null;

            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);

            Disposed?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        public void Sleep(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        public virtual void ClearDeferredWriteData()
        {
            lock (lockObject)
            {
                if (disposedValue)
                    return;
                deferredWriteAdrAndData.Clear();
            }
            lock (lockObjectPrior)
            {
                if (disposedValue)
                    return;
                deferredWriteAdrAndDataPrior.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        public virtual void DeferredWriteData(byte type, byte address, byte data, int wait)
        {
            lock (lockObject)
            {
                if (disposedValue)
                    return;
                deferredWriteAdrAndData.Add(new PortWriteData() { Type = type, Address = address, Data = data, Wait = wait });
            }
            autoResetEvent.Set();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        public virtual void DeferredWriteData(byte[] type, byte[] address, byte[] data, int wait)
        {
            lock (lockObject)
            {
                if (disposedValue)
                    return;
                for (int i = 0; i < type.Length; i++)
                    deferredWriteAdrAndData.Add(new PortWriteData() { Type = type[i], Address = address[i], Data = data[i], Wait = wait });
            }
            autoResetEvent.Set();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        public virtual void DeferredWriteDataPrior(byte[] type, byte[] address, byte[] data, int wait)
        {
            lock (lockObjectPrior)
            {
                if (disposedValue)
                    return;
                for (int i = 0; i < type.Length; i++)
                    deferredWriteAdrAndDataPrior.Add(new PortWriteData() { Type = type[i], Address = address[i], Data = data[i], Wait = wait });
            }
            autoResetEventPrior.Set();
        }

        /// <summary>
        /// 
        /// </summary>
        private void deferredWriteDataTask()
        {
            try
            {
                while (!disposedValue)
                {
                    autoResetEvent.WaitOne();
                    PortWriteData[] dd;
                    lock (lockObject)
                    {
                        if (deferredWriteAdrAndData.Count == 0)
                            continue;
                        dd = deferredWriteAdrAndData.ToArray();
                        deferredWriteAdrAndData.Clear();
                    }
                    if (dd.Length != 0)
                        DataWriter?.Write(dd);
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
        private void deferredWriteDataTaskPrior()
        {
            try
            {
                while (!disposedValue)
                {
                    autoResetEventPrior.WaitOne();
                    PortWriteData[] dd;
                    lock (lockObjectPrior)
                    {
                        if (deferredWriteAdrAndDataPrior.Count == 0)
                            continue;
                        dd = deferredWriteAdrAndDataPrior.ToArray();
                        deferredWriteAdrAndDataPrior.Clear();
                    }

                    if (dd.Length != 0)
                        DataWriter?.Write(dd);
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
        public virtual void FlushDeferredWriteData()
        {
            //autoResetEvent.Set();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        public virtual void FlushDeferredWriteDataAndWait()
        {
            try
            {
                PortWriteData[] dd;
                lock (lockObject)
                {
                    if (disposedValue)
                        return;

                    if (deferredWriteAdrAndData.Count == 0)
                        return;
                    dd = deferredWriteAdrAndData.ToArray();
                    deferredWriteAdrAndData.Clear();
                }
                if (dd.Length != 0)
                    DataWriter?.Write(dd);
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
        public virtual void WriteData(byte type, byte address, byte data, int wait)
        {
            try
            {
                PortWriteData[] dd;
                lock (lockObject)
                {
                    if (disposedValue)
                        return;

                    deferredWriteAdrAndData.Add(new PortWriteData() { Type = type, Address = address, Data = data, Wait = wait });
                    dd = deferredWriteAdrAndData.ToArray();
                    deferredWriteAdrAndData.Clear();
                }
                DataWriter?.Write(dd);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
        }

        public void Abort()
        {
            try
            {
                lock (lockObject)
                {
                    deferredWriteAdrAndData.Clear();
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
            try
            {
                lock (lockObjectPrior)
                {
                    deferredWriteAdrAndDataPrior.Clear();
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
            
            DataWriter?.Abort();
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

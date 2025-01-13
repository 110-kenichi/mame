using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.Gimic;
using static zanac.MAmidiMEmo.Gimic.GimicManager;

namespace zanac.VGMPlayer
{
    public class PortWriterGimic : PortWriter
    {
        private bool disposedValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftdiPort"></param>
        public PortWriterGimic() : base("Gimic")
        {
        }

        private int? opnaIndex;

        public int OpnaIndex
        {
            get
            {
                if(opnaIndex == null)
                    opnaIndex = GimicManager.GetModuleIndex(GimicManager.ChipType.CHIP_OPNA);
                return opnaIndex.Value;
            }
        }

        private int? opmIndex;

        public int OpmIndex
        {
            get
            {
                if (opmIndex == null)
                    opmIndex = GimicManager.GetModuleIndex(GimicManager.ChipType.CHIP_OPM);
                return opmIndex.Value;
            }
        }

        private int? opn3lIndex;

        public int Opn3lIndex
        {
            get
            {
                if (opn3lIndex == null)
                    opn3lIndex = GimicManager.GetModuleIndex(GimicManager.ChipType.CHIP_OPN3L);
                return opn3lIndex.Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public int GetModuleIndex(ChipType chipType)
        {
            TryInitializeGimmic();

            switch (chipType)
            {
                case ChipType.CHIP_OPNA:
                    return OpnaIndex;
                case ChipType.CHIP_OPN3L:
                    return Opn3lIndex;
                case ChipType.CHIP_OPM:
                    return OpmIndex;
            }

            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <param name="wait"></param>
        public override void Write(PortWriteData[] data)
        {
            try
            {
                List<uint> opnaDataAdr = new List<uint>();
                List<byte> opnaDataData = new List<byte>();
                List<uint> opmDataAdr = new List<uint>();
                List<byte> opmDataData = new List<byte>();
                List<uint> opn3lDataAdr = new List<uint>();
                List<byte> opn3lDataData = new List<byte>();

                foreach (var dt in data)
                {
                    switch (dt.Type)
                    {
                        case 0: //OPNA
                            opnaDataAdr.Add(dt.Address);
                            opnaDataData.Add(dt.Data);
                            break;
                        case 1: //OPNA
                            opnaDataAdr.Add((uint)(dt.Address + 0x100));
                            opnaDataData.Add(dt.Data);
                            break;
                        case 2: //OPM
                            opmDataAdr.Add(dt.Address);
                            opmDataData.Add(dt.Data);
                            break;
                        case 3: //OPN3L
                            opn3lDataAdr.Add(dt.Address);
                            opn3lDataData.Add(dt.Data);
                            break;
                        case 4: //OPN3L
                            opn3lDataAdr.Add((uint)(dt.Address + 0x100));
                            opn3lDataData.Add(dt.Data);
                            break;
                    }
                }
                if (opnaDataAdr.Count > 0 && OpnaIndex >= 0)
                    GimicManager.SetRegister2(OpnaIndex, opnaDataAdr.ToArray(), opnaDataData.ToArray());
                if (opmDataAdr.Count > 0 && OpmIndex >= 0)
                    GimicManager.SetRegister2(OpmIndex, opmDataAdr.ToArray(), opmDataData.ToArray());
                if (opn3lDataAdr.Count > 0 && Opn3lIndex >= 0)
                    GimicManager.SetRegister2(Opn3lIndex, opn3lDataAdr.ToArray(), opn3lDataData.ToArray());
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                }
                lock (LockObject)
                {
                    GimicManager.TryReleaseGimic();
                }
                disposedValue = true;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="wait"></param>
        public override void RawWrite(byte[] data, int[] wait)
        {

        }

    }

}

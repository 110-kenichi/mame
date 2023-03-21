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

        public int OpnaIndex
        {
            get;
            private set;
        } = -1;

        public int OpmIndex
        {
            get;
            private set;
        } = -1;

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
                    if (OpnaIndex < 0)
                    {
                        OpnaIndex = GimicManager.GetModuleIndex(GimicManager.ChipType.CHIP_OPNA);
                        //HACK: OPN3L
                        if (OpnaIndex < 0)
                            OpnaIndex = GimicManager.GetModuleIndex(GimicManager.ChipType.CHIP_OPN3L);
                    }
                    return OpnaIndex;
                case ChipType.CHIP_OPM:
                    if (OpmIndex < 0)
                        OpmIndex = GimicManager.GetModuleIndex(GimicManager.ChipType.CHIP_OPM);
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
                foreach (var dt in data)
                {
                    int id = 0;
                    switch (dt.Type)
                    {
                        case 0: //OPNA
                            if (OpnaIndex < 0)
                            {
                                OpnaIndex = GimicManager.GetModuleIndex(GimicManager.ChipType.CHIP_OPNA);
                                //HACK: OPN3L
                                if (OpnaIndex < 0)
                                    OpnaIndex = GimicManager.GetModuleIndex(GimicManager.ChipType.CHIP_OPN3L);
                            }
                            if (OpnaIndex >= 0)
                                GimicManager.SetRegister(id, dt.Address, dt.Data, false);
                            break;
                        case 1: //OPNA
                            if (OpnaIndex < 0)
                            {
                                OpnaIndex = GimicManager.GetModuleIndex(GimicManager.ChipType.CHIP_OPNA);
                                //HACK: OPN3L
                                if (OpnaIndex < 0)
                                    OpnaIndex = GimicManager.GetModuleIndex(GimicManager.ChipType.CHIP_OPN3L);
                            }
                            if (OpnaIndex >= 0)
                                GimicManager.SetRegister(id, (uint)(dt.Address + 0x100), dt.Data, false);
                            break;
                        case 2: //OPM
                            if (OpmIndex < 0)
                                OpmIndex = GimicManager.GetModuleIndex(GimicManager.ChipType.CHIP_OPM);
                            if (OpmIndex >= 0)
                                GimicManager.SetRegister(id, dt.Address, dt.Data, false);
                            break;
                    }
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
        public override void RawWrite(byte[] data, int wait)
        {

        }

    }

}

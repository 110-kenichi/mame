// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Scci
{
    /// <summary>
    /// 
    /// </summary>
    public interface IScciWrapper : IDisposable
    {
        uint InitializeScci();

        void ReleaseScci();

        IntPtr GetSoundChip(uint iSoundChipType, uint clock);

        void ReleaseSoundChip(IntPtr pSoundChip);

        void SetRegister(IntPtr pChip, uint dAddr, uint pData);

        uint GetWrittenRegisterData(IntPtr pChip, uint addr);

        bool IsBufferEmpty(IntPtr pChip);
    }
}

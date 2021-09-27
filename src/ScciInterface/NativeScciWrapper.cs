// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Scci
{
    /// <summary>
    /// 
    /// </summary>
    public class NativeScciWrapper : IScciWrapper
    {
        private const string SCCI_WRAPPER_DLL_NAME = "ScciWrapper";

        /// <summary>
        /// 
        /// </summary>
        public NativeScciWrapper()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {

        }

        [DllImport(SCCI_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "GetSoundChip", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetSoundChipInternal(uint iSoundChipType, uint clock);

        public IntPtr GetSoundChip(uint iSoundChipType, uint clock)
        {
            return (IntPtr)GetSoundChipInternal(iSoundChipType, clock);
        }

        [DllImport(SCCI_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "GetWrittenRegisterData", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint GetWrittenRegisterDataInternal(IntPtr pChip, uint addr);

        public uint GetWrittenRegisterData(IntPtr pChip, uint addr)
        {
            return GetWrittenRegisterDataInternal(pChip, addr);
        }

        [DllImport(SCCI_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "InitializeScci", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint InitializeScciInternal();

        public uint InitializeScci()
        {
            return InitializeScciInternal();
        }

        [DllImport(SCCI_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "IsBufferEmpty", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool IsBufferEmptyInternal(IntPtr pChip);

        public bool IsBufferEmpty(IntPtr pChip)
        {
            return IsBufferEmptyInternal(pChip);
        }

        [DllImport(SCCI_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "ReleaseScci", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ReleaseScciInternal();

        public void ReleaseScci()
        {
            ReleaseScciInternal();
        }

        [DllImport(SCCI_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "ReleaseSoundChip", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ReleaseSoundChipInternal(IntPtr pSoundChip);

        public void ReleaseSoundChip(IntPtr pSoundChip)
        {
            ReleaseSoundChipInternal(pSoundChip);
        }

        [DllImport(SCCI_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "SetRegister", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetRegisterInternal(IntPtr pChip, uint dAddr, uint pData);

        public void SetRegister(IntPtr pChip, uint dAddr, uint pData)
        {
            SetRegisterInternal(pChip, dAddr, pData);
        }
    }
}

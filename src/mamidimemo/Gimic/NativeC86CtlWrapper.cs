// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Gimic
{
    /// <summary>
    /// 
    /// </summary>
    public class NativeC86CtlWrapper
    {
        private const string C86CTL_WRAPPER_DLL_NAME = "ScciWrapper";

        /// <summary>
        /// 
        /// </summary>
        public NativeC86CtlWrapper()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {

        }

        [DllImport(C86CTL_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "GimicInitialize", CallingConvention = CallingConvention.Cdecl)]
        private static extern int InitializeGimicInternal();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int InitializeGimic()
        {
            return InitializeGimicInternal();
        }

        [DllImport(C86CTL_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "GimicDeinitialize", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DeinitializeGimicInternal();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int DeinitializeGimic()
        {
            return DeinitializeGimicInternal();
        }

        [DllImport(C86CTL_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "GimicGetNumberOfChip", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetNumberOfChipInternal();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfChip()
        {
            return GetNumberOfChipInternal();
        }

        [DllImport(C86CTL_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "GimicGetModule", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetModuleIndexInternal(uint moduleIndex, uint chipType);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="chipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public int TryGetModuleIndex(uint moduleIndex, uint chipType)
        {
            return GetModuleIndexInternal(moduleIndex, chipType);
        }

        [DllImport(C86CTL_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "GimicReset", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ResetInternal(int moduleIndex);

        public void Reset(int moduleIndex)
        {
            ResetInternal(moduleIndex);
        }

        [DllImport(C86CTL_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "GimicSetRegister", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetRegisterInternal(int moduleIndex, uint dAddr, uint pData);

        public void SetRegister(int moduleIndex, uint dAddr, uint pData)
        {
            SetRegisterInternal(moduleIndex, dAddr, pData);
        }

        [DllImport(C86CTL_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "GimicSetRegisterDirect", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetRegisterDirectInternal(int moduleIndex, uint dAddr, uint pData);

        public void SetRegisterDirect(int moduleIndex, uint dAddr, uint pData)
        {
            SetRegisterDirectInternal(moduleIndex, dAddr, pData);
        }

        [DllImport(C86CTL_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "GimicSetRegister2", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetRegister2Internal(int moduleIndex, ref uint dAddr, ref byte pData, uint sz);

        public void SetRegister2(int moduleIndex, uint dAddr, byte pData, uint sz)
        {
            SetRegister2Internal(moduleIndex, ref dAddr, ref pData, sz);
        }

        [DllImport(C86CTL_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "GimicGetWrittenRegisterData", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint GetWrittenRegisterDataInternal(int moduleIndex, uint addr);

        public uint GetWrittenRegisterData(int moduleIndex, uint addr)
        {
            return GetWrittenRegisterDataInternal(moduleIndex, addr);
        }

        [DllImport(C86CTL_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "GimicSetClock", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint SetClockInternal(int moduleIndex, uint clock);

        public uint SetClock(int moduleIndex, uint clock)
        {
            return SetClockInternal(moduleIndex, clock);
        }

        [DllImport(C86CTL_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "GimicSetSSGVolume", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint SetSSGVolumeInternal(int moduleIndex, uint volume);

        public void SetSSGVolume(int moduleIndex, uint volume)
        {
            SetSSGVolumeInternal(moduleIndex, volume);
        }

        public const int C86CTL_ERR_NONE = 0;
        public const int C86CTL_ERR_UNKNOWN = -1;
        public const int C86CTL_ERR_INVALID_PARAM = -2;
        public const int C86CTL_ERR_UNSUPPORTED = -3;
        public const int C86CTL_ERR_NODEVICE = -1000;
        public const int C86CTL_ERR_NOT_IMPLEMENTED = -9999;
    }
}

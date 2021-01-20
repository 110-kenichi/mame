// copyright-holders:K.Ito
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.Client;
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
    public class X64toX86BridgeScciWrapper : IScciWrapper
    {
        private const string SCCI_WRAPPER_DLL_NAME = "ScciWrapper.dll";

        private WrapperClient wrapperClient;

        /// <summary>
        /// 
        /// </summary>
        public X64toX86BridgeScciWrapper()
        {
            wrapperClient = new WrapperClient(SCCI_WRAPPER_DLL_NAME, TargetArchitecture.X86);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (wrapperClient != null)
                wrapperClient.Dispose();

            wrapperClient = null;
        }

        private delegate uint GetSoundChipDelegate(uint iSoundChipType, uint clock);

        public IntPtr GetSoundChip(uint iSoundChipType, uint clock)
        {
            return (IntPtr)(uint)wrapperClient.Invoke<GetSoundChipDelegate>("GetSoundChip", new uint[] { iSoundChipType, clock });
        }

        private delegate uint GetWrittenRegisterDataDelegate(uint pChip, uint addr);

        public uint GetWrittenRegisterData(IntPtr pChip, uint addr)
        {
            return (uint)wrapperClient.Invoke<GetSoundChipDelegate>("GetWrittenRegisterData", new uint[] { (uint)pChip, addr });
        }

        private delegate uint InitializeScciDelegate();

        public uint InitializeScci()
        {
            return (uint)wrapperClient.Invoke<InitializeScciDelegate>("InitializeScci", new uint[] { });
        }

        private delegate bool IsBufferEmptyDelegate(int pChip);

        public bool IsBufferEmpty(IntPtr pChip)
        {
            return (bool)wrapperClient.Invoke<IsBufferEmptyDelegate>("IsBufferEmpty", new uint[] { });
        }

        private delegate void ReleaseScciDelegate();

        public void ReleaseScci()
        {
            wrapperClient.Invoke<ReleaseScciDelegate>("ReleaseScci", new uint[] { });
        }

        private delegate void ReleaseSoundChipDelegate(uint pSoundChip);

        public void ReleaseSoundChip(IntPtr pSoundChip)
        {
            wrapperClient.Invoke<ReleaseSoundChipDelegate>("ReleaseSoundChip", new uint[] { (uint)pSoundChip });
        }

        private delegate void SetRegisterDelegate(uint pChip, uint dAddr, uint pData);

        public void SetRegister(IntPtr pChip, uint dAddr, uint pData)
        {
            wrapperClient.Invoke<SetRegisterDelegate>("SetRegister", new uint[] { (uint)pChip, dAddr, pData });
        }
    }

}

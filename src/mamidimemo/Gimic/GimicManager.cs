// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Gimic
{

    public static class GimicManager
    {
        private static Dictionary<int, Dictionary<uint, uint>> writtenDataCache;

        private static object lockObject = new object();

        private static NativeC86CtlWrapper wrapperClient;

        private static bool initialized;

        /// <summary>
        /// 
        /// </summary>
        public static bool IsScciInitialized
        {
            get
            {
                return initialized;
            }
        }

        private static void TryInitializeGimmic()
        {
            lock (lockObject)
            {
                if (!initialized)
                {
                    if (wrapperClient == null)
                        wrapperClient = new NativeC86CtlWrapper();

                    FormProgress.RunDialog(Resources.ConnectingGimic, new Action<FormProgress>((f) =>
                    {
                        f.Percentage = -1;

                        initialized = wrapperClient.InitializeGimic() == 0 ? true : false;

                        if (initialized && writtenDataCache == null)
                            writtenDataCache = new Dictionary<int, Dictionary<uint, uint>>();
                    }));
                }
            }
        }

        public static void TryReleaseGimic()
        {
            lock (lockObject)
            {
                if (initialized)
                {
                    if (wrapperClient.DeinitializeGimic() == 0)
                    {
                        writtenDataCache.Clear();

                        initialized = false;
                    }
                }
            }
        }

        public static int GetNumberOfChip()
        {
            lock (lockObject)
            {
                if (initialized)
                    return wrapperClient.GetNumberOfChip();
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chipType"></param>
        /// <returns></returns>
        public static int GetModuleIndex(ChipType chipType)
        {
            TryInitializeGimmic();

            lock (lockObject)
            {
                if (initialized)
                {
                    for (int i = 0; i < GetNumberOfChip(); i++)
                    {
                        if (writtenDataCache.ContainsKey(i))
                            continue;
                        if (wrapperClient.TryGetModuleIndex((uint)i, (uint)chipType) == NativeC86CtlWrapper.C86CTL_ERR_NONE)
                        {
                            writtenDataCache.Add(i, new Dictionary<uint, uint>());
                            return i;
                        }
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public static int GetModuleIndex(String[] chipNames)
        {
            TryInitializeGimmic();

            lock (lockObject)
            {
                if (initialized)
                {
                    for (int i = 0; i < GetNumberOfChip(); i++)
                    {
                        if (writtenDataCache.ContainsKey(i))
                            continue;
                        for (int j = 0; j < chipNames.Length; j++)
                        {
                            if (wrapperClient.TryGetModuleIndex((uint)i, chipNames[j]) ==
                                NativeC86CtlWrapper.C86CTL_ERR_NONE)
                            {
                                writtenDataCache.Add(i, new Dictionary<uint, uint>());
                                return i;
                            }
                        }
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static void ReleaseModule(int moduleIndex)
        {
            lock (lockObject)
            {
                if (writtenDataCache.ContainsKey(moduleIndex))
                    writtenDataCache.Remove(moduleIndex);

                if (writtenDataCache.Count == 0)
                {
                    TryReleaseGimic();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pChip"></param>
        public static void Reset(int moduleIndex)
        {
            lock (lockObject)
            {
                if (initialized)
                    wrapperClient.Reset(moduleIndex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pChip"></param>
        /// <param name="dAddr"></param>
        /// <param name="pData"></param>
        public static uint SetClock(int moduleIndex, uint clock)
        {
            lock (lockObject)
            {
                if (initialized)
                    return wrapperClient.SetClock(moduleIndex, clock);
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pChip"></param>
        /// <param name="dAddr"></param>
        /// <param name="pData"></param>
        /// <param name="useCache"></param>
        public static void SetRegisterDirect(int moduleIndex, uint dAddr, uint pData, bool useCache)
        {
            lock (lockObject)
            {
                if (initialized)
                {
                    if (useCache)
                    {
                        var prevData = GetWrittenRegisterData(moduleIndex, dAddr);
                        if (prevData != pData)
                        {
                            wrapperClient.SetRegisterDirect(moduleIndex, dAddr, pData);
                            if (writtenDataCache.ContainsKey(moduleIndex))
                                writtenDataCache[moduleIndex][dAddr] = pData;
                        }
                    }
                    else
                    {
                        wrapperClient.SetRegisterDirect(moduleIndex, dAddr, pData);
                        if (writtenDataCache.ContainsKey(moduleIndex))
                            writtenDataCache[moduleIndex][dAddr] = pData;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pChip"></param>
        /// <param name="dAddr"></param>
        /// <param name="pData"></param>
        /// <param name="useCache"></param>
        public static void SetRegister2(int moduleIndex, uint dAddr, byte pData)
        {
            lock (lockObject)
            {
                if (initialized)
                {
                    wrapperClient.SetRegister2(moduleIndex, dAddr, pData, 1);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pChip"></param>
        /// <param name="dAddr"></param>
        /// <param name="pData"></param>
        /// <param name="useCache"></param>
        public static void SetRegister2(int moduleIndex, uint dAddr, byte pData, byte type)
        {
            lock (lockObject)
            {
                if (initialized)
                {
                    wrapperClient.SetRegister2(moduleIndex, dAddr, pData, 1, type);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static UInt32 GetWrittenRegisterData(int moduleIndex, uint addr)
        {
            lock (lockObject)
            {
                if (writtenDataCache.ContainsKey(moduleIndex))
                    if (writtenDataCache[moduleIndex].ContainsKey(addr))
                        return writtenDataCache[moduleIndex][addr];
            }
            return 0;

            //return wrapperClient.GetWrittenRegisterData(pChip, addr);
        }

        public enum ChipType
        {
            CHIP_UNKNOWN = 0,
            CHIP_OPNA,
            CHIP_OPM,
            CHIP_OPN3L,
            CHIP_OPL3,
            CHIP_SPC,
            CHIP_STIC_OPLL,
            CHIP_STIC_SSG,
            CHIP_STIC_AYPSG,
            CHIP_STIC_OPN2,
            CHIP_STIC_OPN2C,
            CHIP_STIC_OPN2L,
            CHIP_STIC_SPSG,
            CHIP_STIC_OPN3L,
        };

        public readonly static string[] ChipName = new string[]
        {
            "", //CHIP_UNKNOWN = 0,
            "", //CHIP_OPNA,
            "", //CHIP_OPM,
            "", //CHIP_OPN3L,
            "", //CHIP_OPL3,
            "", //CHIP_SPC
            "GMC-S2413",    //STIC OPLL(GMC-S2413)
            "GMC-S2149",    //STIC SSG(GMC-S2149)
            "GMC-S8910",    //STIC AYPSG(GMC-S8910)
            "GMC-S2612",    //STIC OPN2(GMC-S2612)
            "GMC-S3438",    //STIC OPN2C(GMC-S3438)
            "GMC-S276",     //STIC OPN2L(GMC-S276)
            "GMC-S5377",    //STIC SPSG(GMC-S5377)
            "GMC-S288",     //STIC OPN3L(GMC-S288)
        };
    }


}

// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Scci
{
    public static class ScciManager
    {
        private static object lockObject = new object();

        private const string SCCI_WRAPPER_DLL_NAME = "ScciWrapper";

        [DllImport(SCCI_WRAPPER_DLL_NAME, SetLastError = true)]
        private static extern UInt32 InitializeScci();

        private static bool initialized;

        private static void TryInitializeScci()
        {
            if (Environment.Is64BitProcess)
                return;

            lock (lockObject)
            {
                if (!initialized)
                    initialized = InitializeScci() == 0 ? true : false;
            }
        }

        [DllImport(SCCI_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "ReleaseScci")]
        private static extern void ReleaseScciInternal();

        public static void TryReleaseScci()
        {
            if (Environment.Is64BitProcess)
                return;

            if (initialized)
                lock (lockObject)
                    ReleaseScciInternal();
        }

        [DllImport(SCCI_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "GetSoundChip")]
        private static extern IntPtr GetSoundChipInternal(SoundChipType iSoundChipType, SC_CHIP_CLOCK clock);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static IntPtr TryGetSoundChip(SoundChipType iSoundChipType, SC_CHIP_CLOCK clock)
        {
            if (Environment.Is64BitProcess)
                return IntPtr.Zero;

            TryInitializeScci();

            lock (lockObject)
                return GetSoundChipInternal(iSoundChipType, clock);
        }

        [DllImport(SCCI_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "ReleaseSoundChip")]
        private static extern bool ReleaseSoundChipInternal(IntPtr pSoundChip);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static bool ReleaseSoundChip(IntPtr pSoundChip)
        {
            if (Environment.Is64BitProcess)
                return true;

            lock (lockObject)
                return ReleaseSoundChipInternal(pSoundChip);
        }

        [DllImport(SCCI_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "SetRegister")]
        private static extern bool SetRegisterInternal(IntPtr pChip, uint dAddr, uint pData);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static bool SetRegister(IntPtr pChip, uint dAddr, uint pData)
        {
            if (Environment.Is64BitProcess)
                return false;

            lock (lockObject)
                return SetRegisterInternal(pChip, dAddr, pData);
        }


        [DllImport(SCCI_WRAPPER_DLL_NAME, SetLastError = true, EntryPoint = "GetWrittenRegisterData")]
        private static extern UInt32 GetWrittenRegisterDataInternal(IntPtr pChip, uint addr);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static UInt32 GetWrittenRegisterData(IntPtr pChip, uint addr)
        {
            if (Environment.Is64BitProcess)
                return 0;

            lock (lockObject)
                return GetWrittenRegisterDataInternal(pChip, addr);
        }

    }

    public enum SoundChipType
    {
        SC_TYPE_NONE = 0,
        SC_TYPE_YM2608,
        SC_TYPE_YM2151,
        SC_TYPE_YM2610,
        SC_TYPE_YM2203,
        SC_TYPE_YM2612,
        SC_TYPE_AY8910,
        SC_TYPE_SN76489,
        SC_TYPE_YM3812,
        SC_TYPE_YMF262,
        SC_TYPE_YM2413,
        SC_TYPE_YM3526,
        SC_TYPE_YMF288,
        SC_TYPE_SCC,
        SC_TYPE_SCCS,
        SC_TYPE_Y8950,
        SC_TYPE_YM2164,     // OPP:OPMとはハードウェアLFOの制御が違う
        SC_TYPE_YM2414,     // OPZ:OPMとピンコンパチ
        SC_TYPE_AY8930,     // APSG:拡張PSG
        SC_TYPE_YM2149,     // SSG:PSGとはDACが違う(YM3439とは同一とみていいと思う)
        SC_TYPE_YMZ294,     // SSGL:SSGとはDACが違う(YMZ284とは同一とみていいと思う)
        SC_TYPE_SN76496,    // DCSG:76489とはノイズジェネレータの生成式が違う
        SC_TYPE_YM2420,     // OPLL2:OPLLとはFnumの設定方法が違う。音は同じ。
        SC_TYPE_YMF281,     // OPLLP:OPLLとは内蔵ROM音色が違う。制御は同じ。
        SC_TYPE_YMF276,     // OPN2L:OPN2/OPN2CとはDACが違う
        SC_TYPE_YM2610B,    // OPNB-B:OPNBとはFM部のch数が違う。
        SC_TYPE_YMF286,     // OPNB-C:OPNBとはDACが違う。
        SC_TYPE_YM2602,     // 315-5124: 76489/76496とはノイズジェネレータの生成式が違う。POWON時に発振しない。
        SC_TYPE_UM3567,     // OPLLのコピー品（だけどDIP24なのでそのままリプレースできない）
        SC_TYPE_YMF274,     // OPL4:試作未定
        SC_TYPE_YM3806,     // OPQ:試作予定
        SC_TYPE_YM2163,     // DSG:試作中
        SC_TYPE_YM7129,     // OPK2:試作中
        SC_TYPE_YMZ280,     // PCM8:ADPCM8ch:試作予定
        SC_TYPE_YMZ705,     // SSGS:SSG*2set+ADPCM8ch:試作中
        SC_TYPE_YMZ735,     // FMS:FM8ch+ADPCM8ch:試作中
        SC_TYPE_YM2423,     // YM2413の音色違い
        SC_TYPE_SPC700,     // SPC700
        SC_TYPE_NBV4,       // NBV4用
        SC_TYPE_AYB02,      // AYB02用
        SC_TYPE_8253,       // i8253（及び互換チップ用）
        SC_TYPE_315_5124,   // DCSG互換チップ
        SC_TYPE_SPPCM,      // SPPCM
        SC_TYPE_C140,       // NAMCO C140(SPPCMデバイス）
        SC_TYPE_SEGAPCM,    // SEGAPCM(SPPCMデバイス）
        SC_TYPE_SPW,        // SPW
        SC_TYPE_SAM2695,    // SAM2695
        SC_TYPE_MIDI,       // MIDIインターフェース
        SC_TYPE_MAX,        // 使用可能デバイスMAX値
                            // 以降は、専用ハード用

        // 実験ハード用
        SC_TYPE_OTHER = 1000,   // その他デバイス用、アドレスがA0-A3で動作する
        SC_TYPE_UNKNOWN,        // 開発デバイス向け
        SC_TYPE_YMF825,			// YMF825（暫定）
    }

    public enum SC_CHIP_CLOCK : uint
    {
        SC_CLOCK_NONE = 0,
        SC_CLOCK_1789773 = 1789773, // SSG,OPN,OPM,SN76489 etc
        SC_CLOCK_1996800 = 1996800, // SSG,OPN,OPM,SN76489 etc
        SC_CLOCK_2000000 = 2000000, // SSG,OPN,OPM,SN76489 etc
        SC_CLOCK_2048000 = 2048000, // SSGLP(4096/2|6144/3)
        SC_CLOCK_3579545 = 3579545, // SSG,OPN,OPM,SN76489 etc
        SC_CLOCK_3993600 = 3993600, // OPN(88)
        SC_CLOCK_4000000 = 4000000, // SSF,OPN,OPM etc
        SC_CLOCK_7159090 = 7159090, // OPN,OPNA,OPNB,OPN2,OPN3L etc
        SC_CLOCK_7670454 = 7670454, // YM-2612 etc
        SC_CLOCK_7987200 = 7987200, // OPNA(88)
        SC_CLOCK_8000000 = 8000000, // OPNB etc
        SC_CLOCK_10738635 = 10738635, // 315-5124
        SC_CLOCK_12500000 = 12500000, // RF5C164
        SC_CLOCK_14318180 = 14318180, // OPL2
        SC_CLOCK_16934400 = 16934400, // YMF271
        SC_CLOCK_23011361 = 23011361, // PWM
    };

}

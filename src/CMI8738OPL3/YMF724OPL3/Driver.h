/*++

Module Name:

    driver.h

Abstract:

    This file contains the driver definitions.

Environment:

    Kernel-mode Driver Framework

--*/

#include <ntddk.h>
#include <wdf.h>
#include <initguid.h>

#include "device.h"
#include "queue.h"
#include "trace.h"

EXTERN_C_START

//
// WDFDRIVER Events
//

DRIVER_INITIALIZE DriverEntry;
EVT_WDF_DRIVER_DEVICE_ADD YMF724OPL3EvtDeviceAdd;
EVT_WDF_OBJECT_CONTEXT_CLEANUP YMF724OPL3EvtDriverContextCleanup;

typedef
UCHAR
(*PREAD_PORT)(
    IN UCHAR* Register
    );

typedef
VOID
(*PWRITE_PORT)(
    IN UCHAR* Register,
    IN UCHAR  Value
    );

__inline
UCHAR
ReadPortUChar(
    IN  UCHAR* x
)
{
    return READ_PORT_UCHAR(x);
}
__inline
VOID
WritePortUChar(
    IN  UCHAR* x,
    IN  UCHAR  y
)
{
    WRITE_PORT_UCHAR(x, y);
}

#define IOCTL_WRITE_DATA_TO_PORT   \
            CTL_CODE(FILE_DEVICE_SOUND, 2048, METHOD_DIRECT_TO_HARDWARE, FILE_WRITE_ACCESS)

#define IOCTL_READ_DATA_TO_PORT   \
            CTL_CODE(FILE_DEVICE_SOUND, 2049, METHOD_DIRECT_TO_HARDWARE, FILE_READ_ACCESS)



/*///////////////////////////////////////////////////////// */
/* */
/*    Global Control Register  */
/* */
/*///////////////////////////////////////////////////////// */
#define		INTERRUPT_FLAG		0x04
#define		ACTIVITY		0x06
#define		GLOBAL_CONTROL		0x08


 /*///////////////////////////////////////////////////////// */
 /* */
 /*    Timer Control Register */
 /* */
 /*///////////////////////////////////////////////////////// */
#define		TIMER_CONTROL		0x10
#define		TIMER_COUNT		0x12


 /*///////////////////////////////////////////////////////// */
 /* */
 /*    SPDIF Control Register */
 /* */
 /*///////////////////////////////////////////////////////// */
#define		SPDIFOUT_CONTROL	0x18
#define		SPDIFOUT_STATUS		0x1c
#define		SPDIFIN_CONTROL		0x34
#define		SPDIFIN_STATUS		0x38

 /*///////////////////////////////////////////////////////// */
 /* */
 /*    EEPROM Control Register */
 /* */
 /*///////////////////////////////////////////////////////// */
#define		EEPROM_CONTROL		0x2c


 /*///////////////////////////////////////////////////////// */
 /* */
 /*    AC3 Control Register */
 /* */
 /*///////////////////////////////////////////////////////// */
#define		AC3_DATA			0x40
#define		AC3_ADDRESS			0x42
#define		AC3_STATUS			0x44


 /*///////////////////////////////////////////////////////// */
 /* */
 /*    AC97 Control Register */
 /* */
 /*///////////////////////////////////////////////////////// */
#define		AC97_CMD_DATA		0x60
#define		AC97_CMD_ADDRESS	0x62
#define		AC97_STATUS_DATA	0x64
#define		AC97_STATUS_ADDRESS	0x66
#define		AC97_SEC_STATUS_DATA	0x68
#define		AC97_SEC_STATUS_ADDR	0x6A
#define		AC97_SEC_CONFIG		0x70

 /*///////////////////////////////////////////////////////// */
 /* */
 /*    Volume Control Register */
 /* */
 /*///////////////////////////////////////////////////////// */
#define		LEGACY_OUTPUT		0x80
#define		LEGACY_OUTPUT_LCH	0x80
#define		LEGACY_OUTPUT_RCH	0x82
#define		NATIVE_DAC		0x84
#define		NATIVE_DAC_LCH		0x84
#define		NATIVE_DAC_RCH		0x86
#define		ZVOUTVOL		0x88
#define		ZV_OUTPUT_LCH		0x88
#define		ZV_OUTPUT_RCH		0x8a
#define		AC3_OUTPUT		0x8c
#define		AC3_OUTPUT_LCH		0x8c
#define		AC3_OUTPUT_RCH		0x8e
#define		ADC_OUTPUT		0x90
#define		ADC_OUTPUT_LCH		0x90
#define		ADC_OUTPUT_RCH		0x92
#define		LEGACY_LOOPBACK		0x94
#define		LEGACY_LOOPBACK_LCH	0x94
#define		LEGACY_LOOPBACK_RCH	0x96
#define		NATIVE_LOOPBACK		0x98
#define		NATIVE_LOOPBACK_LCH	0x98
#define		NATIVE_LOOPBACK_RCH	0x9a
#define		ZVLOOPVOL		0x9c
#define		ZVLOOPBACK_LCH		0x9c
#define		ZVLOOPBACK_RCH		0x9e
#define		AC3_LOOPBACK		0xa0
#define		AC3_LOOPBACK_LCH	0xa0
#define		AC3_LOOPBACK_RCH	0xa2
#define		ADC_LOOPBACK		0xa4
#define		ADC_LOOPBACK_LCH	0xa4
#define		ADC_LOOPBACK_RCH	0xa6
#define		NATIVE_ADC_INPUT	0xa8
#define		NATIVE_ADC_INPUT_LCH	0xa8
#define		NATIVE_ADC_INPUT_RCH	0xaa
#define		NATIVE_REC_INPUT	0xac
#define		NATIVE_REC_INPUT_LCH	0xac
#define		NATIVE_REC_INPUT_RCH	0xae
#define 		BUF441OUTVOL            0xB0
#define 		BUF441OUTVOLL           0xB0
#define 		BUF441OUTVOLR           0xB2
#define 		BUF441LOOPVOL           0xB4
#define 		BUF441LOOPVOLL          0xB4
#define 		BUF441LOOPVOLR          0xB6
#define 		SPDIFOUTVOL             0xB8
#define 		SPDIFOUTVOLL            0xB8
#define 		SPDIFOUTVOLR            0xBA
#define 		SPDIFLOOPVOL            0xBC
#define 		SPDIFLOOPVOLL           0xBC
#define 		SPDIFLOOPVOLR           0xBE


 /*///////////////////////////////////////////////////////// */
 /* */
 /*    Sampling Rate Control Register */
 /* */
 /*///////////////////////////////////////////////////////// */
#define		ADC_SAMPLING_RATE	0xc0
#define		REC_SAMPLING_RATE	0xc4
#define		ADC_FORMAT		0xc8
#define        	REC_FORMAT          	0xcc


 /*///////////////////////////////////////////////////////// */
 /* */
 /*    PCI Native Audio Control Register */
 /* */
 /*///////////////////////////////////////////////////////// */
#define		STATUS			0x100
#define		CONTROL_SELECT		0x104
#define		MODE			0x108
#define		SAMPLE_COUNT		0x10c
#define		NUM_OF_SAMPLES		0x110
#define		CONFIG			0x114
#define		PLAY_CNTRL_SIZE		0x140
#define		REC_CNTRL_SIZE		0x144
#define		EFF_CNTRL_SIZE		0x148
#define		WORK_SIZE		0x14c
#define		MAP_OF_REC		0x150
#define		MAP_OF_EFFECTS		0x154
#define		PLAY_CNTRL_BASE		0x158
#define		REC_CNTRL_BASE		0x15c
#define		EFF_CNTRL_BASE		0x160
#define		WORK_BASE		0x164


 /*///////////////////////////////////////////////////////// */
 /* */
 /*    Instruction RAM */
 /* */
 /*///////////////////////////////////////////////////////// */
#define		DSP_INST_RAM		0x1000
#define		CONTROL_INST_RAM	0x4000


 /*///////////////////////////////////////////////////////// */
 /* */
 /*    Map Length */
 /* */
 /*///////////////////////////////////////////////////////// */
#define		MAP_LENGTH		0x8000



EXTERN_C_END

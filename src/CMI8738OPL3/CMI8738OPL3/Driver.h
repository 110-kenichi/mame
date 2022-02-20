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
EVT_WDF_DRIVER_DEVICE_ADD CMI8738OPL3EvtDeviceAdd;
EVT_WDF_OBJECT_CONTEXT_CLEANUP CMI8738OPL3EvtDriverContextCleanup;


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

EXTERN_C_END

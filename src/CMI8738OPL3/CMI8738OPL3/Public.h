/*++

Module Name:

    public.h

Abstract:

    This module contains the common declarations shared by driver
    and user applications.

Environment:

    user and kernel

--*/

//
// Define an Interface Guid so that apps can find the device and talk to it.
//

DEFINE_GUID (GUID_DEVINTERFACE_CMI8738OPL3,
    0x52d42d43,0x8c67,0x4b79,0xae,0x4c,0x1a,0x5f,0xaa,0xdf,0xbc,0x14);
// {52d42d43-8c67-4b79-ae4c-1a5faadfbc14}

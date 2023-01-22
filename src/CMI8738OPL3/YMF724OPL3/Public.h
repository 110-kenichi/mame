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

DEFINE_GUID (GUID_DEVINTERFACE_YMF724OPL3,
    0xd9ffe190,0x44d1,0x4f45,0xb5,0xf6,0xd5,0x9c,0x01,0x04,0xc8,0x04);
// {d9ffe190-44d1-4f45-b5f6-d59c0104c804}

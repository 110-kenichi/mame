/*++

Module Name:

    driver.c

Abstract:

    This file contains the driver entry points and callbacks.

Environment:

    Kernel-mode Driver Framework

--*/

//パソコンをテストモードに移行する
//http://ashiter.com/?page_id=2473
// 
//Windows10時代のデバイスドライバ開発とデバッグ
//http://nahitafu.cocolog-nifty.com/nahitafu/2017/08/windows10-a5bb.html

//デバイスドライバーを完全に削除する - デバイスドライバのドライバストアからの削除 (Windows Tips)
//https://www.ipentec.com/document/windows-delete-device-driver-in-driver-store

#include "driver.h"
#include "driver.tmh"

#ifdef __INTELLISENSE__
#undef TraceEvents
#define TraceEvents(...)
#endif

#ifdef ALLOC_PRAGMA
#pragma alloc_text (INIT, DriverEntry)
#pragma alloc_text (PAGE, CMI8738OPL3EvtDeviceAdd)
#pragma alloc_text (PAGE, CMI8738OPL3EvtDriverContextCleanup)
#endif

NTSTATUS
DriverEntry(
    _In_ PDRIVER_OBJECT  DriverObject,
    _In_ PUNICODE_STRING RegistryPath
    )
/*++

Routine Description:
    DriverEntry initializes the driver and is the first routine called by the
    system after the driver is loaded. DriverEntry specifies the other entry
    points in the function driver, such as EvtDevice and DriverUnload.

Parameters Description:

    DriverObject - represents the instance of the function driver that is loaded
    into memory. DriverEntry must initialize members of DriverObject before it
    returns to the caller. DriverObject is allocated by the system before the
    driver is loaded, and it is released by the system after the system unloads
    the function driver from memory.

    RegistryPath - represents the driver specific path in the Registry.
    The function driver can use the path to store driver related data between
    reboots. The path does not store hardware instance specific data.

Return Value:

    STATUS_SUCCESS if successful,
    STATUS_UNSUCCESSFUL otherwise.

--*/
{
    WDF_DRIVER_CONFIG config;
    NTSTATUS status;
    WDF_OBJECT_ATTRIBUTES attributes;

    //
    // Initialize WPP Tracing
    //
    WPP_INIT_TRACING(DriverObject, RegistryPath);

    TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_DRIVER, "%!FUNC! Entry");
    //DbgPrint("CMI8738OPL3 DriverEntry Entry\r\n");

    //
    // Register a cleanup callback so that we can call WPP_CLEANUP when
    // the framework driver object is deleted during driver unload.
    //
    WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
    attributes.EvtCleanupCallback = CMI8738OPL3EvtDriverContextCleanup;

    WDF_DRIVER_CONFIG_INIT(&config,
                           CMI8738OPL3EvtDeviceAdd
                           );

    status = WdfDriverCreate(DriverObject,
                             RegistryPath,
                             &attributes,
                             &config,
                             WDF_NO_HANDLE
                             );

    if (!NT_SUCCESS(status)) {
        TraceEvents(TRACE_LEVEL_ERROR, TRACE_DRIVER, "WdfDriverCreate failed %!STATUS!", status);
        //DbgPrint("CMI8738OPL3 WdfDriverCreate failed(%d) \r\n", status);
        WPP_CLEANUP(DriverObject);
        return status;
    }

    TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_DRIVER, "%!FUNC! Exit");
    //DbgPrint("CMI8738OPL3 DriverEntry Exit\r\n");

    return status;
}


NTSTATUS
MapHWResources(
    IN OUT PDEVICE_CONTEXT deviceContext,
    IN WDFCMRESLIST  ResourcesRaw,
    IN WDFCMRESLIST  ResourcesTranslated
)
/*++
Routine Description:

    Gets the HW resources assigned by the bus driver and:
    1) Maps them to system address space.
    2) If PCIDRV_CREATE_INTERRUPT_IN_PREPARE_HARDWARE is defined,
        it creates a WDFINTERRUPT object.

    Called during EvtDevicePrepareHardware callback.

    Three base address registers are supported by the 8255x:
    2) CSR I/O Mapped Base Address Register (BAR 1 at offset 14)

    The 8255x requires one BAR for I/O mapping and one BAR for memory
    mapping of these registers anywhere within the 32-bit memory address space.
    The driver determines which BAR (I/O or Memory) is used to access the
    Control/Status Registers.

    Just for illustration, this driver maps both memory and I/O registers and
    shows how to use READ_PORT_xxx or READ_REGISTER_xxx functions to perform
    I/O in a platform independent basis. On some platforms, the I/O registers
    can get mapped into memory space and your driver should be able to handle
    this transparently.

Arguments:

    FdoData     Pointer to our FdoData
    ResourcesRaw - Pointer to list of raw resources passed to
                        EvtDevicePrepareHardware callback
    ResourcesTranslated - Pointer to list of translated resources passed to
                        EvtDevicePrepareHardware callback

Return Value:

    NTSTATUS

--*/
{
    PCM_PARTIAL_RESOURCE_DESCRIPTOR descriptor;
    ULONG       i;
    NTSTATUS    status = STATUS_SUCCESS;
    BOOLEAN     bResPort = FALSE;

    UNREFERENCED_PARAMETER(ResourcesRaw);

    PAGED_CODE();

    for (i = 0; i < WdfCmResourceListGetCount(ResourcesTranslated); i++) {

        descriptor = WdfCmResourceListGetDescriptor(ResourcesTranslated, i);

        if (!descriptor) {
            return STATUS_DEVICE_CONFIGURATION_ERROR;
        }

        switch (descriptor->Type) {

        case CmResourceTypePort:
            //
            // The port is in I/O space on this machine.
            // We should use READ_PORT_Xxx, and WRITE_PORT_Xxx routines
            // to read or write to the port.
            //

            deviceContext->IoBaseAddress = descriptor->u.Port.Start.LowPart;
            deviceContext->IoRange = descriptor->u.Port.Length;

            DbgPrint("CMI8738OPL3 MapHWResources BAR = %xH\r\n", deviceContext->IoBaseAddress);
            DbgPrint("CMI8738OPL3 MapHWResources Len = %d\r\n", deviceContext->IoRange);

            bResPort = TRUE;
            break;

        default:
            //
            // This could be device-private type added by the PCI bus driver. We
            // shouldn't filter this or change the information contained in it.
            //
            break;
        }

    }

    //
    // Make sure we got all the 3 resources to work with.
    //
    if (!(bResPort)) {
        status = STATUS_DEVICE_CONFIGURATION_ERROR;
        return status;
    }

    return status;

}

NTSTATUS
PciDrvEvtDevicePrepareHardware(
    WDFDEVICE      Device,
    WDFCMRESLIST   Resources,
    WDFCMRESLIST   ResourcesTranslated
)
/*++

Routine Description:

    EvtDeviceStart event callback performs operations that are necessary
    to make the driver's device operational. The framework calls the driver's
    EvtDeviceStart callback when the PnP manager sends an IRP_MN_START_DEVICE
    request to the driver stack.

Arguments:

    Device - Handle to a framework device object.

    Resources - Handle to a collection of framework resource objects.
                This collection identifies the raw (bus-relative) hardware
                resources that have been assigned to the device.

    ResourcesTranslated - Handle to a collection of framework resource objects.
                This collection identifies the translated (system-physical)
                hardware resources that have been assigned to the device.
                The resources appear from the CPU's point of view.
                Use this list of resources to map I/O space and
                device-accessible memory into virtual address space

Return Value:

    WDF status code

--*/
{
    NTSTATUS     status = STATUS_SUCCESS;
    PDEVICE_CONTEXT contextData = NULL;

    UNREFERENCED_PARAMETER(Resources);
    UNREFERENCED_PARAMETER(ResourcesTranslated);

    PAGED_CODE();

    contextData = DeviceGetContext(Device);

    status = MapHWResources(contextData, Resources, ResourcesTranslated);
    if (!NT_SUCCESS(status)) {
        return status;
    }

    return status;

}


NTSTATUS
PciDrvEvtDeviceReleaseHardware(
    IN  WDFDEVICE    Device,
    IN  WDFCMRESLIST ResourcesTranslated
)
/*++

Routine Description:

    EvtDeviceReleaseHardware is called by the framework whenever the PnP manager
    is revoking ownership of our resources.  This may be in response to either
    IRP_MN_STOP_DEVICE or IRP_MN_REMOVE_DEVICE.  The callback is made before
    passing down the IRP to the lower driver.

    In this callback, do anything necessary to free those resources.

Arguments:

    Device - Handle to a framework device object.

    ResourcesTranslated - Handle to a collection of framework resource objects.
                This collection identifies the translated (system-physical)
                hardware resources that have been assigned to the device.
                The resources appear from the CPU's point of view.
                Use this list of resources to map I/O space and
                device-accessible memory into virtual address space

Return Value:

    NTSTATUS - Failures will be logged, but not acted on.

--*/
{
    UNREFERENCED_PARAMETER(Device);
    UNREFERENCED_PARAMETER(ResourcesTranslated);

    PAGED_CODE();

    return STATUS_SUCCESS;
}


NTSTATUS
CMI8738OPL3EvtDeviceAdd(
    _In_    WDFDRIVER       Driver,
    _Inout_ PWDFDEVICE_INIT DeviceInit
    )
/*++
Routine Description:

    EvtDeviceAdd is called by the framework in response to AddDevice
    call from the PnP manager. We create and initialize a device object to
    represent a new instance of the device.

Arguments:

    Driver - Handle to a framework driver object created in DriverEntry

    DeviceInit - Pointer to a framework-allocated WDFDEVICE_INIT structure.

Return Value:

    NTSTATUS

--*/
{
    NTSTATUS status;
    WDF_PNPPOWER_EVENT_CALLBACKS    pnpPowerCallbacks;

    UNREFERENCED_PARAMETER(Driver);

    PAGED_CODE();

    TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_DRIVER, "%!FUNC! Entry");
    //DbgPrint("CMI8738OPL3 EvtDeviceAdd Start\r\n");

    WDF_PNPPOWER_EVENT_CALLBACKS_INIT(&pnpPowerCallbacks);
    pnpPowerCallbacks.EvtDevicePrepareHardware = PciDrvEvtDevicePrepareHardware;
    pnpPowerCallbacks.EvtDeviceReleaseHardware = PciDrvEvtDeviceReleaseHardware;
    WdfDeviceInitSetPnpPowerEventCallbacks(DeviceInit, &pnpPowerCallbacks);

    status = CMI8738OPL3CreateDevice(DeviceInit);

    //DbgPrint("CMI8738OPL3 EvtDeviceAdd End\r\n");
    TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_DRIVER, "%!FUNC! Exit");

    return status;
}

VOID
CMI8738OPL3EvtDriverContextCleanup(
    _In_ WDFOBJECT DriverObject
    )
/*++
Routine Description:

    Free all the resources allocated in DriverEntry.

Arguments:

    DriverObject - handle to a WDF Driver object.

Return Value:

    VOID.

--*/
{
    UNREFERENCED_PARAMETER(DriverObject);

    PAGED_CODE();

    TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_DRIVER, "%!FUNC! Entry");

    //
    // Stop WPP Tracing
    //
    WPP_CLEANUP(WdfDriverWdmGetDriverObject((WDFDRIVER)DriverObject));
}

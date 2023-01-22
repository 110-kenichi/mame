/*++

Module Name:

	driver.c

Abstract:

	This file contains the driver entry points and callbacks.

Environment:

	Kernel-mode Driver Framework

--*/

#include "driver.h"
#include "driver.tmh"

#ifdef __INTELLISENSE__
#undef TraceEvents
#define TraceEvents(...)
#endif

#ifdef ALLOC_PRAGMA
#pragma alloc_text (INIT, DriverEntry)
#pragma alloc_text (PAGE, YMF724OPL3EvtDeviceAdd)
#pragma alloc_text (PAGE, YMF724OPL3EvtDriverContextCleanup)
#endif

#define WRITEB(a,d) bar0va->bRegister[a] = d
#define READB(a) bar0va^>bRegister[a]
#define WRITEW(a,d) devc->wRegister[a>>1] = d
#define READW(a) devc->wRegister[a>>1]
#define WRITEL(a,d) devc->dwRegister[a>>2] = d
#define READL(a) (devc->dwRegister[a>>2])

typedef struct ymf7xx_devc
{
	unsigned int* base0virt;
	volatile unsigned int* dwRegister;
	volatile unsigned short* wRegister;
	volatile unsigned char* bRegister;
} ymf7xx_devc;

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

	//
	// Register a cleanup callback so that we can call WPP_CLEANUP when
	// the framework driver object is deleted during driver unload.
	//
	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.EvtCleanupCallback = YMF724OPL3EvtDriverContextCleanup;

	WDF_DRIVER_CONFIG_INIT(&config,
		YMF724OPL3EvtDeviceAdd
	);

	status = WdfDriverCreate(DriverObject,
		RegistryPath,
		&attributes,
		&config,
		WDF_NO_HANDLE
	);

	if (!NT_SUCCESS(status)) {
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_DRIVER, "WdfDriverCreate failed %!STATUS!", status);
		WPP_CLEANUP(DriverObject);
		return status;
	}

	/*
	PHYSICAL_ADDRESS portAddr;
	ULONG busNumber = (ULONG)-1;
	
	CM_RESOURCE_LIST driverList;
	driverList.Count = 1;
	driverList.List[0].PartialResourceList.Count = 1;
	driverList.List[0].BusNumber = busNumber;
	driverList.List[0].InterfaceType = PCIBus;
	driverList.List[0].PartialResourceList.Version = 1;
	driverList.List[0].PartialResourceList.Revision = 1;
	driverList.List[0].PartialResourceList.PartialDescriptors[0].Type = CmResourceTypePort;
	driverList.List[0].PartialResourceList.PartialDescriptors[0].Flags = CM_RESOURCE_PORT_IO | CM_RESOURCE_PORT_16_BIT_DECODE;
	//driverList.List[0].PartialResourceList.PartialDescriptors[0].ShareDisposition = CmResourceShareDeviceExclusive;
	driverList.List[0].PartialResourceList.PartialDescriptors[0].ShareDisposition = CmResourceShareShared; // CmResourceShareDeviceExclusive;
	portAddr.QuadPart = 0x388;
	driverList.List[0].PartialResourceList.PartialDescriptors[0].u.Port.Start = portAddr;
	driverList.List[0].PartialResourceList.PartialDescriptors[0].u.Port.Length = 4;

	//IO_RESOURCE_REQUIREMENTS_LIST ioList;
	//ioList.InterfaceType = PCIBus;
	//ioList.ListSize = 1;
	//ioList.List[0].Count = 1;
	//ioList.List[0].Version = 1;
	//ioList.List[0].Revision = 1;
	//ioList.List[0].Descriptors[0].Type = CmResourceTypePort;
	//ioList.List[0].Descriptors[0].Flags = CM_RESOURCE_PORT_IO | CM_RESOURCE_PORT_16_BIT_DECODE;
	//ioList.List[0].Descriptors[0].ShareDisposition = CmResourceShareShared; // CmResourceShareDeviceExclusive;
	//ioList.List[0].Descriptors[0].u.Port.Length = 4;
	//ioList.List[0].Descriptors[0].u.Port.Alignment = FILE_BYTE_ALIGNMENT;
	//portAddr.QuadPart = 0x388;
	//ioList.List[0].Descriptors[0].u.Port.MinimumAddress = portAddr;
	//portAddr.LowPart += 4 - 1;
	//ioList.List[0].Descriptors[0].u.Port.MaximumAddress = portAddr;

	//PCM_RESOURCE_LIST pdriverList = &driverList;
	//status = IoAssignResources(RegistryPath, NULL, DriverObject, NULL, &ioList, &pdriverList);
	//if (!NT_SUCCESS(status)) {
	//	DbgPrint("YMF724OPL3 IoAssignResources Failed(%x)\r\n", status);
	//}

	BOOLEAN conflict = FALSE;
	status = IoReportResourceForDetection(
		DriverObject,
		&driverList,
		1,
		NULL,
		NULL,
		0,
		&conflict);
	if (!NT_SUCCESS(status)) {
		DbgPrint("YMF724OPL3 IoReportResourceForDetection Failed(%x)\r\n", status);
	}
	if (conflict) {
		DbgPrint("YMF724OPL3 IoReportResourceForDetection Conflicted\r\n");
		return STATUS_CONFLICTING_ADDRESSES;
	}
	*/

	TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_DRIVER, "%!FUNC! Exit");

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
	//BOOLEAN     bResPort = FALSE;

	UNREFERENCED_PARAMETER(deviceContext);
	UNREFERENCED_PARAMETER(ResourcesRaw);

	PAGED_CODE();

	for (i = 0; i < WdfCmResourceListGetCount(ResourcesTranslated); i++) {

		descriptor = WdfCmResourceListGetDescriptor(ResourcesTranslated, i);

		if (!descriptor) {
			return STATUS_DEVICE_CONFIGURATION_ERROR;
		}

		switch (descriptor->Type) {

		case CmResourceTypeMemory:
		{
			deviceContext->MemoryBaseAddress = descriptor->u.Memory.Start;
			deviceContext->MemoryRange = descriptor->u.Memory.Length;

			PULONG bar0va = (PULONG)MmMapIoSpace(descriptor->u.Memory.Start, descriptor->u.Memory.Length, MmNonCached);

			ymf7xx_devc sdevc;
			ymf7xx_devc *devc = &sdevc;

			devc->base0virt = (unsigned int*)bar0va;
			devc->dwRegister = (unsigned int*)devc->base0virt;
			devc->wRegister = (unsigned short*)devc->base0virt;
			devc->bRegister = (unsigned char*)devc->base0virt;

			WRITEL(NATIVE_DAC, 0x00000000);
			WRITEL(MODE, 0x00010000);
			WRITEL(MODE, 0x00000000);
			WRITEL(MAP_OF_REC, 0x00000000);
			WRITEL(MAP_OF_EFFECTS, 0x00000000);
			WRITEL(PLAY_CNTRL_BASE, 0x00000000);
			WRITEL(REC_CNTRL_BASE, 0x00000000);
			WRITEL(EFF_CNTRL_BASE, 0x00000000);
			WRITEL(CONTROL_SELECT, 1);
			WRITEL(GLOBAL_CONTROL, READL(GLOBAL_CONTROL) & ~0x0007);
			WRITEL(ZVOUTVOL, 0xFFFFFFFF);
			WRITEL(ZVLOOPVOL, 0xFFFFFFFF);
			WRITEL(SPDIFOUTVOL, 0xFFFFFFFF);
			WRITEL(SPDIFLOOPVOL, 0x3FFF3FFF);
			WRITEL(ZVOUTVOL, 0xFFFFFFFF);
			WRITEL(ZVLOOPVOL, 0xFFFFFFFF);
			WRITEL(SPDIFOUTVOL, 0xFFFFFFFF);
			WRITEL(SPDIFLOOPVOL, 0x3FFF3FFF);
			WRITEL(LEGACY_OUTPUT, 0xFFFFFFFF);

			DbgPrint("YMF724OPL3 MapHWResources Mem = %xH\r\n", deviceContext->MemoryBaseAddress.LowPart);
			DbgPrint("YMF724OPL3 MapHWResources Len = %d\r\n", deviceContext->MemoryRange);

			break;
		}
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
	//if (!(bResPort)) {
	//	status = STATUS_DEVICE_CONFIGURATION_ERROR;
	//	return status;
	//}

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
		DbgPrint("YMF724OPL3 MapHWResources Failed(%x)\r\n", status);
		return status;
	}

	return status;

}

/// <summary>
/// 
/// </summary>
/// <param name="Device"></param>
/// <param name="RelationType"></param>
/// <returns></returns>
NTSTATUS
PciDrvEvtRelationQuery(
	IN WDFDEVICE Device,
	IN DEVICE_RELATION_TYPE RelationType
)
{
	UNREFERENCED_PARAMETER(Device);
	UNREFERENCED_PARAMETER(RelationType);

	PAGED_CODE();

	return STATUS_SUCCESS;
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
YMF724OPL3EvtDeviceAdd(
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
	WDF_PDO_EVENT_CALLBACKS    pdoCallbacks;

	UNREFERENCED_PARAMETER(Driver);

	PAGED_CODE();

	TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_DRIVER, "%!FUNC! Entry");

	//https://cwiki.apache.org/confluence/display/NUTTX/Device+Drivers+vs.+Bus+Drivers+and+GPIO+Drivers
	//https://github.com/spurious/usbip-windows-mirror/blob/master/driver/buspdo.c
	WDF_PDO_EVENT_CALLBACKS_INIT(&pdoCallbacks);
	//pdoCallbacks.EvtDeviceResourcesQuery
	WdfPdoInitSetEventCallbacks(DeviceInit, &pdoCallbacks);

	WDF_PNPPOWER_EVENT_CALLBACKS_INIT(&pnpPowerCallbacks);
	pnpPowerCallbacks.EvtDevicePrepareHardware = PciDrvEvtDevicePrepareHardware;
	pnpPowerCallbacks.EvtDeviceReleaseHardware = PciDrvEvtDeviceReleaseHardware;
	pnpPowerCallbacks.EvtDeviceRelationsQuery = PciDrvEvtRelationQuery;
	WdfDeviceInitSetPnpPowerEventCallbacks(DeviceInit, &pnpPowerCallbacks);

	status = YMF724OPL3CreateDevice(Driver, DeviceInit);

	TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_DRIVER, "%!FUNC! Exit");

	return status;
}

VOID
YMF724OPL3EvtDriverContextCleanup(
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

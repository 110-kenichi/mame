/*++

Module Name:

	device.c - Device handling events for example driver.

Abstract:

   This file contains the device entry points and callbacks.

Environment:

	Kernel-mode Driver Framework

--*/

#include "driver.h"
#include "device.tmh"

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, YMF724OPL3CreateDevice)
#endif


//http://www.hollistech.com/Resources/Misc articles/getbusdata.htm

NTSTATUS HtsGenericCompletion(
	IN PDEVICE_OBJECT   DeviceObject,
	IN PIRP             Irp,
	IN PVOID            Context
)
{
	UNREFERENCED_PARAMETER(DeviceObject);
	UNREFERENCED_PARAMETER(Irp);

	KeSetEvent((PKEVENT)Context, IO_NO_INCREMENT, FALSE);
	return STATUS_MORE_PROCESSING_REQUIRED; // Keep this IRP
}

/// <summary>
/// 
/// </summary>
/// <param name="DeviceObject"></param>
/// <param name="WhichSpace"></param>
/// <param name="Buffer"></param>
/// <param name="Offset"></param>
/// <param name="Length"></param>
/// <param name="ReadConfig"></param>
/// <returns></returns>
NTSTATUS ReadWritePciConfig(
	PDEVICE_OBJECT DeviceObject,
	ULONG WhichSpace,
	PVOID Buffer,
	ULONG Offset,
	ULONG Length,
	BOOLEAN ReadConfig)
{
	ASSERT(DeviceObject);
	ASSERT(Buffer);
	ASSERT(Length);

	PIRP Irp = IoAllocateIrp(DeviceObject->StackSize, FALSE);

	if (!Irp) {
		return STATUS_INSUFFICIENT_RESOURCES;
	}

	Irp->IoStatus.Status = STATUS_NOT_SUPPORTED;

	KEVENT Event;
	KeInitializeEvent(&Event, NotificationEvent, FALSE);

	IoSetCompletionRoutine(Irp, HtsGenericCompletion,
		&Event, TRUE, TRUE, TRUE);

	PIO_STACK_LOCATION IoStack = IoGetNextIrpStackLocation(Irp);
	IoStack->MajorFunction = IRP_MJ_PNP;
	IoStack->MinorFunction = ReadConfig ? IRP_MN_READ_CONFIG : IRP_MN_WRITE_CONFIG;
	IoStack->Parameters.ReadWriteConfig.WhichSpace = WhichSpace;
	IoStack->Parameters.ReadWriteConfig.Buffer = Buffer;
	IoStack->Parameters.ReadWriteConfig.Offset = Offset;
	IoStack->Parameters.ReadWriteConfig.Length = Length;

	if (ReadConfig) {
		RtlZeroMemory(Buffer, Length);
	}

	NTSTATUS Status = IoCallDriver(DeviceObject, Irp);
	if (Status == STATUS_PENDING) {
		LARGE_INTEGER timeout;
		timeout.QuadPart = 10 * 1000 * 1000 * 10;	// 10sec
		KeWaitForSingleObject(&Event, Executive, KernelMode, FALSE, &timeout);
		Status = Irp->IoStatus.Status;
	}

	IoFreeIrp(Irp);

	return Status;
}

/// <summary>
/// 
/// </summary>
/// <param name="pDriverObj"></param>
/// <param name="pDevObj"></param>
/// <param name="portBase"></param>
/// <param name="portSpan"></param>
/// <param name="Irq"></param>
/// <returns></returns>
NTSTATUS ClaimHardware(PDRIVER_OBJECT pDriverObj,
	ULONG portBase,
	ULONG portSpan)
	//,ULONG Irq)
{
	UNREFERENCED_PARAMETER(pDriverObj);
	UNREFERENCED_PARAMETER(portBase);
	UNREFERENCED_PARAMETER(portSpan);

	NTSTATUS status = 0;
	/*
	PHYSICAL_ADDRESS maxPortAddr;
	PIO_RESOURCE_REQUIREMENTS_LIST pRRList = NULL;

	SIZE_T rrSize = sizeof(IO_RESOURCE_REQUIREMENTS_LIST) +
		sizeof(IO_RESOURCE_DESCRIPTOR);
	pRRList = (PIO_RESOURCE_REQUIREMENTS_LIST)
		ExAllocatePool2(POOL_FLAG_PAGED, rrSize, (ULONG)'opl3');
	if (pRRList == NULL) {
		DbgPrint("YMF724OPL3 ClaimHardware ExAllocatePool Failed.\r\n" );
		return STATUS_MEMORY_NOT_ALLOCATED;
	}

	pRRList->ListSize = (ULONG)rrSize;
	pRRList->AlternativeLists = 1; // only 1 Resource List
	pRRList->InterfaceType = PCIBus;
	pRRList->List[0].Version = 1;
	pRRList->List[0].Revision = 1;
	pRRList->List[0].Count = 1; //2; // 1 Resource Descriptors: port //& irq
	pRRList->List[0].Descriptors[0].Type = CmResourceTypePort;
	pRRList->List[0].Descriptors[0].ShareDisposition = CmResourceShareDeviceExclusive;
	pRRList->List[0].Descriptors[0].Flags = CM_RESOURCE_PORT_IO;
	pRRList->List[0].Descriptors[0].u.Port.Length = portSpan;
	pRRList->List[0].Descriptors[0].u.Port.Alignment = FILE_BYTE_ALIGNMENT;
	maxPortAddr.QuadPart = portBase;
	pRRList->List[0].Descriptors[0].u.Port.MinimumAddress = maxPortAddr;
	maxPortAddr.LowPart += portSpan - 1;
	pRRList->List[0].Descriptors[0].u.Port.MaximumAddress = maxPortAddr;
	//リソースリストにない割込みを使用する方法 RRS feed
	//https://social.msdn.microsoft.com/Forums/ja-JP/6df9d7f2-9286-498d-aeae-a81bc4e9dbb5?forum=windowsgeneraldevelopmentissuesja
	//pRRList->List[0].Descriptors[1].Type = CmResourceTypeInterrupt;
	//pRRList->List[0].Descriptors[1].ShareDisposition = CmResourceShareDeviceExclusive;
	//pRRList->List[0].Descriptors[1].Flags = CM_RESOURCE_INTERRUPT_LATCHED;
	//pRRList->List[0].Descriptors[1].u.Interrupt.MinimumVector = Irq;
	//pRRList->List[0].Descriptors[1].u.Interrupt.MaximumVector = Irq;

	ULONG busNumber = (ULONG)-1;
	ULONG slotNumber = (ULONG)-1;
	//ULONG ResultLength = 0;
	//status = IoGetDeviceProperty(pDevObj, DevicePropertyBusNumber, sizeof(ULONG), (PVOID)&busNumber, &ResultLength);
	//if (!NT_SUCCESS(status)) {
	//	DbgPrint("YMF724OPL3 ClaimHardware IoGetDeviceProperty Failed(%x)\r\n", status);
	//}
	//else
	{
		PDEVICE_OBJECT pDevObj2 = NULL;
		status = IoReportDetectedDevice(
			pDriverObj, // DriverObject
			PCIBus, // Bus type
			busNumber, // Bus number
			slotNumber, // SlotNumber
			NULL, // Driver RESOURCE_LIST
			pRRList, // Device Resource List
			FALSE, // Already claimed?
			&pDevObj2); // device object
		if (!NT_SUCCESS(status)) {
			DbgPrint("YMF724OPL3 ClaimHardware IoReportDetectedDevice Failed(%x)\r\n", status);
		}
	}
	ExFreePool(pRRList);
	*/
	//*
	PHYSICAL_ADDRESS portAddr;
	ULONG busNumber = (ULONG)-1;

	CM_RESOURCE_LIST driverList;
	driverList.Count = 1;
	driverList.List[0].BusNumber = busNumber;
	driverList.List[0].InterfaceType = PCIBus;
	driverList.List[0].PartialResourceList.Version = 1;
	driverList.List[0].PartialResourceList.Revision = 1;
	driverList.List[0].PartialResourceList.Count = 1;
	driverList.List[0].PartialResourceList.PartialDescriptors[0].Type = CmResourceTypePort;
	driverList.List[0].PartialResourceList.PartialDescriptors[0].Flags = CM_RESOURCE_PORT_IO | CM_RESOURCE_PORT_16_BIT_DECODE;
	driverList.List[0].PartialResourceList.PartialDescriptors[0].ShareDisposition = CmResourceShareShared; // CmResourceShareDeviceExclusive;
	portAddr.QuadPart = portBase;
	driverList.List[0].PartialResourceList.PartialDescriptors[0].u.Port.Start = portAddr;
	driverList.List[0].PartialResourceList.PartialDescriptors[0].u.Port.Length = portSpan;

	BOOLEAN conflict = FALSE;
		status = IoReportResourceForDetection(
		pDriverObj,
		&driverList,
		1,
		NULL,
		NULL,
		0,
		&conflict);
	if (!NT_SUCCESS(status)) {
		DbgPrint("YMF724OPL3 ClaimHardware IoReportResourceForDetection Failed(%x)\r\n", status);
	}
	if (conflict) {
		DbgPrint("YMF724OPL3 ClaimHardware IoReportResourceForDetection Conflicted\r\n");
		return STATUS_CONFLICTING_ADDRESSES;
	}
	//*/
	// Result: status = STATUS_NO_SUCH_DEVICE

	/*
	//  We need a DpcForIsr registration
	IoInitializeDpcRequest(pDevObj, DpcForIsr);

	//  Create & connect to an Interrupt object
	//  To make interrupts real, we must translate irq into
	//  a HAL irq and vector (with processor affinity)
	KIRQL kIrql;
	KAFFINITY kAffinity; ULONG kVector =
		HalGetInterruptVector(Internal, 0, pDevExt->Irq, 0,
			&kIrql, &kAffinity);
	status = IoConnectInterrupt(
		&pDevExt->pIntObj, // the Interrupt object
		Isr, // our ISR
		pDevExt, // Service Context
		NULL, // no spin lock
		kVector, // vector
		kIrql, // DIRQL
		kIrql, // DIRQL
		LevelSensitive, // Latched or Level
		TRUE, // Shared?
		-1, // processors in an MP set
		FALSE ); // save FP registers?
		if (!NT_SUCCESS(status)) {
			// if it fails now, must delete Device object IoDeleteDevice( pDevObj );
			return status;
		}
	*/

	return status;
}

NTSTATUS
YMF724OPL3CreateDevice(
	_In_    WDFDRIVER       Driver,
	_Inout_ PWDFDEVICE_INIT DeviceInit
)
/*++

Routine Description:

	Worker routine called to create a device and its software resources.

Arguments:

	DeviceInit - Pointer to an opaque init structure. Memory for this
					structure will be freed by the framework when the WdfDeviceCreate
					succeeds. So don't access the structure after that point.

Return Value:

	NTSTATUS

--*/
{
	WDF_OBJECT_ATTRIBUTES deviceAttributes;
	PDEVICE_CONTEXT deviceContext;
	WDFDEVICE device;
	NTSTATUS status;
	//PCI_COMMON_CONFIG pciConfig;

	PAGED_CODE();

	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&deviceAttributes, DEVICE_CONTEXT);

	//  Since this driver controlls real hardware,
	//  the hardware controlled must be discovered.
	//  Chapter 9 will discuss auto-detection,
	//  but for now we will hard-code the hardware
	//  resource for the common printer port.
	//  We use IoReportResourceForDetection to mark
	//  PORTs and IRQs as "in use."
	//  This call will fail if another driver
	//  (such as the standard parallel driver(s))
	//  already control the hardware
	//PDRIVER_OBJECT pdo = WdfDriverWdmGetDriverObject(Driver);
	//status = ClaimHardware(pdo,
	//	0x388,
	//	4);
	//if (!NT_SUCCESS(status)) {
	//	// if it fails now, must delete Device object IoDeleteDevice( pDevObj );
	//	DbgPrint("YMF724OPL3 ClaimHardware Failed(%x)\r\n", status);
	//	return status;
	//}

	status = WdfDeviceCreate(&DeviceInit, &deviceAttributes, &device);
	if (NT_SUCCESS(status)) {
		//
		// Get a pointer to the device context structure that we just associated
		// with the device object. We define this structure in the device.h
		// header file. DeviceGetContext is an inline function generated by
		// using the WDF_DECLARE_CONTEXT_TYPE_WITH_NAME macro in device.h.
		// This function will do the type checking and return the device context.
		// If you pass a wrong object handle it will return NULL and assert if
		// run under framework verifier mode.
		//
		deviceContext = DeviceGetContext(device);

		//
		// Initialize the context.
		//
		deviceContext->WdfDevice = device;

		//
		// Create a device interface so that applications can find and talk
		// to us.
		//
		status = WdfDeviceCreateDeviceInterface(
			device,
			&GUID_DEVINTERFACE_YMF724OPL3,
			NULL // ReferenceString
		);

		if (NT_SUCCESS(status)) {
			//
			// Initialize the I/O Package and any Queues
			//
			status = YMF724OPL3QueueInitialize(device);
		}
		else {
			DbgPrint("YMF724OPL3 Queue Initialize Failed\r\n");
		}

		UCHAR   DeviceSpecific;

		//RESET
		status = ReadWritePciConfig(WdfDeviceWdmGetDeviceObject(device), PCI_WHICHSPACE_CONFIG, &DeviceSpecific, 0x48, sizeof(DeviceSpecific), TRUE);
		if (!NT_SUCCESS(status)) {
			DbgPrint("YMF724OPL3 ReadPciConfig RESET1 Failed(%x)\r\n", status);
			return status;
		}
		if (DeviceSpecific & 0x03)
		{
			UCHAR wd = 0;
			wd = DeviceSpecific & 0xFC;
			status = ReadWritePciConfig(WdfDeviceWdmGetDeviceObject(device), PCI_WHICHSPACE_CONFIG, &wd, 0x48, sizeof(wd), FALSE);
			if (!NT_SUCCESS(status)) {
				DbgPrint("YMF724OPL3 ReadPciConfig RESET1 Failed(%x)\r\n", status);
				return status;
			}
			wd = DeviceSpecific | 0x03;
			status = ReadWritePciConfig(WdfDeviceWdmGetDeviceObject(device), PCI_WHICHSPACE_CONFIG, &wd, 0x48, sizeof(wd), FALSE);
			if (!NT_SUCCESS(status)) {
				DbgPrint("YMF724OPL3 ReadPciConfig RESET1 Failed(%x)\r\n", status);
				return status;
			}
			wd = DeviceSpecific & 0xFC;
			status = ReadWritePciConfig(WdfDeviceWdmGetDeviceObject(device), PCI_WHICHSPACE_CONFIG, &wd, 0x48, sizeof(wd), FALSE);
			if (!NT_SUCCESS(status)) {
				DbgPrint("YMF724OPL3 ReadPciConfig RESET1 Failed(%x)\r\n", status);
				return status;
			}
		}

		//Enable 16bit I/O addressing
		//Enable LAD
		// 
		//status = ReadWritePciConfig(WdfDeviceWdmGetDeviceObject(device), PCI_WHICHSPACE_CONFIG, &DeviceSpecific, 0x40, sizeof(DeviceSpecific), TRUE);
		//if (!NT_SUCCESS(status)) {
		//	DbgPrint("YMF724OPL3 ReadPciConfig0 Failed(%x)\r\n", status);
		//	return status;
		//}
		DeviceSpecific = 0x02;
		status = ReadWritePciConfig(WdfDeviceWdmGetDeviceObject(device), PCI_WHICHSPACE_CONFIG, &DeviceSpecific, 0x40, sizeof(DeviceSpecific), FALSE);
		if (!NT_SUCCESS(status)) {
			DbgPrint("YMF724OPL3 WritePciConfig0 Failed(%x)\r\n", status);
			return status;
		}

		////Enable LAD
		//status = ReadWritePciConfig(WdfDeviceWdmGetDeviceObject(device), PCI_WHICHSPACE_CONFIG, &DeviceSpecific, 0x41, sizeof(DeviceSpecific), TRUE);
		//if (!NT_SUCCESS(status)) {
		//	DbgPrint("YMF724OPL3 ReadPciConfig1 Failed(%x)\r\n", status);
		//	return status;
		//}
		//DeviceSpecific = DeviceSpecific & 0x7f;
		//status = ReadWritePciConfig(WdfDeviceWdmGetDeviceObject(device), PCI_WHICHSPACE_CONFIG, &DeviceSpecific, 0x41, sizeof(DeviceSpecific), FALSE);
		//if (!NT_SUCCESS(status)) {
		//	DbgPrint("YMF724OPL3 WritePciConfig1 Failed(%x)\r\n", status);
		//	return status;
		//}

		//FMBase
		status = ReadWritePciConfig(WdfDeviceWdmGetDeviceObject(device), PCI_WHICHSPACE_CONFIG, &DeviceSpecific, 0x42, sizeof(DeviceSpecific), TRUE);
		if (!NT_SUCCESS(status)) {
			DbgPrint("YMF724OPL3 ReadPciConfig2 Failed(%x)\r\n", status);
			return status;
		}
		//DeviceSpecific &= 0xfc;
		//DeviceSpecific |= 0;
		switch (DeviceSpecific & 0x3)
		{
		case 0:
			deviceContext->FMBaseAddress = 0x388;
			break;
		case 1:
			deviceContext->FMBaseAddress = 0x398;
			break;
		case 2:
			deviceContext->FMBaseAddress = 0x3A0;
			break;
		case 3:
			deviceContext->FMBaseAddress = 0x3A8;
			break;
		default:
			break;
		}
		status = ReadWritePciConfig(WdfDeviceWdmGetDeviceObject(device), PCI_WHICHSPACE_CONFIG, &DeviceSpecific, 0x42, sizeof(DeviceSpecific), FALSE);
		if (!NT_SUCCESS(status)) {
		    DbgPrint("YMF724OPL3 WritePciConfig2 Failed(%x)\r\n", status);
		    return status;
		}

		//  Since this driver controlls real hardware,
		//  the hardware controlled must be discovered.
		//  Chapter 9 will discuss auto-detection,
		//  but for now we will hard-code the hardware
		//  resource for the common printer port.
		//  We use IoReportResourceForDetection to mark
		//  PORTs and IRQs as "in use."
		//  This call will fail if another driver
		//  (such as the standard parallel driver(s))
		//  already control the hardware
		PDRIVER_OBJECT pdo = WdfDriverWdmGetDriverObject(Driver);
		status = ClaimHardware(pdo,
			deviceContext->FMBaseAddress,
			4);
		if (!NT_SUCCESS(status)) {
			// if it fails now, must delete Device object IoDeleteDevice( pDevObj );
			DbgPrint("YMF724OPL3 ClaimHardware Failed(%x)\r\n", status);
			return status;
		}

		DbgPrint("YMF724OPL3 CreateDevice FMBase = %xH\r\n", deviceContext->FMBaseAddress);

	}

	return status;
}

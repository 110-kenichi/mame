/*++

Module Name:

    queue.c

Abstract:

    This file contains the queue entry points and callbacks.

Environment:

    Kernel-mode Driver Framework

--*/

#include "driver.h"
#include "queue.tmh"

#ifdef __INTELLISENSE__
#undef TraceEvents
#define TraceEvents(...)
#endif

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, YMF724OPL3QueueInitialize)
#endif

NTSTATUS
YMF724OPL3QueueInitialize(
    _In_ WDFDEVICE Device
    )
/*++

Routine Description:

     The I/O dispatch callbacks for the frameworks device object
     are configured in this function.

     A single default I/O Queue is configured for parallel request
     processing, and a driver context memory allocation is created
     to hold our structure QUEUE_CONTEXT.

Arguments:

    Device - Handle to a framework device object.

Return Value:

    VOID

--*/
{
    WDFQUEUE queue;
    NTSTATUS status;
    WDF_IO_QUEUE_CONFIG queueConfig;

    PAGED_CODE();

    //
    // Configure a default queue so that requests that are not
    // configure-fowarded using WdfDeviceConfigureRequestDispatching to goto
    // other queues get dispatched here.
    //
    WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(
         &queueConfig,
        WdfIoQueueDispatchParallel
        );

    queueConfig.EvtIoDeviceControl = YMF724OPL3EvtIoDeviceControl;
    queueConfig.EvtIoStop = YMF724OPL3EvtIoStop;

    status = WdfIoQueueCreate(
                 Device,
                 &queueConfig,
                 WDF_NO_OBJECT_ATTRIBUTES,
                 &queue
                 );

    if(!NT_SUCCESS(status)) {
        TraceEvents(TRACE_LEVEL_ERROR, TRACE_QUEUE, "WdfIoQueueCreate failed %!STATUS!", status);
        return status;
    }

    return status;
}

VOID
YMF724OPL3EvtIoDeviceControl(
    _In_ WDFQUEUE Queue,
    _In_ WDFREQUEST Request,
    _In_ size_t OutputBufferLength,
    _In_ size_t InputBufferLength,
    _In_ ULONG IoControlCode
    )
/*++

Routine Description:

    This event is invoked when the framework receives IRP_MJ_DEVICE_CONTROL request.

Arguments:

    Queue -  Handle to the framework queue object that is associated with the
             I/O request.

    Request - Handle to a framework request object.

    OutputBufferLength - Size of the output buffer in bytes

    InputBufferLength - Size of the input buffer in bytes

    IoControlCode - I/O control code.

Return Value:

    VOID

--*/
{
	NTSTATUS                status = STATUS_SUCCESS;
	PDEVICE_CONTEXT			deviceContext = NULL;
	WDFDEVICE               hDevice;
	WDF_REQUEST_PARAMETERS  params;
	PVOID                   inputDataBuffer;
	PVOID                   outputDataBuffer;

	UNREFERENCED_PARAMETER(OutputBufferLength);
	UNREFERENCED_PARAMETER(InputBufferLength);

	TraceEvents(TRACE_LEVEL_INFORMATION,
		TRACE_QUEUE,
		"%!FUNC! Queue 0x%p, Request 0x%p OutputBufferLength %d InputBufferLength %d IoControlCode %d",
		Queue, Request, (int)OutputBufferLength, (int)InputBufferLength, IoControlCode);
	//DbgPrint("YMF724OPL3 Queue 0x%p, Request 0x%p OutputBufferLength %d InputBufferLength %d IoControlCode %d",
	//	Queue, Request, (int)OutputBufferLength, (int)InputBufferLength, IoControlCode);

	hDevice = WdfIoQueueGetDevice(Queue);
	deviceContext = DeviceGetContext(hDevice);

	WDF_REQUEST_PARAMETERS_INIT(&params);

	WdfRequestGetParameters(
		Request,
		&params
	);

	{
		switch (IoControlCode)
		{
		case (IOCTL_WRITE_DATA_TO_PORT >> 2) & 0xfff:
		{
			//DbgPrint("YMF724OPL3 IOCTL_WRITE_DATA_TO_PORT requested");

			status = WdfRequestRetrieveInputBuffer(Request,
				2,
				&inputDataBuffer,
				&InputBufferLength);
			if (NT_SUCCESS(status))
			{
				if (InputBufferLength == 2)
				{
					UCHAR* ucInputDataBuffer = inputDataBuffer;
					UCHAR portOfst = ucInputDataBuffer[0];
					if (0 <= portOfst && portOfst < 4)
					{
						UCHAR portData = ucInputDataBuffer[1];
						UNREFERENCED_PARAMETER(portData);

						PUCHAR portAdrs = (PUCHAR)ULongToPtr(deviceContext->FMBaseAddress + portOfst);

						//BUSY Wait
						//while ((ReadPortUChar(ULongToPtr(deviceContext->FMBaseAddress)) & 0x5) != 0);

						DbgPrint("YMF724OPL3 IOCTL_WRITE_DATA_TO_PORT(%ld,%d,%d) ", deviceContext->FMBaseAddress, portOfst, portData);

						WRITE_PORT_UCHAR(portAdrs, portData);
					}
					else {
						//DbgPrint("YMF724OPL3 Invalid request(port=%d)", portOfst);
					}
					status = STATUS_SUCCESS;
				}
				else
				{
					//DbgPrint("YMF724OPL3 Invalid request(len=%lld)", InputBufferLength);
					status = STATUS_INVALID_DEVICE_REQUEST;
				}
			}
			else {
				//DbgPrint("YMF724OPL3 Invalid request(stat=%x, len=%lld)", status, InputBufferLength);
			}
			break;
		}
		case (IOCTL_READ_DATA_TO_PORT >> 2) & 0xfff:
		{
			//DbgPrint("YMF724OPL3 IOCTL_READ_DATA_TO_PORT requested");

			status = WdfRequestRetrieveInputBuffer(Request,
				1,
				&inputDataBuffer,
				&InputBufferLength);
			if (NT_SUCCESS(status))
			{
				status = WdfRequestRetrieveOutputBuffer(Request,
					1,
					&outputDataBuffer,
					&OutputBufferLength);
				if (NT_SUCCESS(status))
				{
					if (InputBufferLength == 1 && OutputBufferLength == 1)
					{
						UCHAR* ucInputDataBuffer = inputDataBuffer;
						UCHAR portOfst = ucInputDataBuffer[0];
						if (0 <= portOfst && portOfst < 4)
						{
							UCHAR* ucOutputDataBuffer = outputDataBuffer;

							PUCHAR portAdrs = (PUCHAR)ULongToPtr(deviceContext->FMBaseAddress + portOfst);

							UCHAR portData = ReadPortUChar(portAdrs);
							ucOutputDataBuffer[0] = portData;
						}
						else {
							//DbgPrint("YMF724OPL3 Invalid request(port=%d)", portOfst);
						}
						status = STATUS_SUCCESS;
					}
					else
					{
						//DbgPrint("YMF724OPL3 Invalid request(len=%lld,%lld)", InputBufferLength, OutputBufferLength);
						status = STATUS_INVALID_DEVICE_REQUEST;
					}
				}
			}
			else {
				//DbgPrint("YMF724OPL3 Invalid IOCTL request(%lld)", InputBufferLength);
			}
			break;
		}
		default:
		{
			//DbgPrint("YMF724OPL3 Invalid IOCTL request(%d)", IoControlCode);
			ASSERTMSG(FALSE, "Invalid IOCTL request\n");
			status = STATUS_INVALID_DEVICE_REQUEST;
			break;
		}
		}
	}

	//DbgPrint("YMF724OPL3 Queue End(stat=%x)", status);
	WdfRequestComplete(Request, status);
	return;
}

VOID
YMF724OPL3EvtIoStop(
    _In_ WDFQUEUE Queue,
    _In_ WDFREQUEST Request,
    _In_ ULONG ActionFlags
)
/*++

Routine Description:

    This event is invoked for a power-managed queue before the device leaves the working state (D0).

Arguments:

    Queue -  Handle to the framework queue object that is associated with the
             I/O request.

    Request - Handle to a framework request object.

    ActionFlags - A bitwise OR of one or more WDF_REQUEST_STOP_ACTION_FLAGS-typed flags
                  that identify the reason that the callback function is being called
                  and whether the request is cancelable.

Return Value:

    VOID

--*/
{
    TraceEvents(TRACE_LEVEL_INFORMATION, 
                TRACE_QUEUE, 
                "%!FUNC! Queue 0x%p, Request 0x%p ActionFlags %d", 
                Queue, Request, ActionFlags);

    //
    // In most cases, the EvtIoStop callback function completes, cancels, or postpones
    // further processing of the I/O request.
    //
    // Typically, the driver uses the following rules:
    //
    // - If the driver owns the I/O request, it calls WdfRequestUnmarkCancelable
    //   (if the request is cancelable) and either calls WdfRequestStopAcknowledge
    //   with a Requeue value of TRUE, or it calls WdfRequestComplete with a
    //   completion status value of STATUS_SUCCESS or STATUS_CANCELLED.
    //
    //   Before it can call these methods safely, the driver must make sure that
    //   its implementation of EvtIoStop has exclusive access to the request.
    //
    //   In order to do that, the driver must synchronize access to the request
    //   to prevent other threads from manipulating the request concurrently.
    //   The synchronization method you choose will depend on your driver's design.
    //
    //   For example, if the request is held in a shared context, the EvtIoStop callback
    //   might acquire an internal driver lock, take the request from the shared context,
    //   and then release the lock. At this point, the EvtIoStop callback owns the request
    //   and can safely complete or requeue the request.
    //
    // - If the driver has forwarded the I/O request to an I/O target, it either calls
    //   WdfRequestCancelSentRequest to attempt to cancel the request, or it postpones
    //   further processing of the request and calls WdfRequestStopAcknowledge with
    //   a Requeue value of FALSE.
    //
    // A driver might choose to take no action in EvtIoStop for requests that are
    // guaranteed to complete in a small amount of time.
    //
    // In this case, the framework waits until the specified request is complete
    // before moving the device (or system) to a lower power state or removing the device.
    // Potentially, this inaction can prevent a system from entering its hibernation state
    // or another low system power state. In extreme cases, it can cause the system
    // to crash with bugcheck code 9F.
    //

    return;
}

// dllmain.cpp : DLL アプリケーションのエントリ ポイントを定義します。
#include "pch.h"
#include <setupapi.h>
#include <initguid.h>

#define IOCTL_WRITE_DATA_TO_PORT    2048
#define IOCTL_READ_DATA_TO_PORT     2049

#define MAX_LEN                 64

DEFINE_GUID(GUID_DEVINTERFACE_CMI8738OPL3,
    0x52d42d43, 0x8c67, 0x4b79, 0xae, 0x4c, 0x1a, 0x5f, 0xaa, 0xdf, 0xbc, 0x14);

GUID        InterfaceGuid;// = GUID_DEVINTERFACE_PCIDRV;
ULONG       DeviceIndex;

typedef struct _DEVICE_INFO
{
    LIST_ENTRY      ListEntry;
    HANDLE          hDevice; // file handle
    TCHAR           DeviceName[MAX_PATH];// friendly name of device description
    TCHAR           DevicePath[MAX_PATH];//
    ULONG           DeviceIndex; // Serial number of the device.

} DEVICE_INFO, * PDEVICE_INFO;

/// <summary>
/// 
/// </summary>
/// <param name="hModule"></param>
/// <param name="ul_reason_for_call"></param>
/// <param name="lpReserved"></param>
/// <returns></returns>
BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}

    InterfaceGuid = GUID_DEVINTERFACE_CMI8738OPL3;

	return TRUE;
}

//GUIDからデバイスドライバのデバイスインターフェース名を取得する
//https://kana-soft.com/tech/sample_0003.htm

//windows-driver-samples-master\general\pcidrv\test\testapp.c

/// <summary>
/// 
/// </summary>
/// <param name="memberIndex"></param>
/// <returns></returns>
PSP_DEVICE_INTERFACE_DETAIL_DATA GetDeviceInterfaceDetaildData(int memberIndex)
{
    HDEVINFO                            hardwareDeviceInfo;
    SP_DEVICE_INTERFACE_DATA            deviceInterfaceData;
    PSP_DEVICE_INTERFACE_DETAIL_DATA    deviceInterfaceDetailData = NULL;
    ULONG                               predictedLength = 0;
    ULONG                               requiredLength = 0;
    DWORD                               error;
    PDEVICE_INFO                        deviceInfo = NULL;

    //DisplayV(TEXT("Entered EnumExistingDevices"));

    hardwareDeviceInfo = SetupDiGetClassDevs(
        (LPGUID)&InterfaceGuid,
        NULL, // Define no enumerator (global)
        NULL, // Define no
        (DIGCF_PRESENT | // Only Devices present
            DIGCF_DEVICEINTERFACE)); // Function class devices.
    if (INVALID_HANDLE_VALUE == hardwareDeviceInfo)
    {
        goto Error;
    }

    //
    // Enumerate devices of a specific interface class
    //
    deviceInterfaceData.cbSize = sizeof(deviceInterfaceData);

    if (SetupDiEnumDeviceInterfaces(hardwareDeviceInfo, 
        0, // No care about specific PDOs
        (LPGUID)&InterfaceGuid,
        memberIndex, //
        &deviceInterfaceData)) {

        //
        // Allocate a function class device data structure to
        // receive the information about this particular device.
        //

        if (!SetupDiGetDeviceInterfaceDetail(
            hardwareDeviceInfo,
            &deviceInterfaceData,
            NULL, // probing so no output buffer yet
            0, // probing so output buffer length of zero
            &requiredLength,
            NULL) && (error = GetLastError()) != ERROR_INSUFFICIENT_BUFFER)
        {
            goto Error;
        }
        predictedLength = requiredLength;

        deviceInterfaceDetailData = (PSP_DEVICE_INTERFACE_DETAIL_DATA)HeapAlloc(GetProcessHeap(), HEAP_ZERO_MEMORY, predictedLength);
        if (deviceInterfaceDetailData == NULL) {
            goto Error;
        }

        deviceInterfaceDetailData->cbSize =
            sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA);


        if (!SetupDiGetDeviceInterfaceDetail(
            hardwareDeviceInfo,
            &deviceInterfaceData,
            deviceInterfaceDetailData,
            predictedLength,
            &requiredLength,
            NULL)) {
            goto Error;
        }
    }

    SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
    return deviceInterfaceDetailData;

Error:
    //error = GetLastError();
    //MessageBox(hWnd, TEXT("EnumExisting Devices failed"), TEXT("Error!"), MB_OK);
    if (deviceInterfaceDetailData)
        HeapFree(GetProcessHeap(), 0, deviceInterfaceDetailData);

    SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
    return NULL;
}

/// <summary>
/// 
/// </summary>
/// <param name="memberIndex"></param>
/// <returns></returns>
__declspec(dllexport) HANDLE __cdecl  OpenOPL3Port(int memberIndex)
{
    PSP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData = GetDeviceInterfaceDetaildData(memberIndex);
    if (!deviceInterfaceDetailData)
        return NULL;

    HANDLE handle = CreateFile(deviceInterfaceDetailData->DevicePath, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);

    if (deviceInterfaceDetailData)
        HeapFree(GetProcessHeap(), 0, deviceInterfaceDetailData);

    return handle;
}

/// <summary>
/// 
/// </summary>
/// <param name="OPL3PortHandle"></param>
/// <returns></returns>
__declspec(dllexport) void __cdecl  CloseOPL3Port(HANDLE OPL3PortHandle)
{
	if (OPL3PortHandle == INVALID_HANDLE_VALUE)
		return;

	CloseHandle(OPL3PortHandle);
}


/// <summary>
/// 
/// </summary>
/// <param name="OPL3PortHandle"></param>
/// <returns></returns>
__declspec(dllexport) void __cdecl  WriteOPL3PortData(HANDLE OPL3PortHandle, UCHAR offset, UCHAR data)
{
    if (OPL3PortHandle == INVALID_HANDLE_VALUE)
        return;

    DWORD dwBytesReturned = 0;
    UCHAR inBuffer[] = {offset, data};

    DeviceIoControl(OPL3PortHandle, IOCTL_WRITE_DATA_TO_PORT, &inBuffer, 2, NULL, 0, &dwBytesReturned, NULL);
}


/// <summary>
/// 
/// </summary>
/// <param name="OPL3PortHandle"></param>
/// <returns></returns>
__declspec(dllexport) UCHAR __cdecl  ReadOPL3PortData(HANDLE OPL3PortHandle, UCHAR offset)
{
	if (OPL3PortHandle == INVALID_HANDLE_VALUE)
		return 0;

	DWORD dwBytesReturned = 0;
	UCHAR nDataFromDriver = 0;

	DeviceIoControl(OPL3PortHandle, IOCTL_READ_DATA_TO_PORT, &offset, 1, &nDataFromDriver, 1, &dwBytesReturned, NULL);

	return nDataFromDriver;
}


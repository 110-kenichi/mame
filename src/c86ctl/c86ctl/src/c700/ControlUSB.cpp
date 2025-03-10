/*
 *  ControlUSB.cpp
 *  VOPM
 *
 *  Created by osoumen on 2013/09/07.
 *  Copyright 2013 __MyCompanyName__. All rights reserved.
 *
 */

#include "ControlUSB.h"

#pragma comment(lib, "setupapi.lib")
#pragma comment(lib, "winusb.lib")

#include <setupapi.h>

ControlUSB::ControlUSB()
: mIsRun(false)
, mIsPlugged(false)
, mWriteBufferPtr(0)
{
	mReadBufferReadPtr = 0;
	mReadBufferWritePtr = 0;
	mDeviceAddedFunc = NULL;
	mDeviceRemovedFunc = NULL;
}
ControlUSB::~ControlUSB()
{
	removeDevice();
	EndPortWait();
}

void ControlUSB::BeginPortWait(LPGUID guid)
{
	BOOL bResult = TRUE;

	HDEVINFO devinf = INVALID_HANDLE_VALUE;
	SP_DEVICE_INTERFACE_DATA spid;
	PSP_DEVICE_INTERFACE_DETAIL_DATA fc_data = NULL;

	devinf = SetupDiGetClassDevs(
		guid,
		NULL,
		0,
		DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

	if (devinf) {
		for (int i=0;; i++) {
			ZeroMemory(&spid, sizeof(spid));
			spid.cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);
			if (!SetupDiEnumDeviceInterfaces(devinf, NULL,
				guid, i, &spid)) {
				break;
			}

			unsigned long sz;
			std::basic_string<TCHAR> devpath;

			// 必要なバッファサイズ取得
			bResult = SetupDiGetDeviceInterfaceDetail(devinf, &spid, NULL, 0, &sz, NULL);
			PSP_INTERFACE_DEVICE_DETAIL_DATA dev_det = (PSP_INTERFACE_DEVICE_DETAIL_DATA)(malloc(sz));
			dev_det->cbSize = sizeof(SP_INTERFACE_DEVICE_DETAIL_DATA);

			// デバイスノード取得
			if (!SetupDiGetDeviceInterfaceDetail(devinf, &spid, dev_det, sz, &sz, NULL)) {
				free(dev_det);
				break;
			}

			devpath = dev_det->DevicePath;
			free(dev_det);
			dev_det = NULL;

			if (openDevice(devpath)) {
				mIsRun = true;
			}
			else {
				mIsRun = false;
			}
		}
		SetupDiDestroyDeviceInfoList(devinf);
	}
}

bool ControlUSB::openDevice(std::basic_string<TCHAR> devpath)
{
	HANDLE hNewDev = CreateFile(
		devpath.c_str(),
		GENERIC_READ | GENERIC_WRITE,
		0 /*FILE_SHARE_READ|FILE_SHARE_WRITE*/,
		NULL,
		OPEN_EXISTING,
		FILE_FLAG_OVERLAPPED, //FILE_FLAG_NO_BUFFERING,
		NULL);

	if (hNewDev == INVALID_HANDLE_VALUE) {
		return false;
	}

	HANDLE hNewWinUsb = NULL;
	if (!WinUsb_Initialize(hNewDev, &hNewWinUsb)) {
		//DWORD err = GetLastError();
		CloseHandle(hNewDev);
		return false;
	}

	// エンドポイント情報取得
	USB_INTERFACE_DESCRIPTOR desc;
	if (!WinUsb_QueryInterfaceSettings(hNewWinUsb, 0, &desc)) {
		WinUsb_Free(hNewWinUsb);
		CloseHandle(hNewDev);
		return false;
	}

	for (int i = 0; i<desc.bNumEndpoints; i++) {
		WINUSB_PIPE_INFORMATION pipeInfo;
		if (WinUsb_QueryPipe(hNewWinUsb, 0, (UCHAR)i, &pipeInfo)) {
			if (pipeInfo.PipeType == UsbdPipeTypeBulk &&
				USB_ENDPOINT_DIRECTION_OUT(pipeInfo.PipeId)) {
				mOutPipeId = pipeInfo.PipeId;
				mOutPipeMaxPktSize = pipeInfo.MaximumPacketSize;
			}
			else if (pipeInfo.PipeType == UsbdPipeTypeBulk &&
				USB_ENDPOINT_DIRECTION_IN(pipeInfo.PipeId)) {
				mInPipeId = pipeInfo.PipeId;
				mInPipeMaxPktSize = pipeInfo.MaximumPacketSize;
			}
		}
	}

	// タイムアウト設定
	ULONG timeout = 500; //ms
	::WinUsb_SetPipePolicy(hNewWinUsb, mOutPipeId, PIPE_TRANSFER_TIMEOUT, sizeof(ULONG), &timeout);
	::WinUsb_SetPipePolicy(hNewWinUsb, mInPipeId, PIPE_TRANSFER_TIMEOUT, sizeof(ULONG), &timeout);

	// ここでハンドル更新
	m_hDev = hNewDev;
	m_hWinUsb = hNewWinUsb;
	mDevPath = devpath;

	WinUsb_FlushPipe(m_hWinUsb, mOutPipeId);
	WinUsb_FlushPipe(m_hWinUsb, mInPipeId);

	mIsPlugged = true;
	if (mDeviceAddedFunc) {
		mDeviceAddedFunc(mDeviceAddedFuncClass);
	}

	return true;
}
void ControlUSB::EndPortWait()
{
	mIsRun = false;
}

void ControlUSB::removeDevice()
{
	if (mIsPlugged) {
		WinUsb_Free(m_hWinUsb);
		CloseHandle(m_hDev);
		m_hDev = NULL;
		m_hWinUsb = NULL;
		mIsPlugged = false;

		if (mDeviceRemovedFunc) {
			mDeviceRemovedFunc(mDeviceRemovedFuncClass);
		}
	}
}
bool ControlUSB::resetrPipe()
{
	WinUsb_FlushPipe(m_hWinUsb, mInPipeId);
	return true;
}
bool ControlUSB::resetwPipe()
{
	WinUsb_FlushPipe(m_hWinUsb, mOutPipeId);
	return true;
}
int	 ControlUSB::bulkWrite(UINT8 *buf, UINT32 size)
{
	if (!mIsPlugged) return -1;

	ULONG len = size;
	UINT32	bufferRest = WRITE_BUFFER_SIZE - mWriteBufferPtr;
	if (bufferRest < len) {
		mWriteBufferPtr = 0;
	}
	memcpy(&mWriteBuffer[mWriteBufferPtr], buf, len);

	DWORD wlen;	
	BOOL ret = WinUsb_WritePipe(m_hWinUsb, mOutPipeId, &mWriteBuffer[mWriteBufferPtr], len, &wlen, NULL);

	if (ret == FALSE || size != wlen){
		//printf("Write error in bulkWrite.\n");
		//DWORD err = GetLastError();
		//printErr(err);
		return -1;
	}
	mWriteBufferPtr += len;
	return wlen;
}
int	 ControlUSB::bulkWriteAsync(UINT8 *buf, UINT32 size)
{
	return bulkWrite(buf, size);
}
int	 ControlUSB::bulkRead(UINT8 *buf, UINT32 size, UINT32 timeout)
{
	if (!mIsPlugged) return -1;

	DWORD rlen;
	BOOL ret = WinUsb_ReadPipe(m_hWinUsb, mInPipeId, buf, size, &rlen, NULL);

	if (ret == FALSE){
		//printf("Write error in bulkRead.\n");
		//DWORD err = GetLastError();
		//printErr(err);
		return -1;
	}
	return rlen;
}
int	 ControlUSB::read(UINT8 *buf, UINT32 size)
{
	return 0;
}
int	 ControlUSB::getReadableBytes()
{
	return 0;
}

void ControlUSB::setDeviceAddedFunc(void(*func) (void* ownerClass), void* ownerClass)
{
	mDeviceAddedFunc = func;
	mDeviceAddedFuncClass = ownerClass;
}

void ControlUSB::setDeviceRemovedFunc(void(*func) (void* ownerClass), void* ownerClass)
{
	mDeviceRemovedFunc = func;
	mDeviceRemovedFuncClass = ownerClass;
}
#endif

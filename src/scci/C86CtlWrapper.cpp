// copyright-holders:K.Ito

#include "C86CtlWrapper.h"

extern "C"
{
	__declspec(dllexport) DWORD __cdecl  GimicInitialize()
	{
		// scci.dll‚Ì“Çž‚Ý
		hC87Ctl = ::LoadLibrary(L"c86ctl_mami");
		if (hC87Ctl == NULL) {
			return GetLastError();
		}

		TCreateInstance pCI;

		pCI = (TCreateInstance)::GetProcAddress(hC87Ctl, "CreateInstance");
		(*pCI)(c86ctl::IID_IRealChipBase, (void**)&pChipBase);
		int ret = pChipBase->initialize();
		if (ret != C86CTL_ERR_NONE)
			return ret;

		int nchip = pChipBase->getNumberOfChip();
		for (int i = 0; i < nchip; i++)
		{
			c86ctl::IRealChip* pRC = NULL;
			if (S_OK == pChipBase->getChipInterface(i, c86ctl::IID_IRealChip, (void**)&pRC))
			{
				pRC->reset();

				pRC->Release();
			}
		}

		return C86CTL_ERR_NONE;
	}

	__declspec(dllexport) DWORD __cdecl  GimicDeinitialize()
	{
		int ret = C86CTL_ERR_NONE;
		if (pChipBase != NULL)
			ret = pChipBase->deinitialize();
		if (ret != C86CTL_ERR_NONE)
			return ret;

		pChipBase = NULL;
		return C86CTL_ERR_NONE;
	}

	__declspec(dllexport) DWORD __cdecl  GimicGetNumberOfChip()
	{
		return pChipBase->getNumberOfChip();
	}

	__declspec(dllexport) DWORD __cdecl  GimicGetModule(DWORD moduleIndex, DWORD chipType)
	{
		c86ctl::IGimic2* pGimicModule;
		if (S_OK == pChipBase->getChipInterface(moduleIndex, c86ctl::IID_IGimic2, (void**)&pGimicModule)) {
			c86ctl::ChipType ct;

			pGimicModule->getModuleType(&ct);

			pGimicModule->Release();
			if (ct == chipType)
				return C86CTL_ERR_NONE;
		}
		return C86CTL_ERR_INVALID_PARAM;
	}

	__declspec(dllexport) DWORD __cdecl  GimicSetClock(DWORD moduleIndex, DWORD clock)
	{
		c86ctl::IGimic2* pGimicModule;
		if (S_OK == pChipBase->getChipInterface(moduleIndex, c86ctl::IID_IGimic2, (void**)&pGimicModule)) {
			pGimicModule->setPLLClock(clock);
			UINT clk;
			pGimicModule->getPLLClock(&clk);

			pGimicModule->Release();
			return clk;
		}
		return 0;
	}

	__declspec(dllexport) void __cdecl  GimicSetRegister(DWORD moduleIndex, DWORD addr, DWORD data)
	{
		c86ctl::IGimic2* pGimicModule;
		if (S_OK == pChipBase->getChipInterface(moduleIndex, c86ctl::IID_IGimic2, (void**)&pGimicModule)) {

			c86ctl::IRealChip2* pRC = NULL;
			if (S_OK == pChipBase->getChipInterface(moduleIndex, c86ctl::IID_IRealChip2, (void**)&pRC))
			{
				pRC->out(addr, (UCHAR)data);

				pRC->Release();
			}

			pGimicModule->Release();
		}
	}

	__declspec(dllexport) void __cdecl  GimicSetRegisterDirect(DWORD moduleIndex, DWORD addr, DWORD data)
	{
		c86ctl::IGimic2* pGimicModule;
		if (S_OK == pChipBase->getChipInterface(moduleIndex, c86ctl::IID_IGimic2, (void**)&pGimicModule)) {

			c86ctl::IRealChip2* pRC = NULL;
			if (S_OK == pChipBase->getChipInterface(moduleIndex, c86ctl::IID_IRealChip2, (void**)&pRC))
			{
				pRC->directOut(addr, (UCHAR)data);

				pRC->Release();
			}

			pGimicModule->Release();
		}
	}

	__declspec(dllexport) DWORD __cdecl  GimicGetWrittenRegisterData(DWORD moduleIndex, DWORD addr)
	{
		c86ctl::IGimic2* pGimicModule;
		if (S_OK == pChipBase->getChipInterface(moduleIndex, c86ctl::IID_IGimic2, (void**)&pGimicModule)) {
			UCHAR data = 0;
			
			c86ctl::IRealChip2* pRC = NULL;
			if (S_OK == pChipBase->getChipInterface(moduleIndex, c86ctl::IID_IRealChip2, (void**)&pRC))
			{
				UCHAR data = pRC->in(addr);

				pRC->Release();
			}

			pGimicModule->Release();

			return data;
		}

		return 0;
	}

	__declspec(dllexport) void __cdecl  GimicSetSSGVolume(DWORD moduleIndex, UCHAR volume)
	{
		c86ctl::IGimic2* pGimicModule;
		if (S_OK == pChipBase->getChipInterface(moduleIndex, c86ctl::IID_IGimic2, (void**)&pGimicModule)) {

			pGimicModule->setSSGVolume(volume);

			pGimicModule->Release();
		}
	}
}
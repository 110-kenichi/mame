// copyright-holders:K.Ito
#include "emu.h"
#include "osdepend.h"
#include "emumem.h"
#include "machine.h"
#include "vgmwrite.h"
#include "..\frontend\mame\mame.h"
#include "..\frontend\mame\cheat.h"
#include "..\devices\sound\fm.h"
#include "..\devices\sound\ym2413.h"
#include "..\devices\sound\emu2413.h"
#include "..\devices\sound\2612intf.h"
#include "..\devices\sound\gb.h"
#include "..\devices\sound\sn76496.h"
#include "..\devices\sound\namco.h"
#include "..\devices\sound\nes_apu.h"
#include "..\devices\sound\3812intf.h"
#include "..\devices\sound\k051649.h"
#include "..\devices\sound\msm5232.h"
#include "..\devices\sound\ay8910.h"
#include "..\devices\sound\mos6581.h"
#include "..\devices\sound\beep.h"
#include "..\devices\sound\c140.h"
#include "..\devices\sound\c6280.h"
#include "..\mame\audio\snes_snd.h"
#include "..\devices\sound\pokey.h"
#include "..\devices\sound\2610intf.h"
#include "..\devices\sound\mt32.h"
#include "..\munt\mt32emu\src\c_interface\c_interface.h"
#include "..\munt\mt32emu\soxr\src\soxr.h"
#include "..\devices\sound\cm32p.h"
#include "..\devices\sound\cm32p.h"
#include "..\devices\sound\262intf.h"
#include "..\devices\sound\2608intf.h"
#include "..\devices\sound\tms5220.h"
#include "..\devices\sound\sp0256.h"
#include "..\devices\sound\samples.h"
#include "..\devices\sound\sn76477.h"
#include "..\devices\sound\upd1771.h"
#include "..\devices\sound\multipcm.h"
#include "..\devices\sound\rf5c68.h"
#include "..\devices\sound\ymfm\src\ymfm_opm.h"
#include "..\devices\sound\ymfm\src\ymfm_opz.h"
#include "..\devices\sound\ymfm\src\ymfm_opn.h"
#include "..\devices\sound\ymfm\src\ymfm_opl.h"
#include "..\devices\sound\ymfm\src\ymfm_opq.h"
#include "..\devices\sound\saa1099.h"
#include ".\machine\8364_paula.h"
#include "..\devices\sound\tiaintf.h"
#include "..\devices\sound\vlm5030.h"

#define DllExport extern "C" __declspec (dllexport)

address_space *dummy;

extern "C"
{
	//memodimemo

	DllExport int sample_rate()
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return 0;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return 0;

		return rm->sample_rate();
	}

	DllExport void start_recording_to(char* name)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		rm->sound().start_recording_to(name);
	}

	DllExport void stop_recording()
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		rm->sound().stop_recording();
	}

	DllExport void start_vgm_recording_to(unsigned int unitNumber, char* name, char * filePath)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->device((std::string(name) + num).c_str()));
		//device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->root_device().subdevice((std::string(name) + num).c_str()));
		if (sd == nullptr)
			return;

		sd->vgm_start(filePath);
	}

	DllExport void stop_vgm_recording(unsigned int unitNumber, char* name)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->device((std::string(name) + num).c_str()));
		//device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->root_device().subdevice((std::string(name) + num).c_str()));
		if (sd == nullptr)
			return;

		sd->vgm_stop();
	}

	DllExport void device_reset(unsigned int unitNumber, char* name)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);

		device_t *dev = dynamic_cast<device_t  *>(rm->device((std::string(name) + num).c_str()));
		//device_t *dev = dynamic_cast<device_t  *>(rm->root_device().subdevice((std::string(name) + num).c_str()));
		if (dev == nullptr)
			return;

		dev->reset();
	}

	DllExport void set_device_enable(unsigned int unitNumber, char* name, int enable)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->device((std::string(name) + num).c_str()));
		//device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->root_device().subdevice((std::string(name) + num).c_str()));
		if (sd == nullptr)
			return;

		sd->set_enable(enable);
	}

	DllExport void set_device_passthru(unsigned int unitNumber, char* name, int passthru)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->device((std::string(name) + num).c_str()));
		//device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->root_device().subdevice((std::string(name) + num).c_str()));
		if (sd == nullptr)
			return;

		sd->set_passthru(passthru);
	}

	DllExport void set_clock(unsigned int unitNumber, char* name, unsigned int clock)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		device_t *dev = dynamic_cast<device_t  *>(rm->device((std::string(name) + num).c_str()));
		//device_t *dev = dynamic_cast<device_t  *>(rm->root_device().subdevice((std::string(name) + num).c_str()));
		if (dev == nullptr)
			return;

		dev->set_clock(clock);
	}

	DllExport void set_filter(int unitNumber, char* name, device_sound_interface::FilterMode filterMode, double cutoff, double resonance)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->device((std::string(name) + num).c_str()));
		//device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->root_device().subdevice((std::string(name) + num).c_str()));
		if (sd == nullptr)
			return;

		sd->setFilterMode(filterMode);
		sd->setCutoff(cutoff);
		sd->setResonance(resonance);
	}

	DllExport void set_output_gain(unsigned int unitNumber, char* name, int channel, float gain)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		if (unitNumber != UINT32_MAX)
		{
			std::string num = std::to_string(unitNumber);
			device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->device((std::string(name) + num).c_str()));
			//device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->root_device().subdevice((std::string(name) + num).c_str()));
			if (sd == nullptr)
				return;

			sd->set_output_gain(channel, gain);
		}
		else
		{
			rm->osd().set_mastervolume(-32 * (1.0 - gain));
		}
	}

	std::map<std::string, device_sound_interface *> speakerSoundInterface;

	DllExport s32 *getLastOutputBuffer(char* name, int insts, unsigned int deviceId, unsigned int unitNumber)
	{
		std::string key = std::string(name).c_str();
		auto itr = speakerSoundInterface.find(key);
		if (itr == speakerSoundInterface.end())
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return NULL;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return NULL;

			device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->device((std::string(name)).c_str()));
			//device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->root_device().subdevice((std::string(name)).c_str()));
			if (sd == nullptr)
				return NULL;

			speakerSoundInterface[key] = sd;
		}
		//limitation: 1st buffer is last unit buffer data
		if (deviceId == UINT32_MAX)
			speakerSoundInterface[key]->lastOutBufferNumber = UINT32_MAX;
		else
			speakerSoundInterface[key]->lastOutBufferNumber = (insts * unitNumber) + (deviceId - 1);

		return speakerSoundInterface[key]->lastOutBuffer;
	}

	DllExport int getLastOutputBufferSamples(char* name)
	{
		std::string key = std::string(name).c_str();
		auto itr = speakerSoundInterface.find(key);
		if (itr == speakerSoundInterface.end())
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return 0;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return 0;

			device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->device(std::string(name).c_str()));
			//device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->root_device().subdevice((std::string(name)).c_str()));
			if (sd == nullptr)
				return 0;

			speakerSoundInterface[key] = sd;
		}
		return speakerSoundInterface[key]->lastOutBufferSamples;
	}

	DllExport void set_stream_update_callback(char* name, STREAM_UPDATE_CALLBACK callback)
	{
		std::string key = std::string(name).c_str();
		auto itr = speakerSoundInterface.find(key);
		if (itr == speakerSoundInterface.end())
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->device((std::string(name)).c_str()));
			if (sd == nullptr)
				return;

			speakerSoundInterface[key] = sd;
		}
		speakerSoundInterface[key]->set_stream_update_callback(callback);
	}

	DllExport void set_vst_fx_callback(unsigned int unitNumber, char* name, VST_FX_CALLBACK callback)
	{
		mame_machine_manager *mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine *rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		std::string num = std::to_string(unitNumber);
		device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->device((std::string(name) + num).c_str()));
		//device_sound_interface *sd = dynamic_cast<device_sound_interface *>(rm->root_device().subdevice((std::string(name) + num).c_str()));
		if (sd == nullptr)
			return;

		sd->set_vst_fx_callback(callback);
	}

	DllExport void parameter_automated()
	{
		mame_machine_manager* mmm = mame_machine_manager::instance();
		if (mmm == nullptr)
			return;
		running_machine* rm = mmm->machine();
		if (rm == nullptr || rm->phase() == machine_phase::EXIT)
			return;

		return rm->parameter_automated();
	}

	ymfm_opm_device *ym2151_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void ym2151_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (ym2151_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			ymfm_opm_device*ym2151 = dynamic_cast<ymfm_opm_device*>(rm->device((std::string("ym2151_") + num).c_str()));
			//ym2151_device *ym2151 = dynamic_cast<ym2151_device *>(rm->root_device().subdevice((std::string("ym2151_") + num).c_str()));
			if (ym2151 == nullptr)
				return;

			ym2151_devices[unitNumber] = ym2151;
		}
		ym2151_devices[unitNumber]->write(address, data);
	}

	ymfm_opn2_device *ym2612_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void ym2612_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (ym2612_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			ymfm_opn2_device*ym2612 = dynamic_cast<ymfm_opn2_device*>(rm->device((std::string("ym2612_") + num).c_str()));
			//ym2612_device *ym2612 = dynamic_cast<ym2612_device *>(rm->root_device().subdevice((std::string("ym2612_") + num).c_str()));
			if (ym2612 == nullptr)
				return;

			ym2612_devices[unitNumber] = ym2612;
		}
		ym2612_devices[unitNumber]->write(address, data);
	}

	DllExport void ym2612_set_pcm_callback(unsigned int unitNumber, OPN2_PCM_CALLBACK callback)
	{
		if (ym2612_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			ymfm_opn2_device* ym2612 = dynamic_cast<ymfm_opn2_device*>(rm->device((std::string("ym2612_") + num).c_str()));
			//ym2612_device *ym2612 = dynamic_cast<ym2612_device *>(rm->root_device().subdevice((std::string("ym2612_") + num).c_str()));
			if (ym2612 == nullptr)
				return;

			ym2612_devices[unitNumber] = ym2612;
		}

		ym2612_devices[unitNumber]->set_pcm_callback(callback);
	}

	DllExport void ym2612_set_pcm_frequency(unsigned int unitNumber, double frequency)
	{
		if (ym2612_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			ymfm_opn2_device* ym2612 = dynamic_cast<ymfm_opn2_device*>(rm->device((std::string("ym2612_") + num).c_str()));
			//ym2612_device *ym2612 = dynamic_cast<ym2612_device *>(rm->root_device().subdevice((std::string("ym2612_") + num).c_str()));
			if (ym2612 == nullptr)
				return;

			ym2612_devices[unitNumber] = ym2612;
		}

		ym2612_devices[unitNumber]->set_pcm_frequency(frequency);
	}

	ym3812_device *ym3812_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void ym3812_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (ym3812_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			ym3812_device *ym3812 = dynamic_cast<ym3812_device *>(rm->device((std::string("ym3812_") + num).c_str()));
			//ym3812_device *ym3812 = dynamic_cast<ym3812_device *>(rm->root_device().subdevice((std::string("ym3812_") + num).c_str()));
			if (ym3812 == nullptr)
				return;

			ym3812_devices[unitNumber] = ym3812;
		}
		ym3812_devices[unitNumber]->write(address, data);
	}

	ymf262_device *ymf262_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void ymf262_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (ymf262_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			ymf262_device *ymf262 = dynamic_cast<ymf262_device *>(rm->device((std::string("ymf262_") + num).c_str()));
			if (ymf262 == nullptr)
				return;

			ymf262_devices[unitNumber] = ymf262;
		}
		ymf262_devices[unitNumber]->write(address, data);
	}

	ymfm_opll_device *ym2413_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void ym2413_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (ym2413_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			ymfm_opll_device* ym2413 = dynamic_cast<ymfm_opll_device*>(rm->device((std::string("ym2413_") + num).c_str()));
			//emu2413_device  *ym2413 = dynamic_cast<emu2413_device  *>(rm->device((std::string("ym2413_") + num).c_str()));
			//ym2413_device *ym2413 = dynamic_cast<ym2413_device *>(rm->root_device().subdevice((std::string("ym2413_") + num).c_str()));
			if (ym2413 == nullptr)
				return;

			ym2413_devices[unitNumber] = ym2413;
		}
		ym2413_devices[unitNumber]->write(address, data);
	}

	dmg_apu_device *dmg_apu_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void gb_apu_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (dmg_apu_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			dmg_apu_device *gb_apu = dynamic_cast<dmg_apu_device *>(rm->device((std::string("gbsnd_") + num).c_str()));
			//dmg_apu_device *gb_apu = dynamic_cast<dmg_apu_device *>(rm->root_device().subdevice((std::string("gbsnd_") + num).c_str()));
			if (gb_apu == nullptr)
				return;

			dmg_apu_devices[unitNumber] = gb_apu;
		}
		dmg_apu_devices[unitNumber]->sound_w(address, data);
	}

	DllExport unsigned char gb_apu_read(unsigned int unitNumber, unsigned int address)
	{
		if (dmg_apu_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return 0;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return 0;

			std::string num = std::to_string(unitNumber);
			dmg_apu_device *gb_apu = dynamic_cast<dmg_apu_device *>(rm->device((std::string("gbsnd_") + num).c_str()));
			//dmg_apu_device *gb_apu = dynamic_cast<dmg_apu_device *>(rm->root_device().subdevice((std::string("gbsnd_") + num).c_str()));
			if (gb_apu == nullptr)
				return 0;

			dmg_apu_devices[unitNumber] = gb_apu;
		}
		return dmg_apu_devices[unitNumber]->sound_r(address);
	}

	DllExport void gb_apu_wave_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (dmg_apu_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			dmg_apu_device *gb_apu = dynamic_cast<dmg_apu_device *>(rm->device((std::string("gbsnd_") + num).c_str()));
			//dmg_apu_device *gb_apu = dynamic_cast<dmg_apu_device *>(rm->root_device().subdevice((std::string("gbsnd_") + num).c_str()));
			if (gb_apu == nullptr)
				return;

			dmg_apu_devices[unitNumber] = gb_apu;
		}
		dmg_apu_devices[unitNumber]->wave_w(address, data);
	}

	sn76496_base_device *sn76496_base_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void sn76496_write(unsigned int unitNumber, unsigned char data)
	{
		if (sn76496_base_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			sn76496_base_device *sn76496 = dynamic_cast<sn76496_base_device *>(rm->device((std::string("sn76496_") + num).c_str()));
			//sn76496_base_device *sn76496 = dynamic_cast<sn76496_base_device *>(rm->root_device().subdevice((std::string("sn76496_") + num).c_str()));
			if (sn76496 == nullptr)
				return;

			sn76496_base_devices[unitNumber] = sn76496;
		}
		sn76496_base_devices[unitNumber]->write(data);
	}

	namco_cus30_device *namco_cus30_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void namco_cus30_w(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (namco_cus30_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			namco_cus30_device *cus30 = dynamic_cast<namco_cus30_device *>(rm->device((std::string("namco_cus30_") + num).c_str()));
			//namco_cus30_device *cus30 = dynamic_cast<namco_cus30_device *>(rm->root_device().subdevice((std::string("namco_cus30_") + num).c_str()));
			if (cus30 == nullptr)
				return;

			namco_cus30_devices[unitNumber] = cus30;
		}
		namco_cus30_devices[unitNumber]->namcos1_cus30_w(address, data);
	}

	DllExport unsigned char namco_cus30_r(unsigned int unitNumber, unsigned int address)
	{
		if (namco_cus30_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return 0;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return 0;

			std::string num = std::to_string(unitNumber);
			namco_cus30_device *cus30 = dynamic_cast<namco_cus30_device *>(rm->device((std::string("namco_cus30_") + num).c_str()));
			//namco_cus30_device *cus30 = dynamic_cast<namco_cus30_device *>(rm->root_device().subdevice((std::string("namco_cus30_") + num).c_str()));
			if (cus30 == nullptr)
				return 0;

			namco_cus30_devices[unitNumber] = cus30;
		}
		return namco_cus30_devices[unitNumber]->namcos1_cus30_r(address);
	}

	nesapu_device *nesapu_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void nes_apu_regwrite(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (nesapu_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			nesapu_device *nesapu = dynamic_cast<nesapu_device *>(rm->device((std::string("nes_apu_") + num).c_str()));
			//nesapu_device *nesapu = dynamic_cast<nesapu_device *>(rm->root_device().subdevice((std::string("nes_apu_") + num).c_str()));
			if (nesapu == nullptr)
				return;

			nesapu_devices[unitNumber] = nesapu;
		}
		nesapu_devices[unitNumber]->write(address, data);
	}

	DllExport unsigned char nes_apu_regread(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (nesapu_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return 0;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return 0;

			std::string num = std::to_string(unitNumber);
			nesapu_device *nesapu = dynamic_cast<nesapu_device *>(rm->device((std::string("nes_apu_") + num).c_str()));
			//nesapu_device *nesapu = dynamic_cast<nesapu_device *>(rm->root_device().subdevice((std::string("nes_apu_") + num).c_str()));
			if (nesapu == nullptr)
				return 0;

			nesapu_devices[unitNumber] = nesapu;
		}
		return nesapu_devices[unitNumber]->read(address);
	}

	DllExport void nes_apu_set_dpcm(unsigned int unitNumber, unsigned char* address, unsigned int length)
	{
		if (nesapu_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			nesapu_device *nesapu = dynamic_cast<nesapu_device *>(rm->device((std::string("nes_apu_") + num).c_str()));
			//nesapu_device *nesapu = dynamic_cast<nesapu_device *>(rm->root_device().subdevice((std::string("nes_apu_") + num).c_str()));
			if (nesapu == nullptr)
				return;

			nesapu_devices[unitNumber] = nesapu;
		}
		nesapu_devices[unitNumber]->set_dpcm(address, length);
	}

	k051649_device *k051649_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void SCC1_waveform_w(unsigned int unitNumber, unsigned int address, char *data, int length)
	{
		if (k051649_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->device((std::string("scc1_") + num).c_str()));
			//k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->root_device().subdevice((std::string("scc1_") + num).c_str()));
			if (scc1 == nullptr)
				return;

			k051649_devices[unitNumber] = scc1;
		}
		for (int i = 0; i < length; i++)
			k051649_devices[unitNumber]->k052539_waveform_w(address + i, data[i]);
	}

	DllExport void SCC1_volume_w(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (k051649_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->device((std::string("scc1_") + num).c_str()));
			//k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->root_device().subdevice((std::string("scc1_") + num).c_str()));
			if (scc1 == nullptr)
				return;

			k051649_devices[unitNumber] = scc1;
		}
		k051649_devices[unitNumber]->k051649_volume_w(address, data);
	}

	DllExport void SCC1_frequency_w(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (k051649_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->device((std::string("scc1_") + num).c_str()));
			//k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->root_device().subdevice((std::string("scc1_") + num).c_str()));
			if (scc1 == nullptr)
				return;

			k051649_devices[unitNumber] = scc1;
		}
		k051649_devices[unitNumber]->k051649_frequency_w(address, data);
	}

	DllExport void SCC1_keyonoff_w(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (k051649_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->device((std::string("scc1_") + num).c_str()));
			//k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->root_device().subdevice((std::string("scc1_") + num).c_str()));
			if (scc1 == nullptr)
				return;

			k051649_devices[unitNumber] = scc1;
		}
		k051649_devices[unitNumber]->k051649_keyonoff_w(data);
	}

	DllExport unsigned char SCC1_keyonoff_r(unsigned int unitNumber, unsigned int address)
	{
		if (k051649_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return 0;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return 0;

			std::string num = std::to_string(unitNumber);
			k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->device((std::string("scc1_") + num).c_str()));
			//k051649_device *scc1 = dynamic_cast<k051649_device *>(rm->root_device().subdevice((std::string("scc1_") + num).c_str()));
			if (scc1 == nullptr)
				return 0;

			k051649_devices[unitNumber] = scc1;
		}
		return k051649_devices[unitNumber]->k051649_keyonoff_r();
	}

	msm5232_device *msm5232_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void msm5232_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (msm5232_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			msm5232_device *msm5232 = dynamic_cast<msm5232_device *>(rm->device((std::string("msm5232_") + num).c_str()));
			//msm5232_device *msm5232 = dynamic_cast<msm5232_device *>(rm->root_device().subdevice((std::string("msm5232_") + num).c_str()));
			if (msm5232 == nullptr)
				return;

			msm5232_devices[unitNumber] = msm5232;
		}
		msm5232_devices[unitNumber]->write(address, data);
	}

	DllExport void msm5232_set_volume(unsigned int unitNumber, unsigned int ch, unsigned char data)
	{
		if (msm5232_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			msm5232_device *msm5232 = dynamic_cast<msm5232_device *>(rm->device((std::string("msm5232_") + num).c_str()));
			//msm5232_device *msm5232 = dynamic_cast<msm5232_device *>(rm->root_device().subdevice((std::string("msm5232_") + num).c_str()));
			if (msm5232 == nullptr)
				return;

			msm5232_devices[unitNumber] = msm5232;
		}
		msm5232_devices[unitNumber]->set_volume(ch, data);
	}


	DllExport void msm5232_set_capacitors(unsigned int unitNumber, double cap1, double cap2, double cap3, double cap4, double cap5, double cap6, double cap7, double cap8)
	{
		if (msm5232_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			msm5232_device *msm5232 = dynamic_cast<msm5232_device *>(rm->device((std::string("msm5232_") + num).c_str()));
			//msm5232_device *msm5232 = dynamic_cast<msm5232_device *>(rm->root_device().subdevice((std::string("msm5232_") + num).c_str()));
			if (msm5232 == nullptr)
				return;

			msm5232_devices[unitNumber] = msm5232;
		}
		msm5232_devices[unitNumber]->set_capacitors(cap1, cap2, cap3, cap4, cap5, cap6, cap7, cap8);
	}

	ay8910_device *ay8910_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void ay8910_address_data_w(unsigned int unitNumber, int offset, unsigned char data)
	{
		if (ay8910_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			ay8910_device *ay8910 = dynamic_cast<ay8910_device*>(rm->device((std::string("ay8910_") + num).c_str()));
			//ay8910_device *ay8910 = dynamic_cast<ay8910_device*>(rm->root_device().subdevice((std::string("ay8910_") + num).c_str()));
			if (ay8910 == nullptr)
				return;

			ay8910_devices[unitNumber] = ay8910;
		}
		ay8910_devices[unitNumber]->address_data_w(offset, data);
	}

	DllExport unsigned char ay8910_read_ym(unsigned int unitNumber)
	{
		if (ay8910_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return 0;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return 0;

			std::string num = std::to_string(unitNumber);
			ay8910_device *ay8910 = dynamic_cast<ay8910_device*>(rm->device((std::string("ay8910_") + num).c_str()));
			//ay8910_device *ay8910 = dynamic_cast<ay8910_device*>(rm->root_device().subdevice((std::string("ay8910_") + num).c_str()));
			if (ay8910 == nullptr)
				return 0;

			ay8910_devices[unitNumber] = ay8910;
		}
		return ay8910_devices[unitNumber]->data_r();
	}

	mos8580_device *mos8580_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void mos8580_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (mos8580_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			mos8580_device *mos8580 = dynamic_cast<mos8580_device*>(rm->device((std::string("mos8580_") + num).c_str()));
			//mos8580_device *mos8580 = dynamic_cast<mos8580_device*>(rm->root_device().subdevice((std::string("mos8580_") + num).c_str()));
			if (mos8580 == nullptr)
				return;

			mos8580_devices[unitNumber] = mos8580;
		}
		mos8580_devices[unitNumber]->write(address, data);
	}

	mos6581_device *mos6581_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void mos6581_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (mos6581_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			mos6581_device *mos6581 = dynamic_cast<mos6581_device*>(rm->device((std::string("mos6581_") + num).c_str()));
			//mos6581_device *mos6581 = dynamic_cast<mos6581_device*>(rm->root_device().subdevice((std::string("mos6581_") + num).c_str()));
			if (mos6581 == nullptr)
				return;

			mos6581_devices[unitNumber] = mos6581;
		}
		mos6581_devices[unitNumber]->write(address, data);
	}

	beep_device *beep_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void beep_set_clock(unsigned int unitNumber, int state, unsigned int frequency)
	{
		if (beep_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			beep_device *beep = dynamic_cast<beep_device *>(rm->device((std::string("beep_") + num).c_str()));
			//beep_device *beep = dynamic_cast<beep_device *>(rm->root_device().subdevice((std::string("beep_") + num).c_str()));
			if (beep == nullptr)
				return;

			if (frequency != 0)
				beep->set_clock(frequency);

			beep_devices[unitNumber] = beep;
		}
		beep_devices[unitNumber]->set_state(state);
	}

	c140_device *c140_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void c140_w(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (c140_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			c140_device *c140 = dynamic_cast<c140_device *>(rm->device((std::string("c140_") + num).c_str()));
			//c140_device *c140 = dynamic_cast<c140_device *>(rm->root_device().subdevice((std::string("c140_") + num).c_str()));
			if (c140 == nullptr)
				return;

			c140_devices[unitNumber] = c140;
		}
		c140_devices[unitNumber]->c140_w(address, data);
	}

	DllExport void c140_set_callback(unsigned int unitNumber, C140_CALLBACK callback)
	{
		if (c140_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			c140_device *c140 = dynamic_cast<c140_device *>(rm->device((std::string("c140_") + num).c_str()));
			//c140_device *c140 = dynamic_cast<c140_device *>(rm->root_device().subdevice((std::string("c140_") + num).c_str()));
			if (c140 == nullptr)
				return;

			c140_devices[unitNumber] = c140;
		}
		c140_devices[unitNumber]->set_callback(callback);
	}


	c6280_device *c6280_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void c6280_w(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (c6280_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			c6280_device *c6280 = dynamic_cast<c6280_device *>(rm->device((std::string("c6280_") + num).c_str()));
			if (c6280 == nullptr)
				return;

			c6280_devices[unitNumber] = c6280;
		}
		c6280_devices[unitNumber]->c6280_w(address, data);
	}


	DllExport void c6280_set_pcm_callback(unsigned int unitNumber, C6280_PCM_CALLBACK callback)
	{
		if (c6280_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			c6280_device *c6280 = dynamic_cast<c6280_device *>(rm->device((std::string("c6280_") + num).c_str()));
			if (c6280 == nullptr)
				return;

			c6280_devices[unitNumber] = c6280;
		}

		c6280_devices[unitNumber]->set_pcm_callback(callback);
	}

	snes_sound_device *spc700_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void spc_ram_w(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (spc700_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			snes_sound_device *spc700 = dynamic_cast<snes_sound_device *>(rm->device((std::string("snes_sound_") + num).c_str()));
			if (spc700 == nullptr)
				return;

			spc700_devices[unitNumber] = spc700;
		}
		spc700_devices[unitNumber]->spc_ram_w(address, data);
	}

	DllExport unsigned char spc_ram_r(unsigned int unitNumber, unsigned int address)
	{
		if (spc700_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return 0;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return 0;

			std::string num = std::to_string(unitNumber);
			snes_sound_device *spc700 = dynamic_cast<snes_sound_device *>(rm->device((std::string("snes_sound_") + num).c_str()));
			if (spc700 == nullptr)
				return 0;

			spc700_devices[unitNumber] = spc700;
		}
		return spc700_devices[unitNumber]->spc_ram_r(address);
	}

	DllExport void spc700_set_callback(unsigned int unitNumber, SPC700_CALLBACK callback)
	{
		if (spc700_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			snes_sound_device *spc700 = dynamic_cast<snes_sound_device *>(rm->device((std::string("snes_sound_") + num).c_str()));
			if (spc700 == nullptr)
				return;

			spc700_devices[unitNumber] = spc700;
		}
		spc700_devices[unitNumber]->set_callback(callback);
	}

	DllExport void spc700_resample(double org_rate, double target_rate, short* org_buffer, size_t org_len, short* target_buffer, size_t target_len)
	{
		soxr_datatype_t itype = SOXR_INT16_I;
		soxr_datatype_t otype = SOXR_INT16_I;
		soxr_io_spec_t iospec = soxr_io_spec(itype, otype);
		soxr_quality_spec_t qSpec = soxr_quality_spec(SOXR_20_BITQ, 0);

		size_t idone = 0;
		size_t odone = 0;

		soxr_oneshot(org_rate, target_rate, 1, org_buffer, org_len, &idone, target_buffer, target_len, &odone, &iospec, &qSpec, NULL);
	}

	pokey_device *pokey_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void pokey_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (pokey_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			pokey_device *pokey = dynamic_cast<pokey_device *>(rm->device((std::string("pokey_") + num).c_str()));
			if (pokey == nullptr)
				return;

			pokey_devices[unitNumber] = pokey;
		}
		pokey_devices[unitNumber]->write(address, data);
	}

	DllExport unsigned char pokey_read(unsigned int unitNumber, unsigned int address)
	{
		if (pokey_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return 0;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return 0;

			std::string num = std::to_string(unitNumber);
			pokey_device *pokey = dynamic_cast<pokey_device *>(rm->device((std::string("pokey_") + num).c_str()));
			if (pokey == nullptr)
				return 0;

			pokey_devices[unitNumber] = pokey;
		}
		return pokey_devices[unitNumber]->read(address);
	}

	DllExport void pokey_set_output_type(unsigned int unitNumber, int type, double r, double c, double v)
	{
		if (pokey_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			pokey_device *pokey = dynamic_cast<pokey_device *>(rm->device((std::string("pokey_") + num).c_str()));
			if (pokey == nullptr)
				return;

			pokey_devices[unitNumber] = pokey;
		}
		pokey_devices[unitNumber]->set_output_type(type, r, c, v);
	}


	ym2610b_device  *ym2610b_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void ym2610b_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (ym2610b_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			ym2610b_device *ym2610b = dynamic_cast<ym2610b_device  *>(rm->device((std::string("ym2610b_") + num).c_str()));
			if (ym2610b == nullptr)
				return;

			ym2610b_devices[unitNumber] = ym2610b;
		}
		ym2610b_devices[unitNumber]->write(address, data);
	}


	DllExport unsigned char ym2610b_read(unsigned int unitNumber, unsigned int address)
	{
		if (ym2610b_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return 0;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return 0;

			std::string num = std::to_string(unitNumber);
			ym2610b_device *ym2610b = dynamic_cast<ym2610b_device  *>(rm->device((std::string("ym2610b_") + num).c_str()));
			if (ym2610b == nullptr)
				return 0;

			ym2610b_devices[unitNumber] = ym2610b;
		}
		return ym2610b_devices[unitNumber]->read(address);
	}

	DllExport void ym2610b_set_adpcm_callback(unsigned int unitNumber, OPNA_ADPCM_CALLBACK callback)
	{
		if (ym2610b_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			ym2610b_device *ym2610b = dynamic_cast<ym2610b_device  *>(rm->device((std::string("ym2610b_") + num).c_str()));
			if (ym2610b == nullptr)
				return;

			ym2610b_devices[unitNumber] = ym2610b;
		}
		ym2610b_devices[unitNumber]->set_adpcm_callback(callback);
	}

	mt32_device  *mt32_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	/** Enqueues a single short MIDI message to be processed ASAP. The message must contain a status byte. */
	DllExport void mt32_play_msg(unsigned int unitNumber, mt32emu_bit32u msg)
	{
		if (mt32_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			mt32_device *mt32 = dynamic_cast<mt32_device   *>(rm->device((std::string("mt32_") + num).c_str()));
			if (mt32 == nullptr)
				return;

			mt32_devices[unitNumber] = mt32;
		}

		mt32_devices[unitNumber]->play_msg(msg);
	}

	/** Enqueues a single well formed System Exclusive MIDI message to be processed ASAP. */
	DllExport void mt32_play_sysex(unsigned int unitNumber, const mt32emu_bit8u *sysex, mt32emu_bit32u len)
	{
		if (mt32_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			mt32_device *mt32 = dynamic_cast<mt32_device   *>(rm->device((std::string("mt32_") + num).c_str()));
			if (mt32 == nullptr)
				return;

			mt32_devices[unitNumber] = mt32;
		}

		mt32_devices[unitNumber]->play_sysex(sysex, len);
	}

	cm32p_device  *cm32p_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	/** Enqueues a single short MIDI message to be processed ASAP. The message must contain a status byte. */
	DllExport void cm32p_play_msg(unsigned int unitNumber, unsigned char type, unsigned char channel, unsigned int param1, unsigned int param2)
	{
		if (cm32p_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			cm32p_device *cm32p = dynamic_cast<cm32p_device   *>(rm->device((std::string("cm32p_") + num).c_str()));
			if (cm32p == nullptr)
				return;

			cm32p_devices[unitNumber] = cm32p;
		}

		cm32p_devices[unitNumber]->play_msg(type, channel, param1, param2);
	}

	DllExport void cm32p_play_sysex(unsigned int unitNumber, const u8 *sysex, u32 len)
	{
		if (cm32p_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			cm32p_device *cm32p = dynamic_cast<cm32p_device   *>(rm->device((std::string("cm32p_") + num).c_str()));
			if (cm32p == nullptr)
				return;

			cm32p_devices[unitNumber] = cm32p;
		}

		cm32p_devices[unitNumber]->play_sysex(sysex, len);
	}

	DllExport fluid_sfont_t * cm32p_load_sf(unsigned int unitNumber, unsigned char card_id, const char* filename)
	{
		if (cm32p_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return 0;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return 0;

			std::string num = std::to_string(unitNumber);
			cm32p_device *cm32p = dynamic_cast<cm32p_device   *>(rm->device((std::string("cm32p_") + num).c_str()));
			if (cm32p == nullptr)
				return 0;

			cm32p_devices[unitNumber] = cm32p;
		}

		return cm32p_devices[unitNumber]->load_sf(card_id, filename);
	}


	DllExport void cm32p_add_sf(unsigned int unitNumber, u8 card_id, fluid_sfont_t * sf)
	{
		if (cm32p_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			cm32p_device *cm32p = dynamic_cast<cm32p_device   *>(rm->device((std::string("cm32p_") + num).c_str()));
			if (cm32p == nullptr)
				return;

			cm32p_devices[unitNumber] = cm32p;
		}

		return cm32p_devices[unitNumber]->add_sf(card_id, sf);
	}

	DllExport void cm32p_set_tone(unsigned int unitNumber, unsigned char card_id, unsigned short tone_no, unsigned short sf_preset_no)
	{
		if (cm32p_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			cm32p_device *cm32p = dynamic_cast<cm32p_device   *>(rm->device((std::string("cm32p_") + num).c_str()));
			if (cm32p == nullptr)
				return;

			cm32p_devices[unitNumber] = cm32p;
		}

		cm32p_devices[unitNumber]->set_tone(card_id, tone_no, sf_preset_no);
	}


	DllExport void cm32p_set_card(unsigned int unitNumber, unsigned char card_id)
	{
		if (cm32p_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			cm32p_device *cm32p = dynamic_cast<cm32p_device   *>(rm->device((std::string("cm32p_") + num).c_str()));
			if (cm32p == nullptr)
				return;

			cm32p_devices[unitNumber] = cm32p;
		}

		cm32p_devices[unitNumber]->set_card(card_id);
	}


	DllExport void cm32p_initialize_memory(unsigned int unitNumber)
	{
		if (cm32p_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			cm32p_device *cm32p = dynamic_cast<cm32p_device   *>(rm->device((std::string("cm32p_") + num).c_str()));
			if (cm32p == nullptr)
				return;

			cm32p_devices[unitNumber] = cm32p;
		}

		cm32p_devices[unitNumber]->initialize_memory();
	}


	DllExport void cm32p_set_chanAssign(unsigned int unitNumber, u8 *assign)
	{
		if (cm32p_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			cm32p_device *cm32p = dynamic_cast<cm32p_device   *>(rm->device((std::string("cm32p_") + num).c_str()));
			if (cm32p == nullptr)
				return;

			cm32p_devices[unitNumber] = cm32p;
		}

		cm32p_devices[unitNumber]->set_chanAssign(assign);
	}


	DllExport void cm32p_get_chanAssign(unsigned int unitNumber, u8 *assign)
	{
		if (cm32p_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			cm32p_device *cm32p = dynamic_cast<cm32p_device   *>(rm->device((std::string("cm32p_") + num).c_str()));
			if (cm32p == nullptr)
				return;

			cm32p_devices[unitNumber] = cm32p;
		}

		cm32p_devices[unitNumber]->get_chanAssign(assign);
	}


	ym2608_device  *ym2608_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void ym2608_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (ym2608_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			ym2608_device *ym2608 = dynamic_cast<ym2608_device  *>(rm->device((std::string("ym2608_") + num).c_str()));
			if (ym2608 == nullptr)
				return;

			ym2608_devices[unitNumber] = ym2608;
		}
		ym2608_devices[unitNumber]->write(address, data);
	}


	DllExport unsigned char ym2608_read(unsigned int unitNumber, unsigned int address)
	{
		if (ym2608_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return 0;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return 0;

			std::string num = std::to_string(unitNumber);
			ym2608_device *ym2608 = dynamic_cast<ym2608_device  *>(rm->device((std::string("ym2608_") + num).c_str()));
			if (ym2608 == nullptr)
				return 0;

			ym2608_devices[unitNumber] = ym2608;
		}
		return ym2608_devices[unitNumber]->read(address);
	}

	DllExport void ym2608_set_adpcma_callback(unsigned int unitNumber, OPNA_ADPCM_CALLBACK callback)
	{
		if (ym2608_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			ym2608_device *ym2608 = dynamic_cast<ym2608_device  *>(rm->device((std::string("ym2608_") + num).c_str()));
			if (ym2608 == nullptr)
				return;

			ym2608_devices[unitNumber] = ym2608;
		}
		ym2608_devices[unitNumber]->set_adpcma_callback(callback);
	}

	DllExport void ym2608_set_adpcmb_callback(unsigned int unitNumber, OPNA_ADPCM_CALLBACK callback)
	{
		if (ym2608_devices[unitNumber] == NULL)
		{
			mame_machine_manager *mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine *rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			ym2608_device *ym2608 = dynamic_cast<ym2608_device  *>(rm->device((std::string("ym2608_") + num).c_str()));
			if (ym2608 == nullptr)
				return;

			ym2608_devices[unitNumber] = ym2608;
		}
		ym2608_devices[unitNumber]->set_adpcmb_callback(callback);
	}

	tms5220_device* tms5220_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void tms5220_data_w(unsigned int unitNumber, unsigned char data)
	{
		if (tms5220_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			tms5220_device* tms5220 = dynamic_cast<tms5220_device*>(rm->device((std::string("tms5220_") + num).c_str()));
			if (tms5220 == nullptr)
				return;

			tms5220_devices[unitNumber] = tms5220;
		}
		tms5220_devices[unitNumber]->data_w(data);
	}

	DllExport int tms5220_status_r(unsigned int unitNumber)
	{
		if (tms5220_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return 0;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return 0;

			std::string num = std::to_string(unitNumber);
			tms5220_device* tms5220 = dynamic_cast<tms5220_device*>(rm->device((std::string("tms5220_") + num).c_str()));
			if (tms5220 == nullptr)
				return 0;

			tms5220_devices[unitNumber] = tms5220;
		}
		return tms5220_devices[unitNumber]->status_r();
	}

	sp0256_device* sp0256_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void sp0256_ald_w(unsigned int unitNumber, unsigned char data)
	{
		if (sp0256_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			sp0256_device* sp0256 = dynamic_cast<sp0256_device*>(rm->device((std::string("sp0256_") + num).c_str()));
			if (sp0256 == nullptr)
				return;

			sp0256_devices[unitNumber] = sp0256;
		}
		sp0256_devices[unitNumber]->ald_w(data);
	}

	DllExport int sp0256_sby_r(unsigned int unitNumber)
	{
		if (sp0256_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return 0;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return 0;

			std::string num = std::to_string(unitNumber);
			sp0256_device* sp0256 = dynamic_cast<sp0256_device*>(rm->device((std::string("sp0256_") + num).c_str()));
			if (sp0256 == nullptr)
				return 0;

			sp0256_devices[unitNumber] = sp0256;
		}
		return sp0256_devices[unitNumber]->sby_r();
	}

	DllExport int sp0256_lrq_r(unsigned int unitNumber)
	{
		if (sp0256_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return 0;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return 0;

			std::string num = std::to_string(unitNumber);
			sp0256_device* sp0256 = dynamic_cast<sp0256_device*>(rm->device((std::string("sp0256_") + num).c_str()));
			if (sp0256 == nullptr)
				return 0;

			sp0256_devices[unitNumber] = sp0256;
		}
		return sp0256_devices[unitNumber]->lrq_r();
	}

	DllExport void sp0256_set_clock(unsigned int unitNumber, int clock)
	{
		if (sp0256_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			sp0256_device* sp0256 = dynamic_cast<sp0256_device*>(rm->device((std::string("sp0256_") + num).c_str()));
			if (sp0256 == nullptr)
				return;

			sp0256_devices[unitNumber] = sp0256;
		}
		 sp0256_devices[unitNumber]->set_clock(clock);
	}

	samples_device* sam_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void sam_start_raw(unsigned int unitNumber, uint8_t channel, const int16_t* sampledata, uint32_t samples, uint32_t frequency, bool loop)
	{
		if (sam_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			samples_device* sam = dynamic_cast<samples_device*>(rm->device((std::string("sam_") + num).c_str()));
			if (sam == nullptr)
				return;

			sam_devices[unitNumber] = sam;
		}
		sam_devices[unitNumber]->start_raw(channel, sampledata,samples, frequency, loop);
	}

	DllExport void sam_stop(unsigned int unitNumber, uint8_t channel)
	{
		if (sam_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			samples_device* sam = dynamic_cast<samples_device*>(rm->device((std::string("sam_") + num).c_str()));
			if (sam == nullptr)
				return;

			sam_devices[unitNumber] = sam;
		}
		sam_devices[unitNumber]->stop(channel);
	}

	DllExport void sam_set_frequency(unsigned int unitNumber, uint8_t channel, uint32_t frequency)
	{
		if (sam_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			samples_device* sam = dynamic_cast<samples_device*>(rm->device((std::string("sam_") + num).c_str()));
			if (sam == nullptr)
				return;

			sam_devices[unitNumber] = sam;
		}
		sam_devices[unitNumber]->set_frequency(channel, frequency);
	}

	DllExport void sam_set_volume(unsigned int unitNumber, uint8_t channel, float volume)
	{
		if (sam_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			samples_device* sam = dynamic_cast<samples_device*>(rm->device((std::string("sam_") + num).c_str()));
			if (sam == nullptr)
				return;

			sam_devices[unitNumber] = sam;
		}
		sam_devices[unitNumber]->set_volume(channel, volume);
	}

	sn76477_device* sn76477_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void sn76477_logic_w(unsigned int unitNumber, uint8_t type, int state)
	{
		if (sam_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			sn76477_device* sn76477 = dynamic_cast<sn76477_device*>(rm->device((std::string("sn76477_") + num).c_str()));
			if (sn76477 == nullptr)
				return;

			sn76477_devices[unitNumber] = sn76477;
		}
		switch (type)
		{
			case 0:
				sn76477_devices[unitNumber]->enable_w(state);
				break;
			case 1:
				sn76477_devices[unitNumber]->mixer_a_w(state);
				break;
			case 2:
				sn76477_devices[unitNumber]->mixer_b_w(state);
				break;
			case 3:
				sn76477_devices[unitNumber]->mixer_c_w(state);
				break;
			case 4:
				sn76477_devices[unitNumber]->envelope_1_w(state);
				break;
			case 5:
				sn76477_devices[unitNumber]->envelope_2_w(state);
				break;
			case 6:
				sn76477_devices[unitNumber]->vco_w(state);
				break;
			case 7:
				sn76477_devices[unitNumber]->noise_clock_w(state);
				break;
		}
	}


	DllExport void sn76477_res_w(unsigned int unitNumber, uint8_t type, double data)
	{
		if (sam_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			sn76477_device* sn76477 = dynamic_cast<sn76477_device*>(rm->device((std::string("sn76477_") + num).c_str()));
			if (sn76477 == nullptr)
				return;

			sn76477_devices[unitNumber] = sn76477;
		}
		switch (type)
		{
			case 0:
				sn76477_devices[unitNumber]->one_shot_res_w(data);
				break;
			case 1:
				sn76477_devices[unitNumber]->slf_res_w(data);
				break;
			case 2:
				sn76477_devices[unitNumber]->vco_res_w(data);
				break;
			case 3:
				sn76477_devices[unitNumber]->noise_clock_res_w(data);  /* = 0 if the noise gen is clocked via noise_clock */
				break;
			case 4:
				sn76477_devices[unitNumber]->noise_filter_res_w(data);
				break;
			case 5:
				sn76477_devices[unitNumber]->decay_res_w(data);
				break;
			case 6:
				sn76477_devices[unitNumber]->attack_res_w(data);
				break;
			case 7:
				sn76477_devices[unitNumber]->amplitude_res_w(data);
				break;
			case 8:
				sn76477_devices[unitNumber]->feedback_res_w(data);
				break;
		}
	}


	DllExport void sn76477_cap_w(unsigned int unitNumber, uint8_t type, double data)
	{
		if (sam_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			sn76477_device* sn76477 = dynamic_cast<sn76477_device*>(rm->device((std::string("sn76477_") + num).c_str()));
			if (sn76477 == nullptr)
				return;

			sn76477_devices[unitNumber] = sn76477;
		}
		switch (type)
		{
			case 0:
				sn76477_devices[unitNumber]->one_shot_cap_w(data);
				break;
			case 1:
				sn76477_devices[unitNumber]->one_shot_cap_voltage_w(data);
				break;
			case 2:
				sn76477_devices[unitNumber]->slf_cap_w(data);
				break;
			case 3:
				sn76477_devices[unitNumber]->slf_cap_voltage_w(data);
				break;
			case 4:
				sn76477_devices[unitNumber]->vco_cap_w(data);
				break;
			case 5:
				sn76477_devices[unitNumber]->vco_cap_voltage_w(data);
				break;
			case 6:
				sn76477_devices[unitNumber]->noise_filter_cap_w(data);
				break;
			case 7:
				sn76477_devices[unitNumber]->noise_filter_cap_voltage_w(data);
				break;
			case 8:
				sn76477_devices[unitNumber]->attack_decay_cap_w(data);
				break;
			case 9:
				sn76477_devices[unitNumber]->attack_decay_cap_voltage_w(data);
				break;
		}
	}


	DllExport void sn76477_voltage_w(unsigned int unitNumber, uint8_t type, double data)
	{
		if (sam_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			sn76477_device* sn76477 = dynamic_cast<sn76477_device*>(rm->device((std::string("sn76477_") + num).c_str()));
			if (sn76477 == nullptr)
				return;

			sn76477_devices[unitNumber] = sn76477;
		}
		switch (type)
		{
		case 0:
			sn76477_devices[unitNumber]->vco_voltage_w(data);
			break;
		case 1:
			sn76477_devices[unitNumber]->pitch_voltage_w(data);
			break;
		}
	}


	upd1771c_device* upd1771c_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void uPD1771_write(unsigned int unitNumber, uint8_t data)
	{
		if (upd1771c_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			upd1771c_device* upd1771c = dynamic_cast<upd1771c_device*>(rm->device((std::string("upd1771_") + num).c_str()));
			if (upd1771c == nullptr)
				return;

			upd1771c_devices[unitNumber] = upd1771c;
		}
		upd1771c_devices[unitNumber]->write(data);
	}


	ymfm_opz_device* ymfm_opz_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void ymfm_opz_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (ymfm_opz_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			ymfm_opz_device* ymfm_opz = dynamic_cast<ymfm_opz_device*>(rm->device((std::string("ymfm_opz_") + num).c_str()));
			if (ymfm_opz == nullptr)
				return;

			ymfm_opz_devices[unitNumber] = ymfm_opz;
		}
		ymfm_opz_devices[unitNumber]->write(address, data);
	}


	ymfm_opq_device* ymfm_opq_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void ymfm_opq_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (ymfm_opq_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			ymfm_opq_device* ymfm_opq = dynamic_cast<ymfm_opq_device*>(rm->device((std::string("ymfm_opq_") + num).c_str()));
			if (ymfm_opq == nullptr)
				return;

			ymfm_opq_devices[unitNumber] = ymfm_opq;
		}
		ymfm_opq_devices[unitNumber]->write(address, data);
	}


	multipcm_device* multipcm_device_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void multipcm_device_reg_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (multipcm_device_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			multipcm_device* multipcm = dynamic_cast<multipcm_device*>(rm->device((std::string("multipcm_") + num).c_str()));
			if (multipcm == nullptr)
				return;

			multipcm_device_devices[unitNumber] = multipcm;
		}
		multipcm_device_devices[unitNumber]->write(address, data);
	}

	DllExport void multipcm_device_mem_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (multipcm_device_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			multipcm_device* multipcm = dynamic_cast<multipcm_device*>(rm->device((std::string("multipcm_") + num).c_str()));
			if (multipcm == nullptr)
				return;

			multipcm_device_devices[unitNumber] = multipcm;
		}
		multipcm_device_devices[unitNumber]->write_byte(address, data);
	}

	rf5c68_device* rf5c68_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void rf5c68_device_mem_w(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (rf5c68_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			rf5c68_device* rf5c68 = dynamic_cast<rf5c68_device*>(rm->device((std::string("rf5c164_") + num).c_str()));
			if (rf5c68 == nullptr)
				return;

			rf5c68_devices[unitNumber] = rf5c68;
		}
		rf5c68_devices[unitNumber]->rf5c68_mem_w(address, data);
	}

	DllExport void rf5c68_device_w(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (rf5c68_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			rf5c68_device* rf5c68 = dynamic_cast<rf5c68_device*>(rm->device((std::string("rf5c164_") + num).c_str()));
			if (rf5c68 == nullptr)
				return;

			rf5c68_devices[unitNumber] = rf5c68;
		}
		rf5c68_devices[unitNumber]->rf5c68_w(address, data);
	}

	saa1099_device* saa1099_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void saa1099_device_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (saa1099_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			saa1099_device* saa1099 = dynamic_cast<saa1099_device*>(rm->device((std::string("saa1099_") + num).c_str()));
			if (saa1099 == nullptr)
				return;

			saa1099_devices[unitNumber] = saa1099;
		}
		saa1099_devices[unitNumber]->write(address, data);
	}

	paula_8364_device* paula_8364_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void paula_8364_write(unsigned int unitNumber, unsigned int address, unsigned short data)
	{
		if (paula_8364_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			paula_8364_device* paula = dynamic_cast<paula_8364_device*>(rm->device((std::string("paula_8364_") + num).c_str()));
			if (paula == nullptr)
				return;

			paula_8364_devices[unitNumber] = paula;
		}
		paula_8364_devices[unitNumber]->reg_w(address, data);
	}

	DllExport void paula_8364_keyon(unsigned int unitNumber, uint8_t ch, uint8_t id, uint8_t vol, uint16_t period, uint16_t length, uint16_t loop)
	{
		if (paula_8364_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			paula_8364_device* paula = dynamic_cast<paula_8364_device*>(rm->device((std::string("paula_8364_") + num).c_str()));
			if (paula == nullptr)
				return;

			paula_8364_devices[unitNumber] = paula;
		}
		paula_8364_devices[unitNumber]->keyon(ch, id, vol, period, length, loop);
	}

	DllExport void paula_8364_keyoff(unsigned int unitNumber, uint8_t ch)
	{
		if (paula_8364_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			paula_8364_device* paula = dynamic_cast<paula_8364_device*>(rm->device((std::string("paula_8364_") + num).c_str()));
			if (paula == nullptr)
				return;

			paula_8364_devices[unitNumber] = paula;
		}
		paula_8364_devices[unitNumber]->keyoff(ch);
	}

	DllExport void paula_8364_set_callback(unsigned int unitNumber, PAULA_CALLBACK callback)
	{
		if (paula_8364_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			paula_8364_device* paula = dynamic_cast<paula_8364_device*>(rm->device((std::string("paula_8364_") + num).c_str()));
			if (paula == nullptr)
				return;

			paula_8364_devices[unitNumber] = paula;
		}
		paula_8364_devices[unitNumber]->set_callback(callback);
	}

	tia_device* tia_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void tia_device_write(unsigned int unitNumber, unsigned int address, unsigned char data)
	{
		if (tia_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			tia_device* tia = dynamic_cast<tia_device*>(rm->device((std::string("tia_") + num).c_str()));
			if (tia == nullptr)
				return;

			tia_devices[unitNumber] = tia;
		}
		tia_devices[unitNumber]->tia_sound_w(address, data);
	}

	vlm5030_device* vlm_devices[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

	DllExport void vlm_device_write_data_and_play(unsigned int unitNumber, unsigned char *data, unsigned short length, unsigned char stat)
	{
		if (vlm_devices[unitNumber] == NULL)
		{
			mame_machine_manager* mmm = mame_machine_manager::instance();
			if (mmm == nullptr)
				return;
			running_machine* rm = mmm->machine();
			if (rm == nullptr || rm->phase() == machine_phase::EXIT)
				return;

			std::string num = std::to_string(unitNumber);
			vlm5030_device* vlm = dynamic_cast<vlm5030_device*>(rm->device((std::string("vlm_") + num).c_str()));
			if (vlm == nullptr)
				return;

			vlm_devices[unitNumber] = vlm;
		}

		vlm_devices[unitNumber]->write_data_and_play(data, length, stat);
	}
}

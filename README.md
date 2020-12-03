MAmidiMEmo V2.6.2.0 / Itoken (c)2019, 2020 / GPL-2.0

*** What is the MAmidiMEmo? ***

MAmidiMEmo is a virtual chiptune sound MIDI module using a MAME sound engine.
You can control various chips and make sound via MIDI I/F.
So, you don't need to use dedicated tracker and so on anymore. You can use your favorite MIDI sequencer to make a chip sound.

MAmidiMEmo adopts multi timbre method. That mean you can play multi chords on MIDI 1ch if the chip has multi ch.

e.g.) YM2151 has 8ch FM sounds, so you can play 8 chords on MIDI 1ch or sharing 8ch with MIDI 16ch like a SC-55 and so on.
      As well, NES APU has 2ch for square wave, so you can play 2 chords on MIDI 1ch when select a square wave timbre.
	  However, when you select a triangle wave timbre, you can play 1 chord on MIDI 1ch. Because NES APU has only 1ch for triangle wave.

*** How to use MAmidiMEmo ***

0. Requirements

	*H/W

		CPU: Intel Core series CPU or equivalent, at least 2.0 GHz
		MEM: at least 4GB
		Sound: DirectSound capable sound card/onboard audio
		MIDI: MIDI IN I/F

	*S/W

		OS: Windows 7 SP1 or lator
		Runtime: .NET Framework 4.7 or lator
		         VC++ 2012 Runtime https://www.microsoft.com/en-au/download/details.aspx?id=30679

1. Extract downloaded zip file.
   Execute "DelZoneID.ps1" on PowerShell to remove "Zone.Identifier" flag.

2. Install VC++ runtime 2012 and .NET Framework 4.7 or lator.

3. Launch MAmidiMEmo.exe

   Note: You can change the value of audio latency, sampling rate and audio output interface by [Tool] menu.
         PortAudio is a low latency sound engine. See http://www.portaudio.com/

4. Select MIDI I/F from toolbar. MAmidiMEmo will recevie MIDI message from the selected MIDI I/F.

   Note: You can use the loopMIDI (See http://www.tobias-erichsen.de/software/loopmidi.html) to send MIDI message from this PC to MAmidiMEmo.

5. Add your favorite chips from the [Instruments] menu on the toolbar.

   Note: Currently supported chips are the following.

        YM2151, YM2612, YM3812, YM2413, YM2610B, YMF262
		YM2601 ★★★ Place legitimate ym2608_adpcm_rom.bin file in the Mami dir ★★★
        SID, POKEY, GB APU, SN76496, NES APU, MSM5232(+TA7630), AY-3-8910
        NAMCO CUS30, SCC, HuC6280
        C140, SPC700
        MT-32 ★★★ Place legitimate MT32_CONTROL.ROM and MT32_PCM.ROM files in the Mami dir ★★★
		CM-32P This is an incomplete simulator, not an emulator.
		       You can mod and add your custom sounds by editing tbl and sound font files.
		       Please contact me if you can help me to create CM-32P and SN-U110 sound fonts.

   Note: You can add the chip up to 8 per same chip type and MAmidiMEmo eats more CPU power.

6. Select the chip from the left pane and configure the chip on the right pane.

   *[Timbres]
    You can edit sound character from this property. It's selected by "Program Change" MIDI message.
	Please refer the following articles or MAME sources to understand these timbre parameters.

	YM2151:
	 https://www16.atwiki.jp/mxdrv/pages/24.html

	YM2612:
	 https://www.plutiedev.com/ym2612-registers
	 http://www.smspower.org/maxim/Documents/YM2612

	NES APU:
	 http://hp.vector.co.jp/authors/VA042397/nes/apu.html
	 https://wiki.nesdev.com/w/index.php/APU
	 https://wiki.nesdev.com/w/index.php/APU_DMC

	GB APU:
	 http://bgb.bircd.org/pandocs.htm#soundcontrolregisters
	 https://gbdev.gg8.se/wiki/articles/Gameboy_sound_hardware
	 http://mydocuments.g2.xrea.com/
	 http://marc.rawer.de/Gameboy/Docs/GBCPUman.pdf
	 http://www.devrs.com/gb/files/hosted/GBSOUND.txt

	NAMCO CUS30:
     https://www.walkofmind.com/programming/pie/wsg3.htm
	 http://fpga.blog.shinobi.jp/fpga/おんげん！

	SN76489:
	 http://www.smspower.org/Development/SN76489
	 http://www.st.rim.or.jp/~nkomatsu/peripheral/SN76489.html

	SCC:
	 http://bifi.msxnet.org/msxnet/tech/scc.html

	YM3812:
	 http://www.oplx.com/opl2/docs/adlib_sb.txt

	YM2413:
	 http://d4.princess.ne.jp/msx/datas/OPLL/YM2413AP.html#31
	 http://www.smspower.org/maxim/Documents/YM2413ApplicationManual
	 http://hp.vector.co.jp/authors/VA054130/yamaha_curse.html

	MSM5232(+TA7630):
	 http://www.citylan.it/wiki/images/3/3e/5232.pdf
	 http://sr4.sakura.ne.jp/acsound/taito/taito5232.html

	AY-3-8910:
	 http://ngs.no.coocan.jp/doc/wiki.cgi/TechHan?page=1%BE%CF+PSG%A4%C8%B2%BB%C0%BC%BD%D0%CE%CF
	 https://w.atwiki.jp/msx-sdcc/pages/45.html
	 http://f.rdw.se/AY-3-8910-datasheet.pdf

	SID:
	 https://www.waitingforfriday.com/?p=661#6581_SID_Block_Diagram
	 http://www.bellesondes.fr/wiki/doku.php?id=mos6581#mos6581_sound_interface_device_sid
	 https://www.sfpgmr.net/blog/entry/mos-sid-6581を調べた.html

	HuC6280:
	 http://www.magicengine.com/mkit/doc_hard_psg.html

	SPC700:
	 https://wiki.superfamicom.org/spc700-reference

	POKEY:
	 https://en.wikipedia.org/wiki/POKEY
	 http://ftp.pigwa.net/stuff/collections/SIO2SD_DVD/PC/RMT%201.19/docs/rmt_en.htm
	 https://www.atariarchives.org/dere/chapt07.php
	 http://user.xmission.com/~trevin/atari/pokey_regs.html

	YM2610:
	 http://www.ajworld.net/neogeodev/ym2610am2.html

    MT-32:
     https://sourceforge.net/projects/munt/
     http://lib.roland.co.jp/support/jp/manuals/res/1809744/MT-32_j2.pdf

    CM-64:
	 http://lib.roland.co.jp/support/jp/manuals/res/1809003/CM-64_j.pdf

	YMF262:
	 http://map.grauw.nl/resources/sound/yamaha_ymf262.pdf

	YM2608:
     https://www.quarter-dev.info/archives/yamaha/YM2608_Applicatin_Manual.pdf


   *[Channels]
    Select which MIDI ch messages the chip receives.

7. Play MIDI file by your favorite sequencer or player.
   Of course, you can connect your favrite keyboard to MAmidiMEmo for live performance.

   MAmidiMEmo currently supports the following MIDI messages.

    Note On with velocity
    Note Off
    Program Change
	Control Change
		Pitch and PitchRange
		Volume and Expression
		Panpot
		Modulation, Modulation Depth, Modulation Range, Modulation Delay
		Portamento, Portamento Time
		All Note Off, (All Sound Off)
		CC#126/127 Mono/Poly mode. Spec is almost same with FITOM.
		Sound Control (for modifying a Timbre properties dynamically)
		Effect Depth (for modifying a VST properties dynamically)
		General Purpose Control (for modifying an instrument properties dynamically)

8. Also, you can set the following sound driver settings from Timbre settings.

   Arpeggio
   ADSR
   Effect (Pitch/Volue/Duty macro)

9. You can modify current timbre parameters via Sound control MIDI Message (70-75,79) dynamically.
   You can modify VST parameters via Effect Depth control MIDI Message (91-95) dynamically.
   You can modify other parameters via General Purpose control MIDI Message (16-19,80-83) dynamically.

10. You can modify receving MIDI ch for the specific instrument via NRPN MIDI Message.

   NRPN format is the following.

   Bx 63 41
   Bx 62 <Device ID> ... Specify Device ID of existing instrument.
   Bx 26 <Unit No>   ... Specify Unit No of the above Device ID of existing instrument.
   Bx 06 <Receiving MIDI ch(1-7) bit sets. 1=On, 0=Off>

         bit  6  5  4  3  2  1  0
	      ch   7  6  5  4  3  2  1

   Bx 63 42
   Bx 62 <Device ID> ... Specify Device ID of existing instrument.
   Bx 26 <Unit No>   ... Specify Unit No of the above Device ID of existing instrument.
   Bx 06 <Receiving MIDI ch(8-14) bit sets. 1=On, 0=Off>

         bit  6  5  4  3  2  1  0
	      ch  14 13 12 11 10  9  8

   Bx 63 43
   Bx 62 <Device ID> ... Specify Device ID of existing instrument.
   Bx 26 <Unit No>   ... Specify Unit No of the above Device ID of existing instrument.
   Bx 06 <Receiving MIDI ch(15-16) bit sets. 1=On, 0=Off>

         bit  6  5  4  3  2  1  0
          ch xx xx xx xx xx 16 15


11. (TBD)
   You can modify current environment and all timbre parameters via System Exclusive MIDI Message.

   Master Volume: "F0 7F 7F 04 01 00 nn F7"

	YM2151:(TBD)
	YM2612:(TBD)
	NES APU:(TBD)
	GB APU:(TBD)
	NAMCO CUS30:(TBD)
	SN76489:(TBD)
	:::

12. SPFM

   You can use a real sound chip instead of software emulation chip on 32bit version.
   Currently supported chips are YM2151 and YM2608 on SPFM.
   Before using the SPFM, you must setup SCCI by using the scciconfig.exe.

*** Known issues and limitations *** 

   1. MT-32 & CM32-P can not store/restore last settings.
   2. HuC6820 suddenly stop sounding. Please restart MAmi.
   
*** How to create build environment ***

   1. Install Visual Studio 2017 version 15.7.6. w/ VC++, C#, Windows Universal CRT SDK, .NET 4.7 SDK and Targeting Pack
   2. Install Windows 8.1 SDK and 10 SDK
   3. Install MinGW Development Environment(https://www.mamedev.org/tools/)
   4. Install vcpkg

*** Donate for supporting the MAmidiMEmo *** 

   [![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=SNQ9JE3JAQMNQ)

*** Changes ***

2.6.2.0 Turned off write cache for frequency register on the real YM2608 and YM2515 chip.
2.6.1.0 Fixed CH mode set 3 instead of 6 on YM2608. Affected only S/W emulation.
2.6.0.0 Fixed crashing when FM operators was reset.
		Fixed SSG tone frequency on real YM2608 chip.
2.5.9.0 Supported downloading and opening a text file from the FM tone downloading dialog. The main reason is to make sure of the license and warning messages.
2.5.8.0 Supported downloading a FM tone (In the near future, you can download other data maybe) from cloud. Thanks to DM-88-san.
2.5.7.0 Added "Dumping sound" to RHYTHM sound on YM2608 when key off received.
        Fixed ADPCM-B not sounding unexpectedly on YM2608.
		Improved ADPCM-B on YM2608 transfer speed via SCCI.
		Fixed SSG sounding unexpectedly when volume changing on YM2608 and YM2610B
		Supported GM RESET ans GS RESET SysEx message. When received, reset all MIDI parameters and off all notes.
2.5.6.0 Supported importing the MUCOM88, FMP, PMD, VOPM sound font file into the FM Timbres props.
2.5.5.0 Added tone selector dialog that shows when imported a tone file that has multiple tones.
2.5.4.0 Supported importing the MUCOM88, FMP, PMD, VOPM sound font file into the FM Synthesis Editor.
        Supported loading a WAV file (16bit mono) as ADPCM-B data for YM2608, YM2610B chips.
2.5.3.0 Fixed PSG sounding unexpectedly when volume changing.
		Supported MIDI IN B.
        Supported Master Volume SysEx command. Try to send "F0 7F 7F 04 01 00 nn F7" to change master volume.
		Fixed CUS30 Volume calculation.
		Added YM2608(OPNA) chip. Place legitimate ym2608_adpcm_rom.bin file in the Mami dir to play rhythm sound.
		Supported the SPFM to sound on real chip for YM2151 and YM2608 chips. *Only 32 bit version*
2.5.2.0 Supported dynamic change FM Synthesis Op.Enable value.
        Added FM Synthesis register value randomizer to FM Synthesis Editor.
		Added FM Synthesis global register to FM Synthesis Editor.
2.5.1.0 Improved MIDI file Player UI.
2.5.0.0 Added MIDI file Player tab.
		Supported MAmidi file that is MAmi file and midi file are archived file. To create MAmidi file, load midi file and export MAmidi file.
		Fixed portamento time (Almost the same as the GS module portamento time).
2.4.0.1 Improved UI.
		Supported basic formula for SoundControlChangeSettings and GeneralPurposeControlSettings properties.
		Added Data Entry slider to Piano GUI. Use a mouse wheel to change the value.
		Fixed freezing on MT32.
		Fixed key off behavior of Fx Engine.
		Updated MSGS.SF for CM32-P.
2.4.0.0 Added Envelope Editor.
2.3.0.2 Fixed key off ignored issue while modulation is active on OPL.
		Fixed to turn off modulation after key off.
		Applied Metro Style GUI and improved UI.
2.3.0.1 Fixed YMF262 sample file and some minor bugs.
2.3.0.0 Added YMF262(OPL3) chip.
		Fixed Combined Drum does not sounding properly.
2.2.5.1 Improved FM Synthesis Editor UI.
2.2.5.0 Added PCM playback feature to HuC6280.
		Fixed an error when opening the floating point value slider on some props.
		Fixed SR(Sustain Rate) is extra parameter for OPL is not affected.
		Added a FM Synthesis GUI Editor.
2.2.4.0 Fixed issue of modulation CC.
		Fixed an error on MSM5232.
		Improved NOISE ch freqeucncy on SN76496, GBAPU, AY8910. You can change freq by pitch change CC.
		Improved NOISE ch function on AY8910.
2.2.3.0 Fixed issues related with SID property.
		Improved sf loader for SPC700 and C140.
		Added ZoneID remover script.
2.2.2.0 Fixed not applying Relese Point for Envelope.
        Fixed sound off timing while envelope processing.
		Added sf2 loading feature to context menu of C140 instrument.
		Removed DrumTimbreTable prop from C140 and SPC700 insts (Not suitable for PCM insts). Please use DrumTimbres prop to sound drum.
		Improved assignment of YM2413 drum sounds.
		Improved YM2413 custom sound sounding algorithm.
		Fixed error when opening a YM2610B Timbre prop.
2.2.1.0 Fixed HuC6820 WSG sound can't delete last noise sound.
2.2.0.0 Fixed Piano GUI for CM32-P and MT-32.
        Fixed HuC6820 volume calculation algorithm.
        Fixed not saving WSG Type of NAMCO CUS30 Timbre.
		Fixed error when opening a YM2413 Timbre property.
		Added sf2 loading feature to context menu of SPC700 instrument.
2.1.0.0 Changed YM2413 engine to emu2413 engine to get more sounds accuracy.
        Added Tone Envelope property for YM2413 FxS settings. 
		Added CM32-P Card #16.
		Added MML like serialize property to the FM Synthesis chip.
		Fixed FM Synthesis sounds.
2.0.4.0 Extended POLY mode control change message. You can specify the number of reserved voices.
        Fixed crashing on boot.
2.0.3.0 Improved sounds output timing accuracy.
        Supported HOLD1 control change message.
2.0.2.0 Improved MT-32 sounds output timing & latency.
2.0.1.0 Fixed crashing on some chip...
2.0.0.0 Fixed some minor bugs.
			Panic button sometimes does not work.
			SerializeData does not work and cause crash.
			FxS Arp does not work properly on Fixed mode.
			Property Reset menu does not work properly.
			Mono mode does not work properly.
			RP2A03 Tri channel is stopped by Noise channel.
			Specific property value does not save.
		Added KeyShift, PitchShift, IgnoreKeyOff prop to Timbre prop.
		Added Combined Timbre feature to Timbre prop. Treat patched Timbre as one Timbre.
		Added Follower mode feature to Timbre prop. Share voice ch with another units.
		Added Drum part to Timbre prop.
		Added Global Arpeggio Settins to Instrument prop.
		Added Instrument cloning menu in the instrument pane on the Main window.
		Exposed RP2A03 Liner Counter Length.
		Applied "Force Dump mode" always to FM Synthesis unit to prevent incomplete attack rate.
		Added virtual SR parameter to YM2413.
		Added sample of MAmi files.
		Added drag & drop feature that MAmi file can be dropped into instrument list pnae.
1.3.1.0 Added VGM supported chips.
			GB APU, HuC6280
1.3.0.0 Synced sound engine to MAME 0221 (May improved some sound accuracy).
        Added wave file output feature. Please re-open option dialog and press [OK] to commit new settings.
        Added VGM file separetedly output feature. Only supported the following chips.
			YM2151, YM2612, YM3812, YM2413, POKEY, SN76496, NES APU, AY-3-8910
1.2.1.0 Added CM-32P SN-U110-10 simulation .
1.2.0.0 Added CM-32P (This is an incomplete simulator).
            Using FluidLite https://github.com/divideconcept/FluidLite , 
			Using GeneralUser GS http://schristiancollins.com/generaluser.php
		Fixed RPN/NRPN MIDI massages can not be handled properly. OMG.
1.1.0.0 Added MT-32 MIDI module ( imported from MUNT https://sourceforge.net/projects/munt/ )
            Place legitimate MT32_CONTROL.ROM and MT32_PCM.ROM files in the Mami dir.
1.0.0.0 Added YM2610B chip.
0.9.4.2 Fixed Key ch of piano pane is not applied properly.
0.9.4.1 Fixed YM2413 serialized data could not apply properly.
0.9.4.0 Added FDS, VRC6 tone type to the NES APU.
            FDS, VRC6 was imported from VirtuaNES https://github.com/ivysnow/virtuanes
            Added HuC6280 and SPC700(RAM limit breaking) and POKEY.
            Fixed and changed "Partial Reserve" feature for GBA ( and HuC6280 ).
0.9.3.1 Fixed invalid portamento source note and followed portamento speed to GM2 spec.
0.9.3.0 Added alternate property editor window. That can be popup from toolbar in the Property pane.
            Added "Sound Control Settings" feature in Timbre settings. You can link the Sound control MIDI message value with the Timbre property value. (Also VST effects and other props, too)
            Added modifying receving MIDI ch for the specific instrument via NRPN MIDI Message feature.
            See the section No.8 of this README.
            Fixed arpeggio algorithm. When last one key is up, the key is not held in hold mode. Otherwise, keep arpeggio.
            Fixed 2nd AY8910 outputs noise, C140 panpot gain formula follows GM2 spec, some minor bugs.
0.8.0.0 Supports piano clicks by mouse. Supports Mono mode(CC#126,CC#127) almost same with FITOM
0.7.0.0 Added SID, C140(RAM limit breaking) chips, Displays Oscilloscope, Supports VST Effect plugin
0.6.1.0 Changed to new sound timer engine for perfect sound timing
0.6.0.0 Added sound driver effects and portamento feature
0.5.0.0 Added several chips
0.1.0.0 First release

*** Licenses ***

* MAME
https://www.mamedev.org

* DryWetMidi - Copyright (c) 2018 Maxim Dobroselsky
https://github.com/melanchall/drywetmidi

* Newtonsoft.Json - Copyright © James Newton-King 2008
https://www.nuget.org/packages/Newtonsoft.Json/12.0.3/license

* ValueInjecter - Copyright (c) 2015 Valentin Plamadeala
https://github.com/omuleanu/ValueInjecter/blob/master/LICENSE

* Fast Colored TextBox for Syntax Highlighting - Copyright (C) Pavel Torgashov, 2011-2016. 
https://github.com/PavelTorgashov/FastColoredTextBox

* MUNT - kingguppy, sergm
https://ja.osdn.net/projects/sfnet_munt/

* FluidLite -  (c) 2016 Robin Lobel
https://github.com/divideconcept/FluidLite

* Font "DSEG" by けしかん
https://www.keshikan.net/fonts.html

* M+ FONT - 森下浩司
http://itouhiro.hatenablog.com/entry/20130602/font

* GeneralUser GS - S. Christian Collins
http://schristiancollins.com/generaluser.php

* MSGS
https://sites.google.com/site/senasan007/Home/cw_midi_c

* VST.NET - Marc Jacobi
https://github.com/obiwanjacobi/vst.net

* FM-SoundConvertor - Copyright (c) 2020 D.M.88
https://github.com/DM-88/FM-SoundConvertor

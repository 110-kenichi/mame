MAmidiMEmo V0.9.4.0 / Itoken (c)2019 / GPL-2.0

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
		*CPU: Intel Core series CPU or equivalent, at least 2.0 GHz
		*MEM: at least 4GB
		*Sound: DirectSound capable sound card/onboard audio
		*MIDI: MIDI IN I/F
	*S/W
		*OS: Windows 7 SP1 or lator
		*Runtime: .NET Framework 4.7 or lator

1. Launch MAmidiMEmo.exe

   Note: You can change the value of audio latency, sampling rate and audio output interface by [Tool] menu.
         PortAudio is a low latency sound engine. See http://www.portaudio.com/

2. Select MIDI I/F from toolbar. MAmidiMEmo will recevie MIDI message from the selected MIDI I/F.

   Note: You can use the loopMIDI (See http://www.tobias-erichsen.de/software/loopmidi.html) to send MIDI message from this PC to MAmidiMEmo.

3. Add your favorite chips from the [Instruments] menu on the toolbar.

   Note: Currently supported chips are the following.
         YM2151, YM2612, YM3812, YM2413,
		 SID, POKEY, GB APU, SN76496, NES APU, MSM5232(+TA7630), AY-3-8910
		 NAMCO CUS30, SCC, HuC6280
		 C140, SPC700

   Note: You can add the chip up to 8 per same chip type and MAmidiMEmo eats more CPU power.

4. Select the chip from the left pane and configure the chip on the right pane.

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
	 http://fpga.blog.shinobi.jp/fpga/���񂰂�I
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
	 https://www.sfpgmr.net/blog/entry/mos-sid-6581�𒲂ׂ�.html
	HuC6280:
	 http://www.magicengine.com/mkit/doc_hard_psg.html
	SPC700:
	 https://wiki.superfamicom.org/spc700-reference
	POKEY:
	 https://en.wikipedia.org/wiki/POKEY
	 http://ftp.pigwa.net/stuff/collections/SIO2SD_DVD/PC/RMT%201.19/docs/rmt_en.htm
	 https://www.atariarchives.org/dere/chapt07.php
	 http://user.xmission.com/~trevin/atari/pokey_regs.html


   *[Channels]
    Select which MIDI ch messages the chip receives.

5. Play MIDI file by your favorite sequencer or player.
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

6. Also, you can set the following sound driver settings from Timbre settings.

   Arpeggio
   ADSR
   Effect (Pitch/Volue/Duty macro)

7. You can modify current timbre parameters via Sound control MIDI Message (70-75,79) dynamically.
   You can modify VST parameters via Effect Depth control MIDI Message (91-95) dynamically.
   You can modify other parameters via General Purpose control MIDI Message (16-19,80-83) dynamically.

8. You can modify receving MIDI ch for the specific instrument via NRPN MIDI Message.

   NRPN format is the following.

   Bx 63 41
   Bx 62 <Device ID> ... Specify Device ID of existing instrument.
   Bx 26 <Unit No>   ... Specify Unit No of the above Device ID of existing instrument.
   Bx 06 <Receiving MIDI ch(1-7) bit sets. 1=On, 0=Off>

         bit  6  5  4  3  2  1  0
		  ch  7  6  5  4  3  2  1

   Bx 63 42
   Bx 62 <Device ID> ... Specify Device ID of existing instrument.
   Bx 26 <Unit No>   ... Specify Unit No of the above Device ID of existing instrument.
   Bx 06 <Receiving MIDI ch(8-14) bit sets. 1=On, 0=Off>

         bit  6  5  4  3  2  1  0
		  ch 14 13 12 11 10  9  8

   Bx 63 43
   Bx 62 <Device ID> ... Specify Device ID of existing instrument.
   Bx 26 <Unit No>   ... Specify Unit No of the above Device ID of existing instrument.
   Bx 06 <Receiving MIDI ch(15-16) bit sets. 1=On, 0=Off>

         bit  6  5  4  3  2  1  0
          ch xx xx xx xx xx 16 15


9. (TBD)
   You can modify current environment and all timbre parameters via System Exclusive MIDI Message.

   SysEx format:

	YM2151:(TBD)
	YM2612:(TBD)
	NES APU:(TBD)
	GB APU:(TBD)
	NAMCO CUS30:(TBD)
	SN76489:(TBD)
	:::
 
8. Donate for supporting the MAmidiMEmo

   [![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=SNQ9JE3JAQMNQ)

9. Changes

0.9.4.1 Fixed YM2413 serialized data could not apply properly.
0.9.4.0 Added FDS, VRC6 tone type to the NES APU. FDS, VRC6 was imported from VirtuaNES( https://github.com/ivysnow/virtuanes )
        Added HuC6280 and SPC700(RAM limit breaking) and POKEY.
		Fixed and changed "Partial Reserve" feature for GBA ( and HuC6280 ).
0.9.3.1 Fixed invalid portamento source note and followed portamento speed to GM2 spec.
0.9.3.0 Added alternate property editor window. That can be popup from toolbar in the Property pane.
        Added "Sound Control Settings" feature in Timbre settings. You can link the Sound control MIDI message value with the Timbre property value. (Also VST effects and other props, too)
		Added modifying receving MIDI ch for the specific instrument via NRPN MIDI Message feature. See the section No.8 of this README.
        Fixed arpeggio algorithm. When last one key is up, the key is not held in hold mode. Otherwise, keep arpeggio.
		Fixed 2nd AY8910 outputs noise, C140 panpot gain formula follows GM2 spec, some minor bugs.
0.8.0.0 Supports piano clicks by mouse. Supports Mono mode(CC#126,CC#127) almost same with FITOM
0.7.0.0 Added SID, C140(RAM limit breaking) chips, Displays Oscilloscope, Supports VST Effect plugin
0.6.1.0 Changed to new sound timer engine for perfect sound timing
0.6.0.0 Added sound driver effects and portamento feature
0.5.0.0 Added several chips
0.1.0.0 First release

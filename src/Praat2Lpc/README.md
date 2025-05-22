# Praat2Lpc Itoken (c)2025 / GPL-2.0

## Summary

Converts praat pitch and LPC files to a format suitable for the TMS5200 or VLM5030 speech synthesizers. The program reads the praat pitch file and the praat LPC file, applies the specified energy factors, and outputs the LPC data in a format compatible with the target synthesizer.
Before running the program, ensure that you have the praat pitch and LPC files generated from the praat software. see the example below for generating these files.

## Usage for TMS5220

    praat --run tms_lpc.praat input.wav output.TMS_PraatPitch output.TMS_PraatLPC <Pitch_floor(Hz)> <Pitch_ceiling(Hz)> <Octave_cost> <Octave_jump_cost> <Voicing_threshold> <Voicing_switch_cost> <Silence_threshold>
    Praat2Lpc -tms5220 output.TMS_PraatPitch output.TMS_PraatLPC <unvoiced energy factor> <voiced energy factor> <output LPC file name>

## Sample for TMS5220

    praat --run tms_lpc.praat input.wav output.TMS_PraatPitch output.TMS_PraatLPC 50 250 0.02 0.40 0.20 0.20 0.03
    Praat2Lpc -tms5220 output.TMS_PraatPitch output.TMS_PraatLPC 0.4 0.6  output_tms.lpc

## Usage for VLM5030

    praat --run vlm_lpc.praat input.wav output.VLM_PraatPitch output.VLM_PraatLPC <Pitch_floor(Hz)> <Pitch_ceiling(Hz)> <Octave_cost> <Octave_jump_cost> <Voicing_threshold> <Voicing_switch_cost> <Silence_threshold>
    Praat2Lpc -tms5220 output.TMS_PraatPitch output.TMS_PraatLPC 0.5 5.0  output_vlm.lpc

## Sample for VLM5030

    praat --run tms_lpc.praat input.wav output.TMS_PraatPitch output.TMS_PraatLPC 50 250 0.02 0.40 0.20 0.20 0.03
    Praat2Lpc -vlm5030 output.VLM_PraatPitch output.VLM_PraatLPC <unvoiced energy factor> <voiced energy factor> <output LPC file name>

## Tips

* Most important parameter is the <Pitch_floor> (<Pitch_ceiling> is slightly important). Try to change the value around from 20 to 60.
* Change the <voiced energy factor> to change voice volume. 

## See also

* praat - PaulBoersma

  https://github.com/praat/praat/releases

* VideoTools - EricLafortune

  https://github.com/EricLafortune/VideoTools/tree/master
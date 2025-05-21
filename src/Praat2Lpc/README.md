# Praat2Lpc Itoken (c)2025 / GPL-2.0

## Summary

Converts praat pitch and LPC files to a format suitable for the TMS5200 or VLM5030 speech synthesizers. The program reads the praat pitch file and the praat LPC file, applies the specified energy factors, and outputs the LPC data in a format compatible with the target synthesizer.
Before running the program, ensure that you have the praat pitch and LPC files generated from the praat software. see the example below for generating these files.

## Usage for TMS5220

    praat --run tms_lpc.praat input.wav output.TMS_PraatPitch output.TMS_PraatLPC 50 250 0.02 0.40 0.20 0.20 0.03
    Praat2Lpc -tms5220 output.TMS_PraatPitch output.TMS_PraatLPC <unvoiced energy factor value> <voiced energy factor value> <output LPC file name>

## Usage for VLM5030

    praat --run vlm_lpc.praat input.wav output.VLM_PraatPitch output.VLM_PraatLPC 50 250 0.02 0.40 0.20 0.20 0.03
    Praat2Lpc -vlm5030 output.VLM_PraatPitch output.VLM_PraatLPC <unvoiced energy factor value> <voiced energy factor value> <output LPC file name>

## See also

* praat - PaulBoersma

  https://github.com/praat/praat/releases

* VideoTools - EricLafortune

  https://github.com/EricLafortune/VideoTools/tree/master
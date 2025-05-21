#!/usr/bin/praat --run
#
# Converts a specified speech audio file to a Praat pitch file and a Praat LPC
# file. These files are suitable for further processing for the TMS5200 speech
# synthesizer.
#
# Usage:
#
#   praat --run lpc.praat input.wav output.Pitch output.LPC pitch_floor pitch_ceiling octave_cost octave_jump_cost voicing_threshold voicing_switch_cost silence_threshold
#
# where only the first argument is required.

# If interactive:  get the variable values from a form.
# If command line: get the variable values from the command line.
form files
    # These parameter types are necessary to properly resolve relative file
    # names, but they aren't available in Praat 6.0.37 yet.
    #infile   Input_wav_file      input.wav
    #outfile  Output_pitch_file   output.Pitch
    #outfile  Output_lpc_file     output.LPC
    sentence Input_wav_file      input.wav
    sentence Output_pitch_file   output.Pitch
    sentence Output_lpc_file     output.LPC
    positive Pitch_floor_(Hz)    75
    positive Pitch_ceiling_(Hz)  550
    positive Octave_cost         0.01
    positive Octave_jump_cost    0.35
    positive Voicing_threshold   0.45
    positive Voicing_switch_cost 0.14
    positive Silence_threshold   0.03
endform

# If command line: make sure we have values.
if input_wav_file$ = ""
  exitScript: "Please specify an input speech sound file."
endif

if output_pitch_file$ = ""
  output_pitch_file$ = left$(input_wav_file$, rindex(input_wav_file$, ".")-1) + ".Pitch"
endif

if output_lpc_file$ = ""
  output_lpc_file$ = left$(input_wav_file$, rindex(input_wav_file$, ".")-1) + ".LPC"
endif

if pitch_floor = undefined
  pitch_floor = 75
endif

if pitch_ceiling = undefined
  pitch_ceiling = 550
endif

if octave_cost = undefined
  octave_cost = 0.01
endif

if octave_jump_cost = undefined
  octave_jump_cost = 0.35
endif

if voicing_threshold = undefined
  voicing_threshold = 0.45
endif

if voicing_switch_cost = undefined
  voicing_switch_cost = 0.14
endif

if silence_threshold = undefined
  silence_threshold = 0.03
endif

# Read the speech audio file.
sound = Read from file: input_wav_file$

# Resample the sound, for the LPC computation later on:
#   New sampling frequency (Hz) (standard value: 10000.0 Hz)
#   Precision (samples)         (standard value: 50)

selectObject: sound
sound_8000 = Resample: 8000, 50

# Extract the pitches:
#   Time step (s)            (standard value: 0.0)
#   Pitch floor (Hz)         (standard value: 75 Hz)
#   Max number of candidates (standard value: 15)
#   Very accurate            (standard value: 0)
#   Silence threshold        (standard value: 0.03)
#   Voicing threshold        (standard value: 0.45)
#   Octave cost (per octave) (standard value: 0.01)
#   Octave-jump cost         (standard value: 0.35)
#   Voiced / unvoiced cost   (standard value: 0.14)
#   Pitch ceiling (Hz)       (standard value: 600 Hz)
# A time step of 25ms is suitable for the TMS5200 chip (40 fps).
# A pitch floor of 75Hz trims 2 frames from the start and end.
# A pitch floor of 100Hz trims 1.5 frames from the start and end.
# A pitch floor of 250Hz trims 0.5 frame from the start and end.
# The max number of candidates matters to find the best pitch contour
# (evolution of the pitch over the input sound).

selectObject: sound
pitch = To Pitch (ac): 0.025, pitch_floor, 15, 1, silence_threshold, voicing_threshold, octave_cost, octave_jump_cost, voicing_switch_cost, pitch_ceiling

Save as short text file: output_pitch_file$

# Extract the LPC prediction coefficients with Burg's method:
#   Prediction order             (standard value: 16)
#   Analysis window duration (s) (standard value: 0.025 s)
#   Time step (s)                (standard value: 0.005 s)
#   Pre-emphasis frequency (Hz)  (standard value: 50 Hz)
# A prediction order of 10 is suitable for all TMS52xx chips.
# A time step of 25ms is suitable for the TMS5200 chip (40 fps).
# An analysis window of 100ms trims 4 frames from the start and end.
# An analysis window of 75ms trims 3.5 frames from the start and end.
# It might be best to trim the same fractions for pitch and coefficients,
# so skipping a few coefficient frames synchronizes the streams.

selectObject: sound_8000
lpc = To LPC (burg): 10, 0.075, 0.025, 50.0

Save as short text file: output_lpc_file$

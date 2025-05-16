praat --run lpc.praat input.wav output.PraatPitch output.PraatLPC 50 200 0.02 0.40 0.20 0.20 0.03
java -cp videotools.jar ConvertPraatToLpc -addstopframe -tms5220 output.PraatPitch output.PraatLPC 0.4 0.6 output.lpc

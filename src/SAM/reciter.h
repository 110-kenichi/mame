#ifndef RECITER_C
#define RECITER_C

//int TextToPhonemes(char *input, char *output);

__declspec(dllexport) unsigned char* TextToPhonemes(unsigned char *input);

int TextToPhonemesCore(unsigned char* input);

#endif

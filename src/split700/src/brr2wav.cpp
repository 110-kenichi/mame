/**
 * brr2wav.cpp: decodes snes brr into Microsft WAVE.
 * @author gocha <http://github.com/gocha>
 */
#include "pch.h"

#define NOMINMAX

#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>

#include <string>

#include "cpath.h"
#include "SPCSampDir.h"
#include "WavWriter.h"

#ifdef WIN32
#include <Windows.h>
#include <direct.h>
#include <float.h>
#define getcwd _getcwd
#define chdir _chdir
#define isnan _isnan
#define strcasecmp _stricmp
#else
#include <unistd.h>
#endif

#define APP_NAME    "brr2wav"
#define APP_VER     "[2015-11-04]"
#define APP_URL     "http://github.com/gocha/split700"

uint8_t * readfile(const std::string & filename)
{
	off_t filesize = path_getfilesize(filename.c_str());
	if (filesize == -1) {
		fprintf(stderr, "Error: %s: Unable to open\n", filename.c_str());
		return NULL;
	}

	FILE * fp = fopen(filename.c_str(), "rb");
	if (fp == NULL) {
		fprintf(stderr, "Error: %s: Unable to open\n", filename.c_str());
		return NULL;
	}

	uint8_t * data = new uint8_t[filesize + 1];
	if (data == NULL) {
		fprintf(stderr, "Error: Memory allocation error\n");
		fclose(fp);
		return NULL;
	}

	if (filesize != 0) {
		if (fread(data, filesize, 1, fp) != 1) {
			fprintf(stderr, "Error: %s: File read error\n", filename.c_str());
			delete[] data;
			fclose(fp);
			return NULL;
		}
	}

	fclose(fp);
	return data;
}

bool brr2wav(const std::string & brr_filename, uint16_t pitch)
{
	char path_c[PATH_MAX];

	off_t brr_filesize = path_getfilesize(brr_filename.c_str());
	if (brr_filesize == -1) {
		fprintf(stderr, "Error: %s: Unable to open\n", brr_filename.c_str());
		return false;
	}
	if (brr_filesize == 0) {
		fprintf(stderr, "Error: %s: File is empty\n", brr_filename.c_str());
		return false;
	}
	if (brr_filesize > 0x800000) {
		fprintf(stderr, "Error: %s: File too large\n", brr_filename.c_str());
		return false;
	}

	uint8_t * data = readfile(brr_filename);
	if (data == NULL) {
		return false;
	}

	uint8_t * brr = data;
	size_t brr_size = brr_filesize;

	int32_t loop_sample = 0;
	bool has_header = false;
	switch (brr_size % 9) {
	case 0:
		break;

	case 2: {
		uint16_t loop_offset = data[0] | (data[1] << 8);
		loop_sample = loop_offset / 9 * 16;

		if (loop_offset % 9 == 0) {
			printf("%s: addmusicM header detected, loop offset = $%04x (sample #%d).\n", brr_filename.c_str(), loop_offset, loop_sample);
			has_header = true;
		}
		else {
			fprintf(stderr, "%s: warning: illegal length, skip first %d bytes.\n", brr_filename.c_str(), (int)(brr_filesize % 9));
		}

		brr += brr_filesize % 9;
		brr_size -= brr_filesize % 9;
		break;
	}

	default:
		fprintf(stderr, "%s: warning: illegal length, skip first %d bytes.\n", brr_filename.c_str(), (int)(brr_filesize % 9));
		brr += brr_filesize % 9;
		brr_size -= brr_filesize % 9;
		break;
	}

	strcpy(path_c, brr_filename.c_str());
	path_basename(path_c);
	path_stripext(path_c);
	strcat(path_c, ".wav");
	std::string wav_filename(path_c);

	bool looped = false;
	WavWriter wave(SPCSampDir::decode_brr(brr, brr_size, &looped));
	wave.samplerate = pitch * 32000 / 0x1000;
	wave.bitwidth = 16;
	wave.channels = 1;
	if (has_header && looped) {
		wave.SetLoopSample(loop_sample);
	}

	if (!wave.WriteFile(wav_filename)) {
		fprintf(stderr, "Error: %s: %s\n", wav_filename.c_str(), wave.message().c_str());
		delete[] data;
		return false;
	}

	delete[] data;
	return true;
}

static void usage(const char * progname)
{
	printf("%s %s\n", APP_NAME, APP_VER);
	printf("<%s>\n", APP_URL);
	printf("\n");
	printf("Decodes SNES BRR sample into Microsoft WAVE.\n");
	printf("\n");
	printf("Usage\n");
	printf("-----\n");
	printf("\n");
	printf("Syntax: `%s [options] [brr files]`\n", progname);
	printf("\n");

	printf("### Options\n");
	printf("\n");
	printf("`--pitch HEX_VALUE`\n");
	printf("  : Specify pitch (sample rate) for output file (0x1000 = 1.0)\n");
	printf("\n");
	printf("`-?`, `--help`\n");
	printf("  : Display this help.\n");
	printf("\n");
}

int main(int argc, char *argv[])
{
	uint16_t pitch = 0x1000;

	long l;
	char * endptr = NULL;

	if (argc >= 2 && (strcmp(argv[1], "-?") == 0 || strcmp(argv[1], "--help") == 0)) {
		usage(argv[0]);
		return EXIT_FAILURE;
	}
	else if (argc == 1) {
		usage(argv[0]);
		return EXIT_FAILURE;
	}

	int argi;
	for (argi = 1; argi < argc; argi++) {
		if (argv[argi][0] != '-') {
			break;
		}

		if (strcmp(argv[argi], "--pitch") == 0) {
			if (argc <= (argi + 1)) {
				fprintf(stderr, "Error: Too few arguments for \"%s\"\n", argv[argi]);
				return EXIT_FAILURE;
			}

			l = strtol(argv[argi + 1], &endptr, 16);
			if (*endptr != '\0' || errno == ERANGE || l < 0) {
				fprintf(stderr, "Error: Number format error (pitch must be hexadecimal) \"%s\"\n", argv[argi + 1]);
				return EXIT_FAILURE;
			}
			if (l < 0 || l > 0x3fff) {
				fprintf(stderr, "Error: Pitch out of range.\n");
				return EXIT_FAILURE;
			}
			pitch = (uint16_t)l;
			argi++;
		}
		else {
			fprintf(stderr, "Error: Unknown option \"%s\"\n", argv[argi]);
			return EXIT_FAILURE;
		}
	}

	if (argc <= argi) {
		fprintf(stderr, "Error: No input files\n");
		return EXIT_FAILURE;
	}

	int errors = 0;
	for (; argi < argc; argi++) {
		std::string brr_filename(argv[argi]);
		if (!brr2wav(brr_filename, pitch)) {
			//fprintf(stderr, "Error: %s: Conversion failed\n", brr_filename.c_str());
			errors++;
		}
	}

	return (errors == 0) ? EXIT_SUCCESS : EXIT_FAILURE;
}

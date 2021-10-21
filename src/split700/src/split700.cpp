#include "pch.h"

/**
 * split700.cpp: extracts brr samples from spc dump
 * @author gocha <http://github.con/gocha>
 */

#define NOMINMAX

#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>

#include <string>
#include <sstream>
#include <algorithm>

#include "split700.h"
#include "cpath.h"
#include "SPCFile.h"
#include "SPCSampDir.h"
 //#include "WavWriter.h"

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

#define APP_NAME    "split700"
#define APP_VER     "[2015-11-04]"
#define APP_URL     "http://github.com/gocha/split700"

static std::vector<std::string> split(const std::string& str, char delim)
{
	std::vector<std::string> tokens;

	std::istringstream iss(str);
	std::string token;
	while (std::getline(iss, token, delim)) {
		tokens.push_back(token);
	}
	return tokens;
}

static std::string trim(const std::string& str, const char* trim_chars = " \t\v\r\n")
{
	std::string result;

	std::string::size_type left = str.find_first_not_of(trim_chars);
	if (left != std::string::npos) {
		std::string::size_type right = str.find_last_not_of(trim_chars);
		result = str.substr(left, right - left + 1);
	}

	return result;
}

Split700::Split700() :
	loop_point_to_filename(false),
	force(false)
{
}

Split700::~Split700()
{
}

bool Split700::ExportLoopSamples(const std::string& spc_filename, bool export_loop_point)
{
	SPCFile* spc_file_ptr = SPCFile::Load(spc_filename);
	if (spc_file_ptr == NULL) {
		m_message = "File open error (possible invalid format)";
		return false;
	}

	char base_path_c[PATH_MAX];
	strcpy(base_path_c, spc_filename.c_str());
	path_stripext(base_path_c);
	std::string base_path(base_path_c);

	bool result = ExportLoopSamples(*spc_file_ptr, base_path, export_loop_point);
	delete spc_file_ptr;
	return result;
}

bool Split700::ExportLoopSamples(const std::string& spc_filename, const std::vector<uint8_t>& srcns, bool export_loop_point)
{
	SPCFile* spc_file_ptr = SPCFile::Load(spc_filename);
	if (spc_file_ptr == NULL) {
		m_message = "File open error (possible invalid format)";
		return false;
	}

	char base_path_c[PATH_MAX];
	strcpy(base_path_c, spc_filename.c_str());
	path_stripext(base_path_c);
	std::string base_path(base_path_c);

	bool result = ExportLoopSamples(*spc_file_ptr, base_path, srcns, export_loop_point);
	delete spc_file_ptr;
	return result;
}

bool Split700::ExportLoopSamples(const SPCFile& spc_file, const std::string& base_path, bool export_loop_point)
{
	std::vector<uint8_t> srcns(GetSampList(spc_file));
	return ExportLoopSamples(spc_file, base_path, srcns, export_loop_point);
}

bool Split700::ExportLoopSamples(const SPCFile& spc_file, const std::string& base_path, const std::vector<uint8_t>& srcns, bool export_loop_point)
{
	char pwd[PATH_MAX];
	getcwd(pwd, PATH_MAX);

	char base_dir_c[PATH_MAX];
	strcpy(base_dir_c, base_path.c_str());
	path_dirname(base_dir_c);
	std::string base_dir(base_dir_c);

	std::vector<uint8_t> dumpable_srcns = QueryDumpableSamples(spc_file, srcns);
	for (auto itr_srcn = dumpable_srcns.begin(); itr_srcn != dumpable_srcns.end(); ++itr_srcn) {
		uint8_t srcn = *itr_srcn;
		const SPCSampDir& sample = spc_file.samples[srcn];

		std::string brr_filename(GetExportFilename(spc_file, base_path, srcn, ".brr"));

		chdir(base_dir.c_str());
		FILE* brr_file = fopen(brr_filename.c_str(), "wb");
		if (brr_file == NULL) {
			chdir(pwd);
			m_message = brr_filename + ": File open error";
			return false;
		}
		chdir(pwd);

		if (export_loop_point) {
			uint16_t loop_point_rel;
			if (sample.looped && sample.loop_address >= sample.start_address && sample.loop_address < sample.end_address) {
				loop_point_rel = sample.loop_address - sample.start_address;
			}
			else {
				loop_point_rel = sample.end_address - sample.start_address;
			}

			uint8_t data[2] = { (uint8_t)(loop_point_rel & 0xff), (uint8_t)(loop_point_rel >> 8) };
			if (fwrite(data, 2, 1, brr_file) != 1) {
				m_message = brr_filename + ": File write error";
				fclose(brr_file);
				return false;
			}
		}

		if (fwrite(&spc_file.ram[sample.start_address], sample.compressed_size(), 1, brr_file) != 1) {
			m_message = brr_filename + ": File write error";
			fclose(brr_file);
			return false;
		}

		fclose(brr_file);
	}

	return true;
}

//bool Split700::ExportLoopSamplesAsWAV(const std::string & spc_filename, int32_t samplerate)
//{
//	SPCFile * spc_file_ptr = SPCFile::Load(spc_filename);
//	if (spc_file_ptr == NULL) {
//		m_message = "File open error (possible invalid format)";
//		return false;
//	}
//
//	char base_path_c[PATH_MAX];
//	strcpy(base_path_c, spc_filename.c_str());
//	path_stripext(base_path_c);
//	std::string base_path(base_path_c);
//
//
//	bool result = ExportLoopSamplesAsWAV(*spc_file_ptr, base_path, samplerate);
//	delete spc_file_ptr;
//	return result;
//}
//
//bool Split700::ExportLoopSamplesAsWAV(const std::string & spc_filename, const std::vector<uint8_t> & srcns, int32_t samplerate)
//{
//	SPCFile * spc_file_ptr = SPCFile::Load(spc_filename);
//	if (spc_file_ptr == NULL) {
//		m_message = "File open error (possible invalid format)";
//		return false;
//	}
//
//	char base_path_c[PATH_MAX];
//	strcpy(base_path_c, spc_filename.c_str());
//	path_stripext(base_path_c);
//	std::string base_path(base_path_c);
//
//
//	bool result = ExportLoopSamplesAsWAV(*spc_file_ptr, base_path, srcns, samplerate);
//	delete spc_file_ptr;
//	return result;
//}
//
//bool Split700::ExportLoopSamplesAsWAV(const SPCFile & spc_file, const std::string & base_path, int32_t samplerate)
//{
//	std::vector<uint8_t> srcns(GetSampList(spc_file));
//	return ExportLoopSamplesAsWAV(spc_file, base_path, srcns, samplerate);
//}
//
//bool Split700::ExportLoopSamplesAsWAV(const SPCFile & spc_file, const std::string & base_path, const std::vector<uint8_t> & srcns, int32_t samplerate)
//{
//	char pwd[PATH_MAX];
//	getcwd(pwd, PATH_MAX);
//
//	char base_dir_c[PATH_MAX];
//	strcpy(base_dir_c, base_path.c_str());
//	path_dirname(base_dir_c);
//	std::string base_dir(base_dir_c);
//
//	std::vector<uint8_t> dumpable_srcns = QueryDumpableSamples(spc_file, srcns);
//	for (auto itr_srcn = dumpable_srcns.begin(); itr_srcn != dumpable_srcns.end(); ++itr_srcn) {
//		uint8_t srcn = *itr_srcn;
//		const SPCSampDir & sample = spc_file.samples[srcn];
//
//		std::string wav_filename(GetExportFilename(spc_file, base_path, srcn, ".wav"));
//
//		WavWriter wave(SPCSampDir::decode_brr(&spc_file.ram[sample.start_address], sample.compressed_size()));
//		wave.samplerate = samplerate;
//		wave.bitwidth = 16;
//		wave.channels = 1;
//		if (sample.looped) {
//			wave.SetLoopSample(sample.loop_sample());
//		}
//
//		chdir(base_dir.c_str());
//		if (!wave.WriteFile(wav_filename)) {
//			chdir(pwd);
//			m_message = wav_filename + ": " + wave.message();
//			return false;
//		}
//		chdir(pwd);
//	}
//
//	return true;
//}

bool Split700::PrintSPCInfo(const std::string& spc_filename)
{
	char basename[PATH_MAX];
	strcpy(basename, spc_filename.c_str());
	path_basename(basename);
	std::string spc_basename(basename);

	SPCFile* spc_file_ptr = SPCFile::Load(spc_filename);
	if (spc_file_ptr == NULL) {
		m_message = "File open error (possible invalid format)";
		return false;
	}
	SPCFile& spc_file = *spc_file_ptr;

	std::string title(GetSongTitle(spc_file, spc_basename));
	bool result = PrintSPCInfo(*spc_file_ptr, title);
	delete spc_file_ptr;
	return result;
}

bool Split700::PrintSPCInfo(const std::string& spc_filename, const std::vector<uint8_t>& srcns)
{
	char basename[PATH_MAX];
	strcpy(basename, spc_filename.c_str());
	path_basename(basename);
	std::string spc_basename(basename);

	SPCFile* spc_file_ptr = SPCFile::Load(spc_filename);
	if (spc_file_ptr == NULL) {
		m_message = spc_basename + ": " + "File open error (possible invalid format)";
		return false;
	}
	SPCFile& spc_file = *spc_file_ptr;

	std::string title(GetSongTitle(spc_file, spc_basename));
	bool result = PrintSPCInfo(*spc_file_ptr, title, srcns);
	delete spc_file_ptr;
	return result;
}

bool Split700::PrintSPCInfo(const SPCFile& spc_file, const std::string& title)
{
	std::vector<uint8_t> srcns(GetSampList(spc_file));
	return PrintSPCInfo(spc_file, title, srcns);
}

bool Split700::PrintSPCInfo(const SPCFile& spc_file, const std::string& title, const std::vector<uint8_t>& srcns)
{
	printf("### %s\n", title.c_str());
	printf("\n");

	uint16_t dir = spc_file.dsp[0x5d] << 8;
	printf("* Sample DIR address = $%04x\n", dir);
	printf("\n");

	PrintSampList(spc_file.samples, srcns);
	return true;
}

void Split700::PrintSampList(const SPCSampDir samples[], const std::vector<uint8_t>& srcns) const
{
	printf("|SRCN |SA    |LSA   |EA    |Size  |Loop   |\n");
	printf("|-----|------|------|------|------|-------|\n");
	for (auto itr_srcn = srcns.begin(); itr_srcn != srcns.end(); ++itr_srcn) {
		uint8_t srcn = *itr_srcn;
		const SPCSampDir& sample = samples[srcn];

		printf("|$%02x  |$%04X |$%04X |$%04X |%5u |", srcn,
			sample.start_address, sample.loop_address, sample.end_address,
			(unsigned int)sample.compressed_size());

		if (sample.looped) {
			if (sample.loop_address >= sample.start_address && sample.loop_address < sample.end_address) {
				printf("%6d |", sample.loop_sample());
			}
			else {
				printf("%6s |", "?");
			}
		}
		else {
			printf("%6s |", "-");
		}

		printf("\n");
	}
	printf("|-----|------|------|------|------|-------|\n");
	printf("\n");
}

std::vector<uint8_t> Split700::GetSampList(const SPCFile& spc_file) const
{
	std::vector<uint8_t> srcns;

	for (int samp = 0; samp < spc_file.samp_dir_length; samp++) {
		uint8_t srcn = (uint8_t)samp;
		const SPCSampDir& sample = spc_file.samples[srcn];

		if (!force) {
			if (!IsValidSample(spc_file, srcn)) {
				continue;
			}

			// alignment check for overlapped samples
			bool valid_alignment = true;
			for (auto itr_srcn = srcns.begin(); itr_srcn != srcns.end(); ++itr_srcn) {
				const SPCSampDir& valid_sample = spc_file.samples[*itr_srcn];

				// overlapped sample?
				if ((sample.start_address >= valid_sample.start_address && sample.start_address < valid_sample.end_address) ||
					(sample.end_address > valid_sample.start_address && sample.end_address <= valid_sample.end_address)) {

					// alignment should match
					if ((sample.start_address % SPCSampDir::BRR_CHUNK_SIZE) != (valid_sample.start_address % SPCSampDir::BRR_CHUNK_SIZE)) {
						valid_alignment = false;
						break;
					}
				}
			}

			if (!valid_alignment) {
				continue;
			}
		}

		srcns.push_back(srcn);
	}

	return srcns;
}

bool Split700::ParseSampIndexStr(std::vector<uint8_t>& srcns, const std::string& str_samples)
{
	srcns.clear();

	std::vector<std::string> tokens(split(str_samples, ','));
	for (size_t i = 0; i < tokens.size(); i++) {
		tokens[i] = trim(tokens[i]);
	}

	char* endp;
	for (size_t i = 0; i < tokens.size(); i++) {
		std::string token(tokens[i]);

		int radix = 10;
		if (token[0] == '$') {
			radix = 16;
			token = token.substr(1);
		}

		size_t pos_hyphen = token.find_first_of('-', 0);
		if (pos_hyphen != std::string::npos) {
			std::string token_from(token.substr(0, pos_hyphen));
			std::string token_to(token.substr(pos_hyphen + 1));

			long srcn_from = strtol(token_from.c_str(), &endp, radix);
			if (*endp != '\0' || srcn_from < 0 || srcn_from > 255) {
				return false;
			}

			long srcn_to = strtol(token_to.c_str(), &endp, radix);
			if (*endp != '\0' || srcn_to < 0 || srcn_to > 255) {
				return false;
			}

			if (srcn_from > srcn_to) {
				return false;
			}

			for (long srcn = srcn_from; srcn <= srcn_to; srcn++) {
				auto itr_srcn = std::find(srcns.begin(), srcns.end(), (uint8_t)srcn);
				if (itr_srcn == srcns.end()) {
					srcns.push_back((uint8_t)srcn);
				}
			}
		}
		else {
			long srcn = strtol(token.c_str(), &endp, radix);
			if (*endp != '\0' || srcn < 0 || srcn > 255) {
				return false;
			}

			auto itr_srcn = std::find(srcns.begin(), srcns.end(), (uint8_t)srcn);
			if (itr_srcn == srcns.end()) {
				srcns.push_back((uint8_t)srcn);
			}
		}
	}

	std::sort(srcns.begin(), srcns.end());
	return srcns.size() != 0;
}

bool Split700::IsValidSample(const SPCFile& spc_file, uint8_t srcn) const
{
	const SPCSampDir& sample = spc_file.samples[srcn];

	if (!sample.valid) {
		return false;
	}

	// validate loop point (even if it's not a looped sample)
	// loop point must point to the head of brr chunk
	if ((sample.loop_address - sample.start_address) % SPCSampDir::BRR_CHUNK_SIZE != 0) {
		return false;
	}

	// loop point should be in between the start and the end
	// test case (broken loop info with oneshot sample): Wagan Paradise (sample 0-7)
	if (sample.looped) {
		if (sample.loop_address < sample.start_address || sample.loop_address >= sample.end_address) {
			return false;
		}
	}

	if (sample.start_address < 0x200) {
		// usually, samples should not be in direct pages
		return false;
	}

	uint16_t dir = spc_file.dsp[0x5d] << 8;
	uint8_t flg = spc_file.dsp[0x6c];
	uint16_t esa = spc_file.dsp[0x6d] << 8;
	uint8_t edl = spc_file.dsp[0x7d] & 15;
	bool echo_enabled = ((flg & 0x20) == 0);

	unsigned int dir_end = dir + ((srcn + 1) * 4);
	if (dir_end >= 0x10000) {
		return false;
	}

	if ((sample.start_address >= dir && sample.start_address < dir_end) ||
		(sample.end_address > dir && sample.end_address <= dir_end)) {
		return false;
	}

	unsigned int num_of_chunks = (unsigned int)(sample.compressed_size() / SPCSampDir::BRR_CHUNK_SIZE);
	const int LEAST_NUM_OF_CHUNKS = 8;
	const int LEAST_NUM_OF_CHUNKS_HARDER = 2; // test case: Dragon Quest III
	if (num_of_chunks < LEAST_NUM_OF_CHUNKS) {
		if (num_of_chunks < LEAST_NUM_OF_CHUNKS_HARDER) {
			return false;
		}

		if (!sample.looped) {
			if (sample.loop_address < sample.start_address || sample.loop_address >= sample.end_address) {
				return false;
			}
		}
	}

	// samples must not be erased by echo
	if (echo_enabled) {
		uint16_t echo_size = (edl != 0) ? (2048 * edl) : 4;
		uint16_t echo_end = esa + echo_size;

		if (esa < echo_end) {
			if ((sample.start_address >= esa && sample.start_address < echo_end) ||
				(sample.end_address > esa && sample.end_address <= echo_end)) {
				return false;
			}
		}
		else {
			if (sample.start_address >= esa || sample.start_address < echo_end ||
				sample.end_address > esa || sample.end_address <= echo_end) {
				return false;
			}
		}
	}

	return true;
}

std::vector<uint8_t> Split700::QueryDumpableSamples(const SPCFile& spc_file, const std::vector<uint8_t>& srcns)
{
	std::vector<uint8_t> dumpable_srcns;
	for (auto itr_srcn = srcns.begin(); itr_srcn != srcns.end(); ++itr_srcn) {
		uint8_t srcn = *itr_srcn;
		const SPCSampDir& sample = spc_file.samples[srcn];

		if (sample.start_address > sample.end_address) {
			continue;
		}

		if (sample.compressed_size() == 0) {
			continue;
		}

		dumpable_srcns.push_back(srcn);
	}

	return dumpable_srcns;
}

std::string Split700::GetSongTitle(const SPCFile& spc_file, const std::string& filename) const
{
	std::string title(filename);
	if (spc_file.tags.count(SPCFile::XID6ItemId::XID6_SONG_NAME) != 0) {
		std::string song_name(spc_file.GetStringTag(SPCFile::XID6ItemId::XID6_SONG_NAME));
		title = song_name + " (" + filename + ")";
	}
	return title;
}

std::string Split700::GetExportFilename(const SPCFile& spc_file, const std::string& basename, uint8_t srcn, const std::string& extension) const
{
	char tmp[32];

	char pwd[PATH_MAX];
	getcwd(pwd, PATH_MAX);

	sprintf(tmp, "%02x", srcn);
	std::string str_srcn(tmp);

	std::string loop_info;
	if (loop_point_to_filename) {
		const SPCSampDir& sample = spc_file.samples[srcn];
		if (sample.looped) {
			sprintf(tmp, "%d", sample.loop_sample());
			loop_info = "loop" + std::string(tmp);
		}
		else {
			loop_info = "noloop";
		}
	}

	std::string out_filename = basename + "_" + str_srcn + (loop_info.empty() ? "" : "-") + loop_info + extension;
	char out_basename_c[PATH_MAX];
	strcpy(out_basename_c, out_filename.c_str());
	path_basename(out_basename_c);
	return std::string(out_basename_c);
}

enum Split700ProcMode
{
	SPLIT700_PROC_BRR = 0,
	SPLIT700_PROC_WAV,
	SPLIT700_PROC_LIST,
};

static void usage(const char* progname)
{
	printf("%s %s\n", APP_NAME, APP_VER);
	printf("<%s>\n", APP_URL);
	printf("\n");
	printf("Extracts BRR samples from SNES SPC700 format (*.spc)\n");
	printf("\n");
	printf("Usage\n");
	printf("-----\n");
	printf("\n");
	printf("Syntax: `%s [options] [spc files]`\n", progname);
	printf("\n");

	printf("### Options\n");
	printf("\n");
	printf("`-f`, `--force`\n");
	printf("  : Force output every samples (including corrupt samples).\n");
	printf("\n");
	printf("`-n N`, `--srcn N`\n");
	printf("  : Specify target sample number. (example: `--srcn \"1, 2, $10-20\"`)\n");
	printf("\n");
	printf("`-l`, `--list`\n");
	printf("  : Display only voice list, with no file outputs.\n");
	printf("\n");
	printf("`--wav`\n");
	printf("  : Convert BRR samples to WAVE files.\n");
	printf("\n");
	printf("`--pitch HEX`\n");
	printf("  : Specify sample rate for output WAVE file (0x1000 = 32000 Hz).\n");
	printf("\n");
	printf("`-L`\n");
	printf("  : Add loop point info to output filename of the sample.\n");
	printf("\n");
	printf("`-M`\n");
	printf("  : Add file header for addmusicM (i.e. export loop-point).\n");
	printf("\n");
	printf("`-?`, `--help`\n");
	printf("  : Display this help.\n");
	printf("\n");
}

int main(int argc, char* argv[])
{
	Split700 app;
	Split700ProcMode mode = SPLIT700_PROC_BRR;
	bool export_loop_point = false;
	std::vector<uint8_t> srcns;
	int32_t wav_samplerate = 32000;

	long l;
	char* endptr = NULL;

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

		if (strcmp(argv[argi], "-f") == 0 || strcmp(argv[argi], "--force") == 0) {
			app.SetForce(true);
		}
		else if (strcmp(argv[argi], "-l") == 0 || strcmp(argv[argi], "--list") == 0) {
			mode = SPLIT700_PROC_LIST;
		}
		else if (strcmp(argv[argi], "--brr") == 0) {
			mode = SPLIT700_PROC_BRR;
		}
		else if (strcmp(argv[argi], "--wav") == 0) {
			mode = SPLIT700_PROC_WAV;
		}
		else if (strcmp(argv[argi], "--pitch") == 0) {
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
			wav_samplerate = l * 32000 / 0x1000;
			argi++;
		}
		else if (strcmp(argv[argi], "-n") == 0 || strcmp(argv[argi], "--srcn") == 0) {
			if (argc <= (argi + 1)) {
				fprintf(stderr, "Error: Too few arguments for \"%s\"\n", argv[argi]);
				return EXIT_FAILURE;
			}

			if (!Split700::ParseSampIndexStr(srcns, argv[argi + 1])) {
				fprintf(stderr, "Error: Illegal format with target SRCNs\n");
				return EXIT_FAILURE;
			}
			argi++;
		}
		else if (strcmp(argv[argi], "-L") == 0) {
			app.SetLoopPointToFileName(true);
		}
		else if (strcmp(argv[argi], "-M") == 0) {
			export_loop_point = true;
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
		std::string spc_filename(argv[argi]);
		bool result;

		switch (mode) {
		case SPLIT700_PROC_BRR:
			if (srcns.size() != 0) {
				result = app.ExportLoopSamples(spc_filename, srcns, export_loop_point);
			}
			else {
				result = app.ExportLoopSamples(spc_filename, export_loop_point);
			}

			if (!result) {
				fprintf(stderr, "Error: %s: %s\n", spc_filename.c_str(), app.message().c_str());
				errors++;
			}
			break;

		case SPLIT700_PROC_LIST:
			if (srcns.size() != 0) {
				result = app.PrintSPCInfo(spc_filename, srcns);
			}
			else {
				result = app.PrintSPCInfo(spc_filename);
			}

			if (!result) {
				fprintf(stderr, "Error: %s: %s\n", spc_filename.c_str(), app.message().c_str());
				errors++;
			}
			break;

		case SPLIT700_PROC_WAV:
			//if (srcns.size() != 0) {
			//	result = app.ExportLoopSamplesAsWAV(spc_filename, srcns, wav_samplerate);
			//}
			//else {
			//	result = app.ExportLoopSamplesAsWAV(spc_filename, wav_samplerate);
			//}

			//if (!result) {
			//	fprintf(stderr, "Error: %s: %s\n", spc_filename.c_str(), app.message().c_str());
			//	errors++;
			//}
			break;

		default:
			fprintf(stderr, "Error: Unsupported processing mode\n");
			errors++;
			break;
		}
	}

	return (errors == 0) ? EXIT_SUCCESS : EXIT_FAILURE;
}

void FreeExtractedSpcSampleBuffer(uint8_t* sample_buffer_ptr)
{
	free(sample_buffer_ptr);
}

bool ExtractSpcFile(char* fileName, uint8_t** sample_buffers, uint32_t* sample_sizes)
{
	Split700 app;

	std::string spc_filename(fileName);

	SPCFile* spc_file_ptr = SPCFile::Load(spc_filename);
	if (spc_file_ptr == NULL)
		return false;

	SPCFile& spc_file = *spc_file_ptr;

	std::vector<uint8_t> srcns(app.GetSampList(*spc_file_ptr));
	std::vector<uint8_t> dumpable_srcns = app.QueryDumpableSamples(spc_file, srcns);

	int idx = 0;
	for (auto itr_srcn = dumpable_srcns.begin(); itr_srcn != dumpable_srcns.end(); ++itr_srcn) {
		uint8_t srcn = *itr_srcn;
		const SPCSampDir& sample = spc_file.samples[srcn];

		uint16_t loop_point_rel;
		if (sample.looped && sample.loop_address >= sample.start_address && sample.loop_address < sample.end_address) {
			loop_point_rel = sample.loop_address - sample.start_address;
		}
		else {
			loop_point_rel = sample.end_address - sample.start_address;
		}

		std::vector<uint8_t> result_data;

		//sample data
		result_data.push_back((uint8_t)(loop_point_rel & 0xff));
		result_data.push_back((uint8_t)(loop_point_rel >> 8));
		std::copy(&spc_file.ram[sample.start_address],
			&spc_file.ram[sample.start_address + sample.compressed_size()],
			std::back_inserter(result_data));

		//sample size
		sample_sizes[idx] = result_data.size();

		sample_buffers[idx] = (uint8_t*)malloc(result_data.size());
		std::copy(result_data.data(), result_data.data() + result_data.size(), sample_buffers[idx]);

		idx++;
	}

	return true;
}

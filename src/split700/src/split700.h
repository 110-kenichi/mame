
#ifndef SPLIT700_H
#define SPLIT700_H

#include <stdint.h>
#include <string>

#ifdef WIN32
#include <Windows.h>
#endif

#include "SPCFile.h"

class Split700
{
public:
	Split700();
	virtual ~Split700();

	inline bool IsLoopPointToFileName(void) const {
		return loop_point_to_filename;
	}

	inline void SetLoopPointToFileName(bool loop_point_to_filename) {
		this->loop_point_to_filename = loop_point_to_filename;
	}

	inline bool IsForce(void) const {
		return force;
	}

	inline void SetForce(bool force) {
		this->force = force;
	}

	inline const std::string& message(void) const {
		return m_message;
	}

	bool ExportLoopSamples(const std::string & spc_filename, bool export_loop_point = false);
	bool ExportLoopSamples(const std::string & spc_filename, const std::vector<uint8_t> & srcns, bool export_loop_point = false);
	bool ExportLoopSamples(const SPCFile & spc_file, const std::string & base_path, bool export_loop_point = false);
	bool ExportLoopSamples(const SPCFile & spc_file, const std::string & base_path, const std::vector<uint8_t> & srcns, bool export_loop_point = false);
	//bool ExportLoopSamplesAsWAV(const std::string & spc_filename, int32_t samplerate = 32000);
	//bool ExportLoopSamplesAsWAV(const std::string & spc_filename, const std::vector<uint8_t> & srcns, int32_t samplerate = 32000);
	//bool ExportLoopSamplesAsWAV(const SPCFile & spc_file, const std::string & base_path, int32_t samplerate = 32000);
	//bool ExportLoopSamplesAsWAV(const SPCFile & spc_file, const std::string & base_path, const std::vector<uint8_t> & srcns, int32_t samplerate = 32000);
	bool PrintSPCInfo(const std::string & spc_filename);
	bool PrintSPCInfo(const std::string & spc_filename, const std::vector<uint8_t> & srcns);
	bool PrintSPCInfo(const SPCFile & spc_file, const std::string & title);
	bool PrintSPCInfo(const SPCFile & spc_file, const std::string & title, const std::vector<uint8_t> & srcns);
	void PrintSampList(const SPCSampDir samples[], const std::vector<uint8_t> & srcns) const;
	std::vector<uint8_t> GetSampList(const SPCFile & spc_file) const;

	static bool ParseSampIndexStr(std::vector<uint8_t> & srcns, const std::string & str_samples);

	bool IsValidSample(const SPCFile& spc_file, uint8_t srcn) const;
	std::vector<uint8_t> QueryDumpableSamples(const SPCFile& spc_file, const std::vector<uint8_t>& srcns);
	std::string GetSongTitle(const SPCFile& spc_file, const std::string& filename) const;
	std::string GetExportFilename(const SPCFile& spc_file, const std::string& basename, uint8_t srcn, const std::string& extension) const;

protected:
	bool loop_point_to_filename;
	bool force;

	std::string m_message;

private:
	//bool IsValidSample(const SPCFile & spc_file, uint8_t srcn) const;
	//std::vector<uint8_t> QueryDumpableSamples(const SPCFile & spc_file, const std::vector<uint8_t> & srcns);
	//std::string GetSongTitle(const SPCFile & spc_file, const std::string & filename) const;
	//std::string GetExportFilename(const SPCFile & spc_file, const std::string & basename, uint8_t srcn, const std::string & extension) const;
};

#endif

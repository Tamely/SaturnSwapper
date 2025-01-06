#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <xxhash/xxhash.h>

import Saturn.Compression.Oodle;

import <filesystem>;
import <fstream>;

#define OODLELZ_BLOCK_LEN (1<<18)
#define OODLELZ_BLOCK_MAXIMUM_EXPANSION (2)
#define HASH 5462001189984043287

void Oodle::LoadDLL(const char* DllPath) {
	if (!std::filesystem::exists(DllPath)) {
		return;
	}

	std::ifstream ifs(DllPath);
	std::string content((std::istreambuf_iterator<char>(ifs)), (std::istreambuf_iterator<char>()));
	ifs.close();

	uint64_t hash = XXH3_64bits(content.c_str(), content.size());
	if (hash != HASH) {
		MessageBoxW(nullptr, L"Saturn attempted to load a potentially dangerous library disguised as Saturn's Oodle library. Please report this to Saturn staff, delete the Saturn folder in localappdata, then virus scan your PC! Please always download Saturn from the official discord!", L"Blocked loading of potentially malicious library", MB_OK);
		exit(0);
	}

	auto OodleHandle = LoadLibraryA(DllPath);
	OodleLZ_Decompress = (OodleDecompressionFunc)GetProcAddress(OodleHandle, "OodleLZ_Decompress");
	OodleLZ_Compress = (OodleLZ_CompressFunc)GetProcAddress(OodleHandle, "OodleLZ_Compress");
	OodleLZ_GetCompressedBufferSizeNeeded = (OodleLZ_GetCompressedBufferSizeNeededFunc)GetProcAddress(OodleHandle, "OodleLZ_GetCompressedBufferSizeNeeded");
	OodleLZ_CompressOptions_GetDefault = (CompressOptions_GetDefaultFunc)GetProcAddress(OodleHandle, "OodleLZ_CompressOptions_GetDefault");
}

void Oodle::Compress(void* compressedData, int32_t& compressedSize, const void* decompressedData, intptr_t decompressedSize) {
	if (!OodleLZ_Compress) {
		throw std::exception("OodleLZ_Compress is called despite the DLL not being loaded!");
	}

	OodleLZ_CompressOptions* options = OodleLZ_CompressOptions_GetDefault(OodleLZ_Compressor_Kraken, OodleCompressionLevel::Optimal5);
	compressedSize = OodleLZ_Compress(OodleLZ_Compressor_Kraken, (void*)decompressedData, decompressedSize, compressedData, OodleCompressionLevel::Optimal5, options, nullptr, nullptr, nullptr, 0);
}

uint32_t Oodle::GetMaximumCompressedSize(uint32_t InUncompressedSize) {
	if (!OodleLZ_GetCompressedBufferSizeNeeded) {
		throw std::exception("OodleLZ_GetCompressedBufferSizeNeeded is called despite the DLL not being loaded!");
	}

	return OodleLZ_GetCompressedBufferSizeNeeded(OodleLZ_Compressor_Kraken, (intptr_t)InUncompressedSize);
}

void Oodle::Decompress(const void* compressedData, intptr_t compressedSize, void* decompressedData, intptr_t decompressedSize) {
	if (!OodleLZ_Decompress) {
		throw std::exception("OodleLZ_Decompress is called despite the DLL not being loaded!");
	}

	OodleLZ_Decompress(compressedData, compressedSize, decompressedData, decompressedSize, 1, 1, 0, 0, 0, 0, 0, nullptr, 0, 3);
}
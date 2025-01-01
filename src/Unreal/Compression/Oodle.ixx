export module Saturn.Compression.Oodle;

import <cstdint>;

export enum class OodleCompressorType : uint32_t {
	LZH = 0,
	LZHLW = 1,
	LZNIB = 2,
	None = 3,
	LZB16 = 4,
	LZBLW = 5,
	LZA = 6,
	LZNA = 7,
	Kraken = 8,
	Mermaid = 9,
	BitKnit = 10,
	Selkie = 11,
	Hydra = 12,
	Leviathan = 13
};

export enum class OodleCompressionLevel : uint32_t {
	None = 0,
	SuperFast = 1,
	VeryFast = 2,
	Fast = 3,
	Normal = 4,
	Optimal1 = 5,
	Optimal2 = 6,
	Optimal3 = 7,
	Optimal4 = 8,
	Optimal5 = 9
};

export typedef intptr_t(*OodleDecompressionFunc)(
	const void* compressedBuffer,
	intptr_t compressedBufferSize,
	void* rawBuffer,
	intptr_t rawLength,
	int fuzzSafe,
	int checkCRC,
	int verbosity,
	void* decompressedBufferBase,
	intptr_t decompressedBufferSize,
	void* fpCallback,
	void* callbackUserData,
	void* decodederMemory,
	intptr_t decoderMemorySize,
	uint32_t threadPhase);

export typedef intptr_t(*OodleCompressionFunc)(
	OodleCompressorType compressor,
	void* rawBuffer,
	intptr_t rawLength,
	void* compressedBuffer,
	OodleCompressionLevel level,
	void* lrm,
	void* scratchMemory,
	intptr_t scratchSize);

export class Oodle {
public:
	static inline OodleCompressionFunc OodleLZ_Compress;
	static inline OodleDecompressionFunc OodleLZ_Decompress;

	static void LoadDLL(const char* DllPath);
	static void Compress(void* compressedData, int32_t* compressedSize, const void* decompressedData, intptr_t decompressedSize);
	static void Decompress(const void* compressedData, intptr_t compressedSize, void* decompressedData, intptr_t decompressedSize);
	static uint32_t GetMaximumCompressedSize(uint32_t InUncompressedSize);
};
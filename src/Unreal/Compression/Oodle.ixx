export module Saturn.Compression.Oodle;

import <cstdint>;

export typedef enum OodleLZ_Profile
{
	OodleLZ_Profile_Main = 0,         // Main profile (all current features allowed)
	OodleLZ_Profile_Reduced = 1,      // Reduced profile (Kraken only, limited feature set)
	OodleLZ_Profile_Force32 = 0x40000000
} OodleLZ_Profile;

export typedef enum OodleLZ_Jobify
{
	OodleLZ_Jobify_Default = 0,       // Use compressor default for level of internal job usage
	OodleLZ_Jobify_Disable = 1,       // Don't use jobs at all
	OodleLZ_Jobify_Normal = 2,        // Try to balance parallelism with increased memory usage
	OodleLZ_Jobify_Aggressive = 3,    // Maximize parallelism even when doing so requires large amounts of memory
	OodleLZ_Jobify_Count = 4,

	OodleLZ_Jobify_Force32 = 0x40000000,
} OodleLZ_Jobify;

export typedef struct OodleLZ_CompressOptions {
	uint32_t              unused_was_verbosity;               // unused ; was verbosity (set to zero)
	int32_t              minMatchLen;        // minimum match length ; cannot be used to reduce a compressor's default MML, but can be higher.  On some types of data, a large MML (6 or 8) is a space-speed win.
	bool             seekChunkReset;     // whether chunks should be independent, for seeking and parallelism
	int32_t              seekChunkLen;       // length of independent seek chunks (if seekChunkReset) ; must be a power of 2 and >= $OODLELZ_BLOCK_LEN ; you can use $OodleLZ_MakeSeekChunkLen
	OodleLZ_Profile     profile;            // decoder profile to target (set to zero)
	int32_t              dictionarySize;     // sets a maximum offset for matches, if lower than the maximum the format supports.  <= 0 means infinite (use whole buffer).  Often power of 2 but doesn't have to be.
	int32_t              spaceSpeedTradeoffBytes;  // this is a number of bytes; I must gain at least this many bytes of compressed size to accept a speed-decreasing decision
	int32_t              unused_was_maxHuffmansPerChunk;  //  unused ; was maxHuffmansPerChunk
	bool             sendQuantumCRCs;    // should the encoder send a CRC of each compressed quantum, for integrity checks; this is necessary if you want to use OodleLZ_CheckCRC_Yes on decode
	int32_t              maxLocalDictionarySize;  // (Optimals) size of local dictionary before needing a long range matcher.  This does not set a window size for the decoder; it's useful to limit memory use and time taken in the encoder.  maxLocalDictionarySize must be a power of 2.  Must be <= OODLELZ_LOCALDICTIONARYSIZE_MAX
	bool             makeLongRangeMatcher;   // (Optimals) should the encoder find matches beyond maxLocalDictionarySize using an LRM
	int32_t              matchTableSizeLog2; //(non-Optimals)  when variable, sets the size  of the match finder structure (often a hash table) ; use 0 for the compressor's default

	OodleLZ_Jobify      jobify;         // controls internal job usage by compressors
	void* jobifyUserPtr;  // user pointer passed through to RunJob and WaitJob callbacks

	int32_t              farMatchMinLen; // far matches must be at least this len
	int32_t              farMatchOffsetLog2; // if not zero, the log2 of an offset that must meet farMatchMinLen

	int32_t              reserved[4];   // reserved space for adding more options; zero these!
} OodleLZ_CompressOptions;

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

export typedef OodleLZ_CompressOptions*(*CompressOptions_GetDefaultFunc)(
	OodleCompressorType compressor,
	OodleCompressionLevel lzLevel);

export typedef intptr_t(*OodleCompressionFunc)(
	OodleCompressorType compressor,
	void* rawBuffer,
	intptr_t rawLength,
	void* compressedBuffer,
	OodleCompressionLevel level,
	OodleLZ_CompressOptions* pOptions,
	void* dictionaryBase,
	void* lrm,
	void* scratchMemory,
	intptr_t scratchSize);

export class Oodle {
	static inline CompressOptions_GetDefaultFunc OodleLZ_CompressOptions_GetDefault;
	static inline OodleCompressionFunc OodleLZ_Compress;
	static inline OodleDecompressionFunc OodleLZ_Decompress;
public:
	static void LoadDLL(const char* DllPath);
	static void Compress(void* compressedData, int32_t& compressedSize, const void* decompressedData, intptr_t decompressedSize);
	static void Decompress(const void* compressedData, intptr_t compressedSize, void* decompressedData, intptr_t decompressedSize);
	static uint32_t GetMaximumCompressedSize(uint32_t InUncompressedSize);
};
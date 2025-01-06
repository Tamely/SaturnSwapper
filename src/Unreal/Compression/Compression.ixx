export module Saturn.Compression;

import <string>;
import <cstdint>;

export enum ECompressionFlags {
	/** No compression																*/
	COMPRESS_None = 0x00,
	/** Compress with ZLIB - DEPRECATED, USE FNAME									*/
	COMPRESS_ZLIB = 0x01,
	/** Compress with GZIP - DEPRECATED, USE FNAME									*/
	COMPRESS_GZIP = 0x02,
	/** Compress with user defined callbacks - DEPRECATED, USE FNAME                */
	COMPRESS_Custom = 0x04,
	/** Joint of the previous ones to determine if old flags are being used			*/
	COMPRESS_DeprecatedFormatFlagsMask = 0xF,


	/** No flags specified /														*/
	COMPRESS_NoFlags = 0x00,
	/** Prefer compression that compresses smaller (ONLY VALID FOR COMPRESSION)		*/
	COMPRESS_BiasMemory = 0x10,
	COMPRESS_BiasSize = COMPRESS_BiasMemory,
	/** Prefer compression that compresses faster (ONLY VALID FOR COMPRESSION)		*/
	COMPRESS_BiasSpeed = 0x20,
	/** Is the source buffer padded out	(ONLY VALID FOR UNCOMPRESS)					*/
	COMPRESS_SourceIsPadded = 0x80,

	/** Set of flags that are options are still allowed								*/
	COMPRESS_OptionsFlagsMask = 0xF0,

	/** Indicate this compress call is for Packaging (pak/iostore)					*/
	COMPRESS_ForPackaging = 0x100,
	COMPRESS_ForPurposeMask = 0xF00,
};

export class FCompression {
public:
    static bool VerifyCompressionFlagsValid(int32_t InCompressionFlags);
	static int64_t GetMaximumCompressedSize(const std::string& FormatName, int32_t UncompressedSize, ECompressionFlags Flags = COMPRESS_NoFlags, int32_t CompressionData = 0);
	static int64_t CompressMemoryBound(const std::string& FormatName, int32_t UncompressedSize, ECompressionFlags Flags, int32_t CompressionData);
	static void CompressMemory(const std::string& FormatName, const void* UncompressedBuffer, int32_t UncompressedSize, void* CompressedBuffer, int32_t& CompressedSize);
	static void DecompressMemory(const std::string& FormatName, void* UncompressedBuffer, int32_t UncompressedSize, const void* CompressedBuffer, int32_t CompressedSize);
};
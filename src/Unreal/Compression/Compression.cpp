import Saturn.Compression;
import Saturn.Compression.Oodle;

import <string>;

bool FCompression::VerifyCompressionFlagsValid(int32_t InCompressionFlags) {
	const int32_t CompressionFlagsMask = COMPRESS_DeprecatedFormatFlagsMask | COMPRESS_OptionsFlagsMask | COMPRESS_ForPurposeMask;
	if (InCompressionFlags & (~CompressionFlagsMask)) {
		return false;
	}

	return true;
}

int64_t FCompression::GetMaximumCompressedSize(const std::string& FormatName, int32_t UncompressedSize, ECompressionFlags Flags, int32_t CompressionData) {
	if (FormatName == "Oodle") {
		return Oodle::GetMaximumCompressedSize(UncompressedSize);
	}

	return CompressMemoryBound(FormatName, UncompressedSize, Flags, CompressionData);
}

int64_t FCompression::CompressMemoryBound(const std::string& FormatName, int32_t UncompressedSize, ECompressionFlags Flags, int32_t CompressionData) {
	int32_t CompressionBound = UncompressedSize;

	if (FormatName.empty()) {
		return UncompressedSize;
	}
	else if (FormatName == "Zlib") {
		// TODO: Implement Zlib compression
	}
	else if (FormatName == "Gzip") {
		// TODO: Implement Gzip compression
	}
	else if (FormatName == "LZ4") {
		// TODO: Implement LZ4 compression
	}

	return CompressionBound;
}

void FCompression::CompressMemory(const std::string& FormatName, const void* UncompressedBuffer, int32_t UncompressedSize, void* CompressedBuffer, int32_t& CompressedSize) {
	if (UncompressedSize == CompressedSize) {
		memcpy(CompressedBuffer, UncompressedBuffer, UncompressedSize);
		return;
	}

	if (FormatName.contains("Zlib")) {
		// TODO: Implement Zlib decompression
	}
	else if (FormatName.contains("Gzip")) {
		// TODO: Implement Gzip decompression
	}
	else if (FormatName.contains("LZ4")) {
		// TODO: Implement LZ4 decompression
	}
	else if (FormatName.contains("Oodle")) {
		Oodle::Compress(CompressedBuffer, CompressedSize, UncompressedBuffer, UncompressedSize);
	}
	else if (FormatName.contains("None")) {
		memcpy(CompressedBuffer, UncompressedBuffer, UncompressedSize);
		CompressedSize = UncompressedSize;
	}
}

void FCompression::DecompressMemory(const std::string& FormatName, void* UncompressedBuffer, int32_t UncompressedSize, const void* CompressedBuffer, int32_t CompressedSize) {
	if (UncompressedSize == CompressedSize) {
		memcpy(UncompressedBuffer, CompressedBuffer, CompressedSize);
		return;
	}

	if (FormatName.contains("Zlib")) {
		// TODO: Implement Zlib decompression
	}
	else if (FormatName.contains("Gzip")) {
		// TODO: Implement Gzip decompression
	}
	else if (FormatName.contains("LZ4")) {
		// TODO: Implement LZ4 decompression
	}
	else if (FormatName.contains("Oodle")) {
		Oodle::Decompress(CompressedBuffer, CompressedSize, UncompressedBuffer, UncompressedSize);
	}
}
export module Saturn.Pak.PakEntry;

import Saturn.Encryption.AES;
import Saturn.Readers.FArchive;
import Saturn.Pak.PakFileVersion;
import Saturn.Pak.PakCompressedBlock;

import <cstdint>;
import <vector>;
import <string>;

export class FPakEntry {
public:
	FPakEntry() = default;
	FPakEntry(FArchive& reader, EPakFileVersion version); // This is the decoded version
	FPakEntry(FArchive& reader); // This is the encoded version

	std::vector<uint8_t> Read(const std::string& path, EPakFileVersion version, std::vector<std::string> compressionMethods, const FAESKey& key);
private:
	int64_t Offset;
	int64_t CompressedSize;
	int64_t UncompressedSize;
	uint32_t CompressionMethod;
	std::vector<FPakCompressedBlock> CompressionBlocks;
	bool Encrypted;
	uint32_t CompressionBlockSize;
};
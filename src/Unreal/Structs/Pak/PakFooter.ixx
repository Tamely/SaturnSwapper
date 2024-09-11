export module Saturn.Pak.PakFooter;

import Saturn.Structs.Guid;
import Saturn.Structs.SHAHash;
import Saturn.Readers.FArchive;
import Saturn.Pak.PakFileVersion;

import <cstdint>;
import <vector>;
import <string>;

export struct FPakFooter {
	static const uint32_t PAK_FILE_MAGIC = 0x5A6F12E1;
	static const int32_t COMPRESSION_METHOD_NAME_LEN = 32;
	static const int32_t MAX_NUM_COMPRESSION_METHODS = 5;

	uint32_t Magic;
	FGuid EncryptionGuid;
	bool Encrypted;
	uint64_t IndexOffset;
	uint64_t IndexSize;
	FSHAHash IndexHash;
	bool IndexIsFrozen;
	std::vector<std::string> Compression;

	FPakFooter() = default;
	FPakFooter(FArchive& reader, EPakFileVersion version);
public:
	static int64_t GetSerializedSize(EPakFileVersion InVersion) {
		int64_t Size = sizeof(uint32_t) + sizeof(uint32_t) + sizeof(int64_t) + sizeof(int64_t) + sizeof(FSHAHash) + sizeof(bool);
		if (InVersion >= PakFile_Version_EncryptionKeyGuid) Size += sizeof(FGuid);
		if (InVersion >= PakFile_Version_FNameBasedCompressionMethod) Size += COMPRESSION_METHOD_NAME_LEN * MAX_NUM_COMPRESSION_METHODS;
		if (InVersion >= PakFile_Version_FrozenIndex && InVersion < PakFile_Version_PathHashIndex) Size += sizeof(bool);

		return Size;
	}
};
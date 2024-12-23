#include <cassert>

import Saturn.Pak.PakFooter;

import Saturn.Structs.Guid;
import Saturn.Structs.SHAHash;
import Saturn.Readers.FArchive;
import Saturn.Pak.PakFileVersion;

import <cstdint>;
import <vector>;
import <string>;
import <memory>;

FPakFooter::FPakFooter(FArchive& reader, EPakFileVersion version) {
    if (reader.TotalSize() < (reader.Tell() + GetSerializedSize(version))) {
        return;
    }

    if (version >= EPakFileVersion::PakFile_Version_EncryptionKeyGuid) {
        reader << EncryptionGuid;
    }

    if (version >= EPakFileVersion::PakFile_Version_IndexEncryption) {
        uint8_t flag;
        reader << flag;
        Encrypted = flag != 0;
    }

    reader << Magic;
    if (Magic != FPakFooter::PAK_FILE_MAGIC) {
        assert(false && "Invalid pak file magic");
        return;
    }

    uint32_t ver;
    reader << ver;
    if (static_cast<EPakFileVersion>(ver) != version) {
		assert(false && "Invalid pak file version");
	}

    reader << IndexOffset;
	reader << IndexSize;
	reader << IndexHash;
	
    if (version == EPakFileVersion::PakFile_Version_FrozenIndex) {
        uint8_t bIndexIsFrozen;
        reader << bIndexIsFrozen;
        IndexIsFrozen = bIndexIsFrozen != 0;
    }

    Compression.push_back("None");

    if (version < EPakFileVersion::PakFile_Version_FNameBasedCompressionMethod) {
        Compression = { "Zlib", "Gzip", "Oodle" };
    }
    else {
        const int bufferSize = COMPRESSION_METHOD_NAME_LEN * MAX_NUM_COMPRESSION_METHODS;
        auto Methods = std::make_unique<char[]>(bufferSize);
        reader.Serialize(Methods.get(), bufferSize);

        for (int i = 0; i < MAX_NUM_COMPRESSION_METHODS; i++) {
            std::string MethodString = &Methods[i * COMPRESSION_METHOD_NAME_LEN];

            if (MethodString.empty()) {
				break;
			}

            Compression.push_back(MethodString);
        }
    }
}
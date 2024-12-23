module;

#include "Saturn/Defines.h"

export module Saturn.Structs.IoStoreTocHeader;

import Saturn.Structs.Guid;
import Saturn.Structs.IoContainerId;

export enum class EIoContainerFlags : uint8_t {
    None,
    Compressed = (1 << 0),
    Encrypted = (1 << 1),
    Signed = (1 << 2),
    Indexed = (1 << 3)
};

ENUM_CLASS_FLAGS(EIoContainerFlags);

export struct FIoStoreTocHeader {
    static constexpr inline char TocMagicImg[] = "-==--==--==--==-";

	uint8_t	            TocMagic[16];
	uint8_t	            Version;
	uint8_t	            Reserved0 = 0;
	uint16_t	        Reserved1 = 0;
	uint32_t	        TocHeaderSize;
	uint32_t	        TocEntryCount;
	uint32_t	        TocCompressedBlockEntryCount;
	uint32_t	        TocCompressedBlockEntrySize;	// For sanity checking
	uint32_t	        CompressionMethodNameCount;
	uint32_t	        CompressionMethodNameLength;
	uint32_t	        CompressionBlockSize;
	uint32_t	        DirectoryIndexSize;
	uint32_t	        PartitionCount = 0;
	FIoContainerId      ContainerId;
	FGuid	            EncryptionKeyGuid;
	EIoContainerFlags   ContainerFlags;
	uint8_t	            Reserved3 = 0;
	uint16_t	        Reserved4 = 0;
	uint32_t	        TocChunkPerfectHashSeedsCount = 0;
	uint64_t	        PartitionSize = 0;
	uint32_t	        TocChunksWithoutPerfectHashCount = 0;
	uint32_t	        Reserved7 = 0;
	uint64_t	        Reserved8[5] = { 0 };

	void MakeMagic() {
		memcpy((void*)TocMagic, TocMagicImg, sizeof TocMagic);
	}

	bool CheckMagic() const {
		return memcmp((void*)TocMagic, TocMagicImg, sizeof TocMagic) == 0;
	}
};
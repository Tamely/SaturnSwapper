#include <cassert>

import Saturn.Pak.PakEntry;

import Saturn.Context;
import Saturn.Compression;
import Saturn.Structs.Guid;
import Saturn.Readers.FArchive;
import Saturn.Pak.PakFileVersion;
import Saturn.Structs.FileModification;
import Saturn.Readers.FileReaderNoWrite;

FPakEntry::FPakEntry(FArchive& reader, EPakFileVersion version) {
	reader << Offset;
	reader << CompressedSize;
	reader << UncompressedSize;
	
	if (version < EPakFileVersion::PakFile_Version_FNameBasedCompressionMethod) {
		uint32_t legacyCompressionMethod;
		reader << legacyCompressionMethod;
		if (legacyCompressionMethod == 0) {
			CompressionMethod = 0;
		}
		else if (legacyCompressionMethod & 0x01) {
			CompressionMethod = 1; // ZLIB
		}
		else if (legacyCompressionMethod & 0x02) {
			CompressionMethod = 2; // GZIP
		}
		else if (legacyCompressionMethod & 0x04) {
			CompressionMethod = 3; // Custom
		}
		else {
			assert(false && "Invalid compression method!");
		}
	}
	else {
		reader << CompressionMethod;
	}

	if (version <= EPakFileVersion::PakFile_Version_Initial) {
		reader.SeekCur(sizeof(int64_t)); // Timestamp
	}

	reader.SeekCur(sizeof(FGuid)); // Hash

	if (version >= EPakFileVersion::PakFile_Version_CompressionEncryption) {
		if (CompressionMethod != 0) {
			reader << CompressionBlocks;
		}

		uint8_t flag;
		reader << flag;
		Encrypted = flag != 0;

		reader << CompressionBlockSize;
	}
	else {
		CompressionBlockSize = UncompressedSize;
	}

	if (version >= EPakFileVersion::PakFile_Version_RelativeChunkOffsets) {
		for (auto& block : CompressionBlocks) {
			block.CompressedStart += Offset;
			block.CompressedEnd += Offset;
		}
	}
}

FPakEntry::FPakEntry(FArchive& reader) {
	uint32_t bitfield;
	reader << bitfield;

	uint32_t compressionBlockSize;
	if ((bitfield & 0x3F) == 0x3F) {
		reader << compressionBlockSize;
	}
	else {
		compressionBlockSize = (bitfield & 0x3F) << 11;
	}

	CompressionMethod = (bitfield >> 23) & 0x3F;

	bool bIsOffset32BitSafe = (bitfield & (1 << 31)) != 0;
	if (bIsOffset32BitSafe) {
		uint32_t offset;
		reader << offset;
		Offset = offset;
	}
	else {
		reader << Offset;
	}

	bool bIsUncompressedSize32BitSafe = (bitfield & (1 << 30)) != 0;
	if (bIsUncompressedSize32BitSafe) {
		uint32_t uncompressedSize;
		reader << uncompressedSize;
		UncompressedSize = uncompressedSize;
	}
	else {
		reader << UncompressedSize;
	}

	if (CompressionMethod != 0) {
		bool bIsSize32BitSafe = (bitfield & (1 << 29)) != 0;
		if (bIsSize32BitSafe) {
			uint32_t compressedSize;
			reader << compressedSize;
			CompressedSize = compressedSize;
		}
		else {
			reader << CompressedSize;
		}
	}
	else {
		CompressedSize = UncompressedSize;
	}

	Encrypted = (bitfield & (1 << 22)) != 0;

	uint32_t blockCount = (bitfield >> 6) & 0xFFFF;
	CompressionBlocks = std::vector<FPakCompressedBlock>(blockCount);

	CompressionBlockSize = 0;
	if (blockCount > 0) {
		CompressionBlockSize = compressionBlockSize;

		if (blockCount == 1) {
			CompressionBlockSize = UncompressedSize;
		}
	}

	int StructSize = sizeof(int64_t) * 3 + sizeof(uint32_t) * 2 + 1 + 20;
	if (CompressionMethod != 0) {
		StructSize += sizeof(uint32_t) + blockCount * 2 * sizeof(int64_t);
	}

	if (blockCount == 1 && !Encrypted) {
		auto& block = CompressionBlocks[0];
		block.CompressedStart = Offset + StructSize;
		block.CompressedEnd = block.CompressedStart + CompressedSize;
	}
	else if (blockCount > 0) {
		int32_t compressedBlockAlignment = Encrypted ? 16 : 1;

		int64_t compressedBlockOffset = Offset + StructSize;
		for (int compressionBlockIndex = 0; compressionBlockIndex < blockCount; ++compressionBlockIndex) {
			uint32_t size;
			reader << size;

			auto& block = CompressionBlocks[compressionBlockIndex];
			block.CompressedStart = compressedBlockOffset;
			block.CompressedEnd = compressedBlockOffset + size;

			compressedBlockOffset += (block.CompressedEnd - block.CompressedStart) + compressedBlockAlignment - 1 & ~(compressedBlockAlignment - 1);
		}
	}
}

std::vector<uint8_t> FPakEntry::Read(const std::string& path, EPakFileVersion version, std::vector<std::string> compressionMethods, const FAESKey& key) {
	FFileReaderNoWrite reader(path.c_str());

	std::vector<uint8_t> uncompressed(UncompressedSize);

	uint32_t uncompressedOff = 0;

	uint64_t startOff = 0;

	for (const auto& block : CompressionBlocks) {
		reader.Seek(block.CompressedStart);

		int32_t blockSize = block.CompressedEnd - block.CompressedStart;

		int alignmentValue = Encrypted ? 16 : 1;
		int32_t srcSize = blockSize + alignmentValue - 1 & ~(alignmentValue - 1);

		std::vector<uint8_t> compressed(srcSize);

		startOff = reader.Tell();
		reader.Serialize(compressed.data(), srcSize);

		if (Encrypted) {
			key.DecryptData(compressed.data(), srcSize);
		}

		uint32_t uncompressedSize = std::min(static_cast<uint64_t>(CompressionBlockSize), static_cast<uint64_t>(UncompressedSize - uncompressedOff));
		FCompression::DecompressMemory(compressionMethods[CompressionMethod], uncompressed.data() + uncompressedOff, uncompressedSize, compressed.data(), blockSize);

		/*
		* Finish this later
		* 
		if (FContext::SearchArray != nullptr && FContext::ReplaceArray != nullptr) {
			auto searchOffset = std::find(uncompressed.begin() + uncompressedOff, uncompressed.end(), FContext::SearchArray);

			if (searchOffset != uncompressed.end()) {
				std::copy(searchOffset, uncompressed.end(), FContext::ReplaceArray));

				std::vector<uint8_t> stackCompressedBlock(FCompression::GetMaximumCompressedSize(compressionMethods[CompressionMethod], uncompressedSize));

				int32_t compressedLength = 0;

				FCompression::CompressMemory(compressionMethods[CompressionMethod], uncompressed.data() + uncompressedOff, uncompressedSize, stackCompressedBlock.data(), &compressedLength);

				uint8_t* heapCompressedBlock = new uint8_t[compressedLength];
				memcpy(heapCompressedBlock, stackCompressedBlock.data(), compressedLength);

				FFileModification mod;
				mod.FilePath = path;
				mod.BlockOffset = startOff;
				mod.CompressedBlock = heapCompressedBlock;

				FContext::FileModifications.push_back(mod);
			}
		}*/

		uncompressedOff += CompressionBlockSize;
	}

	return uncompressed;
}
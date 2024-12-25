export module Saturn.Structs.IoStoreTocCompressedBlockEntry;

import Saturn.Readers.FArchive;

import <cstdint>;

export struct FIoStoreTocCompressedBlockEntry {
	static constexpr uint32_t OffsetBits = 40;
	static constexpr uint64_t OffsetMask = (1ull << OffsetBits) - 1ull;
	static constexpr uint32_t SizeBits = 24;
	static constexpr uint32_t SizeMask = (1 << SizeBits) - 1;
	static constexpr uint32_t SizeShift = 8;

	inline uint64_t GetOffset() const {
		const uint64_t* Offset = reinterpret_cast<const uint64_t*>(Data);
		return *Offset & OffsetMask;
	}

	inline void SetOffset(uint64_t InOffset) {
		uint64_t* Offset = reinterpret_cast<uint64_t*>(Data);
		*Offset = InOffset & OffsetMask;
	}

	inline uint32_t GetCompressedSize() const {
		const uint32_t* Size = reinterpret_cast<const uint32_t*>(Data) + 1;
		return (*Size >> SizeShift) & SizeMask;
	}

	inline void SetCompressedSize(uint32_t InSize) {
		uint32_t* Size = reinterpret_cast<uint32_t*>(Data) + 1;
		*Size |= (uint32_t(InSize) << SizeShift);
	}

	inline uint32_t GetUncompressedSize() const {
		const uint32_t* UncompressedSize = reinterpret_cast<const uint32_t*>(Data) + 2;
		return *UncompressedSize & SizeMask;
	}

	inline void SetUncompressedSize(uint32_t InSize) {
		uint32_t* UncompressedSize = reinterpret_cast<uint32_t*>(Data) + 2;
		*UncompressedSize = InSize & SizeMask;
	}

	inline uint8_t GetCompressionMethodIndex() const {
		const uint32_t* Index = reinterpret_cast<const uint32_t*>(Data) + 2;
		return static_cast<uint8_t>(*Index >> SizeBits);
	}

	inline void SetCompressionMethodIndex(uint8_t InIndex) {
		uint32_t* Index = reinterpret_cast<uint32_t*>(Data) + 2;
		*Index |= uint32_t(InIndex) << SizeBits;
	}

	friend FArchive& operator<<(FArchive& Ar, FIoStoreTocCompressedBlockEntry& Value) {
		Ar.Serialize(Value.Data, sizeof(Value.Data));
		return Ar;
	}

	friend FArchive& operator>>(FArchive& Ar, FIoStoreTocCompressedBlockEntry& Value) {
		Ar.WriteBuffer(Value.Data, sizeof(Value.Data));
		return Ar;
	}

private:
	/* 5 bytes offset, 3 bytes for size / uncompressed size and 1 byte for compresseion method. */
	uint8_t Data[5 + 3 + 3 + 1];
};
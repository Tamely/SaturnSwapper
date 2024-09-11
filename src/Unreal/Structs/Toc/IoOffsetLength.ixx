export module Saturn.Structs.IoOffsetLength;

import Saturn.Readers.FArchive;

import <cstdint>;

export struct FIoOffsetAndLength {
	FIoOffsetAndLength() = default;

	explicit FIoOffsetAndLength(const uint64_t Offset, const uint64_t Length)
	{
		SetOffset(Offset);
		SetLength(Length);
	}

	inline uint64_t GetOffset() const
	{
		return OffsetAndLength[4]
			| (uint64_t(OffsetAndLength[3]) << 8)
			| (uint64_t(OffsetAndLength[2]) << 16)
			| (uint64_t(OffsetAndLength[1]) << 24)
			| (uint64_t(OffsetAndLength[0]) << 32)
			;
	}

	inline uint64_t GetLength() const
	{
		return OffsetAndLength[9]
			| (uint64_t(OffsetAndLength[8]) << 8)
			| (uint64_t(OffsetAndLength[7]) << 16)
			| (uint64_t(OffsetAndLength[6]) << 24)
			| (uint64_t(OffsetAndLength[5]) << 32)
			;
	}

	inline void SetOffset(uint64_t Offset)
	{
		OffsetAndLength[0] = uint8_t(Offset >> 32);
		OffsetAndLength[1] = uint8_t(Offset >> 24);
		OffsetAndLength[2] = uint8_t(Offset >> 16);
		OffsetAndLength[3] = uint8_t(Offset >> 8);
		OffsetAndLength[4] = uint8_t(Offset >> 0);
	}

	inline void SetLength(uint64_t Length)
	{
		OffsetAndLength[5] = uint8_t(Length >> 32);
		OffsetAndLength[6] = uint8_t(Length >> 24);
		OffsetAndLength[7] = uint8_t(Length >> 16);
		OffsetAndLength[8] = uint8_t(Length >> 8);
		OffsetAndLength[9] = uint8_t(Length >> 0);
	}

	friend FArchive& operator<<(FArchive& Ar, FIoOffsetAndLength& Value) {
		Ar.Serialize(Value.OffsetAndLength, sizeof(Value.OffsetAndLength));
		return Ar;
	}

	friend FArchive& operator>>(FArchive& Ar, FIoOffsetAndLength& Value) {
		Ar.WriteBuffer(Value.OffsetAndLength, sizeof(Value.OffsetAndLength));
		return Ar;
	}

private:
	// We use 5 bytes for offset and size, this is enough to represent
	// an offset and size of 1PB
	uint8_t OffsetAndLength[5 + 5];
};
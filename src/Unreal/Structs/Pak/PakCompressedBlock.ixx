export module Saturn.Pak.PakCompressedBlock;

import Saturn.Readers.FArchive;

import <cstdint>;

export struct FPakCompressedBlock {
	uint64_t CompressedStart;
	uint64_t CompressedEnd;

	bool operator==(const FPakCompressedBlock& other) const {
		return CompressedStart == other.CompressedStart && CompressedEnd == other.CompressedEnd;
	}

	bool operator!=(const FPakCompressedBlock& other) const {
		return !(*this == other);
	}

	friend FArchive& operator<<(FArchive& Ar, FPakCompressedBlock& Value) {
		Ar << Value.CompressedStart;
		Ar << Value.CompressedEnd;

		return Ar;
	}
};
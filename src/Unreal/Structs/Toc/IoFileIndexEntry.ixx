export module Saturn.Structs.IoFileIndexEntry;

import Saturn.Readers.FArchive;

import <cstdint>;

export struct FIoFileIndexEntry {
    uint32_t Name;
    uint32_t NextFileEntry;
    uint32_t UserData;

	friend __forceinline FArchive& operator<<(FArchive& Ar, FIoFileIndexEntry& value) {
		Ar << value.Name;
		Ar << value.NextFileEntry;
		Ar << value.UserData;

		return Ar;
	}
};
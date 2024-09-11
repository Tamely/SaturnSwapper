export module Saturn.Structs.IoDirectoryIndexEntry;

import Saturn.Readers.FArchive;

import <cstdint>;

export struct FIoDirectoryIndexEntry {
	uint32_t Name;
	uint32_t FirstChildEntry;
	uint32_t NextSiblingEntry;
	uint32_t FirstFileEntry;

	friend __forceinline FArchive& operator<<(FArchive& Ar, FIoDirectoryIndexEntry& value) {
		Ar << value.Name;
		Ar << value.FirstChildEntry;
		Ar << value.NextSiblingEntry;
		Ar << value.FirstFileEntry;

		return Ar;
	}
};
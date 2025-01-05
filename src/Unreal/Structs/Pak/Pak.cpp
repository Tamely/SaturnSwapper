#include <cassert>

import Saturn.Pak.Pak;

import Saturn.Encryption.AES;
import Saturn.Pak.PakFooter;
import Saturn.Readers.FileReaderNoWrite;
import Saturn.Readers.MemoryReader;
import Saturn.Pak.PakFileVersion;

import <fstream>;
import <iostream>;

FPak::FPak(const std::string& path, FAESKey key) : EncryptionKey(key) {
	FFileReaderNoWrite reader(path.c_str());
	FPakFooter footer;

	int64_t length = reader.TotalSize();
	int32_t CompatibleVersion = EPakFileVersion::PakFile_Version_Latest + 1;

	do {
		CompatibleVersion--;

		int64_t footerPos = length - FPakFooter::GetSerializedSize(static_cast<EPakFileVersion>(CompatibleVersion));
		if (!footerPos) continue;

		reader.Seek(footerPos);
		footer = FPakFooter(reader, static_cast<EPakFileVersion>(CompatibleVersion));

		if (footer.Magic == FPakFooter::PAK_FILE_MAGIC) {
			Version = static_cast<EPakFileVersion>(CompatibleVersion);
			Compression = footer.Compression;
			break;
		}

	} while (CompatibleVersion >= EPakFileVersion::PakFile_Version_Initial);

	assert(footer.Magic == FPakFooter::PAK_FILE_MAGIC && "Invalid pak file magic!");

	reader.Seek(footer.IndexOffset);
	std::vector<uint8_t> index(footer.IndexSize);
	reader.Serialize(index.data(), index.size());

	if (footer.Encrypted) {
		if (!EncryptionKey.IsValid()) {
			assert(false && "Pak is encrypted, but no AES key provided!");
		}

		EncryptionKey.DecryptData(index.data(), index.size());
	};

	FMemoryReader indexReader(index);

	indexReader << MountPoint;

	if (MountPoint.back() == '\0') {
		MountPoint.pop_back();
	}

	if (MountPoint.length() > 0 && MountPoint[MountPoint.length() - 1] != '/') {
		MountPoint == "/";
	}

	if (Version >= EPakFileVersion::PakFile_Version_PathHashIndex) {
		// entry count
		indexReader.SeekCur(sizeof(uint32_t));
		// path hash seed
		indexReader.SeekCur(sizeof(uint64_t));
		// path hash
		uint32_t count; // bReaderHasPathHashIndex
		indexReader << count;
		if (count != 0) {
			// offset
			indexReader.SeekCur(sizeof(uint64_t));
			// size
			indexReader.SeekCur(sizeof(uint64_t));
			// hash
			indexReader.SeekCur(sizeof(FSHAHash));
		}

		std::vector<std::pair<std::string, uint32_t>> files;

		indexReader << count;
		if (count != 0) {
			uint64_t offset;
			uint64_t size;

			indexReader << offset;
			indexReader << size;
			indexReader.SeekCur(sizeof(FSHAHash));

			reader.Seek(offset);
			std::vector<uint8_t> fullDir(size);
			reader.Serialize(fullDir.data(), size);

			if (footer.Encrypted) {
				EncryptionKey.DecryptData(fullDir.data(), fullDir.size());
			}

			FMemoryReader fullDirReader(fullDir);

			uint32_t dirCount;
			fullDirReader << dirCount;
			
			for (uint32_t i = 0; i < dirCount; ++i) {
				std::string dir;
				fullDirReader << dir;

				dir.pop_back();

				uint32_t fileCount;
				fullDirReader << fileCount;
				for (uint32_t j = 0; j < fileCount; ++j) {
					std::string file;
					fullDirReader << file;
					file = dir + file;

					file.pop_back();

					uint32_t offset;
					fullDirReader << offset;

					files.push_back({ file, offset });
				}
			}
		}

		uint32_t size;
		indexReader << size;
		std::vector<uint8_t> encoded(size);
		indexReader.Serialize(encoded.data(), size);

		FMemoryReader encodedReader(encoded);
		for (auto& [file, offset] : files) {
			encodedReader.Seek(offset);

			Entries[file] = FPakEntry(encodedReader);
		}
	}

	uint32_t entryCount;
	indexReader << entryCount;

	for (uint32_t i = 0; i < entryCount; ++i) {
		std::string name;
		indexReader << name;

		name.pop_back();

		Entries[name] = FPakEntry(indexReader, Version);
	}
}

export module Saturn.Pak.Pak;

import Saturn.Structs.Guid;
import Saturn.Encryption.AES;
import Saturn.Structs.SHAHash;
import Saturn.Pak.PakEntry;
import Saturn.Pak.PakFooter;
import Saturn.Pak.PakFileVersion;

import <string>;
import <vector>;
import <unordered_map>;

export class FPak {
public:
	FPak(const std::string& path, FAESKey key);
public:
	inline EPakFileVersion GetVersion() {
		return Version;
	}

	inline std::string GetMountPoint() const {
		return MountPoint;
	}

	inline std::unordered_map<std::string, FPakEntry> GetEntries() const {
		return Entries;
	}

	inline std::vector<std::string> GetCompressionMethods() {
		return Compression;
	}

	std::vector<uint8_t> Read(const std::string& entry) const;
private:
	EPakFileVersion Version;
	std::string Path;
	std::string MountPoint;
	std::vector<std::string> Compression;
	FAESKey EncryptionKey;
	std::unordered_map<std::string, FPakEntry> Entries;
};
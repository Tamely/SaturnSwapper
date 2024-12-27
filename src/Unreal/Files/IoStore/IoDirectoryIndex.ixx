export module Saturn.IoStore.IoDirectoryIndex;

import <vector>;
import <string>;
import <cstdint>;
import <functional>;

import Saturn.Core.IoStatus;
import Saturn.Encryption.AES;

import Saturn.Structs.IoFileIndexEntry;
import Saturn.Structs.IoDirectoryIndexEntry;

import Saturn.Readers.FArchive;

export struct FIoDirectoryIndexResource {
    std::string MountPoint;
    std::vector<FIoDirectoryIndexEntry> DirectoryEntries;
    std::vector<FIoFileIndexEntry> FileEntries;
    std::vector<std::string> StringTable;

    friend FArchive& operator<<(FArchive& Ar, FIoDirectoryIndexResource& Entry);
};

export class FIoDirectoryIndexHandle {
    static constexpr uint32_t InvalidHandle = ~uint32_t(0);
    static constexpr uint32_t RootHandle = 0;
public:
    FIoDirectoryIndexHandle() = default;

    inline bool IsValid() const {
        return Handle != InvalidHandle;
    }

    inline bool operator<(FIoDirectoryIndexHandle Other) const {
        return Handle < Other.Handle;
    }

    inline bool operator==(FIoDirectoryIndexHandle Other) const {
        return Handle == Other.Handle;
    }

    inline friend uint32_t GetTypeHash(FIoDirectoryIndexHandle InHandle) {
        return InHandle.Handle;
    }

    inline uint32_t ToIndex() const {
        return Handle;
    }

    static inline FIoDirectoryIndexHandle FromIndex(uint32_t Index) {
        return FIoDirectoryIndexHandle(Index);
    }

    static inline FIoDirectoryIndexHandle RootDirectory() {
        return FIoDirectoryIndexHandle(RootHandle);
    }

    static inline FIoDirectoryIndexHandle Invalid() {
        return FIoDirectoryIndexHandle(InvalidHandle);
    }
private:
    FIoDirectoryIndexHandle(uint32_t InHandle) : Handle(InHandle) {}
    uint32_t Handle = InvalidHandle;
};

export using FDirectoryIndexVisitorFunction = std::function<bool(std::string, const uint32_t)>;

export class FIoDirectoryIndexReader {
public:
    FIoDirectoryIndexReader();
    ~FIoDirectoryIndexReader();
    FIoStatus Initialize(std::vector<uint8_t>& InBuffer, FAESKey InDecryptionKey);

    const std::string GetMountPoint() const;
    FIoDirectoryIndexHandle GetChildDirectory(FIoDirectoryIndexHandle Directory) const;
    FIoDirectoryIndexHandle GetNextDirectory(FIoDirectoryIndexHandle Directory) const;
    FIoDirectoryIndexHandle GetFile(FIoDirectoryIndexHandle Directory) const;
    FIoDirectoryIndexHandle GetNextFile(FIoDirectoryIndexHandle File) const;
    std::string GetDirectoryName(FIoDirectoryIndexHandle Directory) const;
    std::string GetFileName(FIoDirectoryIndexHandle File) const;
    uint32_t GetFileData(FIoDirectoryIndexHandle File) const;

    bool IterateDirectoryIndex(FIoDirectoryIndexHandle Directory, std::string Path, FDirectoryIndexVisitorFunction Visit) const;
private:
    class FIoDirectoryIndexReaderImpl* Impl;
};
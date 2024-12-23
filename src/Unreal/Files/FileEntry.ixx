module;

#include "Saturn/Defines.h"

export module Saturn.Files.FileEntry;

export import Saturn.Files.DiskFile;
import Saturn.Readers.FArchive;
import Saturn.Readers.MemoryReader;

import <string>;
import <vector>;

export struct FFileEntryInfo {
    FFileEntryInfo() {
        Entry.TocIndex = 0;
    }

    FFileEntryInfo(uint32_t InIndex) { Entry.TocIndex = InIndex; }
    FFileEntryInfo(int32_t InIndex) { Entry.PakIndex = InIndex; }

    friend FArchive& operator<<(FArchive& Ar, FFileEntryInfo& Info) {
        return Ar << Info.Entry.PakIndex;
    }

    __forceinline bool IsValid() {
        return Entry.PakIndex;
    }

    __forceinline void SetOwningFile(IDiskFile* DiskFile) {
        AssociatedFile = DiskFile;
    }

    __forceinline std::string GetDiskFilePath() {
        return AssociatedFile->GetDiskPath();
    }

    __forceinline IDiskFile* GetAssociatedFile() {
        return AssociatedFile;
    }

    __forceinline uint32_t GetTocIndex() {
        return Entry.TocIndex;
    }

    __forceinline int32_t GetPakIndex() {
        return Entry.PakIndex;
    }

    __forceinline std::vector<uint8_t> LoadBuffer() {
        return GetAssociatedFile()->ReadEntry(*this);
    }

    __forceinline TSharedPtr<FArchive> CreateReader() {
        auto Buf = LoadBuffer();

        if (Buf.empty()) {
            return nullptr;
        }

        return std::make_shared<FBufferReader>(Buf);
    }
protected:
    union {
        int32_t PakIndex;
        uint32_t TocIndex;
    } Entry;

    IDiskFile* AssociatedFile;
};
import Saturn.IoStore.IoDirectoryIndex;

import Saturn.Core.IoStatus;
import Saturn.Encryption.AES;
import Saturn.Readers.FArchive;
import Saturn.Readers.MemoryReader;
import Saturn.Structs.IoFileIndexEntry;
import Saturn.Structs.IoDirectoryIndexEntry;

import <vector>;
import <cstdint>;

FArchive& operator<<(FArchive& Ar, FIoDirectoryIndexResource& DirectoryIndex) {
    Ar << DirectoryIndex.MountPoint;
    Ar << DirectoryIndex.DirectoryEntries;
    Ar << DirectoryIndex.FileEntries;
    Ar << DirectoryIndex.StringTable;

    return Ar;
}

class FIoDirectoryIndexReaderImpl {
public:
    FIoStatus Initialize(std::vector<uint8_t>& InBuffer, FAESKey InDecryptionKey) {
        if (InBuffer.size() == 0) {
            return FIoStatus::Invalid;
        }

        if (InDecryptionKey.IsValid()) {
            InDecryptionKey.DecryptData(InBuffer.data(), InBuffer.size());
        }

        FMemoryReader Ar(InBuffer);
        Ar << DirectoryIndex;

        return FIoStatus::Ok;
    }

    const std::string GetMountPoint() const {
        return DirectoryIndex.MountPoint;
    }

    FIoDirectoryIndexHandle GetChildDirectory(FIoDirectoryIndexHandle Directory) const {
        return Directory.IsValid() && IsValidIndex()
            ? FIoDirectoryIndexHandle::FromIndex(GetDirectoryEntry(Directory).FirstChildEntry)
            : FIoDirectoryIndexHandle::Invalid();
    }

    FIoDirectoryIndexHandle GetNextDirectory(FIoDirectoryIndexHandle Directory) const {
        return Directory.IsValid() && IsValidIndex()
            ? FIoDirectoryIndexHandle::FromIndex(GetDirectoryEntry(Directory).NextSiblingEntry)
            : FIoDirectoryIndexHandle::Invalid();
    }

    FIoDirectoryIndexHandle GetFile(FIoDirectoryIndexHandle Directory) const {
        return Directory.IsValid() && IsValidIndex()
            ? FIoDirectoryIndexHandle::FromIndex(GetDirectoryEntry(Directory).FirstFileEntry)
            : FIoDirectoryIndexHandle::Invalid();
    }

    FIoDirectoryIndexHandle GetNextFile(FIoDirectoryIndexHandle File) const {
        return File.IsValid() && IsValidIndex()
            ? FIoDirectoryIndexHandle::FromIndex(GetFileEntry(File).NextFileEntry)
            : FIoDirectoryIndexHandle::Invalid();
    }

    std::string GetDirectoryName(FIoDirectoryIndexHandle Directory) const {
        if (Directory.IsValid() && IsValidIndex()) {
            uint32_t NameIndex = GetDirectoryEntry(Directory).Name;
            return DirectoryIndex.StringTable[NameIndex];
        }

        return "";
    }

    std::string GetFileName(FIoDirectoryIndexHandle File) const {
        if (File.IsValid() && IsValidIndex()) {
            uint32_t NameIndex = GetFileEntry(File).Name;
            return DirectoryIndex.StringTable[NameIndex];
        }

        return "";
    }

    uint32_t GetFileData(FIoDirectoryIndexHandle File) const {
        return File.IsValid() && IsValidIndex()
            ? DirectoryIndex.FileEntries[File.ToIndex()].UserData
            : ~uint32_t(0);
    }

    bool IterateDirectoryIndex(FIoDirectoryIndexHandle DirectoryIndexHandle, std::string Path, FDirectoryIndexVisitorFunction Visit) {
        FIoDirectoryIndexHandle File = GetFile(DirectoryIndexHandle);
        while (File.IsValid()) {
            const uint32_t TocEntryIndex = GetFileData(File);
            std::string FilePath;

            std::string MountPoint = GetMountPoint();
            std::string FileName = GetFileName(File);

            MountPoint.pop_back(); // Get rid of null terminator
            FileName.pop_back();

            FilePath.append(MountPoint);
            FilePath.append(Path);
            FilePath.append(FileName);

            if (!Visit(FilePath, TocEntryIndex)) {
                return false;
            }

            File = GetNextFile(File);
        }

        FIoDirectoryIndexHandle ChildDirectory = GetChildDirectory(DirectoryIndexHandle);
        while (ChildDirectory.IsValid()) {
            std::string DirectoryName = GetDirectoryName(ChildDirectory);
            std::string ChildDirectoryPath;

            DirectoryName.pop_back();
            ChildDirectoryPath.append(Path);
            ChildDirectoryPath.append(DirectoryName);
            ChildDirectoryPath.append("/");

            if (!IterateDirectoryIndex(ChildDirectory, ChildDirectoryPath, Visit)) {
                return false;
            }

            ChildDirectory = GetNextDirectory(ChildDirectory);
        }

        return true;
    }
private:
    const FIoDirectoryIndexEntry& GetDirectoryEntry(FIoDirectoryIndexHandle Directory) const {
        return DirectoryIndex.DirectoryEntries[Directory.ToIndex()];
    }

    const FIoFileIndexEntry& GetFileEntry(FIoDirectoryIndexHandle File) const {
        return DirectoryIndex.FileEntries[File.ToIndex()];
    }

    bool IsValidIndex() const {
        return DirectoryIndex.DirectoryEntries.size() > 0;
    }

    FIoDirectoryIndexResource DirectoryIndex;
};

FIoDirectoryIndexReader::FIoDirectoryIndexReader() : Impl(new FIoDirectoryIndexReaderImpl) {}
FIoDirectoryIndexReader::~FIoDirectoryIndexReader() { delete Impl; }

FIoStatus FIoDirectoryIndexReader::Initialize(std::vector<uint8_t>& InBuffer, FAESKey InDecryptionKey) {
    return Impl->Initialize(InBuffer, InDecryptionKey);
}

const std::string FIoDirectoryIndexReader::GetMountPoint() const {
    return Impl->GetMountPoint();
}

FIoDirectoryIndexHandle FIoDirectoryIndexReader::GetChildDirectory(FIoDirectoryIndexHandle Directory) const {
    return Impl->GetChildDirectory(Directory);
}

FIoDirectoryIndexHandle FIoDirectoryIndexReader::GetNextDirectory(FIoDirectoryIndexHandle Directory) const {
    return Impl->GetNextDirectory(Directory);
}

FIoDirectoryIndexHandle FIoDirectoryIndexReader::GetFile(FIoDirectoryIndexHandle Directory) const {
    return Impl->GetFile(Directory);
}

FIoDirectoryIndexHandle FIoDirectoryIndexReader::GetNextFile(FIoDirectoryIndexHandle File) const {
    return Impl->GetNextFile(File);
}

std::string FIoDirectoryIndexReader::GetDirectoryName(FIoDirectoryIndexHandle Directory) const {
    return Impl->GetDirectoryName(Directory);
}

std::string FIoDirectoryIndexReader::GetFileName(FIoDirectoryIndexHandle File) const {
    return Impl->GetFileName(File);
}

uint32_t FIoDirectoryIndexReader::GetFileData(FIoDirectoryIndexHandle File) const {
    return Impl->GetFileData(File);
}

bool FIoDirectoryIndexReader::IterateDirectoryIndex(FIoDirectoryIndexHandle Directory, std::string Path, FDirectoryIndexVisitorFunction Visit) const {
    return Impl->IterateDirectoryIndex(Directory, Path, Visit);
}
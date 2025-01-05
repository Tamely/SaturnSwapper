import Saturn.Readers.FileReaderNoWrite;

#include "Saturn/Log.h"
#include <stdexcept>

import <memory>;
import <string>;

#if defined(_WIN32) || defined(_WIN64)
    #include <Windows.h>
#else
    #include <sys/mman.h>
    #include <fcntl.h>
    #include <unistd.h>
#endif

FFileReaderNoWrite::FFileReaderNoWrite() {}

FFileReaderNoWrite::FFileReaderNoWrite(const char* InFilename) {
    FilePath = InFilename;
        
    openFileForMapping();
}

FFileReaderNoWrite::~FFileReaderNoWrite() {
    closeFileMapping();
}

void FFileReaderNoWrite::Seek(int64_t InPos) {
    if (InPos < 0 || InPos >= TotalSize()) {
        LOG_ERROR("Seek position is out of range for file '{0}'.", FilePath);
        throw std::runtime_error("Seek position is out of range.");
    }
    FilePosition = InPos;
}

int64_t FFileReaderNoWrite::Tell() {
    return FilePosition;
}

int64_t FFileReaderNoWrite::TotalSize() {
    return FileSize;
}

bool FFileReaderNoWrite::Close() {
    closeFileMapping();
    return true;
}

bool FFileReaderNoWrite::Serialize(void* V, int64_t Length) {
    if (FilePosition + Length > FileSize) {
        return false;
    }

    memcpy(V, static_cast<char*>(MappedData) + FilePosition, Length);
    FilePosition += Length;
    return true;
}

bool FFileReaderNoWrite::WriteBuffer(void* V, int64_t Length) {
    // We don't write, so this is left as a no-op
    return false;
}

bool FFileReaderNoWrite::IsValid() {
    return MappedData != nullptr;
}

void FFileReaderNoWrite::openFileForMapping() {
#if defined(_WIN32) || defined(_WIN64)
    // Windows: Create a file mapping object
    hFile = CreateFile(FilePath.c_str(), GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
    if (hFile == INVALID_HANDLE_VALUE) {
        LOG_ERROR("Failed to open file '{0}'.", FilePath);
        throw std::runtime_error("Failed to open file.");
    }

    hMapping = CreateFileMapping(hFile, NULL, PAGE_READONLY, 0, 0, NULL);
    if (hMapping == NULL) {
        LOG_ERROR("Failed to create file mapping for file '{0}'.", FilePath);
        throw std::runtime_error("Failed to create file mapping.");
    }

    MappedData = MapViewOfFile(hMapping, FILE_MAP_READ, 0, 0, 0);
    if (MappedData == NULL) {
        CloseHandle(hMapping);
        CloseHandle(hFile);
        LOG_ERROR("Failed to map view of file '{0}'.", FilePath);
        throw std::runtime_error("failed to map view of file.");
    }

    FileSize = GetFileSize(hFile, NULL);
#else
    // Linux: Use mmap for memory-mapped access
    fd = open(FilePath.c_str(), O_RDONLY);
    if (fd == -1) {
        LOG_ERROR("Failed to open file '{0}'.", FilePath);
        throw std::runtime_error("Failed to open file.");
    }

    struct stat sb;
    if (fstat(fd, &sb) == -1) {
        close(fd);
        LOG_ERROR("Failed to get file size for file '{0}'.", FilePath);
        throw std::runtime_error("Failed to get file size.");
    }

    FileSize = sb.st_size;

    MappedData = mmap(NULL, FileSize, PROT_READ, MAP_SHARED, fd, 0);
    if (MappedData == MAP_FAILED) {
        close(fd);
        LOG_ERROR("Failed to map file '{0}'.", FilePath);
        throw std::runtime_error("Failed to map file.");
    }
#endif
}

void FFileReaderNoWrite::closeFileMapping() {
#if defined(_WIN32) || defined(_WIN64)
    if (MappedData) {
        LOG_INFO("Unmapping view of file '{0}'.", FilePath);
        UnmapViewOfFile(MappedData);
        MappedData = nullptr;
    }
    else {
        LOG_WARN("MappedData is already null for file '{0}'.", FilePath);
    }

    if (hMapping) {
        LOG_INFO("Closing file mapping handle for file '{0}'.", FilePath);
        CloseHandle(hMapping);
        hMapping = nullptr;
    }
    else {
        LOG_WARN("hMapping is already null for file '{0}'.", FilePath);
    }

    if (hFile != INVALID_HANDLE_VALUE) {
        LOG_INFO("Closing file handle for file '{0}'.", FilePath);
        CloseHandle(hFile);
        hFile = INVALID_HANDLE_VALUE;
    }
    else {
        LOG_WARN("hFile is already invalid for file '{0}'.", FilePath);
    }
#else
    if (MappedData != MAP_FAILED) {
        LOG_INFO("Unmapping file '{0}'.", FilePath);
        munmap(MappedData, FileSize);
        MappedData = MAP_FAILED;
    }
    else {
        LOG_WARN("MappedData is already unmapped for file '{0}'.", FilePath);
    }

    if (fd != -1) {
        LOG_INFO("Closing file descriptor for file '{0}'.", FilePath);
        close(fd);
        fd = -1;
    }
    else {
        LOG_WARN("File descriptor is already closed for file '{0}'.", FilePath);
    }
#endif
}

import Saturn.Readers.FileReader;

#include "Saturn/Log.h"
#include <stdexcept>
#include <memory>
#include <string>

#if defined(_WIN32) || defined(_WIN64)
    #include <Windows.h>
#else
    #include <sys/mman.h>
    #include <fcntl.h>
    #include <unistd.h>
#endif

FFileReader::FFileReader() {}

FFileReader::FFileReader(const char* InFilename) {
    FilePath = InFilename;
    openFileForMapping();
}

FFileReader::~FFileReader() {
    closeFileMapping();
}

void FFileReader::Seek(int64_t InPos) {
    if (InPos < 0 || InPos > TotalSize()) {
        LOG_ERROR("Seek position is out of range for file '{0}'.", FilePath);
        throw std::runtime_error("Seek position is out of range.");
    }
    FilePosition = InPos;
}

int64_t FFileReader::Tell() {
    return FilePosition;
}

int64_t FFileReader::TotalSize() {
    return FileSize;
}

bool FFileReader::Close() {
    if (MappedData) {
#if defined(_WIN32) || defined(_WIN64)
        FlushViewOfFile(MappedData, FileSize);
        FlushFileBuffers(hFile);
#else
        msync(MappedData, FileSize, MS_SYNC);
#endif
    }
    closeFileMapping();
    return true;
}

bool FFileReader::Serialize(void* V, int64_t Length) {
    if (FilePosition + Length > FileSize) {
        return false;
    }

    memcpy(V, static_cast<char*>(MappedData) + FilePosition, Length);
    FilePosition += Length;
    return true;
}

bool FFileReader::WriteBuffer(void* V, int64_t Length) {
    if (!CanWrite || !MappedData) {
        return false;
    }

    try {
        if (FilePosition + Length > FileSize) {
            extendFile(FilePosition + Length);
        }

        char* writePos = static_cast<char*>(MappedData) + FilePosition;
        memcpy(writePos, V, Length);

#if defined(_WIN32) || defined(_WIN64)
        if (!FlushViewOfFile(writePos, Length) ||
            !FlushFileBuffers(hFile)) {
            return false;
        }
#else
        if (msync(writePos, Length, MS_SYNC) != 0) {
            return false;
        }
#endif

        FilePosition += Length;
        return true;
    }
    catch (const std::exception&) {
        return false;
    }
}

// Add this method to verify file contents
bool FFileReader::VerifyWrite(int64_t position, int64_t length) {
    LOG_INFO("Verifying write at position {0} with length {1}", position, length);

    // Store current position
    int64_t originalPosition = FilePosition;

    // Seek to verification position
    Seek(position);

    // Create verification buffer
    std::vector<char> verifyBuffer(length);
    bool result = Serialize(verifyBuffer.data(), length);

    if (!result) {
        LOG_ERROR("Failed to read back written data");
        return false;
    }

    // Calculate checksum or first few bytes for logging
    std::string firstBytes;
    for (int i = 0; i < std::min(length, int64_t(16)); i++) {
        char hex[3];
        sprintf(hex, "%02X", (unsigned char)verifyBuffer[i]);
        firstBytes += hex;
        if (i < 15 && i < length - 1) firstBytes += " ";
    }

    LOG_INFO("First bytes read back: {0}", firstBytes);

    // Restore original position
    Seek(originalPosition);

    return true;
}

bool FFileReader::IsValid() {
    return MappedData != nullptr;
}

void FFileReader::openFileForMapping() {
#if defined(_WIN32) || defined(_WIN64)
    hFile = CreateFileA(FilePath.c_str(),
        GENERIC_READ | GENERIC_WRITE,
        FILE_SHARE_READ | FILE_SHARE_WRITE,
        NULL,
        OPEN_ALWAYS,
        FILE_ATTRIBUTE_NORMAL,
        NULL);

    if (hFile == INVALID_HANDLE_VALUE) {
        throw std::runtime_error("Failed to open file.");
    }

    LARGE_INTEGER fileSize;
    if (!GetFileSizeEx(hFile, &fileSize)) {
        CloseHandle(hFile);
        throw std::runtime_error("Failed to get file size.");
    }
    FileSize = fileSize.QuadPart;

    // If it's a new file, we need a non-zero size for mapping
    if (FileSize == 0) {
        LARGE_INTEGER liSize;
        liSize.QuadPart = 1;  // Minimum size needed for mapping

        if (!SetFilePointerEx(hFile, liSize, NULL, FILE_BEGIN) ||
            !SetEndOfFile(hFile)) {
            CloseHandle(hFile);
            throw std::runtime_error("Failed to set initial file size.");
        }
        FileSize = 1;
    }

    hMapping = CreateFileMappingA(hFile,
        NULL,
        PAGE_READWRITE,
        0,
        0,
        NULL);

    if (hMapping == NULL) {
        CloseHandle(hFile);
        throw std::runtime_error("Failed to create file mapping.");
    }

    MappedData = MapViewOfFile(hMapping,
        FILE_MAP_ALL_ACCESS,
        0,
        0,
        0);

    if (MappedData == NULL) {
        CloseHandle(hMapping);
        CloseHandle(hFile);
        throw std::runtime_error("Failed to map view of file.");
    }

#else
    fd = open(FilePath.c_str(), O_RDWR | O_CREAT, 0644);
    if (fd == -1) {
        throw std::runtime_error("Failed to open file.");
    }

    struct stat sb;
    if (fstat(fd, &sb) == -1) {
        close(fd);
        throw std::runtime_error("Failed to get file size.");
    }
    FileSize = sb.st_size;

    // If it's a new file, set initial size
    if (FileSize == 0) {
        if (ftruncate(fd, 1) == -1) {  // Minimum size needed for mapping
            close(fd);
            throw std::runtime_error("Failed to set initial file size.");
        }
        FileSize = 1;
    }

    MappedData = mmap(NULL, FileSize, PROT_READ | PROT_WRITE, MAP_SHARED, fd, 0);
    if (MappedData == MAP_FAILED) {
        close(fd);
        throw std::runtime_error("Failed to map file.");
    }
#endif

    FilePosition = 0;
}

void FFileReader::closeFileMapping() {
#if defined(_WIN32) || defined(_WIN64)
    if (MappedData) {
        LOG_INFO("Unmapping view of file '{0}'.", FilePath);
        UnmapViewOfFile(MappedData);
        MappedData = nullptr;
    }

    if (hMapping) {
        LOG_INFO("Closing file mapping handle for file '{0}'.", FilePath);
        CloseHandle(hMapping);
        hMapping = nullptr;
    }

    if (hFile != INVALID_HANDLE_VALUE) {
        LOG_INFO("Closing file handle for file '{0}'.", FilePath);
        CloseHandle(hFile);
        hFile = INVALID_HANDLE_VALUE;
    }
#else
    if (MappedData != MAP_FAILED) {
        LOG_INFO("Unmapping file '{0}'.", FilePath);
        munmap(MappedData, FileSize);
        MappedData = MAP_FAILED;
    }

    if (fd != -1) {
        LOG_INFO("Closing file descriptor for file '{0}'.", FilePath);
        close(fd);
        fd = -1;
    }
#endif
}

void FFileReader::writeToFile(void* V, int64_t Length) {
#if defined(_WIN32) || defined(_WIN64)
    // Windows: Copy the data to the mapped view
    memcpy(static_cast<char*>(MappedData) + FilePosition, V, Length);
    FilePosition += Length;
#else
    // Linux: Write data to the file by modifying the memory-mapped region
    memcpy(static_cast<char*>(MappedData) + FilePosition, V, Length);
    FilePosition += Length;
#endif
}

void FFileReader::extendFile(int64_t NewSize) {
#if defined(_WIN32) || defined(_WIN64)
    if (MappedData) {
        UnmapViewOfFile(MappedData);
        MappedData = nullptr;
    }
    if (hMapping) {
        CloseHandle(hMapping);
        hMapping = nullptr;
    }

    LARGE_INTEGER liDistanceToMove;
    liDistanceToMove.QuadPart = NewSize;

    if (!SetFilePointerEx(hFile, liDistanceToMove, NULL, FILE_BEGIN) ||
        !SetEndOfFile(hFile)) {
        throw std::runtime_error("Failed to extend file size.");
    }

    hMapping = CreateFileMappingA(
        hFile,
        NULL,
        PAGE_READWRITE,
        (DWORD)(NewSize >> 32),
        (DWORD)(NewSize & 0xFFFFFFFF),
        NULL
    );

    if (hMapping == NULL) {
        throw std::runtime_error("Failed to create new file mapping.");
    }

    MappedData = MapViewOfFile(
        hMapping,
        FILE_MAP_ALL_ACCESS,
        0,
        0,
        0
    );

    if (MappedData == NULL) {
        CloseHandle(hMapping);
        throw std::runtime_error("Failed to map new view.");
    }

#else
    if (MappedData != MAP_FAILED) {
        munmap(MappedData, FileSize);
        MappedData = MAP_FAILED;
    }

    if (ftruncate(fd, NewSize) == -1) {
        throw std::runtime_error("Failed to extend file size.");
    }

    MappedData = mmap(NULL, NewSize, PROT_READ | PROT_WRITE, MAP_SHARED, fd, 0);
    if (MappedData == MAP_FAILED) {
        throw std::runtime_error("Failed to map extended file.");
    }
#endif

    FileSize = NewSize;
}
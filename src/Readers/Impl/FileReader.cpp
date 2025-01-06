import Saturn.Readers.FileReader;

#include "Saturn/Log.h"
#include <stdexcept>
#include <memory>
#include <string>
#include <mutex>
#include <unordered_map>
#include <vector>

#if defined(_WIN32) || defined(_WIN64)
    #include <Windows.h>
#else
    #include <sys/mman.h>
    #include <fcntl.h>
    #include <unistd.h>
#endif

std::mutex FFileReader::RegistryMutex;
std::unordered_map<std::string, std::vector<FFileReader*>> FFileReader::ActiveReaders;

FFileReader::FFileReader() {}

FFileReader::FFileReader(const char* InFilename) {
    FilePath = InFilename;
    openFileForMapping();
    registerReader();
}

FFileReader::~FFileReader() {
    unregisterReader();
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

    auto activeReaders = getActiveReaders(FilePath);

    for (auto reader : activeReaders) {
        if (reader != this) {
            reader->closeFileMapping();
        }
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

        notifyReadersToRemap(activeReaders);

        return true;
    }
    catch (const std::exception&) {
        return false;
    }
}

bool FFileReader::TrimToSize(int64_t newSize, bool force) {
    if (newSize < 0) {
        LOG_ERROR("Cannot trim file '{0}' to a negative size.", FilePath);
        return false;
    }

    if (newSize > FileSize) {
        LOG_ERROR("New size is larger than the current file size for '{0}'.", FilePath);
        return false;
    }

    auto activeReaders = getActiveReaders(FilePath);

    if (activeReaders.size() > 1 && !force) {
        LOG_ERROR("Cannot trim file '{0}' - {1} other readers are still active", 
                  FilePath, activeReaders.size() - 1);
        return false;
    }

    try {
        if (force) {
            LOG_INFO("Force mode enabled. Notifying other readers to close mappings.");
            for (auto reader : activeReaders) {
                if (reader != this) {
                    reader->closeFileMapping();
                }
            }
        }

        Close(); // Close our own mapping

#if defined(_WIN32) || defined(_WIN64)
        HANDLE resizeHandle = CreateFileA(FilePath.c_str(),
            GENERIC_READ | GENERIC_WRITE,
            0,
            NULL,
            OPEN_EXISTING,
            FILE_ATTRIBUTE_NORMAL,
            NULL);

        if (resizeHandle == INVALID_HANDLE_VALUE) {
            LOG_ERROR("Failed to reopen file for trimming: {0}", GetLastError());
            if (force) {
                notifyReadersToRemap(activeReaders);
            }
            throw std::runtime_error("Failed to reopen file for trimming.");
        }

        LARGE_INTEGER liDistanceToMove;
        liDistanceToMove.QuadPart = newSize;

        BOOL success = SetFilePointerEx(resizeHandle, liDistanceToMove, NULL, FILE_BEGIN) &&
                      SetEndOfFile(resizeHandle);
        CloseHandle(resizeHandle);

        if (!success) {
            LOG_ERROR("Failed to resize file: {0}", GetLastError());
            if (force) {
                notifyReadersToRemap(activeReaders);
            }
            throw std::runtime_error("Failed to resize file.");
        }

#else
        int resizeFd = open(FilePath.c_str(), O_RDWR);
        if (resizeFd == -1) {
            if (force) {
                notifyReadersToRemap(activeReaders);
            }
            throw std::runtime_error("Failed to reopen file for trimming.");
        }

        if (ftruncate(resizeFd, newSize) == -1) {
            close(resizeFd);
            if (force) {
                notifyReadersToRemap(activeReaders);
            }
            throw std::runtime_error("Failed to trim file size.");
        }
        close(resizeFd);
#endif

        FileSize = newSize;
        if (FilePosition > FileSize) {
            FilePosition = FileSize;
        }

        openFileForMapping(); // Remap this reader
        if (force) {
            notifyReadersToRemap(activeReaders);
        }

        LOG_INFO("Successfully trimmed file '{0}' to size {1}.", FilePath, newSize);
        return true;
    } catch (const std::exception& e) {
        LOG_ERROR("Failed to trim file '{0}': {1}", FilePath, e.what());
        try {
            openFileForMapping();
            if (force) {
                notifyReadersToRemap(activeReaders);
            }
        } catch (...) {
            LOG_ERROR("Failed to recover file mapping after trim attempt.");
        }
        return false;
    }
}

void FFileReader::notifyReadersToRemap(const std::vector<FFileReader*>& readers) {
    for (auto reader : readers) {
        if (reader != this) {
            reader->openFileForMapping();
        }
    }
    LOG_INFO("Signaled other readers to remap file '{0}'.", FilePath);
}

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

void FFileReader::registerReader() {
    std::lock_guard<std::mutex> lock(RegistryMutex);
    ActiveReaders[FilePath].push_back(this);
    LOG_INFO("Registered reader for file '{0}'. Active readers: {1}", 
             FilePath, ActiveReaders[FilePath].size());
}

void FFileReader::unregisterReader() {
    std::lock_guard<std::mutex> lock(RegistryMutex);
    auto& readers = ActiveReaders[FilePath];
    readers.erase(std::remove(readers.begin(), readers.end(), this), readers.end());
    
    if (readers.empty()) {
        ActiveReaders.erase(FilePath);
        LOG_INFO("Removed last reader for file '{0}'", FilePath);
    } else {
        LOG_INFO("Unregistered reader for file '{0}'. Remaining readers: {1}", 
                 FilePath, readers.size());
    }
}

std::vector<FFileReader*> FFileReader::getActiveReaders(const std::string& filePath) {
    std::lock_guard<std::mutex> lock(RegistryMutex);
    return ActiveReaders[filePath];
}
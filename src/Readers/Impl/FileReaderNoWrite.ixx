export module Saturn.Readers.FileReaderNoWrite;

export import Saturn.Readers.FArchive;
import <string>;

export class FFileReaderNoWrite : public FArchive {
public:
    FFileReaderNoWrite();
    FFileReaderNoWrite(const char* InFilename);
    ~FFileReaderNoWrite();

    void Seek(int64_t InPos);
    int64_t Tell();
    int64_t TotalSize();
    bool Close();
    bool Serialize(void* V, int64_t Length);
    bool WriteBuffer(void* V, int64_t Length);
    bool IsValid();

private:
    void openFileForMapping();
    void closeFileMapping();

    std::string FilePath;
    int64_t FileSize = 0;
    int64_t FilePosition = 0;

#if defined(_WIN32) || defined(_WIN64)
    void* hFile = ((void*)(long long)-1);
    void* hMapping = 0;
    void* MappedData = nullptr;
#else
    int fd = -1;
    void* MappedData = MAP_FAILED;
#endif
friend class FortniteFunctionLibrary;
};
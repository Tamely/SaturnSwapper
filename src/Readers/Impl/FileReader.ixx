export module Saturn.Readers.FileReader;

export import Saturn.Readers.FArchive;
import <fstream>;
import <string>;
import <mutex>;
import <unordered_map>;
import <vector>;

export class FFileReader : public FArchive {
public:
    FFileReader();
    FFileReader(const char* InFilename);
    ~FFileReader();

    void Seek(int64_t InPos);
    int64_t Tell();
    int64_t TotalSize();
    bool Close();
    bool Serialize(void* V, int64_t Length);
    bool WriteBuffer(void* V, int64_t Length);
    bool TrimToSize(int64_t newSize, bool force = false);
    bool VerifyWrite(int64_t position, int64_t length);
    bool IsValid();
private:
    void openFileForMapping();
    void closeFileMapping();
    void writeToFile(void* V, int64_t Length);
    void extendFile(int64_t NewSize);
    void notifyReadersToRemap(const std::vector<FFileReader*>& readers);

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

    // A flag to indicate if writing operations are enabled
    bool CanWrite = true;
private:
    // Static registry for coordinating operations
    static std::mutex RegistryMutex;
    static std::unordered_map<std::string, std::vector<FFileReader*>> ActiveReaders;
        
    // Helper methods for registry
    void registerReader();
    void unregisterReader();
    static std::vector<FFileReader*> getActiveReaders(const std::string& filePath);
};
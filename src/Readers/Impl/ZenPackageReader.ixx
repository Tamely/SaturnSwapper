export module Saturn.Readers.ZenPackageReader;

import <string>;
import <vector>;
import <cstdint>;

import Saturn.Core.IoStatus;
import Saturn.Misc.IoBuffer;
import Saturn.Readers.MemoryReader;
import Saturn.ZenPackage.ZenPackageHeader;

export class FZenPackageReader : public FMemoryReader {
public:
    FZenPackageReader(FIoBuffer& buffer) : FMemoryReader(buffer.GetData(), buffer.GetSize()) {
        std::vector<uint8_t> bufferAsVector(buffer.GetData(), buffer.GetData() + buffer.GetSize());
        Header = FZenPackageHeader::MakeView(bufferAsVector, Error);

        if (!Error.empty()) {
            Status = FIoStatus(EIoErrorCode::ReadError, "Failed reading package header for provided buffer.");
        }
    }

    FZenPackageReader(std::vector<uint8_t>& buffer) : FMemoryReader(buffer) {
        Header = FZenPackageHeader::MakeView(buffer, Error);

        if (!Error.empty()) {
            Status = FIoStatus(EIoErrorCode::ReadError, "Failed reading package header for provided buffer.");
        }
    }

    FZenPackageReader(uint8_t* buffer, size_t bufferLen) : FMemoryReader(buffer, bufferLen) {
        std::vector<uint8_t> bufferAsVector(buffer, buffer + bufferLen);
        Header = FZenPackageHeader::MakeView(bufferAsVector, Error);

        if (!Error.empty()) {
            Status = FIoStatus(EIoErrorCode::ReadError, "Failed reading package header for provided buffer.");
        }
    }

    std::string& GetError();
    FIoStatus& GetStatus();
    bool IsOk();

    uint32_t GetCookedHeaderSize();
    uint32_t GetExportCount();
    class FNameMap& GetNameMap();
    std::wstring& GetPackageName();
    class FZenPackageSummary* GetPackageSummary();
    std::vector<uint64_t>& GetImportedPublicExportHashes();
    std::vector<class FPackageObjectIndex>& GetImportMap();
    std::vector<class FExportMapEntry>& GetExportMap();
    std::vector<class FBulkDataMapEntry>& GetBulkDataMap();
    std::vector<class FExportBundleEntry>& GetExportBundleEntries();
    std::vector<class FDependencyBundleHeader>& GetDependencyBundleHeaders();
    std::vector<class FDependencyBundleEntry>& GetDependencyBundleEntries();
    std::vector<std::wstring>& GetImportedPackageNames();
private:
    std::string Error;
    FIoStatus Status = FIoStatus::Ok;

    FZenPackageHeader Header;
};
export module Saturn.Readers.ZenPackageReader;

import <string>;
import <vector>;
import <cstdint>;

import Saturn.Core.IoStatus;
import Saturn.Misc.IoBuffer;
import Saturn.Readers.MemoryReader;
import Saturn.ZenPackage.ZenPackageHeader;
import Saturn.Unversioned.UnversionedHeader;

export class FZenPackageReader : public FMemoryReader {
public:
    FZenPackageReader(FIoBuffer& buffer) : FMemoryReader(buffer.GetData(), buffer.GetSize()) {
        std::string OutError;
        std::vector<uint8_t> bufferAsVector(buffer.GetData(), buffer.GetData() + buffer.GetSize());
        PackageHeader = FZenPackageHeader::MakeView(bufferAsVector, OutError);

        if (!OutError.empty()) {
            Status = FIoStatus(EIoErrorCode::ReadError, OutError);
        }
    }

    FZenPackageReader(std::vector<uint8_t>& buffer) : FMemoryReader(buffer) {
        std::string OutError;
        PackageHeader = FZenPackageHeader::MakeView(buffer, OutError);

        if (!OutError.empty()) {
            Status = FIoStatus(EIoErrorCode::ReadError, OutError);
        }
    }

    FZenPackageReader(uint8_t* buffer, size_t bufferLen) : FMemoryReader(buffer, bufferLen) {
        std::string OutError;
        std::vector<uint8_t> bufferAsVector(buffer, buffer + bufferLen);
        PackageHeader = FZenPackageHeader::MakeView(bufferAsVector, OutError);

        if (!OutError.empty()) {
            Status = FIoStatus(EIoErrorCode::ReadError, OutError);
        }
    }

    FIoStatus& GetStatus();
    bool IsOk();

    void LoadProperties();

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

    std::vector<class FFragment>& GetFragments();
    std::vector<uint32_t>& GetZeroMask();
private:
    FIoStatus Status = FIoStatus::Ok;

    FZenPackageHeader PackageHeader;
    FUnversionedHeader PropertyHeader;
};
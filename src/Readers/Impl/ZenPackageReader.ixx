module;

#include "Saturn/Defines.h"

export module Saturn.Readers.ZenPackageReader;

import <string>;
import <vector>;
import <cstdint>;

import Saturn.Structs.Name;
import Saturn.Core.UObject;
import Saturn.Core.IoStatus;
import Saturn.Misc.IoBuffer;
import Saturn.Core.GlobalContext;
import Saturn.Readers.MemoryReader;
import Saturn.Asset.PackageObjectIndex;
import Saturn.ZenPackage.ZenPackageHeader;
import Saturn.Unversioned.UnversionedHeader;

template <typename T> struct TCanBulkSerialize { enum { Value = false }; };
template<> struct TCanBulkSerialize<unsigned int> { enum { Value = true }; };
template<> struct TCanBulkSerialize<unsigned short> { enum { Value = true }; };
template<> struct TCanBulkSerialize<int> { enum { Value = true }; };

export struct FExportObject {
    UObjectPtr Object;
    UObjectPtr TemplateObject;
};

export struct FExportState {
    UObjectPtr TargetObject = nullptr;
    std::string TargetObjectName = {};
    bool LoadTargetOnly = false;
};

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

    void MakePackage(TSharedPtr<GlobalContext> Context, FExportState& ExportState);
    void LoadProperties(UStructPtr Struct, UObjectPtr Object);

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

    friend FZenPackageReader& operator<<(FZenPackageReader& Ar, UObjectPtr& Object);
    friend FZenPackageReader& operator<<(FZenPackageReader& Ar, FName& Name);
    friend FZenPackageReader& operator>>(FZenPackageReader& Ar, FName& Name);

    template<typename T>
    friend FZenPackageReader& operator<<(FZenPackageReader& Ar, std::vector<T>& InArray) {
        if constexpr (sizeof(T) == 1 || TCanBulkSerialize<T>::Value) {
            return Ar.BulkSerializeArray(InArray);
        }

        int32_t ArrayNum;
        Ar << ArrayNum;

        if (ArrayNum == 0) {
            InArray.clear();
            return Ar;
        }

        InArray.resize(ArrayNum);

        for (auto i = 0; i < InArray.size(); i++) {
            Ar << InArray[i];
        }

        return Ar;
    }

    template<typename T>
    friend FZenPackageReader& operator>>(FZenPackageReader& Ar, std::vector<T>& InArray) {
        Ar >> InArray.size();

        for (int i = 0; i < InArray.size(); i++) {
            Ar >> InArray[i];
        }

        return Ar;
    }
private:
    FIoStatus Status = FIoStatus::Ok;

    FZenPackageHeader PackageHeader;
    FUnversionedHeader PropertyHeader;

    TSharedPtr<struct FZenPackageData> PackageData;
    TObjectPtr<class UZenPackage> Package;

    friend class UZenPackage;
};


export struct FZenPackageData {
    TObjectPtr<class UZenPackage> Package;
    class FZenPackageReader Reader;
    struct FExportState ExportState;
    FZenPackageHeader Header;
    std::vector<FExportObject> Exports;

    bool HasFlag(uint32_t Flags) {
        return Header.PackageSummary->PackageFlags & Flags;
    }
};
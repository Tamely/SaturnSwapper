import Saturn.Readers.ZenPackageReader;

#include "Saturn/Log.h"
#include "Saturn/Defines.h"

import <string>;
import <vector>;
import <cstdint>;

import Saturn.Core.UObject;
import Saturn.Core.IoStatus;
import Saturn.Structs.Name;
import Saturn.Asset.NameMap;
import Saturn.Misc.IoBuffer;
import Saturn.Asset.PackageIndex;
import Saturn.Readers.MemoryReader;
import Saturn.Asset.ExportMapEntry;
import Saturn.Unversioned.Fragment;
import Saturn.Asset.BulkDataMapEntry;
import Saturn.Asset.ExportBundleEntry;
import Saturn.Asset.PackageObjectIndex;
import Saturn.Asset.DependencyBundleEntry;
import Saturn.ZenPackage.ZenPackageHeader;
import Saturn.Asset.DependencyBundleHeader;
import Saturn.ZenPackage.ZenPackageSummary;
import Saturn.Unversioned.UnversionedHeader;

FIoStatus& FZenPackageReader::GetStatus() {
    return Status;
}

bool FZenPackageReader::IsOk() {
    return Status.IsOk();
}

void FZenPackageReader::LoadProperties(UStructPtr Struct, UObjectPtr Object) {
    Status = PropertyHeader.Load(*this);
    if (!Status.IsOk()) {
        return;
    }

    if (!PropertyHeader.HasNonZeroValues() or !PropertyHeader.HasValues()) {
        Status = FIoStatus(EIoErrorCode::InvalidParameter, "Provided asset either doesn't have NonZero values or doesn't have values at all.");
        return;
    }

    for (FUnversionedIterator It(PropertyHeader, Struct); It; It.Next()) {
        if (!It.IsNonZero()) continue;

        FProperty* Prop = *It;

        LOG_TRACE("Serializing property {0} {1}", Prop->GetName(), (int)Prop->Type);

        TUniquePtr<IPropValue> Value = Prop->Serialize(*this);

        if (!Value) continue;

        Object->PropertyValues.push_back({ Prop->Name, std::move(Value) });
    }
}

uint32_t FZenPackageReader::GetCookedHeaderSize() {
    return PackageHeader.CookedHeaderSize;
}

uint32_t FZenPackageReader::GetExportCount() {
    return PackageHeader.ExportCount;
}

FNameMap& FZenPackageReader::GetNameMap() {
    return PackageHeader.NameMap;
}

std::wstring& FZenPackageReader::GetPackageName() {
    return PackageHeader.PackageName;
}

FZenPackageSummary* FZenPackageReader::GetPackageSummary() {
    return PackageHeader.PackageSummary;
}

std::vector<uint64_t>& FZenPackageReader::GetImportedPublicExportHashes() {
    return PackageHeader.ImportedPublicExportHashes;
}

std::vector<FPackageObjectIndex>& FZenPackageReader::GetImportMap() {
    return PackageHeader.ImportMap;
}

std::vector<FExportMapEntry>& FZenPackageReader::GetExportMap() {
    return PackageHeader.ExportMap;
}

std::vector<FBulkDataMapEntry>& FZenPackageReader::GetBulkDataMap() {
    return PackageHeader.BulkDataMap;
}

std::vector<FExportBundleEntry>& FZenPackageReader::GetExportBundleEntries() {
    return PackageHeader.ExportBundleEntries;
}

std::vector<FDependencyBundleHeader>& FZenPackageReader::GetDependencyBundleHeaders() {
    return PackageHeader.DependencyBundleHeaders;
}

std::vector<FDependencyBundleEntry>& FZenPackageReader::GetDependencyBundleEntries() {
    return PackageHeader.DependencyBundleEntries;
}

std::vector<std::wstring>& FZenPackageReader::GetImportedPackageNames() {
    return PackageHeader.ImportedPackageNames;
}

std::vector<FExportObject>& FZenPackageReader::GetExports() {
    return Exports;
}

template<typename T>
UObjectPtr FZenPackageReader::IndexToObject(int32_t Index) {
    FPackageObjectIndex& Import = PackageHeader.ImportMap[Index];
    if (Import.IsNull()) {
        return {};
    }

    if (Import.IsExport()) {
        return Exports[Import.ToExport()].Object;
    }

    // We actually are not going to do imports yet
    if (Import.IsImport()) {
        if (Import.IsScriptImport()) {

        }
    }

    return {};
}


FZenPackageReader& operator<<(FZenPackageReader& Ar, UObjectPtr& Object) {
    FPackageIndex Index;
    Ar << Index;

    if (Index.IsNull()) {
        Object = UObjectPtr(nullptr);
        return Ar;
    }

    if (Index.IsExport()) {
        int32_t ExportIndex = Index.ToExport();
        if (ExportIndex < Ar.GetExports().size()) {
            Object = Ar.GetExports()[ExportIndex].Object;
        }
        else {
            Ar.Status = FIoStatus(EIoErrorCode::ReadError, "Export index read is not a valid index.");
        }

        return Ar;
    }

    if (Index.IsImport() && Index.ToImport() < Ar.PackageHeader.ImportMap.size()) {
        Object = Ar.IndexToObject(Index.ToImport());
    }
    else {
        Ar.Status = FIoStatus(EIoErrorCode::ReadError, "Bad object import index.");
    }

    return Ar;
}
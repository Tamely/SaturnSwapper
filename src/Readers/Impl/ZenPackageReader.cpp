import Saturn.Readers.ZenPackageReader;

import <string>;
import <vector>;
import <cstdint>;

import Saturn.Core.IoStatus;
import Saturn.Structs.Name;
import Saturn.Asset.NameMap;
import Saturn.Misc.IoBuffer;
import Saturn.Readers.MemoryReader;
import Saturn.Asset.ExportMapEntry;
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

void FZenPackageReader::LoadProperties() {
    Status = PropertyHeader.Load(*this);
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

std::vector<class FFragment>& FZenPackageReader::GetFragments() {
    return PropertyHeader.Fragments;
}

std::vector<uint32_t>& FZenPackageReader::GetZeroMask() {
    return PropertyHeader.ZeroMask;
}

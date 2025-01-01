import Saturn.Readers.ZenPackageReader;

import <string>;
import <vector>;
import <cstdint>;

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

uint32_t FZenPackageReader::GetCookedHeaderSize() {
    return Header.CookedHeaderSize;
}

uint32_t FZenPackageReader::GetExportCount() {
    return Header.ExportCount;
}

FNameMap& FZenPackageReader::GetNameMap() {
    return Header.NameMap;
}

std::wstring& FZenPackageReader::GetPackageName() {
    return Header.PackageName;
}

FZenPackageSummary* FZenPackageReader::GetPackageSummary() {
    return Header.PackageSummary;
}

std::vector<uint64_t>& FZenPackageReader::GetImportedPublicExportHashes() {
    return Header.ImportedPublicExportHashes;
}

std::vector<FPackageObjectIndex>& FZenPackageReader::GetImportMap() {
    return Header.ImportMap;
}

std::vector<FExportMapEntry>& FZenPackageReader::GetExportMap() {
    return Header.ExportMap;
}

std::vector<FBulkDataMapEntry>& FZenPackageReader::GetBulkDataMap() {
    return Header.BulkDataMap;
}

std::vector<FExportBundleEntry>& FZenPackageReader::GetExportBundleEntries() {
    return Header.ExportBundleEntries;
}

std::vector<FDependencyBundleHeader>& FZenPackageReader::GetDependencyBundleHeaders() {
    return Header.DependencyBundleHeaders;
}

std::vector<FDependencyBundleEntry>& FZenPackageReader::GetDependencyBundleEntries() {
    return Header.DependencyBundleEntries;
}

std::vector<std::wstring>& FZenPackageReader::GetImportedPackageNames() {
    return Header.ImportedPackageNames;
}
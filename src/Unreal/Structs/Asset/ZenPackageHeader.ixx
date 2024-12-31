export module Saturn.Asset.ZenPackageHeader;

import <vector>;
import <string>;
import <cstdint>;
import <optional>;

import Saturn.Structs.Name;

import Saturn.Asset.NameMap;
import Saturn.Asset.ExportMapEntry;
import Saturn.Asset.BulkDataMapEntry;
import Saturn.Asset.ExportBundleEntry;
import Saturn.Asset.PackageObjectIndex;
import Saturn.Asset.DependencyBundleEntry;
import Saturn.Asset.DependencyBundleHeader;

export struct FZenPackageHeader {
    uint32_t CookedHeaderSize = 0;
    uint32_t ExportCount = 0; // Need to keep this count around after ExportMap is cleared
    //std::optional<FZenPackageVersioningInfo> VersioningInfo;
    FNameMap NameMap;
    FName PackageName;

    // Backend by IoBuffer
    //const FZenPackageSummary* PackageSummary = nullptr;
    std::vector<uint64_t> ImportedPublicExportHashes;
    std::vector<FPackageObjectIndex> ImportMap;
    std::vector<FExportMapEntry> ExportMap;
    std::vector<FBulkDataMapEntry> BulkDataMap;
    std::vector<FExportBundleEntry> ExportBundleEntries;
    std::vector<FDependencyBundleHeader> DependencyBundleHeders;
    std::vector<FDependencyBundleEntry> DependencyBundleEntries;

    std::vector<FName> ImportedPackageNames;

    static FZenPackageHeader MakeView(std::vector<uint8_t>& Memory);
    static FZenPackageHeader MakeView(std::vector<uint8_t>& Memory, std::string& OutError);
    void Reset();
};
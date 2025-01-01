export module Saturn.ZenPackage.ZenPackageSummary;

import <cstdint>;

import Saturn.Structs.MappedName;

/**
 * Package summary.
 */
export struct FZenPackageSummary {
    uint32_t bHasVersioningInfo;
    uint32_t HeaderSize;
    FMappedName Name;
    uint32_t PackageFlags;
    uint32_t CookedHeaderSize;
    uint32_t ImportedPublicExportHashesOffset;
    uint32_t ImportMapOffset;
    uint32_t ExportMapOffset;
    uint32_t ExportBundleEntriesOffset;
    uint32_t DependencyBundleHeadersOffset;
    uint32_t DependencyBundleEntriesOffset;
    int32_t ImportedPackageNamesOffset;
};
export module Saturn.Asset.DependencyBundleHeader;

import Saturn.Readers.FArchive;
import Saturn.Asset.ExportBundleEntry;

import <cstdint>;

export struct FDependencyBundleHeader {
    int32_t FirstEntryIndex;
    uint32_t EntryCount[FExportBundleEntry::ExportCommandType_Count][FExportBundleEntry::ExportCommandType_Count];

    friend FArchive& operator<<(FArchive& Ar, FDependencyBundleHeader& DependencyBundleHeader);
};
import Saturn.Asset.DependencyBundleHeader;

import Saturn.Readers.FArchive;
import Saturn.Asset.ExportBundleEntry;

import <cstdint>;

FArchive& operator<<(FArchive& Ar, FDependencyBundleHeader& DependencyBundleHeader) {
    Ar << DependencyBundleHeader.FirstEntryIndex;
    for (int32_t i = 0; i < FExportBundleEntry::ExportCommandType_Count; ++i) {
        for (int32_t j = 0; j < FExportBundleEntry::ExportCommandType_Count; ++j) {
            Ar << DependencyBundleHeader.EntryCount[i][j];
        }
    }

    return Ar;
}
import Saturn.Asset.ExportBundleEntry;

import Saturn.Readers.FArchive;

import <cstdint>;

FArchive& operator<<(FArchive& Ar, FExportBundleEntry& ExportBundleEntry) {
    Ar << ExportBundleEntry.LocalExportIndex;
    Ar << ExportBundleEntry.CommandType;

    return Ar;
}
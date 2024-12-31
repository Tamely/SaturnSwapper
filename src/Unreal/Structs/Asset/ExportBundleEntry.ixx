export module Saturn.Asset.ExportBundleEntry;

import <cstdint>;
import Saturn.Readers.FArchive;

export struct FExportBundleEntry {
    enum EExportCommandType {
        ExportCommandType_Create,
        ExportCommandType_Serialize,
        ExportCommandType_Count
    };
    uint32_t LocalExportIndex;
    uint32_t CommandType;

    friend FArchive& operator<<(FArchive& Ar, FExportBundleEntry& ExportBundleEntry);
};
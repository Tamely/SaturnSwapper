export module Saturn.Asset.ExportMapEntry;

import Saturn.Core.UObject;
import Saturn.Readers.FArchive;
import Saturn.Structs.MappedName;
import Saturn.Asset.ExportFilterFlags;
import Saturn.Asset.PackageObjectIndex;

import <cstdint>;

export class FExportMapEntry {
public:
    uint64_t CookedSerialOffset = 0; // Offset from start of exports data (HeaderSize + CookedSerialOffset gives actual offset in iobuffer)
    uint64_t CookedSerialSize = 0;
    FMappedName ObjectName;
    FPackageObjectIndex OuterIndex;
    FPackageObjectIndex ClassIndex;
    FPackageObjectIndex SuperIndex;
    FPackageObjectIndex TemplateIndex;
    uint64_t PublicExportHash;
    UObject::EObjectFlags ObjectFlags = UObject::EObjectFlags::RF_NoFlags;
    EExportFilterFlags FilterFlags = EExportFilterFlags::None;
    uint8_t Pad[3] = {};

    friend FArchive& operator<<(FArchive& Ar, FExportMapEntry& ExportMapEntry);
};
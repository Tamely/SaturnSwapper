import Saturn.Asset.ExportMapEntry;

import Saturn.Core.UObject;
import Saturn.Asset.MappedName;
import Saturn.Readers.FArchive;
import Saturn.Asset.ExportFilterFlags;
import Saturn.Asset.PackageObjectIndex;

import <cstdint>;

FArchive& operator<<(FArchive& Ar, FExportMapEntry& ExportMapEntry) {
    Ar << ExportMapEntry.CookedSerialOffset;
    Ar << ExportMapEntry.CookedSerialOffset;
    Ar << ExportMapEntry.ObjectName;
    Ar << ExportMapEntry.OuterIndex;
    Ar << ExportMapEntry.ClassIndex;
    Ar << ExportMapEntry.SuperIndex;
    Ar << ExportMapEntry.TemplateIndex;
    Ar << ExportMapEntry.PublicExportHash;

    uint32_t ObjectFlags = uint32_t(ExportMapEntry.ObjectFlags);
    Ar << ObjectFlags;

    ExportMapEntry.ObjectFlags = UObject::EObjectFlags(ObjectFlags);

    uint8_t FilterFlags = uint8_t(ExportMapEntry.FilterFlags);
    Ar << FilterFlags;

    ExportMapEntry.FilterFlags = EExportFilterFlags(FilterFlags);

    Ar.Serialize(&ExportMapEntry.Pad, sizeof(ExportMapEntry.Pad));

    return Ar;
}
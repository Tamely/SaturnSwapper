import Saturn.Asset.BulkDataMapEntry;

import Saturn.Readers.FArchive;

FArchive& operator<<(FArchive& Ar, FBulkDataMapEntry& BulkDataEntry) {
    Ar << BulkDataEntry.SerialOffset;
    Ar << BulkDataEntry.DuplicateSerialOffset;
    Ar << BulkDataEntry.SerialSize;
    Ar << BulkDataEntry.Flags;
    Ar << BulkDataEntry.CookedIndex;
    Ar.Serialize(&BulkDataEntry.Pad, 3);

    return Ar;
}
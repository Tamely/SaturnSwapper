export module Saturn.Asset.BulkDataMapEntry; 

import <cstdint>;

import Saturn.Asset.BulkDataCookedIndex;

export struct FBulkDataMapEntry {
    int64_t SerialOffset = 0;
    int64_t DuplicateSerialOffset =0;
    int64_t SerialSize = 00;
    uint32_t Flags = 0;
    FBulkDataCookedIndex CookedIndex;
    uint8_t Pad[3] = { 0, 0, 0 };

    friend FArchive& operator<<(FArchive& Ar, FBulkDataMapEntry& BulkDataEntry);
};
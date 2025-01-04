module;

#include "Saturn/Log.h"

export module Saturn.Structs.InstancedStruct;

import <cstdint>;

import Saturn.Core.UObject;
import Saturn.Readers.ZenPackageReader;

enum class EVersion : uint8_t {
    InitialVersion = 0,
    VersionPlusOne,
    LatestVersion = VersionPlusOne - 1
};

export struct FInstancedStruct {
public:
    FInstancedStruct() = default;

    friend FZenPackageReader& operator<<(FZenPackageReader& Ar, FInstancedStruct& Struct) {
        EVersion Version = EVersion::LatestVersion;

        Ar.Serialize(&Version, sizeof(Version));

        if (Version > EVersion::LatestVersion) {
            return Ar;
        }

        Ar << Struct.ScriptStruct;

        int32_t SerialSize = 0;
        Ar << SerialSize;

        if (!Struct.ScriptStruct and SerialSize > 0) {
            Ar.SeekCur(SerialSize);
        }
        else if (Struct.ScriptStruct) {
            Struct.ScriptStruct->SerializeItem(Ar);
        }

        return Ar;
    }

    friend FZenPackageReader& operator>>(FZenPackageReader& Ar, FInstancedStruct& Struct) {
        LOG_ERROR("Trying to write InstancedStruct while it has not been implemented yet!");

        return Ar;
    }

    UStructPtr ScriptStruct = nullptr;
};
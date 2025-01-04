module;

#include "Unreal/Hash/CityHash.h"

export module Saturn.Files.PackageId;

import <string>;
import <cstdint>;

import Saturn.Readers.FArchive;

export class FPackageId {
    static constexpr uint64_t InvalidId = 0;
    uint64_t Id = InvalidId;

    inline explicit FPackageId(uint64_t InId) : Id(InId) {}
public:
    FPackageId() = default;

    static FPackageId FromName(const std::string& Name) {
        return FPackageId(CityHash64(Name.data(), Name.size()));
    }

    static FPackageId FromValue(const uint64_t Value) {
        return FPackageId(Value);
    }

    inline bool IsValid() const {
        return Id != InvalidId;
    }

    inline uint64_t Value() const {
        return Id;
    }

    inline bool operator<(FPackageId Other) const {
        return Id < Other.Id;
    }

    inline bool operator==(FPackageId Other) const {
        return Id == Other.Id;
    }

    inline bool operator!=(FPackageId Other) const {
        return Id != Other.Id;
    }

    friend size_t hash_value(const FPackageId& In) {
        return uint32_t(In.Id);
    }

    friend FArchive& operator<<(FArchive& Ar, FPackageId& Value) {
        Ar << Value.Id;

        return Ar;
    }
};
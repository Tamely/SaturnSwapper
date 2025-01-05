export module Saturn.Misc.DateTime;

import <cstdint>;

import Saturn.Readers.FArchive;

export struct FDateTime {
    FDateTime() = default;

    uint64_t Ticks;

    friend FArchive& operator<<(FArchive& Ar, FDateTime& DateTime) {
        return Ar << DateTime.Ticks;
    }

    friend FArchive& operator>>(FArchive& Ar, FDateTime& DateTime) {
        return Ar >> DateTime.Ticks;
    }
};
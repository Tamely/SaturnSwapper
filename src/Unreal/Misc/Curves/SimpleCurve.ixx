export module Saturn.Curves.SimpleCurve;

import <cstdint>;

import Saturn.Readers.FArchive;

export struct FSimpleCurveKey {
    FSimpleCurveKey() = default;

    float Time;
    float Value;

    friend FArchive& operator<<(FArchive& Ar, FSimpleCurveKey& Key) {
        Ar << Key.Time;
        Ar << Key.Value;

        return Ar;
    }

    friend FArchive& operator>>(FArchive& Ar, FSimpleCurveKey& Key) {
        Ar >> Key.Time;
        Ar >> Key.Value;

        return Ar;
    }
};
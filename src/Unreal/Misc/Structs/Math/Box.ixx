export module Saturn.Math.Box;

import Saturn.Math.TIntVector;
import Saturn.Readers.FArchive;
export import Saturn.Math.Vector;

import <cstdint>;

export struct FBox {
    FBox() = default;

    FVector Min;
    FVector Max;
    uint8_t IsValid;

    friend FArchive& operator<<(FArchive& Ar, FBox& Box) {
        return Ar << Box.Min << Box.Max << Box.IsValid;
    }

    friend FArchive& operator>>(FArchive& Ar, FBox& Box) {
        return Ar >> Box.Min >> Box.Max >> Box.IsValid;
    }
};

export struct FBox2D {
    FBox2D() = default;

    FVector2D Min;
    FVector2D Max;
    bool bIsValid;

    friend FArchive& operator<<(FArchive& Ar, FBox2D& Box) {
        return Ar << Box.Min << Box.Max << Box.bIsValid;
    }

    friend FArchive& operator>>(FArchive& Ar, FBox2D& Box) {
        return Ar >> Box.Min >> Box.Max >> Box.bIsValid;
    }
};

export template <typename T>
struct TBox2 {
    TBox2() = default;

    TIntVector2<T> Min;
    TIntVector2<T> Max;
    uint8_t bIsValid;

    friend FArchive& operator<<(FArchive& Ar, TBox2& Box) {
        return Ar << Box.Min << Box.Max << Box.bIsValid;
    }

    friend FArchive& operator>>(FArchive& Ar, TBox2& Box) {
        return Ar >> Box.Min >> Box.Max >> Box.bIsValid;
    }
};
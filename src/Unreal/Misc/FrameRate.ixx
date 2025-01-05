export module Saturn.Misc.FrameRate;

import <cstdint>;

import Saturn.Readers.FArchive;

export struct FFrameRate {
    int32_t Numerator;
    int32_t Denominator;

    friend FArchive& operator<<(FArchive& Ar, FFrameRate& FrameRate) {
        return Ar << FrameRate.Numerator << FrameRate.Denominator;
    }

    friend FArchive& operator>>(FArchive& Ar, FFrameRate& FrameRate) {
        return Ar >> FrameRate.Numerator >> FrameRate.Denominator;
    }
};
export module Saturn.Misc.FrameNumber;

import <cstdint>;

import Saturn.Readers.FArchive;

export struct FFrameNumber {
    FFrameNumber() = default;
    FFrameNumber(int value) 
        : Value(value) {}
    FFrameNumber(float value)
        : Value((int)value) {}

    int32_t Value;

    friend FArchive& operator<<(FArchive& Ar, FFrameNumber& Frame) {
        return Ar << Frame.Value;
    }

    friend FArchive& operator>>(FArchive& Ar, FFrameNumber& Frame) {
        return Ar >> Frame.Value;
    }
};
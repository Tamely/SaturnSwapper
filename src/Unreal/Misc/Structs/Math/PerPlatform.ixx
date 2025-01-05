export module Saturn.Math.PerPlatform;

import <string>;

import Saturn.Misc.FrameRate;
import Saturn.Readers.FArchive;

export template<typename T>
struct TPerPlatformProperty {
    T Value;
};

export struct FPerPlatformBool : public TPerPlatformProperty<bool> {
    bool bCooked;
    bool Default;

    FPerPlatformBool() = default;
    FPerPlatformBool(bool InDefaultValue) : Default(InDefaultValue) {}

    friend FArchive& operator<<(FArchive& Ar, FPerPlatformBool& Property) {
        Ar << Property.bCooked;
        Ar << Property.Default;

        return Ar;
    }

    friend FArchive& operator>>(FArchive& Ar, FPerPlatformBool& Property) {
        Ar >> Property.bCooked;
        Ar >> Property.Default;

        return Ar;
    }
};

export struct FPerPlatformFloat : public TPerPlatformProperty<float> {
    bool bCooked;
    float Default;

    FPerPlatformFloat() = default;
    FPerPlatformFloat(float InDefaultValue) : Default(InDefaultValue) {}

    friend FArchive& operator<<(FArchive& Ar, FPerPlatformFloat& Property) {
        Ar << Property.bCooked;
        Ar << Property.Default;

        return Ar;
    }

    friend FArchive& operator>>(FArchive& Ar, FPerPlatformFloat& Property) {
        Ar >> Property.bCooked;
        Ar >> Property.Default;

        return Ar;
    }
};

export struct FPerPlatformInt : public TPerPlatformProperty<int> {
    bool bCooked;
    int Default;

    FPerPlatformInt() = default;
    FPerPlatformInt(int InDefaultValue) : Default(InDefaultValue) {}

    friend FArchive& operator<<(FArchive& Ar, FPerPlatformInt& Property) {
        Ar << Property.bCooked;
        Ar << Property.Default;

        return Ar;
    }

    friend FArchive& operator>>(FArchive& Ar, FPerPlatformInt& Property) {
        Ar >> Property.bCooked;
        Ar >> Property.Default;

        return Ar;
    }
};

export struct FPerPlatformFString : public TPerPlatformProperty<std::string> {
    bool bCooked;
    std::string Default;

    FPerPlatformFString() = default;
    FPerPlatformFString(const std::string& InDefaultValue) : Default(InDefaultValue) {}

    friend FArchive& operator<<(FArchive& Ar, FPerPlatformFString& Property) {
        Ar << Property.bCooked;
        Ar << Property.Default;

        return Ar;
    }

    friend FArchive& operator>>(FArchive& Ar, FPerPlatformFString& Property) {
        Ar >> Property.bCooked;
        Ar >> Property.Default;

        return Ar;
    }
};

export struct FPerPlatformFrameRate : public TPerPlatformProperty<FFrameRate> {
    bool bCooked;
    FFrameRate Default;

    FPerPlatformFrameRate() = default;
    FPerPlatformFrameRate(FFrameRate& InDefaultValue) : Default(InDefaultValue) {}

    friend FArchive& operator<<(FArchive& Ar, FPerPlatformFrameRate& Property) {
        Ar << Property.bCooked;
        Ar << Property.Default;

        return Ar;
    }

    friend FArchive& operator>>(FArchive& Ar, FPerPlatformFrameRate& Property) {
        Ar >> Property.bCooked;
        Ar >> Property.Default;

        return Ar;
    }
};
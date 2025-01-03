export module Saturn.Math.Vector;

import Saturn.Readers.FArchive;

export struct FVector {
    FVector() = default;

    __forceinline FVector(float InX, float InY, float InZ)
        : X(InX), Y(InY), Z(InZ) {}

    float X;
    float Y;
    float Z;

    friend FArchive& operator<<(FArchive& Ar, FVector& Value) {
        Ar << Value.X << Value.Y << Value.Z;

        return Ar;
    }

    friend FArchive& operator>>(FArchive& Ar, FVector& Value) {
        Ar >> Value.X >> Value.Y >> Value.Z;

        return Ar;
    }
};

export struct FVector2D {
    FVector2D() = default;

    __forceinline FVector2D(float InX, float InY)
        : X(InX), Y(InY) {}

    float X;
    float Y;

    friend FArchive& operator<<(FArchive& Ar, FVector2D& Value) {
        Ar << Value.X << Value.Y;

        return Ar;
    }

    friend FArchive& operator>>(FArchive& Ar, FVector2D& Value) {
        Ar >> Value.X >> Value.Y;

        return Ar;
    }
};

export struct FVector4 {
    FVector4() = default;

    __forceinline FVector4(float InX, float InY, float InZ, float InW)
        : X(InX), Y(InY), Z(InZ), W(InW) {}

    float X;
    float Y;
    float Z;
    float W;

    friend FArchive& operator<<(FArchive& Ar, FVector4& Value) {
        Ar << Value.X << Value.Y << Value.Z << Value.W;

        return Ar;
    }

    friend FArchive& operator>>(FArchive& Ar, FVector4& Value) {
        Ar >> Value.X >> Value.Y >> Value.Z >> Value.W;

        return Ar;
    }
};
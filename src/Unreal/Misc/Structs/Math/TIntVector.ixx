export module Saturn.Math.TIntVector;

import Saturn.Readers.FArchive;

export template <typename T>
struct TIntVector2 {
    T X;
    T Y;

    friend FArchive& operator<<(FArchive& Ar, TIntVector2& Vector) {
        return Ar << Vector.X << Vector.Y;
    }

    friend FArchive& operator>>(FArchive& Ar, TIntVector2& Vector) {
        return Ar >> Vector.X >> Vector.Y;
    }
};

export template <typename T>
struct TIntVector3 {
    T X;
    T Y;
    T Z;

    friend FArchive& operator<<(FArchive& Ar, TIntVector3& Vector) {
        return Ar << Vector.X << Vector.Y << Vector.Z;
    }

    friend FArchive& operator>>(FArchive& Ar, TIntVector3& Vector) {
        return Ar >> Vector.X >> Vector.Y << Vector.Z;
    }
};

export template <typename T>
struct TIntVector4 {
    T X;
    T Y;
    T Z;
    T W;

    friend FArchive& operator<<(FArchive& Ar, TIntVector4& Vector) {
        return Ar << Vector.X << Vector.Y << Vector.Z << Vector.W;
    }

    friend FArchive& operator>>(FArchive& Ar, TIntVector4& Vector) {
        return Ar >> Vector.X >> Vector.Y << Vector.Z << Vector.W;
    }
};
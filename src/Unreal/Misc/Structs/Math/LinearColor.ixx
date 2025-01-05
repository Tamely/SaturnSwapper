export module Saturn.Math.LinearColor;

import <cstdint>;

import Saturn.Readers.FArchive;

export struct FLinearColor {
    FLinearColor() = default;

    float R;
    float G;
    float B;
    float A;

    FLinearColor(float b) : FLinearColor(b, b, b, 255) {}
    FLinearColor(float r, float g, float b) : FLinearColor(r, g, b, 255) {}
    FLinearColor(float r, float g, float b, float a) 
        : R(r), G(g), B(b), A(a) {}
    
    friend FArchive& operator<<(FArchive& Ar, FLinearColor& Color) {
        return Ar << Color.R << Color.G << Color.B << Color.A;
    }

    friend FArchive& operator>>(FArchive& Ar, FLinearColor& Color) {
        return Ar >> Color.R >> Color.G >> Color.B >> Color.A;
    }
};
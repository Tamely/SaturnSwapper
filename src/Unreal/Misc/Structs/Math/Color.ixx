export module Saturn.Math.Color;

import <cstdint>;

import Saturn.Readers.FArchive;

export struct FColor {
    FColor() = default;

    uint8_t B;
    uint8_t G;
    uint8_t R;
    uint8_t A;

    FColor(uint8_t b) : FColor(b, b, b, 255) {}
    FColor(uint8_t r, uint8_t g, uint8_t b) : FColor(r, g, b, 255) {}
    FColor(uint8_t r, uint8_t g, uint8_t b, uint8_t a) 
        : R(r), G(g), B(b), A(a) {}
    
    friend FArchive& operator<<(FArchive& Ar, FColor& Color) {
        return Ar << Color.B << Color.G << Color.R << Color.A;
    }

    friend FArchive& operator>>(FArchive& Ar, FColor& Color) {
        return Ar >> Color.B >> Color.G >> Color.R >> Color.A;
    }
};
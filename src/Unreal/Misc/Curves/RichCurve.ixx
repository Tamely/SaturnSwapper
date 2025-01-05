export module Saturn.Curves.RichCurve;

import <cstdint>;

import Saturn.Curves.RealCurve;
import Saturn.Readers.FArchive;

export enum ERichCurveTangentMode : uint8_t {
    RCTM_Auto,
    RCTM_User,
    RCTM_Break,
    RCTM_None,
    RCTM_SmartAuto
};

export enum ERichCurveTangentWeightMode : uint8_t {
    RCTWM_WeightedNone,
    RCTWM_WeightedArrive,
    RCTWM_WeightedLeave,
    RCTWM_WeightedBoth
};

export enum ERichCurveCompressionFormat : uint8_t {
    RCCF_Empty,
    RCCF_Constant,
    RCCF_Linear,
    RCCF_Cubic,
    RCCF_Mixed,
    RCCF_Weighted
};

export enum ERichCurveKeyTimeCompressionFormat : uint8_t {
    RCKTCF_uint16,
    RCKTCF_float32
};

export struct FRichCurveKey {
    FRichCurveKey() = default;

    ERichCurveInterpMode InterpMode;
    ERichCurveTangentMode TangentMode;
    ERichCurveTangentWeightMode TangentWeightMode;

    float Time;
    float Value;
    float ArriveTangent;
    float ArriveTangentWeight;
    float LeaveTangent;
    float LeaveTangentWeight;

    friend FArchive& operator<<(FArchive& Ar, FRichCurveKey& Key) {
        uint8_t InterpModeAsByte;
        Ar << InterpModeAsByte;
        Key.InterpMode = static_cast<ERichCurveInterpMode>(InterpModeAsByte);

        uint8_t TangentModeAsByte;
        Ar << TangentModeAsByte;
        Key.TangentMode = static_cast<ERichCurveTangentMode>(TangentModeAsByte);

        uint8_t TangentWeightModeAsByte;
        Ar << TangentWeightModeAsByte;
        Key.TangentWeightMode = static_cast<ERichCurveTangentWeightMode>(TangentWeightModeAsByte);

        Ar << Key.Time;
        Ar << Key.Value;
        Ar << Key.ArriveTangent;
        Ar << Key.ArriveTangentWeight;
        Ar << Key.LeaveTangent;
        Ar << Key.LeaveTangentWeight;

        return Ar;
    }

    friend FArchive& operator>>(FArchive& Ar, FRichCurveKey& Key) {
        uint8_t InterpModeAsByte = static_cast<uint8_t>(Key.InterpMode);
        Ar >> InterpModeAsByte;

        uint8_t TangentModeAsByte = static_cast<uint8_t>(Key.TangentMode);;
        Ar >> TangentModeAsByte;

        uint8_t TangentWeightModeAsByte = static_cast<uint8_t>(Key.TangentWeightMode);;
        Ar >> TangentWeightModeAsByte;

        Ar >> Key.Time;
        Ar >> Key.Value;
        Ar >> Key.ArriveTangent;
        Ar >> Key.ArriveTangentWeight;
        Ar >> Key.LeaveTangent;
        Ar >> Key.LeaveTangentWeight;

        return Ar;
    }
};
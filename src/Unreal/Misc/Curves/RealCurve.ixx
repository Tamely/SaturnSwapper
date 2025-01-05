export module Saturn.Curves.RealCurve;

import <cstdint>;

export enum ERichCurveInterpMode : uint8_t {
    RCIM_Linear,
    RCIM_Constant,
    RCIM_Cubic,
    RCIM_None
};

export enum ERichCurveExtrapolation : uint8_t {
    RCCE_Cycle,
    RCCE_CycleWithOffset,
    RCCE_Oscillate,
    RCCE_Linear,
    RCCE_Constant,
    RCCE_None
};
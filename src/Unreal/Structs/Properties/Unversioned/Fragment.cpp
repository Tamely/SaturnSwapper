import Saturn.Unversioned.Fragment;

import <cstdint>;

uint16_t FFragment::Pack() const {
    return SkipNum | (uint16_t)(bHasAnyZeroes ? HasZeroMask : 0) | (uint16_t)(ValueNum << ValueNumShift) | (uint16_t)(bIsLast ? IsLastMask : 0);
}

FFragment FFragment::Unpack(uint16_t Int) {
    FFragment Fragment;
    Fragment.SkipNum = static_cast<uint8_t>(Int & SkipNumMask);
    Fragment.bHasAnyZeroes = (Int & HasZeroMask) != 0;
    Fragment.ValueNum = static_cast<uint8_t>(Int >> ValueNumShift);
    Fragment.bIsLast = (Int & IsLastMask) != 0;
    return Fragment;
}
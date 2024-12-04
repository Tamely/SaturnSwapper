export module Saturn.Unversioned.Fragment;

import <cstdint>;

// https://github.com/EpicGames/UnrealEngine/blob/1308e62273a620dd4584b830f6b32cd8200c2ad3/Engine/Source/Runtime/CoreUObject/Private/Serialization/UnversionedPropertySerialization.cpp#L621
export struct FFragment {
    static constexpr uint32_t SkipMax = 127;
    static constexpr uint32_t ValueMax = 127;

    uint8_t SkipNum = 0; // Number of properties to skip before values
    bool bHasAnyZeroes = false;
    uint8_t ValueNum = 0;
    bool bIsLast = 0;

    static constexpr uint32_t SkipNumMask = 0x007fu;
	static constexpr uint32_t HasZeroMask = 0x0080u;
	static constexpr uint32_t ValueNumShift = 9u;
	static constexpr uint32_t IsLastMask  = 0x0100u;
    
    uint16_t Pack() const;
    static FFragment Unpack(uint16_t Int);
};
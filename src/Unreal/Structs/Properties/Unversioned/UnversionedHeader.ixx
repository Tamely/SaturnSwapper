export module Saturn.Unversioned.UnversionedHeader;

import <cstdint>;
import <vector>;

import Saturn.Readers.FArchive;
import Saturn.Unversioned.Fragment;

// https://github.com/EpicGames/UnrealEngine/blob/1308e62273a620dd4584b830f6b32cd8200c2ad3/Engine/Source/Runtime/CoreUObject/Private/Serialization/UnversionedPropertySerialization.cpp#L564
export class FUnversionedHeader {
public:
    void Save(FArchive& Ar) const;
    void Load(FArchive& Ar);
    bool HasValues() const;
    bool HasNonZeroValues() const;
protected:
    void SaveZeroMaskData(FArchive& Ar, uint32_t NumBits, const uint32_t* Data) const;
    void LoadZeroMaskData(FArchive& Ar, uint32_t NumBits, uint32_t* Data);
protected:
    std::vector<FFragment> Fragments;
    bool bHasNonZeroValues = false;
    std::vector<uint32_t> ZeroMask;
};

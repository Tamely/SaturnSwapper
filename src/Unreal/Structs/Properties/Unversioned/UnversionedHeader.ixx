export module Saturn.Unversioned.UnversionedHeader;

import <cstdint>;
import <vector>;

import Saturn.Core.UObject;
import Saturn.Core.IoStatus;
import Saturn.Readers.FArchive;
import Saturn.Unversioned.Fragment;
import Saturn.Reflection.PropertyIterator;

// https://github.com/EpicGames/UnrealEngine/blob/1308e62273a620dd4584b830f6b32cd8200c2ad3/Engine/Source/Runtime/CoreUObject/Private/Serialization/UnversionedPropertySerialization.cpp#L564
export class FUnversionedHeader {
public:
    void Save(FArchive& Ar);
    FIoStatus Load(FArchive& Ar);
    bool HasValues() const;
    bool HasNonZeroValues() const;
protected:
    void SaveZeroMaskData(FArchive& Ar, uint32_t NumBits, const uint32_t* Data) const;
    FIoStatus LoadZeroMaskData(FArchive& Ar, uint32_t NumBits, uint32_t* Data);
protected:
    std::vector<FFragment> Fragments;
    bool bHasNonZeroValues = false;
    std::vector<bool> ZeroMask;

    friend class FUnversionedIterator;
};

export class FUnversionedIterator {
public:
    FUnversionedIterator(const FUnversionedHeader& Header, UStructPtr& Struct);
    void Next();
    explicit operator bool() const;
    bool IsNonZero() const;
    FProperty* operator*();
private:
    FPropertyIterator It;
    const std::vector<bool>& ZeroMask;
    const FFragment* FragmentIt = nullptr;
    bool bDone = false;
    uint32_t ZeroMaskIndex = 0;
    uint32_t RemainingFragmentValues = 0;

    void Skip();
};
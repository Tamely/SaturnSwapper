#include <Saturn/Log.h>

import Saturn.Unversioned.UnversionedHeader;

import Saturn.Core.UObject;
import Saturn.Core.IoStatus;
import Saturn.Readers.FArchive;
import Saturn.Unversioned.Fragment;
import Saturn.Reflection.PropertyIterator;

import <cstdint>;
import <vector>;

void FUnversionedHeader::Save(FArchive& Ar) {
    for (FFragment Fragment : Fragments) {
        uint16_t Packed = Fragment.Pack();
        Ar >> Packed;
    }

    if (ZeroMask.size() > 0) {
        SaveZeroMaskData(Ar, ZeroMask.size(), (uint32_t*)ZeroMask[0]._Getptr());
    }
}

FIoStatus FUnversionedHeader::Load(FArchive& Ar) {
    FFragment Fragment;
    uint32_t ZeroMaskNum = 0;
    uint32_t UnmaskedNum = 0;
    do {
        if (Ar.Tell() + sizeof(uint16_t) > Ar.TotalSize()) return FIoStatus(EIoErrorCode::ReadError, "Hit end of file while reading FUnversionedHeader FFragments.");

        uint16_t Packed;
        Ar << Packed;
        Fragment = FFragment::Unpack(Packed);

        Fragments.push_back(Fragment);

        (Fragment.bHasAnyZeroes ? ZeroMaskNum : UnmaskedNum) += Fragment.ValueNum;
    }
    while (!Fragment.bIsLast);

    if (ZeroMaskNum > 0) {
        ZeroMask.resize(ZeroMaskNum);
        FIoStatus status = LoadZeroMaskData(Ar, ZeroMaskNum, (uint32_t*)ZeroMask[0]._Getptr());
        if (!status.IsOk()) {
            return status;
        }
        bHasNonZeroValues = UnmaskedNum > 0 || std::find(ZeroMask.begin(), ZeroMask.end(), 0) != ZeroMask.end();
    }
    else {
        bHasNonZeroValues = UnmaskedNum > 0;
    }

    return FIoStatus::Ok;
}

bool FUnversionedHeader::HasValues() const {
    return bHasNonZeroValues | (ZeroMask.size() > 0);
}

bool FUnversionedHeader::HasNonZeroValues() const {
    return bHasNonZeroValues;
}

void FUnversionedHeader::SaveZeroMaskData(FArchive& Ar, uint32_t NumBits, const uint32_t* Data) const {
    uint32_t LastWordMask = ~0u >> ((32u - NumBits) % 32u);
    if (NumBits <= 8) {
        uint8_t Word = static_cast<uint8_t>(*Data & LastWordMask);
        Ar >> Word;
    }
    else if (NumBits <= 16) {
        uint16_t Word = static_cast<uint16_t>(*Data & LastWordMask);
        Ar >> Word;
    }
    else {
        // FMath::DivideAndRoundUp alternative
        uint32_t NumWords = (NumBits + 31) / 32;

        for (uint32_t WordIdx = 0; WordIdx < NumWords - 1; ++WordIdx) {
            uint32_t Word = Data[WordIdx];
            Ar >> Word;
        }

        uint32_t LastWord = Data[NumWords - 1] & LastWordMask;
        Ar >> LastWord;
    }
}

FIoStatus FUnversionedHeader::LoadZeroMaskData(FArchive& Ar, uint32_t NumBits, uint32_t* Data) {
    if (NumBits <= 8) {
        if (Ar.Tell() + sizeof(uint8_t) > Ar.TotalSize()) return FIoStatus(EIoErrorCode::ReadError, "Hit end of file while reading FUnversionedHeader ZeroMaskData.");
        uint8_t Int;
        Ar << Int;
        *Data = Int;
    }
    else if (NumBits <= 16) {
        if (Ar.Tell() + sizeof(uint16_t) > Ar.TotalSize()) return FIoStatus(EIoErrorCode::ReadError, "Hit end of file while reading FUnversionedHeader ZeroMaskData.");
        uint16_t Int;
        Ar << Int;
        *Data = Int;
    }
    else {
        for (uint32_t Idx = 0, Num = (NumBits + 31) / 32; Idx < Num; ++Idx) {
            if (Ar.Tell() + sizeof(uint32_t) > Ar.TotalSize()) return FIoStatus(EIoErrorCode::ReadError, "Hit end of file while reading FUnversionedHeader ZeroMaskData.");
            Ar << Data[Idx];
        }
    }

    return FIoStatus::Ok;
}

FUnversionedIterator::FUnversionedIterator(const FUnversionedHeader& Header, UStructPtr& Struct)
    : It(Struct), ZeroMask(Header.ZeroMask), FragmentIt(Header.Fragments.data()), bDone(!Header.HasValues()) {
    if (!bDone) {
        Skip();
    }
}

void FUnversionedIterator::Next() {
    ++It;
    --RemainingFragmentValues;
    ZeroMaskIndex += FragmentIt->bHasAnyZeroes;

    if (RemainingFragmentValues == 0) {
        if (FragmentIt->bIsLast) {
            bDone = true;
        }
        else {
            ++FragmentIt;
            Skip();
        }
    }
}

FUnversionedIterator::operator bool() const {
    return !bDone;
}

bool FUnversionedIterator::IsNonZero() const {
    return !FragmentIt->bHasAnyZeroes || !ZeroMask[ZeroMaskIndex];
}

FProperty* FUnversionedIterator::operator*() {
    return *It;
}

void FUnversionedIterator::Skip() {
    It += FragmentIt->SkipNum;

    while (FragmentIt->ValueNum == 0) {
        ++FragmentIt;
        It += FragmentIt->SkipNum;
    }

    RemainingFragmentValues = FragmentIt->ValueNum;
}
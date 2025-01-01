import Saturn.Unversioned.UnversionedHeader;

import Saturn.Readers.FArchive;
import Saturn.Unversioned.Fragment;

import <cstdint>;
import <vector>;

void FUnversionedHeader::Save(FArchive& Ar) const {
    for (FFragment Fragment : Fragments) {
        uint16_t Packed = Fragment.Pack();
        Ar >> Packed;
    }

    if (ZeroMask.size() > 0) {
        SaveZeroMaskData(Ar, ZeroMask.size(), ZeroMask.data());
    }
}

void FUnversionedHeader::Load(FArchive& Ar) {
    FFragment Fragment;
    uint32_t ZeroMaskNum = 0;
    uint32_t UnmaskedNum = 0;
    do {
        uint16_t Packed;
        Ar << Packed;
        Fragment = FFragment::Unpack(Packed);

        Fragments.push_back(Fragment);

        (Fragment.bHasAnyZeroes ? ZeroMaskNum : UnmaskedNum) += Fragment.ValueNum;
    }
    while (!Fragment.bIsLast);

    if (ZeroMaskNum > 0) {
        ZeroMask.reserve(ZeroMaskNum);
        LoadZeroMaskData(Ar, ZeroMaskNum, ZeroMask.data());
        bHasNonZeroValues = UnmaskedNum > 0 || std::count(ZeroMask.begin(), ZeroMask.end(), 0) != 0;
    }
    else {
        bHasNonZeroValues = UnmaskedNum > 0;
    }
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

void FUnversionedHeader::LoadZeroMaskData(FArchive& Ar, uint32_t NumBits, uint32_t* Data) {
    if (NumBits <= 8) {
        uint8_t Int;
        Ar << Int;
        *Data = Int;
    }
    else if (NumBits <= 16) {
        uint16_t Int;
        Ar << Int;
        *Data = Int;
    }
    else {
        for (uint32_t Idx = 0, Num = (NumBits + 31) / 32; Idx < Num; ++Idx) {
            uint32_t Int;
            Ar << Int;
            *Data = Int;
        }
    }
}
import Saturn.Asset.NameMap;

import <string>;
import <vector>;
import <cstdint>;
import <algorithm>;

#include "Unreal/Hash/CityHash.h"

import Saturn.Structs.Name;
import Saturn.Readers.FArchive;
import Saturn.Structs.MappedName;
import Saturn.Structs.SerializedNameHeader;

bool CanUseSavedHashes(uint64_t HashVersion) {
    return HashVersion == 0xC1640000; // FNameHash::AlgorithmId
}

struct FNameBatchLoader {
    std::vector<uint64_t> Hashes;
    std::vector<FSerializedNameHeader> Headers;
    std::vector<uint8_t> Strings;
    std::vector<uint8_t> Data;

    bool Read(FArchive& Ar) {
        uint32_t Num = 0;
        Ar << Num;

        if (Num == 0) {
            Hashes.clear();
            Headers.clear();
            Strings.clear();
            Data.clear();
            
            return false;
        }

        uint32_t NumStringBytes = 0;
        Ar << NumStringBytes;

        uint64_t HashVersion = 0;
        Ar << HashVersion;
        bool bUseSavedHashes = CanUseSavedHashes(HashVersion);

        // Allocate and load hashes, headers, and string data in one go
        uint32_t NumHashBytes = sizeof(uint64_t) * Num;
        uint32_t NumHeaderBytes = sizeof(FSerializedNameHeader) * Num;
        Data.resize(NumHashBytes + NumHeaderBytes + NumStringBytes);
        Ar.Serialize(Data.data(), Data.size());

        Hashes.resize(Num);
        std::memcpy(Hashes.data(), Data.data(), NumHashBytes);

        Headers.resize(Num);
        std::memcpy(Headers.data(), Data.data() + NumHashBytes, NumHeaderBytes);

        Strings.resize(NumStringBytes);
        std::memcpy(Strings.data(), Data.data() + NumHashBytes + NumHeaderBytes, NumStringBytes);

        if (!bUseSavedHashes) {
            Hashes.clear();
        }

        return true;
    }

    std::vector<std::wstring> Load() {
        std::vector<std::wstring> Out(Headers.size());

        uint32_t Pos = 0;
        for (size_t i = 0; i < Headers.size(); ++i) {
            const FSerializedNameHeader& Header = Headers[i];
            if (Pos + Header.NumBytes() > Strings.size()) break; // Boundary check

            if (Header.IsUtf16()) {
                std::wstring name(reinterpret_cast<wchar_t*>(Strings.data() + Pos), Header.NumBytes());
                Out[i] = name;
            }
            else {
                std::string name(reinterpret_cast<char*>(Strings.data() + Pos), Header.NumBytes());
                Out[i] = std::wstring(name.begin(), name.end());
            }
            Pos += Header.NumBytes();
        }

        return Out;
    }
};

bool CanSafelyCastToString(const std::wstring& wstr) {
    for (wchar_t ch : wstr) {
        if (ch > CHAR_MAX) {
            return false;
        }
    }
    return true;
}

std::vector<std::wstring> FNameMap::LoadNameBatch(FArchive& Ar) {
    FNameBatchLoader Loader;

    if (Loader.Read(Ar)) {
        return Loader.Load();
    }

    return std::vector<std::wstring>();
}

uint32_t FNameMap::GetNameMapStringBytes(const FNameMap& NameMap) {
    uint32_t NumStringBytes = 0;
    for (const auto& wstr : NameMap.NameEntries) {
        NumStringBytes += CanSafelyCastToString(wstr) ? wstr.size() : wstr.size() * sizeof(wchar_t);
    }
    return NumStringBytes;
}

int32_t FNameMap::GetNameMapByteDifference(const FNameMap& First, const FNameMap& Second) {
    uint32_t FirstBytes = GetNameMapStringBytes(First);
    uint32_t SecondBytes = GetNameMapStringBytes(Second);
    return static_cast<int32_t>(FirstBytes) - static_cast<int32_t>(SecondBytes);
}

void FNameMap::Load(FArchive& Ar, FMappedName::EType InNameMapType) {
    NameEntries = LoadNameBatch(Ar);
    NameMapType = InNameMapType;
}

size_t GetTotalStringBytes(const std::vector<std::wstring>& wstrings) {
    size_t totalBytes = 0;

    for (const auto& wstr : wstrings) {
        if (CanSafelyCastToString(wstr)) {
            std::string str(wstr.begin(), wstr.end());
            totalBytes += str.size();
        }
        else {
            totalBytes += wstr.size();
        }
    }

    return totalBytes;
}

void FNameMap::SaveToBuffer(std::vector<uint8_t>& Memory) {
    uint32_t Offset = Memory.size();

    uint32_t Num = NameEntries.size();
    Memory.resize(Memory.size() + sizeof(uint32_t));
    std::memcpy(Memory.data() + Offset, &Num, sizeof(uint32_t));
    Offset += sizeof(uint32_t);

    uint32_t NumStringBytes = GetTotalStringBytes(NameEntries);
    Memory.resize(Memory.size() + sizeof(uint32_t));
    std::memcpy(Memory.data() + Offset, &NumStringBytes, sizeof(uint32_t));
    Offset += sizeof(uint32_t);

    uint64_t HashVersion = 0xC1640000;
    Memory.resize(Memory.size() + sizeof(uint64_t));
    std::memcpy(Memory.data() + Offset, &HashVersion, sizeof(uint64_t));
    Offset += sizeof(uint64_t);

    Memory.resize(Memory.size() + (Num * (sizeof(uint64_t) + sizeof(FSerializedNameHeader))));

    for (size_t i = 0; i < NameEntries.size(); ++i) {
        std::wstring wstr = NameEntries[i];
        std::transform(wstr.begin(), wstr.end(), wstr.begin(), ::towlower);

        uint64_t Hash = CanSafelyCastToString(wstr)
            ? CityHash64(std::string(wstr.begin(), wstr.end()).c_str(), wstr.size())
            : CityHash64((const char*)wstr.c_str(), wstr.size() * sizeof(wchar_t));

        std::memcpy(Memory.data() + Offset, &Hash, sizeof(uint64_t));
        Offset += sizeof(uint64_t);
    }

    for (size_t i = 0; i < NameEntries.size(); ++i) {
        const auto& wstr = NameEntries[i];

        FSerializedNameHeader Header(wstr.size(), !CanSafelyCastToString(wstr));
        std::memcpy(Memory.data() + Offset, &Header, sizeof(FSerializedNameHeader));
        Offset += sizeof(FSerializedNameHeader);
    }

    for (const auto& wstr : NameEntries) {
        if (CanSafelyCastToString(wstr)) {
            std::string str(wstr.begin(), wstr.end());
            Memory.insert(Memory.end(), str.begin(), str.end());
        }
        else {
            Memory.insert(Memory.end(), (uint8_t*)wstr.c_str(), (uint8_t*)wstr.c_str() + (wstr.size() * sizeof(wchar_t)));
        }
    }
}
import Saturn.Asset.NameMap;

import <string>;
import <vector>;
import <cstdint>;

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
            uint32_t Length = Header.Length();

            if (Header.IsUTF16()) {
                if (Pos + Length * 2 > Strings.size()) break; // Boundary check
                std::wstring name(reinterpret_cast<wchar_t*>(Strings.data() + Pos), Length);
                Out[i] = name;
                Pos += Length * 2;
            }
            else {
                if (Pos + Length > Strings.size()) break; // Boundary check
                std::string name(reinterpret_cast<char*>(Strings.data() + Pos), Length);
                Out[i] = std::wstring(name.begin(), name.end());
                Pos += Length;
            }
        }

        return Out;
    }
};

std::vector<std::wstring> FNameMap::LoadNameBatch(FArchive& Ar) {
    FNameBatchLoader Loader;

    if (Loader.Read(Ar)) {
        return Loader.Load();
    }

    return std::vector<std::wstring>();
}

void FNameMap::Load(FArchive& Ar, FMappedName::EType InNameMapType) {
    NameEntries = LoadNameBatch(Ar);
    NameMapType = InNameMapType;
}
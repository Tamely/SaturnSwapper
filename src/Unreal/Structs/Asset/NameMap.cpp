import Saturn.Asset.NameMap;

import <string>;
import <vector>;
import <cstdint>;

import Saturn.Asset.MappedName;
import Saturn.Readers.FArchive;
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

        std::vector<uint64_t> SavedHashes(reinterpret_cast<uint64_t*>(Data.data()), reinterpret_cast<uint64_t*>(Data.data()) + Num);

        Hashes = bUseSavedHashes ? SavedHashes : std::vector<uint64_t>();

        std::vector<FSerializedNameHeader> Headers(
            reinterpret_cast<FSerializedNameHeader*>(SavedHashes.data() + SavedHashes.size()),
            reinterpret_cast<FSerializedNameHeader*>(SavedHashes.data() + SavedHashes.size() + Num)
        );

        std::vector<uint8_t> Strings(
            reinterpret_cast<uint8_t*>(Headers.data() + Headers.size()),
            reinterpret_cast<uint8_t*>(Headers.data() + Headers.size() + NumStringBytes)
        );

        return true;
    }
};

std::vector<std::string> LoadNameBatch(FArchive& Ar) {
    FNameBatchLoader Loader;

    if (Loader.Read(Ar)) {
        //return Loader.Load();
    }

    return std::vector<std::string>();
}

void FNameMap::Load(FArchive& Ar, FMappedName::EType InNameMapType) {
    NameEntries = LoadNameBatch(Ar);
    NameMapType = InNameMapType;
}
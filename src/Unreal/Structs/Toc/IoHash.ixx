export module Saturn.Structs.IoHash;

import Saturn.Readers.FArchive;
import <cstdint>;

export struct FIoHash {
public:
    friend unsigned __int32 hash_value(const FIoHash& InHash) {
        uint32_t Result = 5381;
        for (int i = 0; i < sizeof InHash.Hash; ++i) {
            Result = Result * 33 + InHash.Hash[i];
        }
        return Result;
    }

    inline friend FArchive& operator<<(FArchive& Ar, FIoHash& ChunkHash) {
        Ar.Serialize(&ChunkHash.Hash, sizeof(ChunkHash.Hash));
        return Ar;
    }

    inline bool operator==(const FIoHash& Rhs) const {
        return memcmp(Hash, Rhs.Hash, sizeof Hash) == 0;
    }

    inline bool operator!=(const FIoHash& Rhs) const {
        return !(*this == Rhs);
    }

    inline uint8_t* GetBytes() {
        return Hash;
    }
private:
    uint8_t Hash[32];
};
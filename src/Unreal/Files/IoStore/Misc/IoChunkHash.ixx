module;

#include "Saturn/Defines.h"

export module Saturn.Structs.IoChunkHash;

import Saturn.Readers.FArchive;
import Saturn.Structs.IoHash;
import Saturn.Core.StringTils;

import <cstdint>;
import <string>;

export class FIoChunkHash {
public:
    friend uint32_t GetTypeHash(const FIoChunkHash& InChunkHash) {
        uint32_t Result = 5381;
        for (int i = 0; i < sizeof Hash; ++i) {
            Result = Result * 33 + InChunkHash.Hash[i];
        }
        return Result;
    }

    friend FArchive& operator<<(FArchive& Ar, FIoChunkHash& ChunkHash) {
        Ar.Serialize(&ChunkHash.Hash, sizeof Hash);
        return Ar;
    }

    inline bool operator==(const FIoChunkHash& Rhs) const {
        return memcmp(Hash, Rhs.Hash, sizeof Hash) == 0;
    }

    inline bool operator!=(const FIoChunkHash& Rhs) const {
        return !(*this == Rhs);
    }

    inline std::string ToString() const {
        return FStringTils::BytesToHex(Hash, 20);
    }

    FIoHash ToIoHash() const {
        FIoHash IoHash;
        memcpy(IoHash.GetBytes(), Hash, sizeof(IoHash));
        return IoHash; 
    }

    static FIoChunkHash CreateFromIoHash(const FIoHash& IoHash) {
        FIoChunkHash Result;
        memcpy(Result.Hash, &IoHash, sizeof IoHash);
        memset(Result.Hash + 20, 0, 12);
        return Result;
    }
private:
    uint8_t Hash[32];
};
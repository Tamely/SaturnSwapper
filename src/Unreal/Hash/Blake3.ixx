module;

#include "Saturn/Defines.h"

export module Saturn.Hash.Blake3;

import <string>;
import <cstdint>;

import Saturn.Readers.FArchive;

export struct FBlake3Hash {
public:
    using ByteArray = uint8_t[32];

    FBlake3Hash() = default;

    inline explicit FBlake3Hash(const ByteArray& Hash);

    inline explicit FBlake3Hash(const char* HexHash);
    inline explicit FBlake3Hash(const std::string& HexHash);

    inline void Reset() { *this = FBlake3Hash(); }
    
    inline bool IsZero() const;

    inline ByteArray& GetBytes() { return Hash; }
    inline const ByteArray& GetBytes() const { return Hash; }

    static const FBlake3Hash Zero;

    inline bool operator==(const FBlake3Hash& B) const {
        return memcmp(GetBytes(), B.GetBytes(), sizeof(decltype(GetBytes()))) == 0;
    }

    inline bool operator!=(const FBlake3Hash& B) const {
        return memcmp(GetBytes(), B.GetBytes(), sizeof(decltype(GetBytes()))) != 0;
    }

    inline bool operator<(const FBlake3Hash& B) const {
        return memcmp(GetBytes(), B.GetBytes(), sizeof(decltype(GetBytes()))) < 0;
    }

    friend inline FArchive& operator<<(FArchive& Ar, FBlake3Hash& Value) {
        Ar.Serialize(Value.GetBytes(), sizeof(decltype(Value.GetBytes())));
        return Ar;
    }

    friend inline uint32_t GetTypeHash(const FBlake3Hash& Value) {
        return *reinterpret_cast<const uint32_t*>(Value.GetBytes());
    }
private:
    alignas(uint32_t) ByteArray Hash{};
};

export class FBlake3 {
public:
    inline FBlake3() { Reset(); }

    FBlake3(const FBlake3&) = delete;
    FBlake3& operator=(const FBlake3&) = delete;

    void Reset();

    void Update(const void* Data, uint64_t Size);

    FBlake3Hash Finalize() const;

    static FBlake3Hash HashBuffer(const void* Data, uint64_t Size);
private:
    TAlignedBytes<1912, 8> HasherBytes;
};
export module Saturn.Structs.IoHash;

import Saturn.Hash.Blake3;

import Saturn.Readers.FArchive;

import <cstdint>;
import <string>;

export struct FIoHash {
public:
    using ByteArray = uint8_t[20];

    FIoHash() = default;

    inline explicit FIoHash(const ByteArray& Hash);

    inline FIoHash(const FBlake3Hash& Hash);

    inline explicit FIoHash(const std::string& HexHash);

    inline void Reset() { *this = FIoHash(); }

    inline bool IsZero() const;

    inline ByteArray& GetBytes() { return Hash; }
    inline const ByteArray& GetBytes() const { return Hash; }

    static inline FIoHash HashBuffer(const void* Data, uint64_t Size);

    static const FIoHash Zero;
private:
    alignas(uint32_t) ByteArray Hash{};

    friend inline bool operator==(const FIoHash& A, const FIoHash& B) {
        return memcmp(A.GetBytes(), B.GetBytes(), sizeof(decltype(A.GetBytes()))) == 0;
    }

    friend inline bool operator!=(const FIoHash& A, const FIoHash& B) {
        return memcmp(A.GetBytes(), B.GetBytes(), sizeof(decltype(A.GetBytes()))) != 0;
    }

    friend inline bool operator<(const FIoHash& A, const FIoHash& B) {
        return memcmp(A.GetBytes(), B.GetBytes(), sizeof(decltype(A.GetBytes()))) < 0;
    }

    friend inline uint32_t GetTypeHash(const FIoHash& Value) {
        return *reinterpret_cast<const uint32_t*>(Value.GetBytes());
    }

    friend inline FArchive& operator<<(FArchive& Ar, FIoHash& InHash) {
        Ar.Serialize(InHash.GetBytes(), sizeof(decltype(InHash.GetBytes())));
        return Ar;
    }
};

export class FIoHashBuilder : public FBlake3 {
public:
    [[nodiscard]] inline FIoHash Finalize() const { return FBlake3::Finalize(); }
    
    [[nodiscard]] inline static FIoHash HashBuffer(const void* Data, uint64_t Size) { return FBlake3::HashBuffer(Data, Size); }
};
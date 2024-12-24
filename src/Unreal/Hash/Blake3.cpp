import Saturn.Hash.Blake3;

#include "Saturn/Defines.h"

#include <blake3/blake3.h>

import <string>;

import Saturn.Core.StringTils;

const FBlake3Hash FBlake3Hash::Zero;

FBlake3Hash::FBlake3Hash(const ByteArray& InHash) {
    memcpy(Hash, InHash, sizeof(ByteArray));
}

FBlake3Hash::FBlake3Hash(const char* HexHash) {
    FStringTils::HexToBytes(HexHash, Hash);
}

FBlake3Hash::FBlake3Hash(const std::string& HexHash) {
    FStringTils::HexToBytes(HexHash, Hash);
}

bool FBlake3Hash::IsZero() const {
    using UInt32Array = uint32_t[8];

    for (uint32_t Value : reinterpret_cast<const UInt32Array&>(Hash)) {
        if (Value != 0) {
            return false;
        }
    }

    return true;
}

void FBlake3::Reset() {
    blake3_hasher& Hasher = reinterpret_cast<blake3_hasher&>(HasherBytes);
    blake3_hasher_init(&Hasher);
}

void FBlake3::Update(const void* Data, uint64_t Size) {
    blake3_hasher& Hasher = reinterpret_cast<blake3_hasher&>(HasherBytes);
    blake3_hasher_update(&Hasher, Data, Size);
}

FBlake3Hash FBlake3::Finalize() const {
    FBlake3Hash Hash;
    FBlake3Hash::ByteArray& Output = Hash.GetBytes();

    const blake3_hasher& Hasher = reinterpret_cast<const blake3_hasher&>(HasherBytes);
    blake3_hasher_finalize(&Hasher, Output, BLAKE3_OUT_LEN);
    return Hash;
}

FBlake3Hash FBlake3::HashBuffer(const void* Data, uint64_t Size) {
    FBlake3 Hash;
    Hash.Update(Data, Size);
    return Hash.Finalize();
}
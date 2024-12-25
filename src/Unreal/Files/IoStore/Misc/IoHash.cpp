import Saturn.Structs.IoHash;

import <string>;
import <memory>;
import <cstdint>;

import Saturn.Hash.Blake3;
import Saturn.Core.StringTils;

const FIoHash FIoHash::Zero;

FIoHash::FIoHash(const ByteArray& InHash) {
    memcpy(Hash, InHash, sizeof(ByteArray));
}

FIoHash::FIoHash(const FBlake3Hash& InHash) {
    memcpy(Hash, InHash.GetBytes(), sizeof(ByteArray));
}

FIoHash::FIoHash(const std::string& HexHash) {
    FStringTils::HexToBytes(HexHash, Hash);
}

bool FIoHash::IsZero() const {
    using UInt32Array = uint32_t[5];
    for (uint32_t Value : reinterpret_cast<const UInt32Array&>(Hash)) {
        if (Value != 0) {
            return false;
        }
    }
    return true;
}

FIoHash FIoHash::HashBuffer(const void* Data, uint64_t Size) {
    return FBlake3::HashBuffer(Data, Size);
}
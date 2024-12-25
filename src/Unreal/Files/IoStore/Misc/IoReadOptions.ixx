module;

#include "Saturn/Defines.h"

export module Saturn.Misc.IoReadOptions;

import <cstdint>;

export enum class EIoReadOptionsFlags : uint32_t {
    None = 0,
    /**
	 * Use this flag to inform the decompressor that the memory is uncached or write-combined and therefore the usage of staging might be needed if reading directly from the original memory
	 */
	HardwareTargetBuffer = 1 << 0,
};
ENUM_CLASS_FLAGS(EIoReadOptionsFlags);

export class FIoReadOptions {
public:
    FIoReadOptions() = default;

    FIoReadOptions(uint64_t InOffset, uint64_t InSize) : RequestedOffset(InOffset), RequestedSize(InSize) {}
    FIoReadOptions(uint64_t InOffset, uint64_t InSize, void* InTargetVa) : RequestedOffset(InOffset), RequestedSize(InSize), TargetVa(InTargetVa) {}
    FIoReadOptions(uint64_t InOffset, uint64_t InSize, void* InTargetVa, EIoReadOptionsFlags InFlags) : RequestedOffset(InOffset), RequestedSize(InSize), TargetVa(InTargetVa), Flags(InFlags) {}

    ~FIoReadOptions() = default;

    void SetRange(uint64_t Offset, uint64_t Size) {
        RequestedOffset = Offset;
        RequestedSize = Size;
    }

    void SetTargetVa(void* InTargetVa) {
        TargetVa = InTargetVa;
    }

    void SetFlags(EIoReadOptionsFlags InValue) {
        Flags = InValue;
    }

    uint64_t GetOffset() const {
        return RequestedOffset;
    }

    uint64_t GetSize() const {
        return RequestedSize;
    }

    void* GetTargetVa() const {
        return TargetVa;
    }

    EIoReadOptionsFlags GetFlags() const {
        return Flags;
    }
private:
    uint64_t RequestedOffset = 0;
    uint64_t RequestedSize = ~uint64_t(0);
    void* TargetVa = nullptr;
    EIoReadOptionsFlags Flags = EIoReadOptionsFlags::None;
};
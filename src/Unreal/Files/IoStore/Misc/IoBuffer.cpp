#include "Saturn/Defines.h"

import Saturn.Misc.IoBuffer;
import Saturn.Core.IoStatus;

import <cstdint>;

FIoBuffer::BufCore::BufCore() {}
FIoBuffer::BufCore::~BufCore() {
    if (IsMemoryOwned()) {
        delete[] Data();
    }
}

FIoBuffer::BufCore::BufCore(const uint8_t* InData, uint64_t InSize, bool InOwnsMemory) {
    SetDataAndSize(InData, InSize);
    SetIsOwned(InOwnsMemory);
}

FIoBuffer::BufCore::BufCore(const uint8_t* InData, uint64_t InSize, TSharedPtr<BufCore> InOuter) : OuterCore(InOuter) {
    SetDataAndSize(InData, InSize);
}

FIoBuffer::BufCore::BufCore(uint64_t InSize) {
    uint8_t* NewBuffer = reinterpret_cast<uint8_t*>(malloc(InSize));
    SetDataAndSize(NewBuffer, InSize);
    SetIsOwned(true);
}

FIoBuffer::BufCore::BufCore(ECloneTag, uint8_t* InData, uint64_t InSize) : FIoBuffer::BufCore(InSize) {
    memcpy(Data(), InData, InSize);
}

void FIoBuffer::BufCore::CheckRefCount() const {
    // Verify that Release() is not being called on an object which is already at aa zero refcount
}

void FIoBuffer::BufCore::SetDataAndSize(const uint8_t* InData, uint64_t InSize) {
    // This is intentionally not split into SetData anad SetSize to enabble different storage
    // strategies for flags in the future (in unused pointer bits)

    DataPtr = const_cast<uint8_t*>(InData);
    DataSizeLow = uint32_t(InSize & 0xffffffffu);
    DataSizeHigh = (InSize >> 32) & 0xffu;
}

void FIoBuffer::BufCore::SetSize(uint64_t InSize) {
    SetDataAndSize(Data(), InSize);
}

void FIoBuffer::BufCore::MakeOwned() {
    if (IsMemoryOwned()) 
        return;

    const uint64_t BufferSize = DataSize();
    uint8_t* NewBuffer = reinterpret_cast<uint8_t*>(malloc(BufferSize));

    memcpy(NewBuffer, Data(), BufferSize);
    
    SetDataAndSize(NewBuffer, BufferSize);

    SetIsOwned(true);
}

FIoStatus FIoBuffer::BufCore::ReleaseMemory(uint8_t* OutBuffer) {
    if (IsMemoryOwned()) {
        OutBuffer = Data();
        SetDataAndSize(nullptr, 0);
        ClearFlags();

        return FIoStatus::Ok;
    }
    else {
        return FIoStatus(EIoErrorCode::InvalidParameter, "Cannot call release on a FIoBuffer unless it owns its memory");
    }
}

FIoBuffer::FIoBuffer() : CorePtr(new BufCore) {}
FIoBuffer::FIoBuffer(uint64_t InSize) : CorePtr(new BufCore(InSize)) {}
FIoBuffer::FIoBuffer(const void* Data, uint64_t InSize, const FIoBuffer& OuterBuffer) : CorePtr(new BufCore((uint8_t*)Data, InSize, OuterBuffer.CorePtr)) {}

FIoBuffer::FIoBuffer(FIoBuffer::EWrapTag, const void* Data, uint64_t InSize) : CorePtr(new BufCore((uint8_t*)Data, InSize, /* ownership */ false)) {}
FIoBuffer::FIoBuffer(FIoBuffer::EAssumeOwnershipTag, const void* Data, uint64_t InSize) : CorePtr(new BufCore((uint8_t*)Data, InSize, /* ownership */ true)) {}
FIoBuffer::FIoBuffer(FIoBuffer::ECloneTag, const void* Data, uint64_t InSize) : CorePtr(new BufCore(Clone, (uint8_t*)Data, InSize)) {}

void FIoBuffer::MakeOwned() const {
    CorePtr->MakeOwned();
}

FIoStatus FIoBuffer::Release(uint8_t* OutBuffer) {
    return CorePtr->ReleaseMemory(OutBuffer);
}
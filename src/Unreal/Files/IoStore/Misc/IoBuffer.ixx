module;

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include "Saturn/Defines.h"

export module Saturn.Misc.IoBuffer;

import <cstdint>;
import <memory>;

import Saturn.Core.IoStatus;

export class FIoBuffer {
public:
    enum EAssumeOwnershipTag { AssumeOwnership };
    enum ECloneTag { Clone };
    enum EWrapTag { Wrap };

    FIoBuffer();
    explicit FIoBuffer(uint64_t InSize);
    FIoBuffer(const void* Data, uint64_t InSize, const FIoBuffer& OuterBuffer);

    FIoBuffer(EAssumeOwnershipTag, const void* Data, uint64_t InSize);
    FIoBuffer(ECloneTag, const void* Data, uint64_t InSize);
    FIoBuffer(EWrapTag, const void* Data, uint64_t InSize);

    // Note: we currently rely on implicit move constructor, thus we do not declare any
    //       destructor or copy/assignment operators or copy constructors

    inline const uint8_t* Data() const { return CorePtr->Data(); }
    inline uint8_t* Data() { return CorePtr->Data(); }
    inline const uint8_t* GetData() const { return CorePtr->Data(); }
    inline uint8_t* GetData() { return CorePtr->Data(); }
    inline uint64_t DataSize() const { return CorePtr->DataSize(); }
    inline uint64_t GetSize() const { return CorePtr->DataSize(); }

    inline void SetSize(uint64_t InSize) { return CorePtr->SetSize(InSize); }

    inline bool IsMemoryOwned() const { return CorePtr->IsMemoryOwned(); }

    inline void EnsureOwned() const { if (!CorePtr->IsMemoryOwned()) { MakeOwned(); } }

    inline bool operator!=(const FIoBuffer& Rhs) const { return DataSize() != Rhs.DataSize() || memcmp(GetData(), Rhs.GetData(), DataSize()) != 0; }

    void MakeOwned() const;

    FIoStatus Release(uint8_t* OutBuffer);
private:
    // Core buffer object. For internal use only, used by FIoBuffer
    // Contains all state pertaining to a buffer
    struct BufCore {
        BufCore();
        ~BufCore();

        explicit BufCore(uint64_t InSize);
        BufCore(const uint8_t* InData, uint64_t InSize, bool InOwnsMemory);
        BufCore(const uint8_t* InData, uint64_t InSize, TSharedPtr<BufCore> InOuter);
        BufCore(ECloneTag, uint8_t* InData, uint64_t InSize);

        BufCore(const BufCore& Rhs) = delete;

        BufCore& operator=(const BufCore& Rhs) = delete;

        inline uint8_t* Data() { return DataPtr; }
        inline uint64_t DataSize() const { return DataSizeLow | (uint64_t(DataSizeHigh) << 32); }

        void SetDataAndSize(const uint8_t* InData, uint64_t InSize);
        void SetSize(uint64_t InSize);

        void MakeOwned();

        FIoStatus ReleaseMemory(uint8_t* OutBuffer);

        inline void SetIsOwned(bool InOwnsMemory) {
            if (InOwnsMemory) {
                Flags |= OwnsMemory;
            }
            else {
                Flags &= ~OwnsMemory;
            }
        }

        inline uint32_t AddRef() const {
            return static_cast<uint32_t>(InterlockedIncrement(&NumRefs));
        }

        inline uint32_t Release() const {
            CheckRefCount();
            const int32_t Refs = static_cast<uint32_t>(InterlockedDecrement(&NumRefs));
            if (Refs == 0) {
                delete this;
            }

            return uint32_t(Refs);
        }

        uint32_t GetRefCount() const {
            return uint32_t(NumRefs);
        }

        bool IsMemoryOwned() const { return Flags & OwnsMemory; }
    private:
        void CheckRefCount() const;

        uint8_t* DataPtr = nullptr;

        uint32_t DataSizeLow = 0;
        mutable LONG NumRefs = 0;

        // Reference-counted outer "core", used for views into other buffer
        // Ultimately this should probably just be an index into a pool
        TSharedPtr<const BufCore> OuterCore;

        // TODO: These two should be packed in the MSBB of DataPtr on x64
        uint8_t DataSizeHigh = 0; // High 8 bits of size (40 bits in total)
        uint8_t Flags = 0;

        enum {
            OwnsMemory = 1 << 0, // Buffer memory is owned by this instance
            ReadOnlyBuffer = 1 << 1, // Buffer memory is immutable
            FlagsMask = (1 << 2) - 1
        };

        void EnsureDataIsResident() {}
        void ClearFlags() {
            Flags = 0;
        }
    };

    // Reference-counted "core"
    // Ultimately this should probably just be an index into a pool
    TSharedPtr<BufCore> CorePtr;
};
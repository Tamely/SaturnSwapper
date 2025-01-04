module;

#include "Saturn/Defines.h"

export module Saturn.Localization.Text;

export import <string>;
import Saturn.Readers.FArchive;

export enum class ELocResVersion : uint8_t {
    /** Legacy format file - will be missing the magic number. */
	Legacy = 0,
	/** Compact format file - strings are stored in a LUT to avoid duplication. */
	Compact,
	/** Optimized format file - namespaces/keys are pre-hashed (CRC32), we know the number of elements up-front, and the number of references for each string in the LUT (to allow stealing). */
	Optimized_CRC32,
	/** Optimized format file - namespaces/keys are pre-hashed (CityHash64, UTF-16), we know the number of elements up-front, and the number of references for each string in the LUT (to allow stealing). */
	Optimized_CityHash64_UTF16,

	LatestPlusOne,
	Latest = LatestPlusOne - 1
};

export enum class ETextGender : uint8_t {
	Masculine,
	Feminine,
	Neuter,
	// Add new enum types at the end only! They are serialized by index.
};

export struct FTextLocalizationResourceString {
    FTextLocalizationResourceString() = default;

    FTextLocalizationResourceString(std::string& InString, int32_t InRefCount = 0) 
        : String(InString) {}

    std::string String;
    uint32_t Pad;

    friend FArchive& operator<<(FArchive& Ar, FTextLocalizationResourceString& A);
    friend FArchive& operator>>(FArchive& Ar, FTextLocalizationResourceString& A);
};

export class FTextKey {
public:
    friend __forceinline bool operator==(const FTextKey& A, const FTextKey& B) {
        return A.Str == B.Str;
    }

    friend __forceinline bool operator!=(const FTextKey& A, const FTextKey& B) {
        return A.Str != B.Str;
    }

    friend size_t hash_value(const FTextKey& i) {
        return i.StrHash;
    }

    __forceinline std::string ToString() const {
        return Str;
    }

    void SerializeAsString(FArchive& Ar);
    void Serialize(FArchive& Ar, ELocResVersion& Ver);

    void WriteAsString(FArchive& Ar);
    void Write(FArchive&, ELocResVersion& Ver);
private:
    std::string Str;
    uint32_t StrHash;
    uint32_t Pad;
};

export struct FTextId {
    FTextId() = default;

    FTextId(const FTextKey& InNamespace, const FTextKey& InKey)
        : Namespace(InNamespace), Key(InKey) {}

    friend __forceinline bool operator==(const FTextId& A, const FTextId& B) {
        return A.Namespace == B.Namespace && A.Key == B.Key;
    }

    friend __forceinline bool operator!=(const FTextId& A, const FTextId& B) {
        return A.Namespace != B.Namespace || A.Key != B.Key;
    }

    friend size_t hash_value(const FTextId& i);

    FTextKey Namespace;
    FTextKey Key;
};

export class ITextData {
public:
    virtual std::string& GetString() = 0;
    virtual void Serialize(class FArchive& Ar) = 0;
    virtual void Write(class FArchive& Ar) = 0;
};

export class FText {
public:
    FText() = default;

    __forceinline std::string& GetCultureInvariantString() {
        return CultureInvariantString;
    }

    __forceinline int32_t GetFlags() {
        return Flags;
    }

    __forceinline std::string& ToString() {
        return Data->GetString();
    }

    friend FArchive& operator<<(FArchive& Ar, FText& Value);
    friend FArchive& operator>>(FArchive& Ar, FText& Value);
private:
    int32_t Flags;
    std::string CultureInvariantString;
    TUniquePtr<ITextData> Data;
    int8_t HistoryType;
};
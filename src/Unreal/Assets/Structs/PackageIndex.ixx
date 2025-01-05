export module Saturn.Asset.PackageIndex;

import <string>;
import <cstdint>;

import Saturn.Readers.FArchive;

/**
 * Wrapper for index into a ULnker's ImportMap or ExportMap.
 * Values greater than zero indicate that this is an index into the ExportMap. The
 * actual array index will be (FPackageIndex - 1).
 *
 * Values less than zero indicate that this is an index into the ImportMap. The actual
 * array index will be (-FPackageIndex - 1)
 */
export class FPackageIndex {
    int32_t Index;

    /** Internal constructor, sets the index directly **/
    __forceinline explicit FPackageIndex(int32_t InIndex)
        : Index(InIndex) {}
public:
    /** Constructor, sets the value to null **/
    __forceinline FPackageIndex()
        : Index(0) {}

    /** return true if this is an index into the import map **/
    __forceinline bool IsImport() const {
        return Index < 0;
    }

    /** return true if this is an index into the export map **/
    __forceinline bool IsExport() const {
        return Index > 0;
    }

    /** return true if this null (i.e. neither aan import nor an export) **/
    __forceinline bool IsNull() const {
        return Index == 0;
    }

    /** Check that this is an import and return the index into the import map **/
    __forceinline int32_t ToImport() const {
        return -Index - 1;
    }

    /** Check that this is an export and return the index into the export map **/
    __forceinline int32_t ToExport() const {
        return Index - 1;
    }

    /** Return the raw value, for debugging purposes **/
    __forceinline int32_t ForDebugging() const {
        return Index;
    }

    /** Create a FPackageIndex from an import index **/
    __forceinline static FPackageIndex FromImport(int32_t ImportIndex) {
        return FPackageIndex(-ImportIndex - 1);
    }

    /** Create a FPackageIndex from an export index **/
    __forceinline static FPackageIndex FromExport(int32_t ExportIndex) {
        return FPackageIndex(ExportIndex + 1);
    }

    /** Compare package indices for equality **/
    __forceinline bool operator==(const FPackageIndex& Other) const {
        return Index == Other.Index;
    }

    /** Compare package indices for inequality **/
    __forceinline bool operator!=(const FPackageIndex& Other) const {
        return Index != Other.Index;
    }

    /** Compare package indices **/
    __forceinline bool operator<(const FPackageIndex& Other) const {
        return Index < Other.Index;
    }

    __forceinline bool operator>(const FPackageIndex& Other) const {
        return Index > Other.Index;
    }

    __forceinline bool operator<=(const FPackageIndex& Other) const {
        return Index <= Other.Index;
    }

    __forceinline bool operator>=(const FPackageIndex& Other) const {
        return Index >= Other.Index;
    }

    /**
	 * Serializes a package index value from or into an archive.
	 *
	 * @param Ar - The archive to serialize from or to.
	 * @param Value - The value to serialize.
	 */
    __forceinline friend FArchive& operator<<(FArchive& Ar, FPackageIndex& Value) {
        Ar << Value.Index;
        return Ar;
    }

    __forceinline friend FArchive& operator>>(FArchive& Ar, FPackageIndex& Value) {
        Ar >> Value.Index;
        return Ar;
    }

    [[nodiscard]] __forceinline friend uint32_t GetTypeHash(const FPackageIndex& In) {
        return uint32_t(In.Index);
    }

    /**
     * Lex functions
     */
    friend std::string LexToString(const FPackageIndex& Value) {
        return std::to_string(Value.Index);
    }
};
export module Saturn.Asset.PackageObjectIndex;

import <cstdint>;
import <string>;

import Saturn.Readers.FArchive;
import Saturn.Asset.PackageImportReference;

export class FPackageObjectIndex {
    static constexpr uint64_t IndexBits = 62ull;
    static constexpr uint64_t IndexMask = (1ull << IndexBits) - 1ull;
    static constexpr uint64_t TypeMask = ~IndexMask;
    static constexpr uint64_t TypeShift = IndexBits;
    static constexpr uint64_t Invalid = ~0ull;

    uint64_t TypeAndId = Invalid;

    enum EType {
        Export,
        ScriptImport,
        PackageImport,
        Null,
        TypeCount = Null
    };

    inline explicit FPackageObjectIndex(EType InType, uint64_t InId) : TypeAndId((uint64_t(InType) << TypeShift) | InId) {}

    static uint64_t GenerateImportHashFromObjectPath(const std::string& ObjectPath);
public:
    FPackageObjectIndex() = default;

    inline static FPackageObjectIndex FromExportIndex(const int32_t Index) {
        return FPackageObjectIndex(Export, Index);
    }

    inline static FPackageObjectIndex FromScriptPath(const std::string& ScriptObjectPath) {
        return FPackageObjectIndex(ScriptImport, GenerateImportHashFromObjectPath(ScriptObjectPath));
    }

    inline static FPackageObjectIndex FromPackageImportRef(const FPackageImportReference& PackageImportRef) {
        uint64_t Id = static_cast<uint64_t>(PackageImportRef.GetImportedPackageIndex()) << 32 | PackageImportRef.GetImportedPublicExportHashIndex();
        return FPackageObjectIndex(PackageImport, Id);
    }

    inline bool IsNull() const { return TypeAndId == Invalid; }
    inline bool IsExport() const { return (TypeAndId >> TypeShift) == Export; }
    inline bool IsImport() const { return IsScriptImport() || IsPackageImport(); }
    inline bool IsScriptImport() const { return (TypeAndId >> TypeShift) == ScriptImport; }
    inline bool IsPackageImport() const { return (TypeAndId >> TypeShift) == PackageImport; }

    inline uint32_t ToExport() const {
        return uint32_t(TypeAndId);
    }

    inline FPackageImportReference ToPackageImportRef() const {
        uint32_t ImportedPackageIndex = static_cast<uint32_t>((TypeAndId & IndexMask) >> 32);
        uint32_t ExportHash = static_cast<uint32_t>(TypeAndId);
        return FPackageImportReference(ImportedPackageIndex, ExportHash);
    }

    inline uint64_t Value() const {
        return TypeAndId & IndexMask;
    }

    inline bool operator==(FPackageObjectIndex Other) const {
        return TypeAndId == Other.TypeAndId;
    }

    inline bool operator!=(FPackageObjectIndex Other) const {
        return TypeAndId != Other.TypeAndId;
    }

    friend FArchive& operator<<(FArchive& Ar, FPackageObjectIndex& Value) {
        Ar << Value.TypeAndId;
        return Ar;
    }

    inline uint32_t GetImportedPackageIndex() const {
        return static_cast<uint32_t>((TypeAndId & IndexMask) >> 32);
    }

    inline friend uint32_t GetTypeHash(const FPackageObjectIndex& Value) {
        return uint32_t(Value.TypeAndId);
    }
};

export struct PackageObjectIndexHasher {
    std::size_t operator()(const FPackageObjectIndex& Index) const {
        return GetTypeHash(Index);
    }
};
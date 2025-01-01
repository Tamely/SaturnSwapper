export module Saturn.Asset.PackageImportReference;

import <cstdint>;

export class FPackageImportReference {
public:
    FPackageImportReference(uint32_t InImportedPackageIndex, uint32_t InImportedPublicExportHashIndex)
        : ImportedPackageIndex(InImportedPackageIndex)
        , ImportedPublicExportHashIndex(InImportedPublicExportHashIndex) {}

        uint32_t GetImportedPackageIndex() const { return ImportedPackageIndex; }
        uint32_t GetImportedPublicExportHashIndex() const { return ImportedPublicExportHashIndex; }
private:
    uint32_t ImportedPackageIndex;
    uint32_t ImportedPublicExportHashIndex;
};
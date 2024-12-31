export module Saturn.Asset.DependencyBundleEntry;

import Saturn.Asset.PackageIndex;
import Saturn.Readers.FArchive;

export struct FDependencyBundleEntry {
    FPackageIndex LocalImportOrExportIndex;

    friend FArchive& operator<<(FArchive& Ar, FDependencyBundleEntry& DependencyBundleEntry);
};
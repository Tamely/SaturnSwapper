export module Saturn.ZenPackage.ZenPackageImportedPackageNamesContainer;

import <vector>;

import Saturn.Structs.Name;
import Saturn.Readers.FArchive;

export struct FZenPackageImportedPackageNamesContainer {
    std::vector<FName> Names;

    friend FArchive& operator<<(FArchive& Ar, FZenPackageImportedPackageNamesContainer& Container);
};
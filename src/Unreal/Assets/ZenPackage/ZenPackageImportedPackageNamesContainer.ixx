export module Saturn.ZenPackage.ZenPackageImportedPackageNamesContainer;

import <string>;
import <vector>;

import Saturn.Readers.FArchive;

export struct FZenPackageImportedPackageNamesContainer {
    std::vector<std::wstring> Names;

    friend FArchive& operator<<(FArchive& Ar, FZenPackageImportedPackageNamesContainer& Container);
};
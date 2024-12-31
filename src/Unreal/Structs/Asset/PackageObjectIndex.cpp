import Saturn.Asset.PackageObjectIndex;

#include "Unreal/Hash/CityHash.h"

import <cstdint>;
import <string>;
import <algorithm>;
import <vector>;

import Saturn.Readers.FArchive;
import Saturn.Asset.PackageImportReference;

uint64_t FPackageObjectIndex::GenerateImportHashFromObjectPath(const std::string& ObjectPath) {
    std::string FullImportPath = ObjectPath;

    std::transform(FullImportPath.begin(), FullImportPath.end(), FullImportPath.begin(),
        [](char c) {
            if (c == '.' || c == ':')
            {
                return '/';
            }
            return static_cast<char>(std::tolower(static_cast<unsigned char>(c)));
        });
    
    uint64_t Hash = CityHash64(FullImportPath.data(), FullImportPath.size());
    Hash &= ~(3ull << 62ull);
    return Hash;
}
import Saturn.Paths.TopLevelAssetPath;

import Saturn.Readers.ZenPackageReader;

FZenPackageReader& operator<<(FZenPackageReader& Ar, FTopLevelAssetPath& Value) {
    Ar << Value.PackageName;
    Ar << Value.AssetName;

    return Ar;
}

FZenPackageReader& operator>>(FZenPackageReader& Ar, FTopLevelAssetPath& Value) {
    Ar >> Value.PackageName;
    Ar >> Value.AssetName;

    return Ar;
}
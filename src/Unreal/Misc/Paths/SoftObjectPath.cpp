import Saturn.Paths.SoftObjectPath;

import Saturn.Readers.ZenPackageReader;

FZenPackageReader& operator<<(FZenPackageReader& Ar, FSoftObjectPath& Value) {
    Ar << Value.AssetPath;
    Ar << Value.SubPathString;

    return Ar;
}

FZenPackageReader& operator>>(FZenPackageReader& Ar, FSoftObjectPath& Value) {
    Ar >> Value.AssetPath;
    Ar >> Value.SubPathString;

    return Ar;
}
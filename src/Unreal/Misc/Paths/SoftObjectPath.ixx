export module Saturn.Paths.SoftObjectPath;

import <string>;

import Saturn.Readers.ZenPackageReader;
import Saturn.Paths.TopLevelAssetPath;

export class FSoftObjectPath {
public:
    FSoftObjectPath() {
        Reset();
    }

    friend FZenPackageReader& operator<<(FZenPackageReader& Ar, FSoftObjectPath& Value) {
        Ar << Value.AssetPath;
        Ar << Value.SubPathString;

        return Ar;
    }

    friend FZenPackageReader& operator>>(FZenPackageReader& Ar, FSoftObjectPath& Value) {
        Ar >> Value.AssetPath;
        Ar >> Value.SubPathString;

        return Ar;
    }

    __forceinline std::string GetAssetPathString() { return AssetPath.ToString(); }
    __forceinline FTopLevelAssetPath GetAssetPath() { return AssetPath; }
    __forceinline std::string GetSubPath() { return SubPathString; }

    __forceinline void operator=(FSoftObjectPath& Other) {
        AssetPath = Other.AssetPath;
        SubPathString = Other.SubPathString;
    }

    void Reset() {
        SubPathString = {};
    }
private:
    FTopLevelAssetPath AssetPath;
    std::string SubPathString;
};
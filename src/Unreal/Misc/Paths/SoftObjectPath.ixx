export module Saturn.Paths.SoftObjectPath;

import <string>;

import Saturn.Paths.TopLevelAssetPath;

export class FSoftObjectPath {
public:
    FSoftObjectPath() {
        Reset();
    }

    friend class FZenPackageReader& operator<<(class FZenPackageReader& Ar, FSoftObjectPath& Value);
    friend class FZenPackageReader& operator>>(class FZenPackageReader& Ar, FSoftObjectPath& Value);

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
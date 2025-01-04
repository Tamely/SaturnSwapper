export module Saturn.Core.LazyPackageObject;

import Saturn.Core.UObject;
import Saturn.Files.PackageId;

export class ULazyPackageObject : public UObject {
    FPackageId PackageId;
public:
    ULazyPackageObject(FPackageId InPackageId) : PackageId(InPackageId) {
        SetFlags(UObject::RF_NeedLoad);
    }

    void Load() override {
        // TODO: This
        ClearFlags(UObject::RF_NeedLoad);
    }
};
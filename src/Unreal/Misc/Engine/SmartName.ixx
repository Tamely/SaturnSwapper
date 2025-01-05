export module Saturn.Engine.SmartName;

import Saturn.Structs.Name;
import Saturn.Readers.ZenPackageReader;

export struct FSmartName {
    FSmartName() = default;
    
    FName DisplayName;

    friend FZenPackageReader& operator<<(FZenPackageReader& Ar, FSmartName& Name) {
        return Ar << Name.DisplayName;
    }

    friend FZenPackageReader& operator>>(FZenPackageReader& Ar, FSmartName& Name) {
        return Ar >> Name.DisplayName;
    }
};
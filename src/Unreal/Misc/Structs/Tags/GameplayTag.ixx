export module Saturn.Tags.GameplayTag;

export import Saturn.Structs.Name;
import Saturn.Readers.ZenPackageReader;

import <string>;

export class FGameplayTag {
public:
    friend FZenPackageReader& operator<<(FZenPackageReader& Ar, FGameplayTag& GameplayTag) {
        Ar << GameplayTag.TagName;

        return Ar;
    }

    friend FZenPackageReader& operator>>(FZenPackageReader& Ar, FGameplayTag& GameplayTag) {
        Ar >> GameplayTag.TagName;

        return Ar;
    }

    __forceinline std::string ToString() const {
        return TagName.ToString();
    }

    __forceinline FName GetName() const {
        return TagName;
    }
private:
    FName TagName;
};
export module Saturn.Tags.GameplayTagContainer;

import Saturn.Tags.GameplayTag;
import Saturn.Readers.ZenPackageReader;

import <vector>;
import <cstdint>;

export class FGameplayTagContainer {
public:
    FGameplayTagContainer() {}

    int32_t Num() const {
        return GameplayTags.size();
    }

    bool IsValid() const {
        return GameplayTags.size() > 0;
    }

    bool IsEmpty() const {
        return GameplayTags.size() == 0;
    }

    bool IsValidIndex(int32_t Index) const {
        return Index < GameplayTags.size();
    }

    FGameplayTag GetByIndex(int32_t Index) const {
        if (IsValidIndex(Index)) {
            return GameplayTags[Index];
        }
        return FGameplayTag();
    }

    FGameplayTag First() const {
        return GameplayTags.size() > 0 ? GameplayTags[0] : FGameplayTag();
    }

    FGameplayTag Last() const {
        return GameplayTags.size() > 0 ? GameplayTags.back() : FGameplayTag();
    }

    friend FZenPackageReader& operator<<(FZenPackageReader& Ar, FGameplayTagContainer& Container) {
        Ar << Container.GameplayTags;

        return Ar;
    }

    friend FZenPackageReader& operator>>(FZenPackageReader& Ar, FGameplayTagContainer& Container) {
        Ar >> Container.GameplayTags;

        return Ar;
    }
private:
    std::vector<FGameplayTag> GameplayTags;
};
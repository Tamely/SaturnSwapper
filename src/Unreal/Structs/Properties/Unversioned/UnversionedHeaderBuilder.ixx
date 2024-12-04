export module Saturn.Unversioned.UnversionedHeaderBuilder;

import Saturn.Unversioned.UnversionedHeader;
import Saturn.Unversioned.Fragment;

// https://github.com/EpicGames/UnrealEngine/blob/1308e62273a620dd4584b830f6b32cd8200c2ad3/Engine/Source/Runtime/CoreUObject/Private/Serialization/UnversionedPropertySerialization.cpp#L795
class FUnversionedHeaderBuilder : public FUnversionedHeader {
public:
    FUnversionedHeaderBuilder();

    void IncludeProperty(bool bIsZero);
    void ExcludeProperty();
    void Finalize();
private:
    void TrimZeroMask(const FFragment& Fragment);
};
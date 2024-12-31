export module Saturn.Asset.NameMap;

import <vector>;
import <cstdint>;
/*
 * Maps serialized naame entries to names.
 */
export class FNameMap {
public:
    inline int32_t Num() const {
        return NameEntries.size();
    }

    void Load(FArchive& Ar, FMappedName::EType NameMapType);

    FName GetName(const FMappedName& MappedName) const {
        return NameEntries[MappedName.GetIndex()].ToName(MappedName.GetNumber());
    }

    bool TryGetName(const FMappedName& MappedName, FName& OutName) const {
        uint32_t Index = MappedName.GetIndex();
        if (Index < uint32_t(NameEntries.size())) {
            OutName = NameEntries[MappedName.GetIndex()].ToName(MappedName.GetNumber());
            return true;
        }
        return false;
    }
};
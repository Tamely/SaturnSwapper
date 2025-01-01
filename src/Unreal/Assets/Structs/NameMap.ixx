export module Saturn.Asset.NameMap;

import <string>;
import <vector>;
import <cstdint>;

import Saturn.Structs.Name;
import Saturn.Readers.FArchive;
import Saturn.Structs.MappedName;

/*
 * Maps serialized naame entries to names.
 */
export class FNameMap {
public:
    inline int32_t Num() const {
        return NameEntries.size();
    }

    void Load(FArchive& Ar, FMappedName::EType NameMapType);
    static std::vector<FName> LoadNameBatch(FArchive& Ar);

    FName GetName(const FMappedName& MappedName) const {
        return FName(MappedName.GetIndex(), MappedName.GetNumber());
    }

    bool TryGetName(const FMappedName& MappedName, FName& OutName) const {
        uint32_t Index = MappedName.GetIndex();
        if (Index < uint32_t(NameEntries.size())) {
            OutName = FName(MappedName.GetIndex(), MappedName.GetNumber());
            return true;
        }
        return false;
    }

    using RangedForConstIteratorType = std::vector<FName>::const_iterator;
    RangedForConstIteratorType begin() const { return NameEntries.begin(); }
    RangedForConstIteratorType end() const { return NameEntries.end(); }
private:
    std::vector<FName> NameEntries;
    FMappedName::EType NameMapType = FMappedName::EType::Global;
};
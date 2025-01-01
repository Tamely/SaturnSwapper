export module Saturn.Asset.NameMap;

import <string>;
import <vector>;
import <cstdint>;

import Saturn.Structs.Name;
import Saturn.Readers.FArchive;
import Saturn.Structs.MappedName;

/*
 * Maps serialized name entries to names.
 */
export class FNameMap {
public:
    inline int32_t Num() const {
        return NameEntries.size();
    }

    void Load(FArchive& Ar, FMappedName::EType NameMapType);
    static std::vector<std::wstring> LoadNameBatch(FArchive& Ar);

    std::wstring GetName(const FMappedName& MappedName) const {
        std::wstring Name = NameEntries[MappedName.GetIndex()];
        if (MappedName.GetNumber() == 0) {
            return Name;
        }

        return Name + L"_" + std::to_wstring(MappedName.GetNumber() + 1);
    }

    bool TryGetName(const FMappedName& MappedName, std::wstring& OutName) const {
        uint32_t Index = MappedName.GetIndex();
        if (Index < uint32_t(NameEntries.size())) {
            OutName = GetName(FMappedName::Create(MappedName.GetIndex(), MappedName.GetNumber(), NameMapType));
            return true;
        }
        return false;
    }

    using RangedForConstIteratorType = std::vector<std::wstring>::const_iterator;
    RangedForConstIteratorType begin() const { return NameEntries.begin(); }
    RangedForConstIteratorType end() const { return NameEntries.end(); }
private:
    std::vector<std::wstring> NameEntries;
    FMappedName::EType NameMapType = FMappedName::EType::Global;
};
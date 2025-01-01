import Saturn.ZenPackage.ZenPackageImportedPackageNamesContainer;

import <string>;
import <vector>;

import Saturn.Structs.Name;
import Saturn.Asset.NameMap;
import Saturn.Readers.FArchive;

FArchive& operator<<(FArchive& Ar, FZenPackageImportedPackageNamesContainer& Container) {
    std::vector<FName> NameEntries = FNameMap::LoadNameBatch(Ar);
    Container.Names.resize(NameEntries.size());
    for (int32_t Index = 0; Index < NameEntries.size(); ++Index) {
        int32_t Number;
        Ar << Number;
        Container.Names[Index] = FName(NameEntries[Index].NameMap, NameEntries[Index].Index, Number);
    }

    return Ar;
}
import Saturn.ZenPackage.ZenPackageImportedPackageNamesContainer;

import <string>;
import <vector>;

import Saturn.Structs.Name;
import Saturn.Asset.NameMap;
import Saturn.Readers.FArchive;

FArchive& operator<<(FArchive& Ar, FZenPackageImportedPackageNamesContainer& Container) {
    std::vector<std::wstring> NameEntries = FNameMap::LoadNameBatch(Ar);
    Container.Names.resize(NameEntries.size());
    for (int32_t Index = 0; Index < NameEntries.size(); ++Index) {
        int32_t Number;
        Ar << Number;

        std::wstring Name = NameEntries[Index];
        if (Number > 0) {
            Name += L"_" + std::to_wstring(Number + 1);
        }
        
        Container.Names[Index] = Name;
    }

    return Ar;
}
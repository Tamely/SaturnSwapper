module;

#include "Saturn/Defines.h"

export module Saturn.IoStore.GlobalToc;

import Saturn.Asset.NameMap;
import Saturn.Structs.MappedName;
import Saturn.Asset.PackageObjectIndex;

export struct FScriptObjectEntry {
    FMappedName MappedName;
    FPackageObjectIndex GlobalIndex;
    FPackageObjectIndex OuterIndex;
    FPackageObjectIndex CDOClassIndex;
};

export struct FGlobalTocData {
    FNameMap NameMap;
    std::unordered_map<FPackageObjectIndex, FScriptObjectEntry, PackageObjectIndexHasher> ScriptObjectByGlobalIdMap;

    void Serialize(class FIoStoreReader* Reader);
};

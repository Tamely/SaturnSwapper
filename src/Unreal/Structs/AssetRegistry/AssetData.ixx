export module Saturn.AssetRegistry.AssetData;

import Saturn.Structs.Name;
import Saturn.Readers.FArchive;
import Saturn.AssetRegistry.AssetRegistryVersion;

export class FAssetData {
public:
	FAssetData() = default;
	FAssetData(FArchive& Ar, FAssetRegistryVersionType Version);
public:
	FName PackageName;
	FName PackagePath;
	FName AssetName;
	FName AssetClass;
};
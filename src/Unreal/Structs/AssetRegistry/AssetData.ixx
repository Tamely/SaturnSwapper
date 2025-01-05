export module Saturn.AssetRegistry.AssetData;

import Saturn.Structs.Name;
import Saturn.Readers.FArchive;
import Saturn.Unreal.AssetRegistryReader;

export class FAssetData {
public:
	FAssetData() = default;
	FAssetData(FArchive& Ar, FAssetRegistryReader& Reader);
public:
	FName PackageName;
	FName PackagePath;
	FName AssetName;
	FName AssetClass;
};
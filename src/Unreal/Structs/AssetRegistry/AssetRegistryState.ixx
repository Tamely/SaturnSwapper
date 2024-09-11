export module Saturn.AssetRegistry.AssetRegistryState;

import Saturn.Readers.FArchive;
import Saturn.AssetRegistry.AssetData;
import Saturn.Unreal.AssetRegistryReader;

import <vector>;

export class FAssetRegistryState {
public:
	FAssetRegistryState();
	FAssetRegistryState(FArchive& Ar);
private:
	void Load(FArchive& Ar, FAssetRegistryReader& Reader);
public:
	std::vector<FAssetData> PreallocatedAssetDataBuffers;
private:
	FAssetRegistryReader Reader;
};
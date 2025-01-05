#include <cassert>

import Saturn.AssetRegistry.AssetRegistryState;

import Saturn.Readers.FArchive;
import Saturn.AssetRegistry.AssetData;
import Saturn.Unreal.AssetRegistryReader;
import Saturn.AssetRegistry.AssetRegistryHeader;
import Saturn.AssetRegistry.AssetRegistryVersion;

import <vector>;

FAssetRegistryState::FAssetRegistryState() {}

FAssetRegistryState::FAssetRegistryState(FArchive& Ar) {
	FAssetRegistryHeader header(Ar);
	FAssetRegistryVersionType version = header.Version;

	if (version < FAssetRegistryVersionType::AddAssetRegistryState) {
		assert(false && "Cannot read registry state before version: " + version);
		return;
	}
	else if (version < FAssetRegistryVersionType::FixedTags) {
		assert(false && "Cannot read version for FixedTags");
		return;
	}
	else {
		Reader = FAssetRegistryReader(Ar, header);
		Load(Ar, Reader);
	}
}

void FAssetRegistryState::Load(FArchive& Ar, FAssetRegistryReader& Reader) {
	uint32_t numAssets;
	Ar << numAssets;

	PreallocatedAssetDataBuffers = std::vector<FAssetData>(numAssets);
	for (uint32_t i = 0; i < numAssets; i++) {
		FAssetData assetData(Ar, Reader);
		PreallocatedAssetDataBuffers[i] = assetData;
	}
}
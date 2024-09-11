export module Saturn.AssetRegistry.AssetRegistryHeader;

import Saturn.Readers.FArchive;
import Saturn.AssetRegistry.AssetRegistryVersion;

export class FAssetRegistryHeader {
public:
	FAssetRegistryHeader() = default;
	FAssetRegistryHeader(FArchive& Ar);
	FAssetRegistryHeader(FAssetRegistryVersionType version, bool filterEditorOnlyData);
public:
	FAssetRegistryVersionType Version;
	bool bFilterEditorOnlyData;
};
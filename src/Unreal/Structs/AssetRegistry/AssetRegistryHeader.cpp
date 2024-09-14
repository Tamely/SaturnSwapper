import Saturn.AssetRegistry.AssetRegistryHeader;

import Saturn.Readers.FArchive;
import Saturn.AssetRegistry.AssetRegistryVersion;

import <cstdint>;

FAssetRegistryHeader::FAssetRegistryHeader(FArchive& Ar) {
	FAssetRegistryVersion::TrySerializeVersion(Ar, Version);
	
	if (Version >= FAssetRegistryVersionType::AddedHeader) {
		uint32_t filterEditorOnlyData;
		Ar << filterEditorOnlyData;
		bFilterEditorOnlyData = filterEditorOnlyData != 0;
	}
}

FAssetRegistryHeader::FAssetRegistryHeader(FAssetRegistryVersionType version, bool filterEditorOnlyData)
	: Version(version)
	, bFilterEditorOnlyData(filterEditorOnlyData) {}
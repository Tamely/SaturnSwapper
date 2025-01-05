import Saturn.AssetRegistry.AssetData;

import Saturn.Structs.Name;
import Saturn.Readers.FArchive;
import Saturn.Paths.TopLevelAssetPath;
import Saturn.Unreal.AssetRegistryReader;
import Saturn.Structs.NameEntrySerialized;
import Saturn.AssetRegistry.AssetRegistryVersion;

import <string>;
import <vector>;

FAssetData::FAssetData(FArchive& Ar, FAssetRegistryReader& Reader) {
	if (Reader.Header.Version < FAssetRegistryVersionType::RemoveAssetPathFNames) {
		FName oldObjectPath = Reader.ReadFName(Ar);
	}

	PackagePath = Reader.ReadFName(Ar);

	if (Reader.Header.Version >= FAssetRegistryVersionType::ClassPaths) {
		FName TopPackageName = Reader.ReadFName(Ar);
		FName TopAssetName = Reader.ReadFName(Ar);
		AssetClass = TopAssetName;
	}
	else {
		AssetClass = Reader.ReadFName(Ar);
	}

	if (Reader.Header.Version < FAssetRegistryVersionType::RemovedMD5Hash) {
		FName oldGroupNames = Reader.ReadFName(Ar);
	}

	PackageName = Reader.ReadFName(Ar);
	AssetName = Reader.ReadFName(Ar);

	uint64_t tagSize;
	Ar << tagSize;

	uint32_t bundleCount;
	Ar << bundleCount;

	for (uint32_t i = 0; i < bundleCount; i++) {
		FName bundleName = Reader.ReadFName(Ar);

		uint32_t assetCount;
		Ar << assetCount;
		for (uint32_t j = 0; j < assetCount; j++) {
			if (Reader.Header.Version >= FAssetRegistryVersionType::ClassPaths) {
				FName TopPackageName = Reader.ReadFName(Ar);
				FName TopAssetName = Reader.ReadFName(Ar);
			}
			else {
				FName a = Reader.ReadFName(Ar);
			}

			std::string b;
			Ar << b;
		}
	}

	uint32_t chunkCount;
	Ar << chunkCount;

	Ar.SeekCur(chunkCount * sizeof(uint32_t)); // Skip chunkids

	uint32_t packageFlags;
	Ar << packageFlags;
}
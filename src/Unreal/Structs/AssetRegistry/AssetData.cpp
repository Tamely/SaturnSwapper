import Saturn.AssetRegistry.AssetData;

import Saturn.Structs.Name;
import Saturn.Structs.NameEntrySerialized;
import Saturn.Structs.TopLevelAssetPath;
import Saturn.Readers.FArchive;
import Saturn.AssetRegistry.AssetRegistryVersion;

import <string>;
import <vector>;

std::vector<FNameEntrySerialized> FName::NameMap;

FAssetData::FAssetData(FArchive& Ar, FAssetRegistryVersionType Version) {
	if (Version < FAssetRegistryVersionType::RemoveAssetPathFNames) {
		FName oldObjectPath;
		Ar << oldObjectPath;
	}

	Ar << PackagePath;

	if (Version >= FAssetRegistryVersionType::ClassPaths) {
		AssetClass = FTopLevelAssetPath(Ar).AssetName;
	}
	else {
		Ar << AssetClass;
	}

	if (Version < FAssetRegistryVersionType::RemovedMD5Hash) {
		FName oldGroupNames;
		Ar << oldGroupNames;
	}

	Ar << PackageName;
	Ar << AssetName;

	uint64_t tagSize;
	Ar << tagSize;

	uint32_t bundleCount;
	Ar << bundleCount;

	for (uint32_t i = 0; i < bundleCount; i++) {
		FName bundleName;
		Ar << bundleName;

		uint32_t assetCount;
		Ar << assetCount;
		for (uint32_t j = 0; j < assetCount; j++) {
			if (Version >= FAssetRegistryVersionType::ClassPaths) {
				FTopLevelAssetPath a(Ar);
			}
			else {
				FName a;
				Ar << a;
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
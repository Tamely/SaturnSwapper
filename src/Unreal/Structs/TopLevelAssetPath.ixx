export module Saturn.Structs.TopLevelAssetPath;

import Saturn.Structs.Name;
import Saturn.Readers.FArchive;

export class FTopLevelAssetPath {
public:
	FTopLevelAssetPath(FArchive& Ar) {
		Ar << PackageName;
		Ar << AssetName;
	}

	FTopLevelAssetPath(FName& package, FName& asset) {
		PackageName = package;
		AssetName = asset;
	}
public:
	FName PackageName;
	FName AssetName;
};
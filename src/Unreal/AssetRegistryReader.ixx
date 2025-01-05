export module Saturn.Unreal.AssetRegistryReader;

import Saturn.Structs.Name;
import Saturn.Asset.NameMap;
import Saturn.Readers.FArchive;
import Saturn.Paths.TopLevelAssetPath;
import Saturn.Structs.NameEntrySerialized;
import Saturn.AssetRegistry.AssetRegistryHeader;

import <cstdint>;
import <vector>;
import <unordered_map>;

export class FAssetRegistryReader {
public:
	FAssetRegistryReader() = default;
	FAssetRegistryReader(FArchive& Ar, FAssetRegistryHeader& header) {
		Header = header;
		NameMap.Load(Ar, FMappedName::EType::Package);

		uint32_t storeMagic;
		Ar << storeMagic;

		int nums[11];
		Ar << nums[0] << nums[1] << nums[2] << nums[3] << nums[4] << nums[5] << nums[6] << nums[7] << nums[8] << nums[9] << nums[10];

		if (storeMagic == 0x12345679) {
			Ar.SeekCur(4);
			for (int i = 0; i < nums[4]; i++) {
				std::string s;
				Ar << s;
			}
		}

		Ar.SeekCur(sizeof(uint32_t) * nums[0]);

		for (int i = 0; i < nums[1]; i++) {
			ReadFName(Ar);
		}

		Ar.SeekCur(((header.Version >= FAssetRegistryVersionType::ClassPaths ? sizeof(uint32_t) : 0) + sizeof(uint32_t) + sizeof(uint32_t) + sizeof(uint32_t)) * nums[2]);

		for (int i = 0; i < nums[3]; i++) {
			if (header.Version >= FAssetRegistryVersionType::ClassPaths) {
				FName PackageName = ReadFName(Ar);
				FName AssetName = ReadFName(Ar);
				FTopLevelAssetPath name(PackageName, AssetName);
			}
			else {
				ReadFName(Ar);
			}

			ReadFName(Ar);
			ReadFName(Ar);
		}

		if (storeMagic == 0x12345678) {
			Ar.SeekCur(4);
			for (int i = 0; i < nums[4]; i++) {
				std::string s;
				Ar << s;
			}
		}

		Ar.SeekCur(sizeof(uint32_t) * nums[5]);
		Ar.SeekCur(sizeof(uint32_t) * nums[6]);
		Ar.SeekCur(nums[7]);
		Ar.SeekCur(nums[8] * 2);

		Ar.SeekCur((sizeof(uint32_t) + sizeof(uint32_t)) * nums[9]);

		for (int i = 0; i < nums[10]; i++) {
			ReadFName(Ar);

			uint32_t value;
			Ar << value;
		}

		Ar.SeekCur(4);
	}

	FName ReadFName(FArchive& Ar) {
		int index;
		Ar << index;
		int number = 0;

		if ((index & FName::AssetRegistryNumberedNameBit) > 0) {
			index -= FName::AssetRegistryNumberedNameBit;
			Ar << number;
		}

		FMappedName MappedName = FMappedName::Create(index, number, FMappedName::EType::Package);
		std::wstring NameW = NameMap.GetName(MappedName);
		std::string Name = std::string(NameW.begin(), NameW.end());
		return Name;
	}
public:
	FAssetRegistryHeader Header;
	FNameMap NameMap;
};
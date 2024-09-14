export module Saturn.Structs.Name;

import Saturn.Readers.FArchive;
import Saturn.Structs.NameEntrySerialized;

import <string>;
import <vector>;

export class FName {
public:
	FName() : Index(0), Number(0) {
		_name = FNameEntrySerialized();
	}

	FName(std::string name, int index = 0, int number = 0) : Index(index), Number(number) {
		_name = FNameEntrySerialized(name);
	}

	FName(int index, int number) : Index(index), Number(number) {
		_name = NameMap[index];
	}

	FName(FNameEntrySerialized name, int index, int number) : Index(index), Number(number) {
		_name = name;
	}

	FName(std::vector<FNameEntrySerialized> nameMap, int index, int number) : Index(index), Number(number) {
		_name = nameMap[Index];
	}
public:
	std::string GetText() {
		return Number == 0 ? PlainText() : (PlainText() + "_" + std::to_string(Number - 1));
	}

	std::string PlainText() {
		return _name.Name.empty() ? "None" : _name.Name;
	}

	static void SetNameMap(std::vector<FNameEntrySerialized> nameMap) {
		NameMap = nameMap;
	}

	friend FArchive& operator<<(FArchive& Ar, FName& Name) {
		int index;
		Ar << index;
		int number = 0;

		if ((index & FName::AssetRegistryNumberedNameBit) > 0) {
			index -= FName::AssetRegistryNumberedNameBit;
			Ar << number;
		}

		Name = FName(index, number);
		return Ar;
	}
public:
	int Index;
	int Number;
	bool ShouldCalculateName = false;
private:
	static std::vector<FNameEntrySerialized> NameMap;
	static const uint32_t AssetRegistryNumberedNameBit = 0x80000000u; // int32 max
	FNameEntrySerialized _name;
};
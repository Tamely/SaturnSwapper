export module Saturn.Structs.Name;

import Saturn.Readers.FArchive;
import Saturn.Structs.NameEntrySerialized;

import <string>;
import <vector>;

export class FName {
public:
	friend class FNameProperty;

	FName() = default;

	FName(std::string& InStr) : Val(InStr) {}
	FName(std::string_view& StrView) : Val(StrView) {}

	__forceinline void operator=(std::string const& Other) {
		Val = Other;
	}

	__forceinline void operator=(FName const& Other) {
		Val = Other.Val;
	}

	bool operator==(FName const& Other) const {
		return Val == Other.Val;
	}

	bool operator!=(FName const& Other) const {
		return Val != Other.Val;
	}

	__forceinline std::string ToString() const {
		return Val;
	}

	__forceinline std::string& GetString() {
		return Val;
	}

	static const uint32_t AssetRegistryNumberedNameBit = 0x80000000u; // int32 max
private:
	std::string Val;
};
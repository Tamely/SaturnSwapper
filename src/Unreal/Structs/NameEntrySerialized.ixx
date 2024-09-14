export module Saturn.Structs.NameEntrySerialized;

import Saturn.Readers.FArchive;

import <string>;
import <vector>;

export class FNameEntrySerialized {
public:
	FNameEntrySerialized() = default;
	FNameEntrySerialized(FArchive& Ar);
	FNameEntrySerialized(const std::string& Name);
public:
	static std::vector<FNameEntrySerialized> LoadNameBatch(FArchive& Ar, int nameCount);
	static std::vector<FNameEntrySerialized> LoadNameBatch(FArchive& Ar);
private:
	static FNameEntrySerialized LoadNameHeader(FArchive& Ar);
public:
	std::string Name;
};
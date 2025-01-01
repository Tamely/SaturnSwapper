import Saturn.Structs.NameEntrySerialized;

import Saturn.Readers.FArchive;
import Saturn.Structs.SerializedNameHeader;

import <string>;
import <vector>;
import <memory>;

FNameEntrySerialized::FNameEntrySerialized(FArchive& Ar) {
	Ar << Name;
	Name.pop_back();

	Ar.SeekCur(sizeof(uint16_t) + sizeof(uint16_t)); // Skip NonCasePreservingHash and CasePreservingHash
}

FNameEntrySerialized::FNameEntrySerialized(const std::string& Name) : Name(Name) {}

std::vector<FNameEntrySerialized> FNameEntrySerialized::LoadNameBatch(FArchive& Ar, int nameCount) {
	std::vector<FNameEntrySerialized> names(nameCount);
	for (int i = 0; i < nameCount; i++) {
		names[i] = LoadNameHeader(Ar);
	}
	return names;
}

std::vector<FNameEntrySerialized> FNameEntrySerialized::LoadNameBatch(FArchive& Ar) {
	int nameCount;
	Ar << nameCount;

	if (nameCount == 0) {
		return {};
	}

	Ar.SeekCur(sizeof(uint32_t)); // Skip numStringBytes
	Ar.SeekCur(sizeof(uint64_t)); // Skip hashVersion

	Ar.SeekCur(nameCount * sizeof(uint64_t)); // Skip hashes

	std::vector<FSerializedNameHeader> headers(nameCount);
	Ar.Serialize(&headers, nameCount * sizeof(FSerializedNameHeader));

	std::vector<FNameEntrySerialized> entries(nameCount);
	for (int i = 0; i < nameCount; i++) {
		FSerializedNameHeader& header = headers[i];

		std::string s;
		if (header.IsUtf16())
		{
			auto WStringData = std::make_unique<wchar_t[]>(header.Len());
			Ar.Serialize(WStringData.get(), header.NumBytes());

			auto Temp = std::wstring(WStringData.get());
			s.assign(Temp.begin(), Temp.end());
		}
		else
		{
			s.resize(header.Len());
			Ar.Serialize(&s[0], header.NumBytes());
		}

		entries[i] = FNameEntrySerialized(s);
	}

	return entries;
}

FNameEntrySerialized FNameEntrySerialized::LoadNameHeader(FArchive& Ar) {
	FSerializedNameHeader header;
	Ar.Serialize(&header, sizeof(FSerializedNameHeader));

	if (header.IsUtf16()) {
		if (Ar.Tell() % 2 == 1) Ar.SeekCur(1);

		std::string s;

		auto WStringData = std::make_unique<wchar_t[]>(header.Len());
		Ar.Serialize(WStringData.get(), header.NumBytes());
		auto Temp = std::wstring(WStringData.get());
		s.assign(Temp.begin(), Temp.end());

		return FNameEntrySerialized(s);
	}

	std::string s;
	s.resize(header.Len());
	Ar.Serialize(&s[0], header.NumBytes());
	return FNameEntrySerialized(s);
}
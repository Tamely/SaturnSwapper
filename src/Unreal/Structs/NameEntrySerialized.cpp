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
	for (int i = 0; i < nameCount; i++) {
		headers[i] = FSerializedNameHeader(Ar);
	}

	std::vector<FNameEntrySerialized> entries(nameCount);
	for (int i = 0; i < nameCount; i++) {
		FSerializedNameHeader& header = headers[i];
		uint32_t length = header.Length();

		std::string s;
		if (header.IsUTF16())
		{
			auto WStringData = std::make_unique<wchar_t[]>(header.Length());
			Ar.Serialize(WStringData.get(), header.Length() * sizeof(wchar_t));

			auto Temp = std::wstring(WStringData.get());
			s.assign(Temp.begin(), Temp.end());
		}
		else
		{
			s.resize(header.Length());
			Ar.Serialize(&s[0], header.Length());
		}

		entries[i] = FNameEntrySerialized(s);
	}

	return entries;
}

FNameEntrySerialized FNameEntrySerialized::LoadNameHeader(FArchive& Ar) {
	FSerializedNameHeader header = FSerializedNameHeader(Ar);

	uint32_t length = header.Length();
	if (header.IsUTF16()) {
		if (Ar.Tell() % 2 == 1) Ar.SeekCur(1);

		std::string s;

		auto WStringData = std::make_unique<wchar_t[]>(length);
		Ar.Serialize(WStringData.get(), length * sizeof(wchar_t));
		auto Temp = std::wstring(WStringData.get());
		s.assign(Temp.begin(), Temp.end());

		return FNameEntrySerialized(s);
	}

	std::string s;
	s.resize(length);
	Ar.Serialize(&s[0], length);
	return FNameEntrySerialized(s);
}
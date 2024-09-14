#include <regex>

import Saturn.Asset.ZenAsset;

import Saturn.Readers.MemoryReader;

ZenAsset::ZenAsset(uint8_t* data, size_t dataLen)
	: m_Buffer(data), m_BufferLen(dataLen) {

	FMemoryReader reader(data, dataLen);

	ReadHeader(reader);
	ReadNameMap(reader);
}

void ZenAsset::Invalidate() {
	memcpy_s(m_Buffer + EndNameMapPos - NameMap[NameMap.size() - 1].Name.size() - NameMap[NameMap.size() - 2].Name.size(),
			 NameMap[NameMap.size() - 2].Name.size(),
			 std::regex_replace(NameMap[NameMap.size() - 2].Name, std::regex("_"), "1").c_str(),
			 NameMap[NameMap.size() - 2].Name.size()
	);
}

void ZenAsset::ReadHeader(FArchive& reader) {
	reader.Seek(0);

	reader << bHasVersioningInfo;
	reader << HeaderSize;
	reader << MappedName;
	reader << PackageFlags;
	reader << CookedHeaderSize;
	reader << ImportedPublicExportHashesOffset;
	reader << ImportMapOffset;
	reader << ExportMapOffset;
	reader << ExportBundleEntriesOffset;
	reader << DependencyBundleHeadersOffset;
	reader << DependencyBundleEntriesOffset;
	reader << ImportedPackageNamesOffset;
}

void ZenAsset::ReadNameMap(FArchive& reader) {
	NameMap = FNameEntrySerialized::LoadNameBatch(reader);
	EndNameMapPos = reader.Tell();
}

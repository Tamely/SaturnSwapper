export module Saturn.Asset.ZenAsset;

import <cstdint>;
import <vector>;

import Saturn.Readers.FArchive;

import Saturn.Asset.MappedName;
import Saturn.Structs.NameEntrySerialized;

export class ZenAsset {
public:
	ZenAsset(uint8_t* data, size_t dataLen);
public:
	void Invalidate();
private:
	void ReadHeader(FArchive& reader);
	void ReadNameMap(FArchive& reader);
protected:
	uint32_t bHasVersioningInfo;
	uint32_t HeaderSize;
	FMappedName MappedName;
	uint32_t PackageFlags;
	uint32_t CookedHeaderSize;
	uint32_t ImportedPublicExportHashesOffset;
	uint32_t ImportMapOffset;
	uint32_t ExportMapOffset;
	uint32_t ExportBundleEntriesOffset;
	uint32_t DependencyBundleHeadersOffset;
	uint32_t DependencyBundleEntriesOffset;
	uint32_t ImportedPackageNamesOffset;

	uint32_t EndNameMapPos;
	std::vector<FNameEntrySerialized> NameMap;
private:
	uint8_t* m_Buffer;
	size_t m_BufferLen;
};
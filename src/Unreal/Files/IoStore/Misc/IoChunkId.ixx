export module Saturn.Structs.IoChunkId;

import <cstdint>;
import <memory>;

import Saturn.Readers.FArchive;

#define BYTESWAP_ORDER16_unsigned(x) ((((x) >> 8) & 0xff) + (((x) << 8) & 0xff00))

export enum class EIoChunkType : uint8_t {
	Invalid = 0,
	ExportBundleData = 1,
	BulkData = 2,
	OptionalBulkData = 3,
	MemoryMappedBulkData = 4,
	ScriptObjects = 5,
	ContainerHeader = 6,
	ExternalFile = 7,
	ShaderCodeLibrary = 8,
	ShaderCode = 9,
	PackageStoreEntry = 10,
	DerivedData = 11,
	EditorDerivedData = 12,
	PackageResource = 13,
	MAX
};

export struct FIoChunkId {
public:
	static const FIoChunkId InvalidChunkId;

	friend uint32_t GetTypeHash(FIoChunkId InId) {
		uint32_t Hash = 5381;
		for (int i = 0; i < sizeof Id; ++i) {
			Hash = Hash * 33 + InId.Id[i];
		}
		return Hash;
	}

	friend FArchive& operator<<(FArchive& Ar, FIoChunkId& ChunkId) {
		//ChunkId.Position = Ar.Tell();
		Ar.Serialize(&ChunkId.Id, sizeof FIoChunkId::Id);
		return Ar;
	}

	friend FArchive& operator>>(FArchive& Ar, FIoChunkId& ChunkId) {
		Ar.WriteBuffer(&ChunkId.Id, sizeof FIoChunkId::Id);
		return Ar;
	}

	inline bool operator ==(const FIoChunkId& Rhs) const {
		return 0 == memcmp(Id, Rhs.Id, sizeof Id);
	}

	inline bool operator !=(const FIoChunkId& Rhs) const {
		return !(*this == Rhs);
	}

	void Set(const void* InIdPtr, size_t InSize) {
		memcpy(Id, InIdPtr, sizeof Id);
	}

	inline bool IsValid() const {
		return *this != InvalidChunkId;
	}

	inline void Invalidate() {
		memset(Id, 0, 8);
	}

	inline const uint64_t GetChunkId() { return *reinterpret_cast<uint64_t*>(&Id[0]); }
	inline const uint16_t GetChunkIndex() { return *reinterpret_cast<uint16_t*>(&Id[8]); }

	inline const uint8_t* GetData() const { return Id; }
	inline uint32_t	GetSize() const { return sizeof Id; }
	//inline uint64_t GetPosition() const { return Position; }

	EIoChunkType GetChunkType() const {
		return static_cast<EIoChunkType>(Id[11]);
	}
private:
	static inline FIoChunkId CreateEmptyId(){
		FIoChunkId ChunkId;
		uint8_t Data[12] = { 0 };
		ChunkId.Set(Data, sizeof Data);

		return ChunkId;
	}

	//uint64_t Position;
	uint8_t	Id[12];
};

// This lets it be used as a key in maps
export template<>
struct std::hash<FIoChunkId> {
	std::size_t operator()(const FIoChunkId& k) const {
		return GetTypeHash(k);
	}
};

export	inline FIoChunkId CreateIoChunkId(uint64_t ChunkId, uint16_t ChunkIndex, EIoChunkType IoChunkType) {
	uint8_t Data[12] = { 0 };

	*reinterpret_cast<uint64_t*>(&Data[0]) = ChunkId;
	*reinterpret_cast<uint16_t*>(&Data[8]) = BYTESWAP_ORDER16_unsigned(ChunkIndex);
	*reinterpret_cast<uint8_t*>(&Data[11]) = static_cast<uint8_t>(IoChunkType);

	FIoChunkId IoChunkId;
	IoChunkId.Set(Data, 12);

	return IoChunkId;
}
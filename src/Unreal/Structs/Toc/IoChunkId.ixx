export module Saturn.Structs.IoChunkId;

import <cstdint>;
import <memory>;

import Saturn.Readers.FArchive;

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
		ChunkId.Position = Ar.Tell();
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

	inline const uint8_t* GetData() const { return Id; }
	inline uint32_t	GetSize() const { return sizeof Id; }
	inline uint64_t GetPosition() const { return Position; }
private:
	static inline FIoChunkId CreateEmptyId(){
		FIoChunkId ChunkId;
		uint8_t Data[12] = { 0 };
		ChunkId.Set(Data, sizeof Data);

		return ChunkId;
	}

	uint64_t Position;
	uint8_t	Id[12];
};
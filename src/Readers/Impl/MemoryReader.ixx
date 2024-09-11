export module Saturn.Readers.MemoryReader;

export import Saturn.Readers.FArchive;

import <intrin.h>;
#define memcpyfst(d, s, c) __movsb((unsigned char*)d, s, c)

export class FMemoryReader : public FArchive
{
public:

	__forceinline virtual int64_t TotalSize() override
	{
		return LimitSize;
	}

	void Serialize(void* Data, int64_t Num) override
	{
		if (Num && Offset + Num <= TotalSize())
		{
			memcpyfst(Data, Bytes + Offset, Num);
			Offset += Num;
		}
	}

	explicit FMemoryReader(std::vector<uint8_t>& InBytes, bool bFreeBuffer = false)
		: Bytes(InBytes.data())
		, LimitSize(InBytes.size())
		, bFree(bFreeBuffer)
	{
	}

	FMemoryReader(uint8_t* InBytes, size_t Size, bool bFreeBuffer = false)
		: Bytes(InBytes)
		, LimitSize(Size)
		, bFree(bFreeBuffer)
	{
	}

	__forceinline void SetLimitSize(int32_t NewLimitSize)
	{
		LimitSize = NewLimitSize;
	}

	__forceinline const uint8_t* GetBuffer() const
	{
		return Bytes;
	}

	__forceinline const uint8_t* GetBufferCur() const
	{
		return Bytes + Offset;
	}

	__forceinline void* Data() override
	{
		return static_cast<void*>(Bytes);
	}

	__forceinline void Seek(int64_t InPos) final
	{
		Offset = InPos;
	}

	__forceinline int64_t Tell() final
	{
		return Offset;
	}

	~FMemoryReader()
	{
		if (bFree && Bytes)
			delete Bytes;
	}

private:

	uint8_t* Bytes;
	int64_t	Offset = 0;
	size_t LimitSize;
	bool bFree;
};

// A memory reader for vectors that keeps the vector alive in memory
export class FBufferReader : public FMemoryReader
{
public:

	FBufferReader(std::vector<uint8_t> Arr) : FMemoryReader(Arr), Buffer(std::move(Arr))
	{
	}

private:

	std::vector<uint8_t> Buffer;
};
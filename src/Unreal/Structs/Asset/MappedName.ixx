export module Saturn.Asset.MappedName;

import <cstdint>;

import Saturn.Readers.FArchive;

export struct FMappedName {
private:
	static constexpr uint32_t InvalidIndex = ~uint32_t(0);
	static constexpr uint32_t IndexBits = 30u;
	static constexpr uint32_t IndexMask = (1u << IndexBits) - 1u;
	static constexpr uint32_t TypeMask = ~IndexMask;
	static constexpr uint32_t TypeShift = IndexBits;
public:
	FMappedName() {}

	enum class EType
	{
		Package,
		Container,
		Global
	};

	inline uint32_t GetIndex() const {
		return Index & IndexMask;
	}

	inline uint32_t GetNumber() const {
		return Number;
	}

	friend FArchive& operator<<(FArchive& Ar, FMappedName& MappedName);
private:
	inline FMappedName(const uint32_t InIndex, const uint32_t InNumber)
		: Index(InIndex), Number(InNumber) {}

	uint32_t Index = InvalidIndex;
	uint32_t Number = InvalidIndex;
};
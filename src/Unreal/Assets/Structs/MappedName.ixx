export module Saturn.Structs.MappedName;

import <cstdint>;

import Saturn.Structs.Name;
import Saturn.Readers.FArchive;

export struct FMappedName {
private:
	static constexpr uint32_t InvalidIndex = ~uint32_t(0);
	static constexpr uint32_t IndexBits = 30u;
	static constexpr uint32_t IndexMask = (1u << IndexBits) - 1u;
	static constexpr uint32_t TypeMask = ~IndexMask;
	static constexpr uint32_t TypeShift = IndexBits;
public:
	enum class EType
	{
		Package,
		Container,
		Global
	};

	inline FMappedName() = default;

	static inline FMappedName Create(const uint32_t InIndex, const uint32_t InNumber, EType InType) {
		return FMappedName((uint32_t(InType) << TypeShift) | InIndex, InNumber);
	}

	inline bool IsValid() const {
		return Index != InvalidIndex && Number != InvalidIndex;
	}

	inline EType GetType() const {
		return static_cast<EType>(uint32_t((Index & TypeMask) >> TypeShift));
	}

	inline bool IsGlobal() const {
		return ((Index & TypeMask) >> TypeShift) != 0;
	}

	inline uint32_t GetIndex() const {
		return Index & IndexMask;
	}

	inline uint32_t GetNumber() const {
		return Number;
	}

	inline bool operator!=(FMappedName Other) const {
		return Index != Other.Index || Number != Other.Number;
	}

	friend FArchive& operator<<(FArchive& Ar, FMappedName& MappedName);
private:
	inline FMappedName(const uint32_t InIndex, const uint32_t InNumber)
		: Index(InIndex), Number(InNumber) {}

	uint32_t Index = InvalidIndex;
	uint32_t Number = InvalidIndex;
};
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

	inline FMappedName() = default;

	static inline FMappedName Create(const uint32_t InIndex, const uint32_t InNumber, EType InType) {
		return FMappedName((uint32_t(InType) << TypeShift) | InIndex, InNumber);
	}

	static inline FMappedName FromMinimalName(const FMinimalName& MinimalName) {
		return *reinterpret_cast<const FMappedName*>(&MinimalName);
	}

	static inline bool IsResolvedToMinimalName(const FMinimalName& MinimalName) {
		// Not completely safe, relies on that no FName will have its Index and Number equal to UINT32_MAX
		const FMappedName MappedName = FromMinimalName(MinimalName);
		return MappedName.IsValid();
	}

	static inline FName SafeMinimalNameToName(const FMinimalName& MinimalName) {
		return IsResolvedToMinimalName(MinimalName) ? MinimalNameToName(MinimalName) : NAME_none;
	}

	inline FMinimalName ToUnresolvedMinimalName() const {
		return *reinterpret_cast<const FMinimalName*>(this);
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
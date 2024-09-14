export module Saturn.Structs.Guid;

import Saturn.Readers.FArchive;

import <cstdint>;

export struct FGuid {
public:
	constexpr FGuid() : A(0), B(0), C(0), D(0) {}
	explicit constexpr FGuid(uint32_t InA, uint32_t InB, uint32_t InC, uint32_t InD) : A(InA), B(InB), C(InC), D(InD) {}
public:
	bool operator==(const FGuid& Y) const {
		return ((A ^ Y.A) | (B ^ Y.B) | (C ^ Y.C) | (D ^ Y.D)) == 0;
	}

	bool operator!=(const FGuid& Y) const {
		return ((A ^ Y.A) | (B ^ Y.B) | (C ^ Y.C) | (D ^ Y.D)) != 0;
	}

	friend __forceinline FArchive& operator<<(FArchive& Ar, FGuid& value) {
		Ar << value.A;
		Ar << value.B;
		Ar << value.C;
		Ar << value.D;

		return Ar;
	}

	void Invalidate() {
		A = B = C = D = 0;
	}
public:
	uint32_t A;
	uint32_t B;
	uint32_t C;
	uint32_t D;
};

export template<>
struct std::hash<FGuid> {
	std::size_t operator()(const FGuid& Guid) const {
		return std::hash<uint32_t>()(Guid.A) ^ std::hash<uint32_t>()(Guid.B) ^ std::hash<uint32_t>()(Guid.C) ^ std::hash<uint32_t>()(Guid.D);
	}
};
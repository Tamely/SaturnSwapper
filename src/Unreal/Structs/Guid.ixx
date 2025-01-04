export module Saturn.Structs.Guid;

import Saturn.Readers.FArchive;

import <cstdint>;
import <string>;
import <sstream>;
import <iomanip>;

export struct FGuid {
public:
	FGuid() : A(0), B(0), C(0), D(0) {}
	FGuid(uint32_t InA, uint32_t InB, uint32_t InC, uint32_t InD) : A(InA), B(InB), C(InC), D(InD) {}
	FGuid(const std::string& HexString) : A(StringToUInt32(HexString.substr(0, 8))), B(StringToUInt32(HexString.substr(8, 8))), C(StringToUInt32(HexString.substr(16, 8))), D(StringToUInt32(HexString.substr(24, 8))) {}
public:
	bool operator==(const FGuid& Y) const {
		return ((A ^ Y.A) | (B ^ Y.B) | (C ^ Y.C) | (D ^ Y.D)) == 0;
	}

	bool operator!=(const FGuid& Y) const {
		return ((A ^ Y.A) | (B ^ Y.B) | (C ^ Y.C) | (D ^ Y.D)) != 0;
	}

	friend __forceinline FArchive& operator<<(FArchive& Ar, FGuid& Value) {
		Ar << Value.A;
		Ar << Value.B;
		Ar << Value.C;
		Ar << Value.D;

		return Ar;
	}

	friend __forceinline FArchive& operator>>(FArchive& Ar, FGuid& Value) {
		Ar >> Value.A;
		Ar >> Value.B;
		Ar >> Value.C;
		Ar >> Value.D;

		return Ar;
	}

	void Invalidate() {
		A = B = C = D = 0;
	}
private:
	uint32_t StringToUInt32(const std::string& hexStr) {
		uint32_t value;
		std::stringstream ss;
		ss << std::hex << hexStr;
		ss >> value;
		return value;
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
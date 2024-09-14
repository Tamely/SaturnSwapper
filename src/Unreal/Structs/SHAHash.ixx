export module Saturn.Structs.SHAHash;

import Saturn.Readers.FArchive;

import <cstdint>;
import <memory>;

export struct FSHAHash {
public:
	static const int SIZE = 20;

	friend FArchive& operator<<(FArchive& Ar, FSHAHash& Value) {
		Ar.Serialize(Value.Hash, FSHAHash::SIZE);

		return Ar;
	}

private:
	uint8_t Hash[SIZE];
};
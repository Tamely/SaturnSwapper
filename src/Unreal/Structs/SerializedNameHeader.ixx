export module Saturn.Structs.SerializedNameHeader;

import Saturn.Readers.FArchive;

import <cstdint>;

export class FSerializedNameHeader {
public:
	const int Size = 2;
public:
	FSerializedNameHeader() : _data0(0), _data1(0) {}

	FSerializedNameHeader(FArchive& Ar) {
		Ar << _data0;
		Ar << _data1;
	}
public:
	FSerializedNameHeader operator=(const FSerializedNameHeader& other) {
		_data0 = other._data0;
		_data1 = other._data1;
		return *this;
	}

	bool operator==(const FSerializedNameHeader& other) const {
		return _data0 == other._data0 && _data1 == other._data1;
	}

	bool operator!=(const FSerializedNameHeader& other) const {
		return !(*this == other);
	}

	bool IsUTF16() const {
		return (_data0 & 0x80) != 0;
	}

	uint32_t Length() const {
		return ((_data0 & 0x7F) << 8) + _data1;
	}
private:
	uint8_t _data0;
	uint8_t _data1;
};
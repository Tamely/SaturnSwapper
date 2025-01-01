export module Saturn.Structs.SerializedNameHeader;

import Saturn.Readers.FArchive;

import <cstdint>;

export struct FSerializedNameHeader {
	FSerializedNameHeader() {}
	FSerializedNameHeader(uint32_t Len, bool bIsUtf16) {
		Data[0] = uint8_t(bIsUtf16) << 7 | static_cast<uint8_t>(Len >> 8);
		Data[1] = static_cast<uint8_t>(Len);
	}

	uint8_t IsUtf16() const {
		return Data[0] & 0x80u;
	}

	uint32_t Len() const {
		return ((Data[0] & 0x7Fu) << 8) + Data[1];
	}

	uint32_t NumBytes() const {
		return IsUtf16() ? sizeof(wchar_t) * Len() : sizeof(char) * Len();
	}

	uint8_t Data[2];
};
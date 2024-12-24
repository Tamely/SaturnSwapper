export module Saturn.Core.StringTils;

import <vector>;
import <string>;
import <cstdint>;

export struct FStringTils {
    template <typename CharType, char LetterA>
    static inline void BytesToHexImpl(std::vector<uint8_t> Bytes, CharType* OutHex) {
        const auto NibbleToHex = [](uint8_t Value) -> CharType { return CharType(Value + (Value > 9 ? LetterA - 10 : '0')); };
        const uint8_t* Data = Bytes.data();

        for (const uint8_t* DataEnd = Data + Bytes.size(); Data != DataEnd; ++Data) {
            *OutHex++ = NibbleToHex(*Data >> 4);
            *OutHex++ = NibbleToHex(*Data & 15);
        }
    }

    static inline void BytesToHex(std::vector<uint8_t> Bytes, char* OutHex) {
        BytesToHexImpl<char, 'A'>(Bytes, OutHex);
    }

    static inline void BytesToHex(const uint8_t* In, int32_t Count, std::string& Out) {
        if (Count) {
            Out.resize(Count + /* add null terminator */ (Out.size() == 0));
            BytesToHex(std::vector<uint8_t>(In, In + Count), const_cast<char*>(Out.c_str()));
            Out[Out.size() - 1] = '\0';
        }
    }

    static inline std::string BytesToHex(const uint8_t* In, int32_t Count) {
        std::string Out;
        BytesToHex(In, Count, Out);
        return Out;
    }

    static inline  const uint8_t TCharToNibble(const char Hex) {
        if (Hex >= '0' && Hex <= '9') {
            return uint8_t(Hex - '0');
        }
        if (Hex >= 'A' && Hex <= 'F') {
            return uint8_t(Hex - 'A' + 10);
        }
        if (Hex >= 'a' && Hex <= 'f') {
            return uint8_t(Hex - 'a' + 10);
        }
        return 0;
    }

    template<typename CharType>
    static inline int32_t HexToBytesImpl(std::vector<CharType> Hex, uint8_t* const OutBytes) {
        const int32_t HexCount = Hex.size();
        const CharType* HexPos = Hex.data();
        const CharType* HexEnd = HexPos + HexCount;
        uint8_t* OutPos = OutBytes;
        if (const bool bPadNibble = (HexCount % 2) == 1) {
            *OutPos++ = TCharToNibble(*HexPos++);
        }
        while (HexPos != HexEnd) {
            const uint8_t HiNibble = uint8_t(TCharToNibble(*HexPos++) << 4);
            *OutPos++ = HiNibble | TCharToNibble(*HexPos++);
        }
        return static_cast<int32_t>(OutPos - OutBytes);
    }

    static inline int32_t HexToBytes(const std::string& Hex, uint8_t* OutBytes) {
        return HexToBytesImpl<char>(std::vector<char>(Hex.c_str(), Hex.c_str() + Hex.size()), OutBytes);
    }

    static inline int32_t HexToBytes(const char* Hex, uint8_t* OutBytes) {
        return HexToBytesImpl<char>(std::vector<char>(Hex, Hex + strlen(Hex)), OutBytes);
    }
};
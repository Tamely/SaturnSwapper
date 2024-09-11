export module Saturn.Encryption.AES;

import <string>;

export struct FAESKey {
	static constexpr int KeySize = 32;
	static constexpr int AESBlockSize = 16;

	uint8_t Key[KeySize];

	FAESKey();
	FAESKey(std::string KeyString);

	bool operator==(const FAESKey& Other) const;
	bool IsValid() const;
	std::string ToString();
	void DecryptData(uint8_t* Contents, uint32_t NumBytes) const;
};
export module Saturn.Structs.FileModification;

import <string>;
import <cstdint>;

export struct FFileModification {
	std::string FilePath;
	uint64_t BlockOffset;
	uint8_t* CompressedBlock;
};
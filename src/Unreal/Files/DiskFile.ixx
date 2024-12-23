module;

#include "Saturn/Defines.h"

export module Saturn.Files.DiskFile;

import <string>;
import <vector>;

export class IDiskFile {
public:
    virtual std::string GetDiskPath() = 0;
    virtual std::vector<uint8_t> ReadEntry(struct FFileEntryInfo& Entry) = 0;
};
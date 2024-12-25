module;

#include "Saturn/Defines.h"

export module Saturn.Structs.IoContainerFlags;

import <cstdint>;

export enum class EIoContainerFlags : uint8_t {
	None,
	Compressed = (1 << 0),
	Encrypted = (1 << 1),
	Signed = (1 << 2),
	Indexed = (1 << 3),
	OnDemand = (1 << 4),
};

ENUM_CLASS_FLAGS(EIoContainerFlags);
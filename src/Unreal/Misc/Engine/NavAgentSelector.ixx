export module Saturn.Engine.NavAgentSelector;

import <cstdint>;

import Saturn.Readers.FArchive;

export struct FNavAgentSelector {
    uint32_t PackedBits;

    friend FArchive& operator<<(FArchive& Ar, FNavAgentSelector& Nav) {
        return Ar << Nav.PackedBits;
    }

    friend FArchive& operator>>(FArchive& Ar, FNavAgentSelector& Nav) {
        return Ar >> Nav.PackedBits;
    }
};
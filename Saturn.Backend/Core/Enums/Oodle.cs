namespace Saturn.Backend.Core.Enums;

public enum OodleFormat : uint
{
    LZH = 0,
    LZHLW = 1,
    LZNIB = 2,
    None = 3,
    LZB16 = 4,
    LZBLW = 5,
    LZA = 6,
    LZNA = 7,
    Kraken = 8,
    Mermaid = 9,
    BitKnit = 10,
    Selkie = 11,
    Hydra = 12,
    Leviathan = 13
}

public enum OodleCompressionLevel : uint
{
    None = 0,
    SuperFast = 1,
    VeryFast = 2,
    Fast = 3,
    Normal = 4,
    Optimal1 = 5,
    Optimal2 = 6,
    Optimal3 = 7,
    Optimal4 = 8,
    Optimal5 = 9
}
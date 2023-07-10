using System.Collections.Generic;

namespace Saturn.Backend.Data.Plugins.TEMPORARY;

public class SoftObjectSwap
{
    public string Search { get; set; }
    public string Replace { get; set; }
}

public class ByteSwap
{
    public byte[] Search { get; set; }
    public byte[] Replace { get; set; }
}

public class Asset
{
    public string AssetPath { get; set; }

    // These can be null
    public List<SoftObjectSwap> SoftObjectSwaps { get; set; }
    public List<ByteSwap> ByteSwaps { get; set; }
    public string AssetSwap { get; set; }
}

public class Plugin
{
    public string Name { get; set; }
    public string Author { get; set; }
    public string Icon { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }
    public List<Asset> Assets { get; set; }
    
    // This can be null
    public List<string> Downloads { get; set; }
}
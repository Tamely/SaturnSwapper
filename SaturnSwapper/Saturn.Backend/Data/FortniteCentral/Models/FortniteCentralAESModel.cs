using System.Collections.Generic;
using Newtonsoft.Json;

namespace Saturn.Backend.Data.FortniteCentral.Models;

public class DynamicKey
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("guid")]
    public string Guid { get; set; }

    [JsonProperty("keychain")]
    public string Keychain { get; set; }

    [JsonProperty("fileCount")]
    public int FileCount { get; set; }

    [JsonProperty("hasHighResTextures")]
    public bool HasHighResTextures { get; set; }

    [JsonProperty("size")]
    public Size Size { get; set; }
}

public class FortniteCentralAESModel
{
    [JsonProperty("version")]
    public string Version { get; set; }

    [JsonProperty("mainKey")]
    public string MainKey { get; set; }

    [JsonProperty("dynamicKeys")]
    public List<DynamicKey> DynamicKeys { get; set; }

    [JsonProperty("unloaded")]
    public List<Unloaded> Unloaded { get; set; }
}

public class Size
{
    [JsonProperty("raw")]
    public int Raw { get; set; }

    [JsonProperty("formatted")]
    public string Formatted { get; set; }
}

public class Unloaded
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("guid")]
    public string Guid { get; set; }

    [JsonProperty("fileCount")]
    public int FileCount { get; set; }

    [JsonProperty("hasHighResTextures")]
    public bool HasHighResTextures { get; set; }

    [JsonProperty("size")]
    public Size Size { get; set; }
}
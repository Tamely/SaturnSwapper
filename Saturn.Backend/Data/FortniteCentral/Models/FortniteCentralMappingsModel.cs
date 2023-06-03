using System;
using Newtonsoft.Json;

namespace Saturn.Backend.Data.FortniteCentral.Models;

public class Meta
{
    [JsonProperty("version")]
    public string Version { get; set; }

    [JsonProperty("compressionMethod")]
    public string CompressionMethod { get; set; }

    [JsonProperty("platform")]
    public string Platform { get; set; }
}

public class FortniteCentralMappingsModel
{
    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("fileName")]
    public string FileName { get; set; }

    [JsonProperty("hash")]
    public string Hash { get; set; }

    [JsonProperty("length")]
    public int Length { get; set; }

    [JsonProperty("uploaded")]
    public DateTime Uploaded { get; set; }

    [JsonProperty("meta")]
    public Meta Meta { get; set; }
}


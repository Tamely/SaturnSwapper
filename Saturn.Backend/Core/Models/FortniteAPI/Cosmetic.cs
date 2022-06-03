using Newtonsoft.Json;
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System;
using System.Collections.Generic;
using System.IO;

namespace Saturn.Backend.Core.Models.FortniteAPI
{
    public class Type
    {
        [JsonProperty("value")] public string Value { get; set; }

        [JsonProperty("displayValue")] public string DisplayValue { get; set; }

        [JsonProperty("backendValue")] public string BackendValue { get; set; }
    }

    public class Rarity
    {
        [JsonProperty("value")] public string Value { get; set; }

        [JsonProperty("displayValue")] public string DisplayValue { get; set; }

        [JsonProperty("backendValue")] public string BackendValue { get; set; }
    }

    public class Series
    {
        [JsonProperty("value")] public string Value { get; set; }

        [JsonProperty("image")] public string Image { get; set; }

        [JsonProperty("backendValue")] public string BackendValue { get; set; }
    }

    public class Set
    {
        [JsonProperty("value")] public string Value { get; set; }

        [JsonProperty("text")] public string Text { get; set; }

        [JsonProperty("backendValue")] public string BackendValue { get; set; }
    }

    public class Images
    {
        [JsonProperty("smallIcon")] public string SmallIcon { get; set; }

        [JsonProperty("icon")] public string Icon { get; set; }

        [JsonProperty("featured")] public string Featured { get; set; }

        [JsonProperty("other")] public object Other { get; set; }
    }
    
    public class Option
    {
        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }
    }

    public class Variants
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("options")]
        public List<Option> Options { get; set; }
    }

    public class Cosmetic
    {
        [JsonProperty("streamImage")] public MemoryStream? StreamImage { get; set; }
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("type")] public Type Type { get; set; }

        [JsonProperty("rarity")] public Rarity Rarity { get; set; }

        [JsonProperty("series")] public Series? Series { get; set; }

        [JsonProperty("set")] public Set Set { get; set; }

        [JsonProperty("introduction")] public object Introduction { get; set; }

        [JsonProperty("images")] public Images Images { get; set; }

        [JsonProperty("variants")] public List<Variants>? Variants { get; set; }

        [JsonProperty("searchTags")] public object SearchTags { get; set; }

        [JsonProperty("gameplayTags")] public List<string> GameplayTags { get; set; }

        [JsonProperty("metaTags")] public object MetaTags { get; set; }

        [JsonProperty("showcaseVideo")] public object ShowcaseVideo { get; set; }

        [JsonProperty("dynamicPakId")] public object DynamicPakId { get; set; }

        [JsonProperty("displayAssetPath")] public string DisplayAssetPath { get; set; }

        [JsonProperty("definitionPath")] public string DefinitionPath { get; set; }

        [JsonProperty("path")] public string Path { get; set; }

        [JsonProperty("added")] public DateTime Added { get; set; }

        [JsonProperty("shopHistory")] public List<DateTime> ShopHistory { get; set; }

        [JsonProperty("isConverted")] public bool IsConverted { get; set; }

        [JsonProperty("printColor")] public Colors PrintColor { get; set; } = Colors.C_WHITE;
        [JsonProperty("isPickingStyles")] public bool IsPickingStyles { get; set; } = false;
        [JsonProperty("hatType")] public HatTypes HatTypes { get; set; } = HatTypes.HT_FaceACC;
        [JsonProperty("cosmeticOptions")] public List<SaturnItem> CosmeticOptions { get; set; }  = new List<SaturnItem>();
        [JsonProperty("isRandom")] public bool IsRandom { get; set; }
        [JsonProperty("isCloudAdded")] public bool IsCloudAdded { get; set; }
        [JsonProperty("variantChannel")] public string VariantChannel { get; set; }
        [JsonProperty("variantTag")] public string? VariantTag { get; set; }
    }

    public class CosmeticList
    {
        [JsonProperty("status")] public int Status { get; set; }

        [JsonProperty("data")] public List<Cosmetic> Data { get; set; }
    }
}
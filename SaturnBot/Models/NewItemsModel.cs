using System.Text.Json.Serialization;

namespace SaturnBot.Models;

// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Type
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("displayValue")]
        public string DisplayValue { get; set; }

        [JsonPropertyName("backendValue")]
        public string BackendValue { get; set; }
    }

    public class Rarity
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("displayValue")]
        public string DisplayValue { get; set; }

        [JsonPropertyName("backendValue")]
        public string BackendValue { get; set; }
    }

    public class Series
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("backendValue")]
        public string BackendValue { get; set; }
    }

    public class Set
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("backendValue")]
        public string BackendValue { get; set; }
    }

    public class Introduction
    {
        [JsonPropertyName("chapter")]
        public string Chapter { get; set; }

        [JsonPropertyName("season")]
        public string Season { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("backendValue")]
        public int BackendValue { get; set; }
    }

    public class Images
    {
        [JsonPropertyName("smallIcon")]
        public string SmallIcon { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("featured")]
        public string Featured { get; set; }

        [JsonPropertyName("other")]
        public object Other { get; set; }
    }

    public class Option
    {
        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }
    }

    public class Variant
    {
        [JsonPropertyName("channel")]
        public string Channel { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("options")]
        public List<Option> Options { get; set; }
    }

    public class Item
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("type")]
        public Type Type { get; set; }

        [JsonPropertyName("rarity")]
        public Rarity Rarity { get; set; }

        [JsonPropertyName("series")]
        public Series Series { get; set; }

        [JsonPropertyName("set")]
        public Set Set { get; set; }

        [JsonPropertyName("introduction")]
        public Introduction Introduction { get; set; }

        [JsonPropertyName("images")]
        public Images Images { get; set; }

        [JsonPropertyName("variants")]
        public List<Variant> Variants { get; set; }

        [JsonPropertyName("searchTags")]
        public object SearchTags { get; set; }

        [JsonPropertyName("gameplayTags")]
        public List<string> GameplayTags { get; set; }

        [JsonPropertyName("metaTags")]
        public object MetaTags { get; set; }

        [JsonPropertyName("showcaseVideo")]
        public string ShowcaseVideo { get; set; }

        [JsonPropertyName("dynamicPakId")]
        public string DynamicPakId { get; set; }

        [JsonPropertyName("displayAssetPath")]
        public object DisplayAssetPath { get; set; }

        [JsonPropertyName("definitionPath")]
        public string DefinitionPath { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("added")]
        public DateTime Added { get; set; }

        [JsonPropertyName("shopHistory")]
        public List<DateTime> ShopHistory { get; set; }

        [JsonPropertyName("itemPreviewHeroPath")]
        public string ItemPreviewHeroPath { get; set; }

        [JsonPropertyName("builtInEmoteIds")]
        public List<string> BuiltInEmoteIds { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("build")]
        public string Build { get; set; }

        [JsonPropertyName("previousBuild")]
        public string PreviousBuild { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("lastAddition")]
        public DateTime LastAddition { get; set; }

        [JsonPropertyName("items")]
        public List<Item> Items { get; set; }
    }

    public class NewItemsModel
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }


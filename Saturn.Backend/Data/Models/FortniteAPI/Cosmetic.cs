using System;
using System.Collections.Generic;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.Models.FortniteAPI
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

    public class Cosmetic
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("type")] public Type Type { get; set; }

        [JsonProperty("rarity")] public Rarity Rarity { get; set; }

        [JsonProperty("series")] public Series Series { get; set; }

        [JsonProperty("set")] public Set Set { get; set; }

        [JsonProperty("introduction")] public object Introduction { get; set; }

        [JsonProperty("images")] public Images Images { get; set; }

        [JsonProperty("variants")] public object Variants { get; set; }

        [JsonProperty("searchTags")] public object SearchTags { get; set; }

        [JsonProperty("gameplayTags")] public List<string> GameplayTags { get; set; }

        [JsonProperty("metaTags")] public object MetaTags { get; set; }

        [JsonProperty("showcaseVideo")] public object ShowcaseVideo { get; set; }

        [JsonProperty("dynamicPakId")] public object DynamicPakId { get; set; }

        [JsonProperty("displayAssetPath")] public string DisplayAssetPath { get; set; }

        [JsonProperty("definitionPath")] public object DefinitionPath { get; set; }

        [JsonProperty("path")] public string Path { get; set; }

        [JsonProperty("added")] public DateTime Added { get; set; }

        [JsonProperty("shopHistory")] public List<DateTime> ShopHistory { get; set; }

        [JsonProperty("isConverted")] public bool IsConverted { get; set; }

        [JsonProperty("printColor")] public Colors PrintColor { get; set; } = Colors.C_WHITE;
        [JsonProperty("isPickingStyles")] public bool IsPickingStyles { get; set; } = false;
        [JsonProperty("hatType")] public HatTypes HatTypes { get; set; } = HatTypes.HT_FaceACC;
        [JsonProperty("cosmeticOptions")] public List<SaturnItem> CosmeticOptions { get; set; } = new()
        {
            new SaturnItem
            {
                ItemDefinition = "CID_A_311_Athena_Commando_F_ScholarFestiveWinter",
                Name = "Blizzabelle",
                Description = "Voted Teen Queen of Winterfest by a jury of her witchy peers.",
                Icon = "https://fortnite-api.com/images/cosmetics/br/cid_a_311_athena_commando_f_scholarfestivewinter/smallicon.png",
                Rarity = "Rare"
            },
            new SaturnItem
            {
                ItemDefinition = "CID_A_007_Athena_Commando_F_StreetFashionEclipse",
                Name = "Ruby Shadows",
                Description = "Sometimes you gotta go dark.",
                Icon = "https://fortnite-api.com/images/cosmetics/br/cid_a_007_athena_commando_f_streetfashioneclipse/smallicon.png",
                Rarity = "Epic",
                Series = "ShadowSeries"
            },
            new SaturnItem
            {
                ItemDefinition = "CID_936_Athena_Commando_F_RaiderSilver",
                Name = "Diamond Diva",
                Description = "Synthetic diamonds need not apply.",
                Icon = "https://fortnite-api.com/images/cosmetics/br/cid_936_athena_commando_f_raidersilver/smallicon.png",
                Rarity = "Rare"
            },
            new SaturnItem
            {
                ItemDefinition = "CID_784_Athena_Commando_F_RenegadeRaiderFire",
                Name = "Blaze",
                Description = "Fill the world with flames.",
                Icon = "https://fortnite-api.com/images/cosmetics/br/cid_784_athena_commando_f_renegaderaiderfire/smallicon.png",
                Rarity = "Legendary"
            }
        };
    }

    public class CosmeticList
    {
        [JsonProperty("status")] public int Status { get; set; }

        [JsonProperty("data")] public List<Cosmetic> Data { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUE4Parse_Conversion.Textures;
using CUE4Parse.GameTypes.FN.Enums;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Objects.GameplayTags;
using CUE4Parse.Utils;
using Saturn.Backend.Data.Swapper.Generation;
using Saturn.Backend.Data.Swapper.Styles;
using Saturn.Backend.Data.Variables;
using Serilog;
using SkiaSharp;

namespace Saturn.Backend.Data.Swapper.Assets;

public class AssetSelectorItem
{
    public SKBitmap IconBitmap;
    public SKBitmap FullBitmap;
    public UTexture2D PreviewTexture;

    public UObject Asset { get; set; }
    public EAssetType Type { get; set; }
    public SKData FullSource { get; set; }
    public string DisplayName { get; set; }
    public string DisplayNameSource { get; set; }
    public string Description { get; set; }
    public bool IsRandom { get; set; }
    public string TooltipName { get; set; }
    public string ID { get; set; }
    public string HID { get; set; }
    public EFortRarity Rarity { get; set; }
    public int SeasonNumber { get; set; }
    public string Series { get; set; }
    public List<StyleSelector> Styles { get; set; }
    public OptionHandler OptionHandler { get; set; }

    public bool HiddenAsset;

    public AssetSelectorItem() {}
    public AssetSelectorItem(UObject asset, UTexture2D previewTexture, EAssetType type, bool isRandomSelector = false,
        FText? displayNameOverride = null, string? descriptionOverride = null, bool hiddenAsset = false)
    {
        Type = type;

        Asset = asset;
        var displayName = displayNameOverride;
        displayName ??= asset.GetOrDefault("DisplayName", new FText("Unnamed"));
        HiddenAsset = hiddenAsset;

        HID = asset.GetOrDefault("HeroDefinition", new UObject()).GetPathName();

        DisplayName = displayName.Text;
        if (DisplayName.Equals("TBD") || string.IsNullOrWhiteSpace(DisplayName))
        {
            DisplayName = asset.Name;
        }

        if (displayName.TextHistory is FTextHistory.Base textHistory)
        {
            DisplayNameSource = textHistory.SourceString;
        }
        ID = asset.Name;
        Description = descriptionOverride ?? asset.GetOrDefault("Description", new FText("No Description.")).Text;

        Rarity = asset.GetOrDefault("Rarity", EFortRarity.Uncommon);
        if (asset.TryGetValue<UObject>(out var series, "Series"))
        {
            Series = series.GetOrDefault<FText>("DisplayName").Text;
        }

        TooltipName = $"{DisplayName} ({ID})";
        IsRandom = isRandomSelector;

        PreviewTexture = previewTexture;
    }

    public async Task OnSelected()
    {
        Styles = new();
        var styles = Asset.GetOrDefault("ItemVariants", Array.Empty<UObject>());
        foreach (var style in styles)
        {
            var channel = style.GetOrDefault("VariantChannelName", new FText("Unknown")).Text.ToLower().TitleCase();
            var optionsName = style.ExportType switch
            {
                "FortCosmeticCharacterPartVariant" => "PartOptions",
                "FortCosmeticMaterialVariant" => "MaterialOptions",
                "FortCosmeticParticleVariant" => "ParticleOptions",
                "FortCosmeticMeshVariant" => "MeshOptions",
                _ => null
            };
            
            if (optionsName is null) continue;

            var options = style.Get<FStructFallback[]>(optionsName);
            if (options.Length == 0) continue;

            var styleSelector = new StyleSelector(channel, options, IconBitmap);
            if (styleSelector.Options.Items.Count == 0) continue;
            Styles.Add(styleSelector);
        }
    }

    public async Task<ExportDataBase> ExportAssets()
    {
        var allStyles = new List<FStructFallback>();
        var styles = Asset.GetOrDefault("ItemVariants", Array.Empty<UObject>());
        foreach (var style in styles)
        {
            var optionsName = style.ExportType switch
            {
                "FortCosmeticCharacterPartVariant" => "PartOptions",
                "FortCosmeticMaterialVariant" => "MaterialOptions",
                "FortCosmeticParticleVariant" => "ParticleOptions",
                _ => null
            };

            if (optionsName is null) continue;

            var options = style.Get<FStructFallback[]>(optionsName);
            if (optionsName.Length == 0) continue;

            allStyles.AddRange(options);
        }

        ExportDataBase exportData = Type switch
        {
            _ => await AssetExportData.Create(Asset, Type, allStyles.ToArray())
        };

        Logger.Log($"Finished exporting all assets for {DisplayName}");

        return exportData;
    }

    public string GetHTMLImage()
    {
        if (FullSource == null)
        {
            var iconBitmap = PreviewTexture.Decode();
            if (iconBitmap is null) return "";
            IconBitmap = iconBitmap;

            FullBitmap = new SKBitmap(iconBitmap.Width, iconBitmap.Height, iconBitmap.ColorType, iconBitmap.AlphaType);
            using (var fullCanvas = new SKCanvas(FullBitmap))
            {
                fullCanvas.DrawBitmap(iconBitmap, 0, 0);
            }
                
            FullSource = FullBitmap.Encode(SKEncodedImageFormat.Png, 1);
        }

        return "data:image/png;base64," + Convert.ToBase64String(FullSource.ToArray());
    }
}
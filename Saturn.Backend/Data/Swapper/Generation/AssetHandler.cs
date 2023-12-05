using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CUE4Parse_Conversion.Textures;
using CUE4Parse.UE4.AssetRegistry.Objects;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.Utils;
using Microsoft.AspNetCore.Components.Forms;
using Saturn.Backend.Data.Swapper.Assets;
using Saturn.Backend.Data.Variables;
using SkiaSharp;

namespace Saturn.Backend.Data.Swapper.Generation;

public class AssetHandler
{
    public readonly AssetHandlerData SkinHandler = new()
    {
        AssetType = EAssetType.Outfit,
        TargetCollection = Constants.Cosmetics,
        ClassNames = new List<string> { "AthenaCharacterItemDefinition" },
        RemoveList = new List<string>() { "_NPC", "_TBD", "CID_VIP", "_Creative", "_SG" },
        IconGetter = asset =>
        {
            asset.TryGetValue(out UTexture2D previewImage, "SmallPreviewImage", "LargePreviewImage");
            if (asset.TryGetValue(out UObject heroDef, "HeroDefinition"))
            {
                heroDef.TryGetValue(out previewImage, "SmallPreviewImage", "LargePreviewImage");
            }

            return previewImage;
        }
    };

    public readonly AssetHandlerData BackpackHandler = new()
    {
        AssetType = EAssetType.Backpack,
        TargetCollection = Constants.Cosmetics,
        ClassNames = new List<string> { "AthenaBackpackItemDefinition" },
        RemoveList = new List<string> { "_STWHeroNoDefaultBackpack", "_TEST", "Dev_", "_NPC", "_TBD" },
        IconGetter = asset => asset.GetOrDefault<UTexture2D?>("SmallPreviewImage", "LargePreviewImage")
    };
    
    public readonly AssetHandlerData PickaxeHandler = new()
    {
        AssetType = EAssetType.Pickaxe,
        TargetCollection = Constants.Cosmetics,
        ClassNames = new List<string> { "AthenaPickaxeItemDefinition" },
        RemoveList = new List<string> { "Dev_", "TBD_" },
        IconGetter = asset =>
        {
            asset.TryGetValue(out UTexture2D? previewImage, "SmallPreviewImage", "LargePreviewImage");
            if (asset.TryGetValue(out UObject heroDef, "WeaponDefinition"))
            {
                heroDef.TryGetValue(out previewImage, "SmallPreviewImage", "LargePreviewImage");
            }

            return previewImage;
        }
    };

    public readonly AssetHandlerData GliderHandler = new()
    {
        AssetType = EAssetType.Glider,
        TargetCollection = Constants.Cosmetics,
        ClassNames = new List<string> { "AthenaGliderItemDefinition" },
        RemoveList = { },
        IconGetter = asset => asset.GetOrDefault<UTexture2D?>("SmallPreviewImage", "LargePreviewImage")
    };
    
    public readonly AssetHandlerData DanceHandler = new()
    {
        AssetType = EAssetType.Dance,
        TargetCollection = Constants.Cosmetics,
        ClassNames = new List<string> { "AthenaDanceItemDefinition" },
        RemoveList = { "_CT", "_NPC" },
        IconGetter = asset => asset.GetOrDefault<UTexture2D?>("SmallPreviewImage", "LargePreviewImage")
    };

    public AssetHandlerData Handler { get; private set; }

    public async Task SwitchHandler(AssetHandlerData Handler)
    {
       this.Handler = (AssetHandlerData)Handler.Clone();
    }

    public async Task Reset()
    {
        await SwitchHandler(Handler.AssetType switch
        {
            EAssetType.Outfit => SkinHandler,
            EAssetType.Backpack => BackpackHandler,
            EAssetType.Pickaxe => PickaxeHandler,
            EAssetType.Glider => GliderHandler,
            EAssetType.Dance => DanceHandler,
            _ => SkinHandler
        });
    }
    
    public async Task Initialize()
    {
        await SwitchHandler(SkinHandler);
        await Handler.Execute();
    }
}

public class AssetHandlerData : ICloneable
{
    public bool HasStarted { get; private set; }
    public bool IsOption { get; private set; }
    public Pauser PauseState { get; } = new();

    public EAssetType AssetType;
    public List<AssetSelectorItem>? TargetCollection;
    public List<string> ClassNames;
    public List<string> RemoveList = Enumerable.Empty<string>().ToList();
    public Func<UObject, UTexture2D?> IconGetter;
    public Func<UObject, FText?>? DisplayNameGetter;

    public async Task Execute()
    {
        if (HasStarted) return;
        HasStarted = true;
        
        var items = Constants.AssetDataBuffers.Where(x => ClassNames.Any(y => x.AssetClass.Text.Equals(y, StringComparison.OrdinalIgnoreCase))).ToList();
        Constants.CosmeticCount = items.Count;
        
        // We want to prioritize Random first because of parallel list positions not syncing
        var random = items.FirstOrDefault(x => x.AssetName.Text.Contains("Random", StringComparison.OrdinalIgnoreCase));
        if (random is not null)
        {
            items.Remove(random);
            await Load(random, AssetType, true);
        }

        items = items.OrderBy(x => Path.GetFileNameWithoutExtension(x.ObjectPath.SubstringAfter('_'))).ToList(); // so we're consistent with the tabs
        var chunked = items.Chunk(Constants.CHUNK_SIZE).ToArray();
        Constants.ChunkCount = chunked.Length;
        items = chunked[Constants.ChunkIndex].ToList();

        foreach (var item in items)
        {
            try
            {
                await Load(item, AssetType);
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to load {item.ObjectPath}", LogLevel.Error);
            }
        }
        
        TargetCollection.RemoveAll(x => x == null);
        TargetCollection = TargetCollection.OrderBy(x => x.ID).ToList();
        HasStarted = false;
    }
    
    public async Task Execute(string pattern)
    {
        if (HasStarted) return;
        HasStarted = true;
        
        var items = Constants.AssetDataBuffers.Where(x => ClassNames.Any(y => x.AssetClass.Text.Equals(y, StringComparison.OrdinalIgnoreCase))).ToList();

        items = items.OrderBy(x => Path.GetFileNameWithoutExtension(x.ObjectPath.SubstringAfter('_'))).ToList(); // so we're consistent with the tabs

        foreach (var item in items)
        {
            try
            {
                await Load(pattern, item, AssetType);
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to load {item.ObjectPath}", LogLevel.Error);
            }
        }
        
        TargetCollection.RemoveAll(x => x == null);
        TargetCollection = TargetCollection.OrderBy(x => x.ID).ToList();
        
        Constants.CosmeticCount = TargetCollection.Count;
        Constants.ChunkCount = 1;
        HasStarted = false;
    }
    
    public async Task<List<AssetSelectorItem>> ExecuteWithFileBias(IEnumerable<string> AllowedFileNames)
    {
        if (HasStarted) return new();
        HasStarted = true;
        IsOption = true;

        List<AssetSelectorItem> ReturnValue = new();
        
        var items = Constants.AssetDataBuffers.Where(x => ClassNames.Any(y => x.AssetClass.Text.Equals(y, StringComparison.OrdinalIgnoreCase))).ToList();

        var allowedFileNames = AllowedFileNames as string[] ?? AllowedFileNames.ToArray();
        if (allowedFileNames.Count() > 1)
            items.RemoveAll(i => !allowedFileNames.Any(x => i.ObjectPath.ToLower().Contains(x) || i.ObjectPath.ToLower().Contains("_random")));

        foreach (var item in items)
        {
            try
            {
                await LoadWithCustomReturn(ReturnValue, item, AssetType);
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to load {item.ObjectPath}", LogLevel.Error);
            }
        }
        
        ReturnValue.RemoveAll(x => x == null);
        ReturnValue = ReturnValue.OrderBy(x => x.ID).ToList();
        HasStarted = false;
        IsOption = false;

        return ReturnValue;
    }
    
    private async Task Load(string pattern, FAssetData data, EAssetType type, bool random = false, string? descriptionOverride = null)
    {
        var asset = await Constants.Provider.LoadObjectAsync(data.ObjectPath);
        await Load(pattern, asset, type, random, descriptionOverride);
    }

    private async Task Load(FAssetData data, EAssetType type, bool random = false, string? descriptionOverride = null)
    {
        var asset = await Constants.Provider.LoadObjectAsync(data.ObjectPath);
        await Load(asset, type, random, descriptionOverride);
    }
    
    private async Task LoadWithCustomReturn(List<AssetSelectorItem> collection, FAssetData data, EAssetType type, bool random = false, string? descriptionOverride = null)
    {
        var asset = await Constants.Provider.LoadObjectAsync(data.ObjectPath);
        await LoadWithCustomReturn(collection, asset, type, descriptionOverride);
    }

    private async Task Load(string pattern, UObject asset, EAssetType type, bool random = false, string? descriptionOverride = null)
    {
        await PauseState.WaitIfPaused();
        
        var previewImage = IconGetter(asset);
        previewImage ??= Constants.PlaceholderTexture;
        if (previewImage is null) return;

        var item = new AssetSelectorItem(asset, previewImage, type, random, DisplayNameGetter?.Invoke(asset), descriptionOverride, RemoveList.Any(x => asset.Name.Contains(x, StringComparison.OrdinalIgnoreCase)));

        if (item.DisplayName.ToLower().Contains(pattern.ToLower()) 
            || item.ID.ToLower().Contains(pattern.ToLower()) 
            || item.Description.ToLower().Contains(pattern.ToLower()))
        {
            Logger.Log($"Item: {item.DisplayName} | {item.ID} | {item.Description} | works!");
            TargetCollection.Add(item);
        }
    }
    
    private async Task Load(UObject asset, EAssetType type, bool random = false, string? descriptionOverride = null)
    {
        await PauseState.WaitIfPaused();
        
        var previewImage = IconGetter(asset);
        previewImage ??= Constants.PlaceholderTexture;
        if (previewImage is null) return;
        
        TargetCollection.Add(new AssetSelectorItem(asset, previewImage, type, random, DisplayNameGetter?.Invoke(asset), descriptionOverride, RemoveList.Any(x => asset.Name.Contains(x, StringComparison.OrdinalIgnoreCase))));
    }
    
    private async Task LoadWithCustomReturn(List<AssetSelectorItem> collection, UObject asset, EAssetType type, string? descriptionOverride = null)
    {
        await PauseState.WaitIfPaused();
        
        var previewImage = IconGetter(asset);
        previewImage ??= Constants.PlaceholderTexture;
        if (previewImage is null) return;
        
        collection.Add(new AssetSelectorItem(asset, previewImage, type, asset.GetPathName().EndsWith("_Random"), DisplayNameGetter?.Invoke(asset), descriptionOverride, RemoveList.Any(x => asset.Name.Contains(x, StringComparison.OrdinalIgnoreCase))));
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
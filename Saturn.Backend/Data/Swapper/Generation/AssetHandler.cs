using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CUE4Parse.UE4.AssetRegistry.Objects;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.Utils;
using Saturn.Backend.Data.Swapper.Assets;
using Saturn.Backend.Data.Variables;

namespace Saturn.Backend.Data.Swapper.Generation;

public class AssetHandler
{
    public readonly AssetHandlerData SkinHandler = new()
    {
        AssetType = EAssetType.Outfit,
        TargetCollection = Constants.Outfits,
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

    public async Task Initialize()
    {
        await SkinHandler.Execute();
    }
}

public class AssetHandlerData
{
    public bool HasStarted { get; private set; }
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
        
        // We want to prioritize Random first because of parallel list positions not syncing
        var random = items.FirstOrDefault(x => x.AssetName.Text.Contains("Random", StringComparison.OrdinalIgnoreCase));
        if (random is not null)
        {
            items.Remove(random);
            await Load(random, AssetType, true);
        }
        
        await Parallel.ForEachAsync(items, async (data, token) =>
        {
            try
            {
                await Load(data, AssetType);
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to load {data.ObjectPath}", LogLevel.Error);
            }
        });
    }

    private async Task Load(FAssetData data, EAssetType type, bool random = false, string? descriptionOverride = null)
    {
        var asset = await Constants.Provider.LoadObjectAsync(data.ObjectPath);
        await Load(asset, type, random, descriptionOverride);
    }

    private async Task Load(UObject asset, EAssetType type, bool random = false, string? descriptionOverride = null)
    {
        await PauseState.WaitIfPaused();
        
        var previewImage = IconGetter(asset);
        previewImage ??= Constants.PlaceholderTexture;
        if (previewImage is null) return;
        
        TargetCollection.Add(new AssetSelectorItem(asset, previewImage, type, random, DisplayNameGetter?.Invoke(asset), descriptionOverride, RemoveList.Any(x => asset.Name.Contains(x, StringComparison.OrdinalIgnoreCase))));
    }
}
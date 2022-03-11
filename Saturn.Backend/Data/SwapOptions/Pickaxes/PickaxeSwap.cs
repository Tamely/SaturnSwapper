using System;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Utils.Swaps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Objects.UObject;
using Saturn.Backend.Data.Models.FortniteAPI;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Services;
using Saturn.Backend.Data.Utils;

namespace Saturn.Backend.Data.SwapOptions.Pickaxes;

internal abstract class PickaxeSwap : AbstractSwap
{
    protected PickaxeSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon, rarityEnum)
    {
        Swaps = swaps;
    }

    public Dictionary<string, string> Swaps { get; }
}

public class AddPickaxes
{
    private async Task<Dictionary<string, string>> GetAssetsFromWID(string wid, DefaultFileProvider _provider)
    {
        var output = new Dictionary<string, string>();
        
        UObject export = await _provider.TryLoadObjectAsync(wid.Split('.')[0]) ?? new UObject();

        export.TryGetValue(out FSoftObjectPath Mesh, "WeaponMeshOverride");
        export.TryGetValue(out FSoftObjectPath[] Material, "WeaponMaterialOverrides");
        export.TryGetValue(out FSoftObjectPath SmallIcon, "SmallPreviewImage");
        export.TryGetValue(out FSoftObjectPath LargeIcon, "LargePreviewImage");
        export.TryGetValue(out FSoftObjectPath FX, "IdleEffect");
        export.TryGetValue(out FSoftObjectPath SwingFX, "SwingEffect");
        export.TryGetValue(out FSoftObjectPath OffhandSwingFX, "SwingEffectOffhandNiagara");
        FPropertyTagType? ImpactCue = null;
        if (export.TryGetValue(out UScriptMap ImpactPhysicalSurfaceSoundsMap, "ImpactPhysicalSurfaceSoundsMap"))
            ImpactPhysicalSurfaceSoundsMap.Properties.TryGetValue(
                ImpactPhysicalSurfaceSoundsMap.Properties.Keys.First(), out ImpactCue);
        FPropertyTagType? EquipCue = null;
        if (export.TryGetValue(out UScriptMap ReloadSoundsMap, "ReloadSoundsMap"))
            ReloadSoundsMap.Properties.TryGetValue(ReloadSoundsMap.Properties.Keys.First(), out EquipCue);
        FPropertyTagType? SwingCue = null;
        if (export.TryGetValue(out UScriptMap PrimaryFireSoundMap, "PrimaryFireSoundMap"))
            PrimaryFireSoundMap.Properties.TryGetValue(PrimaryFireSoundMap.Properties.Keys.First(), out SwingCue);
        export.TryGetValue(out FSoftObjectPath ActorClass, "WeaponActorClass");
        export.TryGetValue(out FSoftObjectPath Trail, "AnimTrails");
        export.TryGetValue(out FSoftObjectPath OffhandTrail, "AnimTrailsOffhand");
        output.Add("Rarity", export.TryGetValue(out EFortRarity Rarity, "Rarity")
            ? ((int)Rarity).ToString()
            : "1");

        string Series = "/";
        if (export.TryGetValue(out UObject SeriesObject, "Series"))
            Series = SeriesObject.GetPathName();

        output.Add("Mesh", string.IsNullOrWhiteSpace(Mesh.AssetPathName.Text) || Mesh.AssetPathName.Text == "None" ? "/" : Mesh.AssetPathName.Text);
        output.Add("Material", Material == null ? "/" : Material[0].AssetPathName.Text);
        output.Add("SmallIcon",
            string.IsNullOrWhiteSpace(SmallIcon.AssetPathName.Text) || SmallIcon.AssetPathName.Text == "None" ? "/" : SmallIcon.AssetPathName.Text);
        output.Add("LargeIcon",
            string.IsNullOrWhiteSpace(LargeIcon.AssetPathName.Text) || LargeIcon.AssetPathName.Text == "None" ? "/" : LargeIcon.AssetPathName.Text);
        output.Add("SwingFX", string.IsNullOrWhiteSpace(SwingFX.AssetPathName.Text) || SwingFX.AssetPathName.Text == "None" ? "/" : SwingFX.AssetPathName.Text);
        output.Add("OffhandSwingFX",
            string.IsNullOrWhiteSpace(OffhandSwingFX.AssetPathName.Text) || OffhandSwingFX.AssetPathName.Text == "None" ? "/" : OffhandSwingFX.AssetPathName.Text);
        output.Add("FX", string.IsNullOrWhiteSpace(FX.AssetPathName.Text) || FX.AssetPathName.Text == "None" ? "/" : FX.AssetPathName.Text);
        output.Add("SwingCue", SwingCue == null ? "/" : ((FSoftObjectPath)SwingCue.GenericValue).AssetPathName.Text);
        output.Add("EquipCue", EquipCue == null ? "/" : ((FSoftObjectPath)EquipCue.GenericValue).AssetPathName.Text);
        output.Add("ImpactCue", ImpactCue == null ? "/" : ((FSoftObjectPath)ImpactCue.GenericValue).AssetPathName.Text);
        output.Add("ActorClass",
            string.IsNullOrWhiteSpace(ActorClass.AssetPathName.Text) || ActorClass.AssetPathName.Text == "None" ? "/" : ActorClass.AssetPathName.Text);
        output.Add("Trail", string.IsNullOrWhiteSpace(Trail.AssetPathName.Text) || Trail.AssetPathName.Text == "None" ? "/" : Trail.AssetPathName.Text);
        output.Add("OffhandTrail",
            string.IsNullOrWhiteSpace(OffhandTrail.AssetPathName.Text) || OffhandTrail.AssetPathName.Text == "None" ? "/" : Trail.AssetPathName.Text);
        output.Add("Series", string.IsNullOrWhiteSpace(Series) || Series == "None" ? "/" : Series);

        return output;
    }

    private protected List<SaturnItem> PickaxeOptions = new List<SaturnItem>()
    {
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_408_MastermindShadow",
            Name = "Mayhem Scythe",
            Description = "Mayhem is golden...",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/pickaxe_id_408_mastermindshadow/smallicon.png",
            Rarity = "Rare"
        },
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_541_StreetFashionEclipseFemale",
            Name = "Shadow Slicer",
            Description = "Not even the darkness is safe.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/pickaxe_id_541_streetfashioneclipsefemale/smallicon.png",
            Rarity = "Epic",
            Series = "ShadowSeries"
        },
        new SaturnItem
        {
            ItemDefinition = "DefaultPickaxe",
            Name = "Default Pickaxe",
            Description = "Perfectly unremarkable.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/defaultpickaxe/smallicon.png",
            Rarity = "Common"
        },
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_713_GumballMale",
            Name = "Gum Brawler",
            Description = "Long-lasting sticky sledge action.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/pickaxe_id_713_gumballmale/smallicon.png",
            Rarity = "Rare"
        }
    };

    public async Task<Cosmetic> AddPickaxeOptions(Cosmetic pickaxe, ISwapperService swapperService,
        DefaultFileProvider _provider)
    {
        var WID = await swapperService.GetWIDByID(pickaxe.Id);

        if (WID == new UObject())
            return null;

        foreach (var option in PickaxeOptions)
        {
            Dictionary<string, string> swaps = await GetAssetsFromWID(WID.GetPathName(), _provider);

            string OGWID = (await swapperService.GetWIDByID(option.ItemDefinition)).GetPathName();
            Dictionary<string, string> OGSwaps = await GetAssetsFromWID(OGWID, _provider);

            bool bDontProceed = false;

            foreach (var (key, value) in swaps)
            {
                if (key == "ActorClass")
                    if (value == "/" || value != OGSwaps[key])
                    {
                        bDontProceed = true;
                        break;
                    }

                if (key == "Material")
                    if (value != "/" && OGSwaps[key] == "/")
                    {
                        bDontProceed = true;
                        break;
                    }

                if (key == "FX")
                    if (value != "/" && OGSwaps[key] == "/")
                    {
                        bDontProceed = true;
                        break;
                    }
            }

            if (bDontProceed) continue;
            
            option.Swaps = swaps;
            pickaxe.CosmeticOptions.Add(option);
        }
        
        if (pickaxe.CosmeticOptions.Count == 0)
        {
            pickaxe.CosmeticOptions.Add(new SaturnItem()
            {
                Name = "No options!",
                Description = "Send a picture of this to Tamely on Discord and tell him to add an option for this!",
                Rarity = "Epic",
                Icon = "img/Saturn.png"
            });
        }

        return pickaxe;
    }
}

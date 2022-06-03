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
        export.TryGetValue(out FSoftObjectPath NFX, "IdleEffectNiagara");
        export.TryGetValue(out FSoftObjectPath SwingFX, "SwingEffect");
        export.TryGetValue(out FSoftObjectPath OffhandSwingFX, "SwingEffectOffhandNiagara");
        FPropertyTagType? ImpactCue = null;
        if (export.TryGetValue(out UScriptMap ImpactPhysicalSurfaceSoundsMap, "ImpactPhysicalSurfaceSoundsMap"))
            ImpactPhysicalSurfaceSoundsMap.Properties.TryGetValue(
                ImpactPhysicalSurfaceSoundsMap.Properties.Keys.First(), out ImpactCue);
        FPropertyTagType? ImpactFX = null;
        if (export.TryGetValue(out UScriptMap OffhandImpactNiagaraPhysicalSurfaceEffects, "OffhandImpactNiagaraPhysicalSurfaceEffects"))
            OffhandImpactNiagaraPhysicalSurfaceEffects.Properties.TryGetValue(
                OffhandImpactNiagaraPhysicalSurfaceEffects.Properties.Keys.First(), out ImpactFX);
        FPropertyTagType? EquipCue = null;
        if (export.TryGetValue(out UScriptMap ReloadSoundsMap, "ReloadSoundsMap"))
            ReloadSoundsMap.Properties.TryGetValue(ReloadSoundsMap.Properties.Keys.First(), out EquipCue);
        FPropertyTagType? SwingCue = null;
        if (export.TryGetValue(out UScriptMap PrimaryFireSoundMap, "PrimaryFireSoundMap"))
            PrimaryFireSoundMap.Properties.TryGetValue(PrimaryFireSoundMap.Properties.Keys.First(), out SwingCue);
        export.TryGetValue(out FSoftObjectPath ActorClass, "PrimaryFireAbility");
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
        output.Add("ImpactFX", ImpactFX == null ? "/" : ((FSoftObjectPath)ImpactFX.GenericValue).AssetPathName.Text);
        output.Add("OffhandSwingFX",
            string.IsNullOrWhiteSpace(OffhandSwingFX.AssetPathName.Text) || OffhandSwingFX.AssetPathName.Text == "None" ? "/" : OffhandSwingFX.AssetPathName.Text);
        output.Add("FX", string.IsNullOrWhiteSpace(FX.AssetPathName.Text) || FX.AssetPathName.Text == "None" ? "/" : FX.AssetPathName.Text);
        output.Add("NFX", string.IsNullOrWhiteSpace(NFX.AssetPathName.Text) || NFX.AssetPathName.Text == "None" ? "/" : NFX.AssetPathName.Text);
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
        /*
        new SaturnItem
        {
            ItemDefinition = "DefaultPickaxe",
            Name = "Default Pickaxe",
            Description = "Perfectly unremarkable.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/defaultpickaxe/smallicon.png",
            Rarity = "Common"
        },*/
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_713_GumballMale",
            Name = "Gum Brawler",
            Description = "Long-lasting sticky sledge action.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/pickaxe_id_713_gumballmale/smallicon.png",
            Rarity = "Rare"
        },
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_143_FlintlockWinter",
            Name = "Frozen Axe",
            Description = "Once crimson, now coated in permafrost.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/pickaxe_id_143_flintlockwinter/smallicon.png",
            Rarity = "Rare",
            Series = "FrozenSeries"
        },
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_616_InnovatorFemale",
            Name = "IO Eradicator",
            Description = "Glowing with top-secret IO technology.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/pickaxe_id_616_innovatorfemale/smallicon.png",
            Rarity = "Epic"
        },
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_715_LoneWolfMale",
            Name = "Blade of the Waning Moon",
            Description = "A treasured artifact from a dangerous quest.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/pickaxe_id_715_lonewolfmale/smallicon.png",
            Rarity = "Epic"
        },
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_671_GhostHunterFemale1H",
            Name = "Torin's Lightblade",
            Description = "A very light sword made with data AND fibres!",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/pickaxe_id_671_ghosthunterfemale1h/smallicon.png",
            Rarity = "Epic"
        },
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_508_HistorianMale_6BQSW",
            Name = "Leviathan Axe",
            Description = "Forged by Brok and Sindri.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/pickaxe_id_508_historianmale_6bqsw/smallicon.png",
            Rarity = "Epic",
            Series = "PlatformSeries"
        },
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_542_TyphoonFemale1H_CTEVQ",
            Name = "Combat Knife",
            Description = "No fate.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/pickaxe_id_542_typhoonfemale1h_ctevq/smallicon.png",
            Rarity = "Epic"
        },
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_457_HightowerSquash1H",
            Name = "Hand of Lightning",
            Description = "Harness the elements.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/pickaxe_id_457_hightowersquash1h/smallicon.png",
            Rarity = "Epic",
            Series = "MarvelSeries"
        },
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_463_Elastic1H",
            Name = "Phantasmic Pulse",
            Description = "Channeling energy from a far-off star.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/pickaxe_id_463_elastic1h/smallicon.png",
            Rarity = "Epic"
        },
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_454_HightowerGrapeMale1H",
            Name = "Groot's Sap Axes",
            Description = "They are also Groot.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/pickaxe_id_454_hightowergrapemale1h/smallicon.png",
            Rarity = "Epic",
            Series = "MarvelSeries"
        },
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_361_HenchmanMale1H",
            Name = "Hack & Smash",
            Description = "Ready for the quick draw.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/pickaxe_id_361_henchmanmale1h/smallicon.png",
            Rarity = "Rare"
        },
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_284_CrazyEight1H",
            Name = "Bank Shots",
            Description = "Run the table.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/pickaxe_id_284_crazyeight1h/smallicon.png",
            Rarity = "Rare"
        },
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_334_SweaterWeatherMale",
            Name = "Snowy",
            Description = "Use your head.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/pickaxe_id_334_sweaterweathermale/smallicon.png",
            Rarity = "Rare"
        },
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_568_ObsidianFemale",
            Name = "Axe-tral Form",
            Description = "Control your emotions - Don't let them control you.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/pickaxe_id_568_obsidianfemale/smallicon.png",
            Rarity = "Epic",
            Series = "DCUSeries"
        },
        new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_313_ShiitakeShaolinMale",
            Name = "Crescent Shroom",
            Description = "Fit for a mushroom king.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/Pickaxe_ID_313_ShiitakeShaolinMale/smallicon.png",
            Rarity = "Rare"
        },
        new SaturnItem()
        {
            ItemDefinition = "Pickaxe_ID_545_CrushFemale1H",
            Name = "Lovestruck Striker",
            Description = "More than just a crush.",
            Icon = "https://fortnite-api.com/images/cosmetics/br/Pickaxe_ID_545_CrushFemale1H/smallicon.png",
            Rarity = "Rare"
        },
        new SaturnItem()
        {
            ItemDefinition = "Pickaxe_ID_480_PoisonFemale",
            Name = "Forsaken Strike",
            Description = "The latest in reaper style.",
            Icon = "https://fortnite-api.com/images/cosmetics/br/Pickaxe_ID_480_PoisonFemale/smallicon.png",
            Rarity = "Rare"
        },
               new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_690_RelishFemale_DC74M",
            Name = "Hot Dogger",
            Description = "Umbrella-developed anti-bioweapon knife, used by those that like to show off.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/Pickaxe_ID_690_RelishFemale_DC74M/smallicon.png",
            Rarity = "Epic",
            Series = "PlatformSeries"
          },
                new SaturnItem
        {
            ItemDefinition = "Pickaxe_ID_721_RustyBoltSliceMale_V3A4N",
            Name = "Butcher Cleaver",
            Description = "A tool for cutting Rockworm meat but also cutting down the COG.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/Pickaxe_ID_721_RustyBoltSliceMale_V3A4N/smallicon.png",
            Rarity = "Epic",
            Series = "PlatformSeries"
          }, 
                /*new SaturnItem
        {
            ItemDefinition = "807_NeonGraffitiLavaFemale",
            Name = "Sulfuric Street Shine",
            Description = "This way to the hottest drop around.",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/807_NeonGraffitiLavaFemale/smallicon.png",
            Rarity = "Epic",
            Series = "LavaSeries"
          },*/ //Removed, waiting for a fix
          new SaturnItem
          {
              ItemDefinition = "Pickaxe_ID_612_AntiqueMale",
              Name = "Chop Chop",
              Description = "Drip chop, who's next?",
              Icon =
                  "https://fortnite-api.com/images/cosmetics/br/Pickaxe_ID_612_AntiqueMale/smallicon.png",
              Rarity = "Rare"
          },
          new SaturnItem
          {
              ItemDefinition = "Pickaxe_ID_766_BinaryFemale",
              Name = "The Imagined Blade",
              Description = "This thing cuts everything. Armor, comms wires, sandwiches...",
              Icon =
                  "https://fortnite-api.com/images/cosmetics/br/Pickaxe_ID_766_BinaryFemale/smallicon.png",
              Rarity = "Rare"
          },
          new SaturnItem
          {
              ItemDefinition = "Pickaxe_ID_562_BananaLeader",
              Name = "Gladius Of Potassius",
              Description = "The valiant never taste of pudding but once.",
              Icon =
                  "https://fortnite-api.com/images/cosmetics/br/Pickaxe_ID_562_BananaLeader/smallicon.png",
              Rarity = "Rare"
          },
    };

    public async Task<Cosmetic> AddPickaxeOptions(Cosmetic pickaxe, ISwapperService swapperService,
        DefaultFileProvider _provider)
    {
        if (pickaxe.CosmeticOptions.Count > 0)
            return pickaxe;
        
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
                    if (value != OGSwaps[key])
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

                if (key == "FX" && pickaxe.Name != "Stellar Axe" && pickaxe.Name != "The Axe of Champions")
                    if (value != "/" && OGSwaps[key] == "/" && OGSwaps["NFX"] == "/")
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

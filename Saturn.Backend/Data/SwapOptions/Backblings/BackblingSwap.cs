using System;
using Saturn.Backend.Data.Utils.Swaps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.UObject;
using Microsoft.JSInterop;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.FortniteAPI;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Services;
using Saturn.Backend.Data.Utils;

namespace Saturn.Backend.Data.SwapOptions.Backblings;

internal abstract class BackblingSwap : AbstractSwap
{
    protected BackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data)
        : base(name, rarity, icon)
    {
        Data = data;
    }
    
    public Dictionary<string, string> Data { get; }
    
}

public class AddBackblings
{
    private async Task<Dictionary<string, string>> GetAssetsFromCP(string cp, DefaultFileProvider _provider)
    {
        var output = new Dictionary<string, string>();
        
        UObject export = await _provider.TryLoadObjectAsync(cp.Split('.')[0]) ?? new UObject();

        string Material = "/";

        export.TryGetValue(out FSoftObjectPath Mesh, "SkeletalMesh");
        if (export.TryGetValue(out FStructFallback[] MaterialOverrides, "MaterialOverrides"))
            foreach (var (material, matIndex) in from materialOverride in MaterialOverrides
                     let material = materialOverride.Get<FSoftObjectPath>("OverrideMaterial").AssetPathName
                         .ToString()
                     let matIndex = materialOverride.Get<int>("MaterialOverrideIndex")
                     select (material, matIndex))
                {
                    if (matIndex == 0)
                        Material = material;
                }
        export.TryGetValue(out FSoftObjectPath FX, "IdleEffect");
        export.TryGetValue(out FSoftObjectPath NiagaraFX, "IdleEffectNiagara");
        export.TryGetValue(out FSoftObjectPath PartModifierBP, "PartModifierBlueprint");

        string AnimBP = "/";
        string SocketName = "None";
        
        if (export.TryGetValue(out UObject AdditionalData, "AdditionalData"))
        {
            AdditionalData.TryGetValue(out FName AttachSocketName, "AttachSocketName");
            AdditionalData.TryGetValue(out FSoftObjectPath ABP, "AnimClass");

            AnimBP = ABP.AssetPathName.Text;
            SocketName = AttachSocketName.Text;
        }

        output.Add("Mesh", string.IsNullOrWhiteSpace(Mesh.AssetPathName.Text) || Mesh.AssetPathName.Text == "None" ? "/" : Mesh.AssetPathName.Text);
        output.Add("Material", Material);
        
        output.Add("FX", string.IsNullOrWhiteSpace(FX.AssetPathName.Text) || FX.AssetPathName.Text == "None" ? "/" : FX.AssetPathName.Text);
        output.Add("NFX", string.IsNullOrWhiteSpace(NiagaraFX.AssetPathName.Text) || NiagaraFX.AssetPathName.Text == "None" ? "/" : NiagaraFX.AssetPathName.Text);
        output.Add("PartModifierBP", string.IsNullOrWhiteSpace(PartModifierBP.AssetPathName.Text) || PartModifierBP.AssetPathName.Text == "None" ? "/" : PartModifierBP.AssetPathName.Text);

        output.Add("Socket", string.IsNullOrWhiteSpace(SocketName) || SocketName == "None" ? "/" : SocketName);
        output.Add("ABP", string.IsNullOrWhiteSpace(AnimBP) || AnimBP == "None" ? "/" : AnimBP);
        
        return output;
    }
    
    
    protected List<SaturnItem> BackblingOptions = new List<SaturnItem>()
    {
        new SaturnItem
        {
            ItemDefinition = "BID_695_StreetFashionEclipse",
            Name = "Blackout Bag",
            Description = "They'll never know what's up.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/bid_695_streetfashioneclipse/smallicon.png",
            Rarity = "Epic",
            Series = "ShadowSeries"
        },
        new SaturnItem
        {
            ItemDefinition = "BID_600_HightowerTapas",
            Name = "Thor's Cloak",
            Description = "Thor's Herald Cloak.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/bid_600_hightowertapas/smallicon.png",
            Rarity = "Epic"
        },
        new SaturnItem
        {
            ItemDefinition = "BID_678_CardboardCrewHolidayMale",
            Name = "Wrapping Caper",
            Description = "Crinkling gracefully in the winter wind.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/BID_678_CardboardCrewHolidayMale/smallicon.png",
            Rarity = "Rare"
        },
        new SaturnItem
        {
            ItemDefinition = "BID_430_GalileoSpeedBoat_9RXE3",
            Name = "The Sith",
            Description = "Force-wielders devoted to the dark side.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/bid_430_galileospeedboat_9rxe3/smallicon.png",
            Rarity = "Epic",
            Series = "ColumbusSeries"
        },
        new SaturnItem
        {
            ItemDefinition = "BID_545_RenegadeRaiderFire",
            Name = "Firestarter",
            Description = "Warning: Contents of firestarter may be hot.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/bid_545_renegaderaiderfire/smallicon.png",
            Rarity = "Legendary",
            Series = "LavaSeries"
        },
        new SaturnItem
        {
            ItemDefinition = "BID_562_CelestialFemale",
            Name = "Nucleus",
            Description = "A whole galaxy to-go.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/bid_562_celestialfemale/smallicon.png",
            Rarity = "Legendary"
        },
        new SaturnItem
        {
            ItemDefinition = "BID_289_Banner",
            Name = "Banner Cape",
            Description = "Customize your look by choosing a Banner and color in your Locker.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/bid_289_banner/smallicon.png",
            Rarity = "Rare"
        },
        new SaturnItem
        {
            ItemDefinition = "BID_266_BunkerMan",
            Name = "Nana Cape",
            Description = "A Peely original.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/bid_266_bunkerman/smallicon.png",
            Rarity = "Epic"
        },
        new SaturnItem
        {
            ItemDefinition = "BID_121_RedRiding",
            Name = "Fabled Cape",
            Description = "Nothing lil' about it.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/bid_121_redriding/smallicon.png",
            Rarity = "Epic"
        },
        new SaturnItem
        {
            ItemDefinition = "BID_122_HalloweenTomato",
            Name = "Night Cloak",
            Description = "Stealthy and saucy.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/bid_122_halloweentomato/smallicon.png",
            Rarity = "Epic"
        },
        new SaturnItem
        {
            ItemDefinition = "BID_073_DarkViking",
            Name = "Frozen Shroud",
            Description = "A relic of the ancients.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/bid_073_darkviking/smallicon.png",
            Rarity = "Legendary"
        },
        new SaturnItem
        {
            ItemDefinition = "BID_167_RedKnightWinterFemale",
            Name = "Frozen Red Shield",
            Description = "The Red Knight's legendary frozen shield.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/bid_167_redknightwinterfemale/smallicon.png",
            Rarity = "Legendary",
            Series = "FrozenSeries"
        },
        new SaturnItem
        {
            ItemDefinition = "BID_003_RedKnight",
            Name = "Red Shield",
            Description = "The Red Knight's legendary shield.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/bid_003_redknight/smallicon.png",
            Rarity = "Legendary"
        },
        new SaturnItem
        {
            ItemDefinition = "BID_343_CubeRedKnight",
            Name = "Dark Shield",
            Description = "Timeless.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/bid_343_cuberedknight/smallicon.png",
            Rarity = "Epic",
            Series = "CUBESeries"
        },
        new SaturnItem
        {
            ItemDefinition = "BID_388_DevilRockMale",
            Name = "Flame Sigil",
            Description = "The underworld beckons.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/bid_388_devilrockmale/smallicon.png",
            Rarity = "Epic"
        },
        new SaturnItem
        {
            ItemDefinition = "BID_319_StreetRacerDriftRemix",
            Name = "Atmosphere",
            Description = "Resonating with energy.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/bid_319_streetracerdriftremix/smallicon.png",
            Rarity = "Legendary"
        },
    };
    
    public async Task<Cosmetic> AddBackblingOptions(Cosmetic backBling, ISwapperService swapperService,
        DefaultFileProvider _provider, IJSRuntime _jsRuntime)
    {
        if (backBling.CosmeticOptions.Count > 0)
            return backBling;
        
        var cp = await swapperService.GetBackblingCP(backBling.Id);

        if (cp == new UObject())
            return null;
        
        Dictionary<string, string> swaps = await GetAssetsFromCP(cp.GetPathName(), _provider);

        backBling.CosmeticOptions.Add(new SaturnItem()
        {
            ItemDefinition = "BID_Athena_Commando_F_Prime", // Cool custom BID ikr
            Name = "No backbling",
            Description = "MUST BE SWAPPED WITH A NO SKIN!",
            Icon =
                "https://fortnite-api.com/images/cosmetics/br/CID_A_272_Athena_Commando_F_Prime/smallicon.png",
            Rarity = "Common"
        });

        foreach (var option in BackblingOptions)
        {
            var uobj = await swapperService.GetBackblingCP(option.ItemDefinition);
            string OGCP = uobj == new UObject()
                ? "ERROR"
                : uobj.GetPathName();

            if (OGCP == "ERROR")
            {
                await _jsRuntime.InvokeVoidAsync("MessageBox", "There was an error getting the backbling option " + option.Name, "Could not get the character part for this option. This might mean your files are corrupt!");
                throw new Exception("Couldn't get character part for backbling option " + option.Name);
            }
            
            Dictionary<string, string> OGSwaps = await GetAssetsFromCP(OGCP, _provider);

            bool bDontProceed = false;

            foreach (var (key, value) in swaps)
            {
                if (key == "Socket")
                    if (!string.Equals(value.ToLower(), OGSwaps[key].ToLower()))
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
                
                if (key == "NFX")
                    if (value != "/" && OGSwaps[key] == "/")
                    {
                        bDontProceed = true;
                        break;
                    }
            }

            if (bDontProceed) continue;
            
            option.Swaps = swaps;
            backBling.CosmeticOptions.Add(option);
        }

        return backBling;
    }
}

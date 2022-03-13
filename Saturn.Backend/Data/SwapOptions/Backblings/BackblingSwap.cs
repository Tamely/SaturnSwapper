using System;
using Saturn.Backend.Data.Utils.Swaps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.UObject;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.FortniteAPI;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Services;

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

        export.TryGetValue(out FSoftObjectPath Mesh, "SkeletalMesh");
        export.TryGetValue(out FSoftObjectPath[] Material, "MaterialOverrides");
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
        output.Add("Material", Material == null || Material[0].AssetPathName.Text == "None" ? "/" : Material[0].AssetPathName.Text);
        
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
        }
    };
    
    public async Task<Cosmetic> AddBackblingOptions(Cosmetic backBling, ISwapperService swapperService,
        DefaultFileProvider _provider)
    {
        var cp = await swapperService.GetBackblingCP(backBling.Id);

        if (cp == new UObject())
            return null;

        foreach (var option in BackblingOptions)
        {
            Dictionary<string, string> swaps = await GetAssetsFromCP(cp.GetPathName(), _provider);

            string OGCP = (await swapperService.GetBackblingCP(option.ItemDefinition)).GetPathName();
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
        
        if (backBling.CosmeticOptions.Count == 0)
        {
            backBling.CosmeticOptions.Add(new SaturnItem()
            {
                Name = "No options!",
                Description = "Send a picture of this to Tamely on Discord and tell him to add an option for this!",
                Rarity = "Epic",
                Icon = "img/Saturn.png"
            });
        }

        return backBling;
    }
}

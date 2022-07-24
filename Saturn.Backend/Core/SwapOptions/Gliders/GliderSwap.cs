using System.Collections.Generic;
using System.Threading.Tasks;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.UObject;
using Saturn.Backend.Core.Models.FortniteAPI;
using Saturn.Backend.Core.Models.Items;
using Saturn.Backend.Core.Services;
using Saturn.Backend.Core.Utils;
using Saturn.Backend.Core.Utils.Swaps;
using Serilog;

namespace Saturn.Backend.Core.SwapOptions.Gliders;

internal abstract class GliderSwap : AbstractSwap
{
    protected GliderSwap(string name, string rarity, string icon, Dictionary<string, string> swaps)
        : base(name, rarity, icon)
    {
        Swaps = swaps;
    }

    public Dictionary<string, string> Swaps { get; }
}

public class AddGliders
{
    public async Task<Dictionary<string, string>> GetGliderData(string ID, DefaultFileProvider _provider)
    {
        Dictionary<string, string> output = new();

        if (!_provider.TryLoadObject(Constants.GliderIdPath + ID, out UObject GliderID))
            return output;

        GliderID.TryGetValue(out FSoftObjectPath SkeletalMesh, "SkeletalMesh");
        if (GliderID.TryGetValue(out FStructFallback[] MaterialOverrides, "MaterialOverrides") && MaterialOverrides.Length > 0)
            if (MaterialOverrides[0].TryGetValue(out FSoftObjectPath Material, "OverrideMaterial"))
                output.Add("Material", Material.AssetPathName.Text);
        
        if (!output.ContainsKey("Material"))
        {
            if (SkeletalMesh.TryLoad(_provider, out USkeletalMesh skeletalMesh))
                output.Add("Material", skeletalMesh.Materials[0].Material.GetPathName());
            else
                Logger.Log("Unable to load skeletal mesh " + SkeletalMesh.AssetPathName.Text);
        }

        output.Add("SkeletalMesh", SkeletalMesh.AssetPathName.Text);
        
        return output;
    }

    protected List<SaturnItem> GliderOptions = new List<SaturnItem>()
    {
        new SaturnItem
        {
            ItemDefinition = "Solo_Umbrella",
            Name = "The Umbrella",
            Description = "The fabled victory umbrella.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/Solo_Umbrella/smallicon.png",
            Rarity = "Common"
        },
        new SaturnItem()
        {
            ItemDefinition = "FounderGlider",
            Name = "Founder's Glider",
            Description = "Founder's exclusive!",
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/FounderGlider/smallicon.png",
            Rarity = "Common"
        },
        new SaturnItem()
        {
            ItemDefinition = "Glider_ID_016_Tactical",
            Name = "Carbon",
            Description = "Precision built carbon fiber glider.",
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/Glider_ID_016_Tactical/smallicon.png",
            Rarity = "Uncommon"
        },
        new SaturnItem()
        {
            ItemDefinition = "Glider_ID_050_StreetRacerCobra",
            Name = "Cruiser",
            Description = "Put it in neutral and glide easily.",
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/Glider_ID_050_StreetRacerCobra/smallicon.png",
            Rarity = "Uncommon"
        },
        new SaturnItem()
        {
            ItemDefinition = "DefaultGlider",
            Name = "Glider",
            Description = "Standard Battle Glider.",
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/DefaultGlider/smallicon.png",
            Rarity = "Common"
        },
        new SaturnItem()
        {
            ItemDefinition = "Glider_ID_244_ChOneGlider",
            Name = "The O.G.",
            Description = "True Original Glider.",
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/Glider_ID_244_ChOneGlider/smallicon.png",
            Rarity = "Uncommon"
        }
    };
    
    public async Task<Cosmetic> AddGliderOptions(Cosmetic glider, ISwapperService swapperService,
        DefaultFileProvider _provider)
    {
        if (glider.CosmeticOptions.Count > 0)
            return glider;
        
        Dictionary<string, string> swaps = await GetGliderData(glider.Id, _provider);
        
        if (swaps.Count == 0)
            return null;
        
        foreach (var option in GliderOptions)
        {
            Dictionary<string, string> OGSwaps = await GetGliderData(option.ItemDefinition, _provider);

            bool bDontProceed = false;
            
            foreach (var (key, value) in swaps)
            {
                if (key == "SkeletalMesh")
                    if (OGSwaps[key] != value)
                    {
                        bDontProceed = true;
                        break;
                    }
            }

            if (bDontProceed) continue;
            
            option.Swaps = swaps;
            glider.CosmeticOptions.Add(option);
        }
        
        if (glider.CosmeticOptions.Count == 0)
        {
            glider.CosmeticOptions.Add(new SaturnItem()
            {
                Name = "No options!",
                Description = "Send a picture of this to Tamely on Discord and tell him to add an option for this!",
                Rarity = "Epic",
                Icon = "img/Saturn.png"
            });
        }

        return glider;
    }
}
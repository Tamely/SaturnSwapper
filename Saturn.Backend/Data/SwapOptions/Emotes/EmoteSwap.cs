using Saturn.Backend.Data.Utils.Swaps;
using System.Collections.Generic;
using System.Threading.Tasks;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Objects.UObject;
using Saturn.Backend.Data.Models.FortniteAPI;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Services;
using Saturn.Backend.Data.Utils;

namespace Saturn.Backend.Data.SwapOptions.Emotes;

internal abstract class EmoteSwap : AbstractSwap
{
    protected EmoteSwap(string name, string rarity, string icon, Dictionary<string, string> swaps)
        : base(name, rarity, icon)
    {
        Swaps = swaps;
    }

    public Dictionary<string, string> Swaps { get; }
}

public class AddEmotes
{
    private async Task<Dictionary<string, string>> GetEmoteData(string ID, DefaultFileProvider _provider)
    {
        Dictionary<string, string> output = new();

        if (!_provider.TryLoadObject(Constants.EidPath + ID, out UObject EID))
            return output;

        EID.TryGetValue(out FSoftObjectPath CMM, "Animation");
        EID.TryGetValue(out FSoftObjectPath CMF, "AnimationFemaleOverride");
        EID.TryGetValue(out FSoftObjectPath LargePreviewImage, "LargePreviewImage");
        EID.TryGetValue(out FSoftObjectPath SmallPreviewImage, "SmallPreviewImage");
        EID.TryGetValue(out float PreviewLength, "PreviewLength");
        
        output.Add("CMM", string.IsNullOrWhiteSpace(CMM.AssetPathName.Text) || CMM.AssetPathName.Text == "None" ? "/" : CMM.AssetPathName.Text);
        output.Add("CMF", string.IsNullOrWhiteSpace(CMF.AssetPathName.Text) || CMF.AssetPathName.Text == "None" ? output["CMM"] : CMF.AssetPathName.Text);
        output.Add("LargeIcon", string.IsNullOrWhiteSpace(LargePreviewImage.AssetPathName.Text) || LargePreviewImage.AssetPathName.Text == "None" ? "/" : LargePreviewImage.AssetPathName.Text);
        output.Add("SmallIcon", string.IsNullOrWhiteSpace(SmallPreviewImage.AssetPathName.Text) || SmallPreviewImage.AssetPathName.Text == "None" ? "/" : SmallPreviewImage.AssetPathName.Text);
        output.Add("PreviewLength", PreviewLength.ToString());

        return output;
    }

    protected List<SaturnItem> EmoteOptions = new List<SaturnItem>()
    {
        new SaturnItem
        {
            ItemDefinition = "EID_DanceMoves",
            Name = "Dance Moves",
            Description = "Express yourself on the battlefield.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/eid_dancemoves/smallicon.png",
            Rarity = "Common"
        },
        new SaturnItem
        {
            ItemDefinition = "EID_BoogieDown",
            Name = "Boogie Down",
            Description = "Boogie Down with Populotus.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/eid_boogiedown/smallicon.png",
            Rarity = "Epic"
        }
    };
    public async Task<Cosmetic> AddEmoteOptions(Cosmetic emote, ISwapperService swapperService,
        DefaultFileProvider _provider)
    {
        Dictionary<string, string> swaps = await GetEmoteData(emote.Id, _provider);

        if (swaps.Count == 0)
            return null;
        
        foreach (var option in EmoteOptions)
        {
            Dictionary<string, string> OGSwaps = await GetEmoteData(option.ItemDefinition, _provider);

            bool bDontProceed = false;

            foreach (var (key, value) in swaps)
            {
                if (key == "PreviewLength")
                    if (float.Parse(value) > 1 && float.Parse(OGSwaps[key]) < 1)
                    {
                        bDontProceed = true;
                        break;
                    }
            }

            if (bDontProceed) continue;
            
            option.Swaps = swaps;
            emote.CosmeticOptions.Add(option);
        }
        
        if (emote.CosmeticOptions.Count == 0)
        {
            emote.CosmeticOptions.Add(new SaturnItem()
            {
                Name = "No options!",
                Description = "Send a picture of this to Tamely on Discord and tell him to add an option for this!",
                Rarity = "Epic",
                Icon = "img/Saturn.png"
            });
        }

        return emote;
    }
}

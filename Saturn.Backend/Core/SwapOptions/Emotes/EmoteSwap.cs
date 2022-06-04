using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Objects.UObject;
using Saturn.Backend.Core.Models.FortniteAPI;
using Saturn.Backend.Core.Models.Items;
using Saturn.Backend.Core.Services;
using Saturn.Backend.Core.Utils;
using Saturn.Backend.Core.Utils.Swaps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saturn.Backend.Core.SwapOptions.Emotes;

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
        EID.TryGetValue(out bool bMovingEmote, "bMovingEmote");
        
        output.Add("CMM", string.IsNullOrWhiteSpace(CMM.AssetPathName.Text) || CMM.AssetPathName.Text == "None" ? "/" : CMM.AssetPathName.Text);
        output.Add("CMF", string.IsNullOrWhiteSpace(CMF.AssetPathName.Text) || CMF.AssetPathName.Text == "None" ? output["CMM"] : CMF.AssetPathName.Text);
        output.Add("LargeIcon", string.IsNullOrWhiteSpace(LargePreviewImage.AssetPathName.Text) || LargePreviewImage.AssetPathName.Text == "None" ? "/" : LargePreviewImage.AssetPathName.Text);
        output.Add("SmallIcon", string.IsNullOrWhiteSpace(SmallPreviewImage.AssetPathName.Text) || SmallPreviewImage.AssetPathName.Text == "None" ? "/" : SmallPreviewImage.AssetPathName.Text);
        output.Add("PreviewLength", PreviewLength.ToString());
        output.Add("bMovingEmote", bMovingEmote.ToString());

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
        },
        new SaturnItem
        {
            ItemDefinition = "EID_Laugh",
            Name = "Laugh It Up",
            Description = "What's so funny?", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/eid_laugh/smallicon.png",
            Rarity = "Rare"
        },
        new SaturnItem
        {
            ItemDefinition = "EID_Saucer",
            Name = "Lil' Saucer",
            Description = "Close encounters of the lil' kind.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/eid_saucer/smallicon.png",
            Rarity = "Epic"
        },
        new SaturnItem
        {
            ItemDefinition = "EID_Believer",
            Name = "Ska-stra-terrestrial",
            Description = "The invasion's fourth wave begins now!", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/eid_believer/smallicon.png",
            Rarity = "Rare"
        },
        new SaturnItem
        {
            ItemDefinition = "EID_Custodial",
            Name = "Clean Sweep",
            Description = "Tidy as you go.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/eid_custodial/smallicon.png",
            Rarity = "Uncommon"
        },
        new SaturnItem
        {
            ItemDefinition = "EID_Roving",
            Name = "Lil' Rover",
            Description = "Onward to lil' discoveries.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/eid_roving/smallicon.png",
            Rarity = "Epic"
        },
        new SaturnItem
        {
            ItemDefinition = "EID_WatchThis",
            Name = "Ready When You Are",
            Description = "No go on. Take your time. I'll wait.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/eid_watchthis/smallicon.png",
            Rarity = "Uncommon"
        },
        new SaturnItem
        {
            ItemDefinition = "EID_Division",
            Name = "Nailed it",
            Description = "I can only feign interest for so long.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/eid_division/smallicon.png",
            Rarity = "Uncommon"
        },
        new SaturnItem
        {
            ItemDefinition = "EID_HighActivity",
            Name = "Kick Back",
            Description = "No sweat.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/eid_highactivity/smallicon.png",
            Rarity = "Rare"
        },
        new SaturnItem
        {
            ItemDefinition = "EID_Terminal",
            Name = "Vulcan Salute",
            Description = "Live long and prosper.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/eid_terminal/smallicon.png",
            Rarity = "Uncommon"
        },
        new SaturnItem
        {
            ItemDefinition = "EID_WIR",
            Name = "Hot Marat",
            Description = "Wreck the dance floor.", 
            Icon = 
                "https://fortnite-api.com/images/cosmetics/br/eid_wir/smallicon.png",
            Rarity = "Rare"
        },
    };
    public async Task<Cosmetic> AddEmoteOptions(Cosmetic emote, ISwapperService swapperService,
        DefaultFileProvider _provider)
    {
        if (emote.CosmeticOptions.Count > 0)
            return emote;
        
        
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
                
                if (key == "bMovingEmote")
                    if (value.ToLower() == "true" && OGSwaps[key].ToLower() == "false")
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

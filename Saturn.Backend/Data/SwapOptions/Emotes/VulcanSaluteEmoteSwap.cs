using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Data.SwapOptions.Emotes;

internal sealed class VulcanSaluteEmoteSwap : EmoteSwap
{
    public VulcanSaluteEmoteSwap(string name, string rarity, string icon, Dictionary<string, string> swaps)
        : base(name, rarity, icon, swaps)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Cosmetics/Dances/EID_Terminal",
                Swaps = new List<SaturnSwap>
                {
                    new()
                    {
                        Search = "/Game/Animation/Game/MainPlayer/Emotes/Terminal/Emote_Terminal_CMM_M.Emote_Terminal_CMM_M",
                        Replace = Swaps["CMM"],
                        Type = SwapType.BodyAnim
                    },
                    new()
                    {
                        Search = "/Game/Animation/Game/MainPlayer/Emotes/Terminal/Emote_Terminal_CMF_M.Emote_Terminal_CMF_M",
                        Replace = Swaps["CMF"],
                        Type = SwapType.BodyAnim
                    },
                    new()
                    {
                        Search = "/Game/UI/Foundation/Textures/Icons/Emotes/T-Icon-Emotes-E-Terminal.T-Icon-Emotes-E-Terminal",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.Modifier
                    },
                    new()
                    {
                        Search = "/Game/UI/Foundation/Textures/Icons/Emotes/T-Icon-Emotes-E-Terminal-L.T-Icon-Emotes-E-Terminal-L",
                        Replace = "/",
                        Type = SwapType.Modifier
                    }
                }
            }
        };
}
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.SwapOptions.Emotes;

internal sealed class NailedItEmoteSwap : EmoteSwap
{
    public NailedItEmoteSwap(string name, string rarity, string icon, Dictionary<string, string> swaps)
        : base(name, rarity, icon, swaps)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Cosmetics/Dances/EID_Division",
                Swaps = new List<SaturnSwap>
                {
                    new()
                    {
                        Search = "/Game/Animation/Game/MainPlayer/Emotes/Division/Emote_Division_CMM_Frontend_M.Emote_Division_CMM_Frontend_M",
                        Replace = Swaps["CMM"],
                        Type = SwapType.BodyAnim
                    },
                    new()
                    {
                        Search = "/Game/Animation/Game/MainPlayer/Emotes/Division/Emote_Division_CMF_Frontend_M.Emote_Division_CMF_Frontend_M",
                        Replace = Swaps["CMF"],
                        Type = SwapType.BodyAnim
                    },
                    new()
                    {
                        Search = "/Game/Animation/Game/MainPlayer/Emotes/Division/Emote_Division_CMM_M.Emote_Division_CMM_M",
                        Replace = Swaps["CMM"],
                        Type = SwapType.BodyAnim
                    },
                    new()
                    {
                        Search = "/Game/Animation/Game/MainPlayer/Emotes/Division/Emote_Division_CMF_M.Emote_Division_CMF_M",
                        Replace = Swaps["CMF"],
                        Type = SwapType.BodyAnim
                    },
                    new()
                    {
                        Search = "/Game/UI/Foundation/Textures/Icons/Emotes/T-Icon-Emotes-E-Division.T-Icon-Emotes-E-Division",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.Modifier
                    },
                    new()
                    {
                        Search = "/Game/UI/Foundation/Textures/Icons/Emotes/T-Icon-Emotes-E-Division-L.T-Icon-Emotes-E-Division-L",
                        Replace = "/",
                        Type = SwapType.Modifier
                    }
                }
            }
        };
}
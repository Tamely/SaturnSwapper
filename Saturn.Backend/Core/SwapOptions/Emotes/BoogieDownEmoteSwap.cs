using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.SwapOptions.Emotes;

internal sealed class BoogieDownEmoteSwap : EmoteSwap
{
    public BoogieDownEmoteSwap(string name, string rarity, string icon, Dictionary<string, string> swaps)
        : base(name, rarity, icon, swaps)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Cosmetics/Dances/EID_BoogieDown",
                Swaps = new List<SaturnSwap>
                {
                    new()
                    {
                        Search = "/Game/Animation/Game/MainPlayer/Emotes/Boogie_Down/Emote_Boogie_Down_CMM.Emote_Boogie_Down_CMM",
                        Replace = Swaps["CMM"],
                        Type = SwapType.BodyAnim
                    },
                    new()
                    {
                        Search = "/Game/Animation/Game/MainPlayer/Emotes/Boogie_Down/Emote_Boogie_Down_CMF.Emote_Boogie_Down_CMF",
                        Replace = Swaps["CMF"],
                        Type = SwapType.BodyAnim
                    },
                    new()
                    {
                        Search = "/Game/UI/Foundation/Textures/Icons/Emotes/T-Icon-Emotes-E-BoogieDown.T-Icon-Emotes-E-BoogieDown",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.Modifier
                    },
                    new()
                    {
                        Search = "/Game/UI/Foundation/Textures/Icons/Emotes/T-Icon-Emotes-E-BoogieDown-L.T-Icon-Emotes-E-BoogieDown-L",
                        Replace = "/",
                        Type = SwapType.Modifier
                    }
                }
            }
        };
}

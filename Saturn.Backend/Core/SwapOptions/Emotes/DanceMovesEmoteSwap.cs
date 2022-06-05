using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.SwapOptions.Emotes;

internal sealed class DanceMovesEmoteSwap : EmoteSwap
{
    public DanceMovesEmoteSwap(string name, string rarity, string icon, Dictionary<string, string> swaps)
        : base(name, rarity, icon, swaps)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Cosmetics/Dances/EID_DanceMoves",
                Swaps = new List<SaturnSwap>
                {
                    new()
                    {
                        Search = "/Game/Animation/Game/MainPlayer/Montages/Emotes/Emote_DanceMoves.Emote_DanceMoves",
                        Replace = Swaps["CMM"],
                        Type = SwapType.BodyAnim
                    },
                    new()
                    {
                        Search = "/Game/UI/Foundation/Textures/Icons/Emotes/T-Icon-Emotes-E-Dance.T-Icon-Emotes-E-Dance",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.Modifier
                    },
                    new()
                    {
                        Search = "/Game/UI/Foundation/Textures/Icons/Emotes/T-Icon-Emotes-E-Dance-L.T-Icon-Emotes-E-Dance-L",
                        Replace = "/",
                        Type = SwapType.Modifier
                    }
                }
            }
        };
}

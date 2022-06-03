using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.SwapOptions.Backblings;

internal sealed class NanaCapeBackblingSwap : BackblingSwap
{
    public NanaCapeBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data)
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_BunkerMan",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Capes/M_MED_Bunker_Man_Cape/Meshes/M_MED_BunkerMan_Cape.M_MED_BunkerMan_Cape",
                        Replace = Data["Mesh"],
                        Type = SwapType.BackblingMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Capes/Common/Fortnite_Base_Cape_AnimBlueprint_ChaosCloth.Fortnite_Base_Cape_AnimBlueprint_ChaosCloth_C",
                        Replace = Data["ABP"] ?? "/Game/Accessories/FORT_Capes/Common/Fortnite_Base_Cape_AnimBlueprint_ChaosCloth.Fortnite_Base_Cape_AnimBlueprint_ChaosCloth_C",
                        Type = SwapType.BackblingAnim
                    }
                }
            }
        };
}
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Data.SwapOptions.Backblings;

internal sealed class NightCloakBackblingSwap : BackblingSwap
{
    public NightCloakBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data)
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_HalloweenTomato",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Capes/F_MED_Halloween_Tomato_Cape/Meshes/F_MED_Halloween_Tomato_Cape.F_MED_Halloween_Tomato_Cape",
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
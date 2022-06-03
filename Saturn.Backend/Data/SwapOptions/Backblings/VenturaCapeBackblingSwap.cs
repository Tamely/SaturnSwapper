using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Data.SwapOptions.Backblings;

internal sealed class VenturaCapeBackblingSwap : BackblingSwap
{
    public VenturaCapeBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data)
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_DecoFemale",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Capes/M_MED_Deco_Cape/Meshes/M_MED_Commando_Deco_Cape.M_MED_Commando_Deco_Cape",
                        Replace = Data["Mesh"],
                        Type = SwapType.BackblingMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Capes/F_Med_Deco_Cape/Materials/F_MED_Deco_Cape.F_MED_Deco_Cape",
                        Replace = Data["Material"],
                        Type = SwapType.BackblingMaterial
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Capes/F_Med_Deco_Cape/Materials/F_MED_Deco_Cape.F_MED_Deco_Cape",
                        Replace = Data["Material"],
                        Type = SwapType.BackblingMaterial
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

using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Data.SwapOptions.Backblings;

internal sealed class AncestralBloomBackblingSwap : BackblingSwap
{
    public AncestralBloomBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data)
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_ExoSuitFemale",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_F_MED_ExoSuit_Cape/Meshes/F_MED_ExoSuit_Cape_Pack.F_MED_ExoSuit_Cape_Pack",
                        Replace = Data["Mesh"],
                        Type = SwapType.BackblingMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_F_MED_ExoSuit_Cape/FX/NS_Exosuit_F_Backpack.NS_Exosuit_F_Backpack",
                        Replace = Data["FX"],
                        Type = SwapType.BackblingFx
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Cosmetics/Blueprints/B_Athena_PartModifier_Generic.B_Athena_PartModifier_Generic_C",
                        Replace = Data["PartModifierBP"],
                        Type = SwapType.BackblingPartBP
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_F_MED_ExoSuit_Cape/Meshes/F_MED_ExoSuit_Cape_Pack_Cloth_AnimBP.F_MED_ExoSuit_Cape_Pack_Cloth_AnimBP_C",
                        Replace = Data["ABP"] ?? "/Game/Accessories/FORT_Backpacks/Backpack_F_MED_ExoSuit_Cape/Meshes/F_MED_ExoSuit_Cape_Pack_Cloth_AnimBP.F_MED_ExoSuit_Cape_Pack_Cloth_AnimBP_C",
                        Type = SwapType.BackblingAnim
                    }
                }
            }
        };
}
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Data.SwapOptions.Backblings;

internal sealed class ThorsCloakBackblingSwap : BackblingSwap
{
    public ThorsCloakBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data)
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_HightowerTapas",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_M_MED_Tapas/Meshes/M_MED_Tapas_Pack.M_MED_Tapas_Pack",
                        Replace = Data["Mesh"],
                        Type = SwapType.BackblingMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_M_MED_Tapas/Meshes/M_MED_Tapas_Pack_AnimBP.M_MED_Tapas_Pack_AnimBP_C",
                        Replace = Data["ABP"] ?? "/Game/Accessories/FORT_Backpacks/Backpack_M_MED_Tapas/Meshes/M_MED_Tapas_Pack_AnimBP.M_MED_Tapas_Pack_AnimBP_C",
                        Type = SwapType.BackblingAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Cosmetics/Blueprints/Part_Modifiers/B_Athena_PartModifier_Backpack_Hightower_Tapas.B_Athena_PartModifier_Backpack_Hightower_Tapas_C",
                        Replace = Data["PartModifierBP"],
                        Type = SwapType.Modifier
                    },
                }
            }
        };
}

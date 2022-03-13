using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Data.SwapOptions.Backblings;

internal sealed class FirestarterBackblingSwap : BackblingSwap
{
    public FirestarterBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data)
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_RenegadeRaiderFire",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_F_MED_RenegadeRaiderFire/Mesh/F_MED_RenegadeRaiderFire_Pack.F_MED_RenegadeRaiderFire_Pack",
                        Replace = Data["Mesh"],
                        Type = SwapType.BackblingMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Cosmetics/Blueprints/Part_Modifiers/B_Athena_PartModifier_Backpack_RenegadeRaider.B_Athena_PartModifier_Backpack_RenegadeRaider_C",
                        Replace = Data["PartModifierBP"],
                        Type = SwapType.BackblingFx
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_F_MED_RenegadeRaiderFire/FX/NS_Backpack_RaiderFire_2DGas.NS_Backpack_RaiderFire_2DGas",
                        Replace = Data["NFX"],
                        Type = SwapType.BackblingFx
                    }
                }
            }
        };
}

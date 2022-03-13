using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Backblings;

internal class NucleusBackblingSwap : BackblingSwap
{
    public NucleusBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data)
        : base(name, rarity, icon, data)
    {}

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_CelestialFemale",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_F_MED_Celestial/Meshes/F_MED_Celestial_Pack.F_MED_Celestial_Pack",
                        Replace = Data["Mesh"],
                        Type = SwapType.BackblingMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Cosmetics/Blueprints/B_Athena_PartModifier_Generic.B_Athena_PartModifier_Generic_C",
                        Replace = Data["PartModifierBP"],
                        Type = SwapType.BackblingFx
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_F_MED_Celestial/FX/NS_Celestial_F_Backpack.NS_Celestial_F_Backpack",
                        Replace = Data["NFX"],
                        Type = SwapType.BackblingFx
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_F_MED_Celestial/Meshes/F_MED_Celestial_Pack_AnimBP.F_MED_Celestial_Pack_AnimBP_C",
                        Replace = Data["ABP"] ?? "/Game/Accessories/FORT_Backpacks/Backpack_F_MED_Celestial/Meshes/F_MED_Celestial_Pack_AnimBP.F_MED_Celestial_Pack_AnimBP_C",
                        Type = SwapType.BackblingPartBP
                    },
                }
            }
        };
}
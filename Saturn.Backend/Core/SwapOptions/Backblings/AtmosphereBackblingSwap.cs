using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.SwapOptions.Backblings;

internal sealed class AtmosphereBackblingswap : BackblingSwap
{
    public AtmosphereBackblingswap(string name, string rarity, string icon, Dictionary<string, string> data)
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_StreetRacerDriftRemix",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_F_MED_Street_Racer_Drift_Remix/Mesh/F_MED_Street_Racer_Drift_Remix_Pack.F_MED_Street_Racer_Drift_Remix_Pack",
                        Replace = Data["Mesh"],
                        Type = SwapType.BackblingMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_F_MED_Street_Racer_Drift_Remix/FX/P_Backpack_StreetRacer_DriftRemix.P_Backpack_StreetRacer_DriftRemix",
                        Replace = Data["FX"],
                        Type = SwapType.BackblingFx
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Athena/Cosmetics/Blueprints/B_Athena_PartModifier_Generic.B_Athena_PartModifier_Generic_C",
                        Replace = Data["PartModifierBP"],
                        Type = SwapType.BackblingPartBP
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_F_MED_Street_Racer_Drift_Remix/Mesh/F_MED_Street_Racer_Drift_Remix_Pack_AnimBP.F_MED_Street_Racer_Drift_Remix_Pack_AnimBP_C",
                        Replace = Data["ABP"] ?? "/Game/Accessories/FORT_Backpacks/Backpack_F_MED_Street_Racer_Drift_Remix/Mesh/F_MED_Street_Racer_Drift_Remix_Pack_AnimBP.F_MED_Street_Racer_Drift_Remix_Pack_AnimBP_C",
                        Type = SwapType.BackblingAnim
                    }
                }
            }
        };
}
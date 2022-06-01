﻿using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Data.SwapOptions.Backblings;

internal sealed class DeepDiveBackblingSwap : BackblingSwap
{
    public DeepDiveBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data)
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_TacticalScuba",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_M_MED_Tactical_Scuba/Meshes/M_MED_Tactical_Scuba_Pack.M_MED_Tactical_Scuba_Pack",
                        Replace = Data["Mesh"],
                        Type = SwapType.BackblingMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_M_MED_Tactical_Scuba/Meshes/M_MED_Tactical_Scuba_Pack2_AnimBP.M_MED_Tactical_Scuba_Pack2_AnimBP_C",
                        Replace = Data["ABP"] ?? "/Game/Accessories/FORT_Backpacks/Backpack_M_MED_Tactical_Scuba/Meshes/M_MED_Tactical_Scuba_Pack2_AnimBP.M_MED_Tactical_Scuba_Pack2_AnimBP_C",
                        Type = SwapType.BackblingAnim
                    }
                }
            }
        };
}

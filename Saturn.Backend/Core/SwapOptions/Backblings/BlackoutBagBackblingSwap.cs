using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.SwapOptions.Backblings;

internal sealed class BlackoutBagBackblingSwap : BackblingSwap
{
    public BlackoutBagBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data)
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_StreetFashionEclipse",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/F_MED_Street_Fashion_Red/Meshes/F_MED_Street_Fashion_Red_Pack.F_MED_Street_Fashion_Red_Pack",
                        Replace = Data["Mesh"],
                        Type = SwapType.BackblingMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/F_MED_Street_Fashion_Red/Skins/Eclipse/Materials/F_MED_StreetFashionEclipse_Backpack.F_MED_StreetFashionEclipse_Backpack",
                        Replace = Data["Material"],
                        Type = SwapType.BackblingMaterial
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/F_MED_Street_Fashion_Red/Meshes/F_MED_Street_Fashion_Red_Pack_AnimBp.F_MED_Street_Fashion_Red_Pack_AnimBp_C",
                        Replace = Data["ABP"] ?? "/Game/Accessories/FORT_Backpacks/F_MED_Street_Fashion_Red/Meshes/F_MED_Street_Fashion_Red_Pack_AnimBp.F_MED_Street_Fashion_Red_Pack_AnimBp_C",
                        Type = SwapType.BackblingAnim
                    }
                }
            }
        };
}

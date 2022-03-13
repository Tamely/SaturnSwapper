using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Data.SwapOptions.Backblings;

internal sealed class BannerCapeBackblingSwap : BackblingSwap
{
    public BannerCapeBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data)
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_Banner",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Capes/F_MED_Banner_Cape/Meshes/F_MED_Banner_Pack.F_MED_Banner_Pack",
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
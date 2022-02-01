using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Data.SwapOptions.Backblings;

internal sealed class WrappingCaperBackblingSwap : BackblingSwap
{
    public WrappingCaperBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data)
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_CardboardCrewHolidayMale",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Capes/M_MED_Cardboard_Crew_Holiday_Cape/Meshes/M_MED_Cardboard_Crew_Holiday_Cape.M_MED_Cardboard_Crew_Holiday_Cape",
                        Replace = Data["Mesh"],
                        Type = SwapType.BackblingMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Capes/M_MED_Cardboard_Crew_Holiday_Cape/Meshes/M_MED_Cardboard_Crew_Holiday_Cape_AnimBP.M_MED_Cardboard_Crew_Holiday_Cape_AnimBP_C",
                        Replace = Data["ABP"] ?? "/Game/Accessories/FORT_Capes/M_MED_Cardboard_Crew_Holiday_Cape/Meshes/M_MED_Cardboard_Crew_Holiday_Cape_AnimBP.M_MED_Cardboard_Crew_Holiday_Cape_AnimBP_C",
                        Type = SwapType.BackblingAnim
                    }
                }
            }
        };
}

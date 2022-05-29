using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Pickaxes;

internal sealed class SulfuricSwap : PickaxeSwap
{
    public SulfuricSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon, swaps, rarityEnum)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Athena/Items/Weapons/WID_Harvest_Pickaxe_NeonGraffitiLavaFemale",
                Swaps = new List<SaturnSwap>()
                {
                     new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[] { 252, 255, 255, 255 }),
                        Replace = System.Convert.ToBase64String(new byte[] { 0, 0, 0, 0 }),
                        Type = SwapType.Property
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-NeonGraffitiLavaPickaxe.T-Icon-Pickaxes-NeonGraffitiLavaPickaxe",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.SmallIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-NeonGraffitiLavaPickaxe-L.T-Icon-Pickaxes-NeonGraffitiLavaPickaxe-L",
                        Replace = "/",
                        Type = SwapType.LargeIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_NeonGraffiti/Meshes/SK_Pickaxe_NeonGraffiti.SK_Pickaxe_NeonGraffiti",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/NeonGraffiti/Pickaxe_NeonGraffiti_Swing_Cue.Pickaxe_NeonGraffiti_Swing_Cue",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/NeonGraffiti/Pickaxe_NeonGraffiti_Equip_Cue.Pickaxe_NeonGraffiti_Equip_Cue",
                        Replace = Swaps["EquipCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/NeonGraffiti/Pickaxe_NeonGraffiti_Impact_Cue.Pickaxe_NeonGraffiti_Impact_Cue",
                        Replace = Swaps["ImpactCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_NeonGraffiti/Skins/Lava_Female/FX/NS_Pickaxe_NeonGraffiti_Lava_AnimTrail.NS_Pickaxe_NeonGraffiti_Lava_AnimTrail",
                        Replace = Swaps["Trail"],
                        Type = SwapType.WeaponTrail
                    }
                }
            }
        };
}

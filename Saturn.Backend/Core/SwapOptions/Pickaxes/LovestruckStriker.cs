using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.SwapOptions.Pickaxes;

internal sealed class LovestruckStrikerSwap : PickaxeSwap
{
    public LovestruckStrikerSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon, swaps, rarityEnum)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Weapons/WID_Harvest_Pickaxe_CrushFemale1H",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[] { 255, 255, 255, 2 }),
                        Replace = System.Convert.ToBase64String(new byte[] { 255, 255, 255, (byte)RarityEnum }),
                        Type = SwapType.Property
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-CrushPickaxe.T-Icon-Pickaxes-CrushPickaxe",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.SmallIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-CrushPickaxe-L.T-Icon-Pickaxes-CrushPickaxe-L",
                        Replace = "/",
                        Type = SwapType.LargeIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Crush_Female/Meshes/Crush_Female.Crush_Female",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/1H_CrushFemale/PickaxeSwing_CrushFemale_Cue.PickaxeSwing_CrushFemale_Cue",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/1H_CrushFemale/PickaxeReady_CrushFemale_Cue.PickaxeReady_CrushFemale_Cue",
                        Replace = Swaps["EquipCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/1H_CrushFemale/PickaxeImpactEnemy_CrushFemale_Cue.PickaxeImpactEnemy_CrushFemale_Cue",
                        Replace = Swaps["ImpactCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Crush_Female/FX/NS_Pickaxe_Crush_Fuse_Idle.NS_Pickaxe_Crush_Fuse_Idle",
                        Replace = Swaps["NFX"],
                        Type = SwapType.WeaponFx
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Crush_Female/FX/NS_Pickaxe_Crush_Fuse_Trail.NS_Pickaxe_Crush_Fuse_Trail",
                        Replace = Swaps["OffhandTrail"],
                        Type = SwapType.WeaponTrail
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Weapons/FORT_Melee/Pickaxe_Crush_Female/FX/NS_Pickaxe_Crush_Fuse_Impact.NS_Pickaxe_Crush_Fuse_Impact",
                        Replace = Swaps["ImpactFX"],
                        Type = SwapType.WeaponFx
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Weapons/FORT_Melee/Pickaxe_Crush_Female/FX/NS_Pickaxe_Crush_Fuse_Trail.NS_Pickaxe_Crush_Fuse_Trail",
                        Replace = Swaps["Trail"],
                        Type = SwapType.WeaponTrail
                    }
                }
            }
        };
}
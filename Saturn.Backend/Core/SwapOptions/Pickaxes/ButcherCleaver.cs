using System.Collections.Generic;
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;

namespace Saturn.Backend.Core.SwapOptions.Pickaxes;

internal sealed class ButcherCleaverSwap : PickaxeSwap
{
    public ButcherCleaverSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon, swaps, rarityEnum)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Weapons/WID_Harvest_Pickaxe_RustyBoltSliceMale",
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
                        Search = System.Convert.ToBase64String(new byte[] { 255, 255, 255, 3 }),
                        Replace = System.Convert.ToBase64String(new byte[] { 255, 255, 255, (byte)RarityEnum }),
                        Type = SwapType.Property
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-RustyBoltSlicePickaxe.T-Icon-Pickaxes-RustyBoltSlicePickaxe",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.SmallIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-RustyBoltSlicePickaxe-L.T-Icon-Pickaxes-RustyBoltSlicePickaxe-L",
                        Replace = "/",
                        Type = SwapType.LargeIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_RustyBolt_Slice_Male/Meshes/RustyBolt_Slice_Male_Axe.RustyBolt_Slice_Male_Axe",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/RustyBoltSliceMale/PickaxeSwing_RustyBoltSliceMale.PickaxeSwing_RustyBoltSliceMale",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/RustyBoltSliceMale/PickaxeReady_RustyBoltSliceMale.PickaxeReady_RustyBoltSliceMale",
                        Replace = Swaps["EquipCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/RustyBoltSliceMale/PickaxeImpactEnemy_RustyBoltSliceMale.PickaxeImpactEnemy_RustyBoltSliceMale",
                        Replace = Swaps["ImpactCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_RustyBolt_Slice_Male/FX/NS_RustyBolt_Slice_Male_Idle.NS_RustyBolt_Slice_Male_Idle",
                        Replace = Swaps["NFX"],
                        Type = SwapType.WeaponFx
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_RustyBolt_Slice_Male/FX/NS_RustyBolt_Slice_Male_Trail.NS_RustyBolt_Slice_Male_Trail",
                        Replace = Swaps["Trail"],
                        Type = SwapType.WeaponTrail
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Weapons/FORT_Melee/Pickaxe_Skirmish_Female/FX/NS_Pickaxe_Metal_Impact.NS_Pickaxe_Metal_Impact",
                        Replace = Swaps["ImpactFX"],
                        Type = SwapType.WeaponFx
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Weapons/FORT_Melee/Pickaxe_RustyBolt_Slice_Male/FX/NS_RustyBolt_Slice_Male_Idle.NS_RustyBolt_Slice_Male_Idle",
                        Replace = Swaps["OffhandTrail"],
                        Type = SwapType.WeaponTrail
                    }
                }
            }
        };
}
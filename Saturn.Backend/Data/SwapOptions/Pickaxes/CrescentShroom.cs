using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Pickaxes;

internal sealed class CrescentShroomSwap : PickaxeSwap
{
    public CrescentShroomSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon, swaps, rarityEnum)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "Game/Athena/Items/Weapons/WID_Harvest_Pickaxe_ShiitakeShaolinMale",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[] { 255, 255, 255, (byte)EFortRarity.Rare  }),
                        Replace = System.Convert.ToBase64String(new byte[] { 255, 255, 255, (byte)RarityEnum }),
                        Type = SwapType.Property
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-ShiitakeShaolinPickaxe.T-Icon-Pickaxes-ShiitakeShaolinPickaxe",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.SmallIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-ShiitakeShaolinPickaxe-L.T-Icon-Pickaxes-ShiitakeShaolinPickaxe-L",
                        Replace = "/",
                        Type = SwapType.LargeIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Shiitake_Shaolin_Male/Meshes/Shiitake_Shaolin_Male_Axe.Shiitake_Shaolin_Male_Axe",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/Shittake/PickAxe_ShittakeShaolinMale_Swing_Cue.PickAxe_ShittakeShaolinMale_Swing_Cue",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/Shittake/PickAxe_ShittakeShaolinMale_Equip_Cue.PickAxe_ShittakeShaolinMale_Equip_Cue",
                        Replace = Swaps["EquipCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/Shittake/PickAxe_ShittakeShaolinMale_EnemyImpact_Cue.PickAxe_ShittakeShaolinMale_EnemyImpact_Cue",
                        Replace = Swaps["ImpactCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Shiitake_Shaolin_Male/FX/NS_Pickaxe_Shiitake_Shaolin_Trail.NS_Pickaxe_Shiitake_Shaolin_Trail",
                        Replace = Swaps["Trail"],
                        Type = SwapType.WeaponTrail
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Weapons/FORT_Melee/Pickaxe_Shiitake_Shaolin_Male/FX/NS_Pickaxe_Shiitake_Shaolin_Idle.NS_Pickaxe_Shiitake_Shaolin_Idle",
                        Replace = Swaps["NFX"],
                        Type = SwapType.WeaponFx
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Weapons/FORT_Melee/Pickaxe_Shiitake_Shaolin_Male/FX/NS_Pickaxe_Shiitake_Shaolin_Impact.NS_Pickaxe_Shiitake_Shaolin_Impact",
                        Replace = Swaps["ImpactFX"],
                        Type = SwapType.WeaponFx
                    }
                }
            }
        };
}

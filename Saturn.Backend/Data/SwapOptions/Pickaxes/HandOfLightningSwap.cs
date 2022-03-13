using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Pickaxes;

internal sealed class HandOfLightningSwap : PickaxeSwap
{
    public HandOfLightningSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon, swaps, rarityEnum)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Weapons/WID_Harvest_Pickaxe_HightowerSquash1H",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[] { 255, 255, 255, 3 }),
                        Replace = System.Convert.ToBase64String(new byte[] { 255, 255, 255, (byte)rarityEnum }),
                        Type = SwapType.Property
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-HTSquashPickaxe.T-Icon-Pickaxes-HTSquashPickaxe",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.SmallIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-HTSquashPickaxe-L.T-Icon-Pickaxes-HTSquashPickaxe-L",
                        Replace = "/",
                        Type = SwapType.LargeIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_Pickaxe/Meshes/M_MED_Pickaxe_Pack.M_MED_Pickaxe_Pack",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/Hightower_Squash/Pickaxe_Hightower_Squash_Swing_Cue.Pickaxe_Hightower_Squash_Swing_Cue",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/Hightower_Squash/Pickaxe_Hightower_Squash_Equip_Cue.Pickaxe_Hightower_Squash_Equip_Cue",
                        Replace = Swaps["EquipCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/Hightower_Squash/Pickaxe_Hightower_Squash_Impact_Cue.Pickaxe_Hightower_Squash_Impact_Cue",
                        Replace = Swaps["ImpactCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_HighTower_Squash_1H/FX/NS_Pickaxe_HighTower_Squash_1H_Animtrail.NS_Pickaxe_HighTower_Squash_1H_Animtrail",
                        Replace = Swaps["Trail"],
                        Type = SwapType.WeaponTrail
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Weapons/FORT_Melee/Pickaxe_HighTower_Squash_1H/FX/NS_PIckaxe_HighTowerSquash_1H_Idle.NS_PIckaxe_HighTowerSquash_1H_Idle",
                        Replace = Swaps["NFX"],
                        Type = SwapType.WeaponFx
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Weapons/FORT_Melee/Pickaxe_HighTower_Squash_1H/FX/NS_Pickaxe_HighTower_Squash_1H_Impact.NS_Pickaxe_HighTower_Squash_1H_Impact",
                        Replace = Swaps["ImpactFX"],
                        Type = SwapType.WeaponFx
                    }
                }
            }
        };
}
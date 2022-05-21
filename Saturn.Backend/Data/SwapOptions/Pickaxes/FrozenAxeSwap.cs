using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Pickaxes;

internal class FrozenAxeSwap : PickaxeSwap
{
    public FrozenAxeSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon, swaps, rarityEnum)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Weapons/WID_Harvest_Pickaxe_FlintlockWinter",
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
                        Search = System.Convert.ToBase64String(new byte[] { 255, 255, 255, 2 }),
                        Replace = System.Convert.ToBase64String(new byte[] { 255, 255, 255, (byte)RarityEnum }),
                        Type = SwapType.Property
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-Pickaxe-ID-143-FlintlockWinter.T-Icon-Pickaxes-Pickaxe-ID-143-FlintlockWinter",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.SmallIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-Pickaxe-ID-143-FlintlockWinter-L.T-Icon-Pickaxes-Pickaxe-ID-143-FlintlockWinter-L",
                        Replace = "/",
                        Type = SwapType.LargeIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Flintlock_RedKnight/Meshes/SK_Pickaxe_Flintlock_RedKnight.SK_Pickaxe_Flintlock_RedKnight",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Flintlock_RedKnight_Winter/Materials/MI_Pickaxe_Flintlock_RedKnight_Winter.MI_Pickaxe_Flintlock_RedKnight_Winter",
                        Replace = Swaps["Material"],
                        Type = SwapType.WeaponMaterial
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/FlintlockWinter/Pickaxe_WinterFlintlock_Swing.Pickaxe_WinterFlintlock_Swing",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/FlintlockWinter/Pickaxe_WinterFlintlock_Ready.Pickaxe_WinterFlintlock_Ready",
                        Replace = Swaps["EquipCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/FlintlockWinter/Pickaxe_WinterFlintlock_Impact.Pickaxe_WinterFlintlock_Impact",
                        Replace = Swaps["ImpactCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Effects/Fort_Effects/Effects/Melee/P_Melee_Trail_Default.P_Melee_Trail_Default",
                        Replace = Swaps["Trail"],
                        Type = SwapType.WeaponTrail
                    }
                }
            }
        };
}
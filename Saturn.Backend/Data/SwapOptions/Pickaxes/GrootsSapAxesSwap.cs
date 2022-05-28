using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Pickaxes;

internal sealed class GrootsSapAxesSwap : PickaxeSwap
{
    public GrootsSapAxesSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon, swaps, rarityEnum)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Weapons/WID_Harvest_Pickaxe_HightowerGrapeMale1H",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[] { 255, 255, 255, 3 }),
                        Replace = System.Convert.ToBase64String(new byte[] { 255, 255, 255, (byte)RarityEnum }),
                        Type = SwapType.Property
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-HightowerGrape1HPickaxe.T-Icon-Pickaxes-HightowerGrape1HPickaxe",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.SmallIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-HightowerGrape1HPickaxe-L.T-Icon-Pickaxes-HightowerGrape1HPickaxe-L",
                        Replace = "/",
                        Type = SwapType.LargeIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_HighTower_Grape_Male_1h/Meshes/HighTower_Grape_Male_1h.HighTower_Grape_Male_1h",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/HightowerGrape/PickaxeSwing_HightowerGrape.PickaxeSwing_HightowerGrape",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/HightowerGrape/PickaxeReady_HightowerGrape.PickaxeReady_HightowerGrape",
                        Replace = Swaps["EquipCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/HightowerGrape/PickaxeImpactEnemy_HightowerGrape.PickaxeImpactEnemy_HightowerGrape",
                        Replace = Swaps["ImpactCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_HighTower_Grape_Male_1h/FX/P_Pickaxe_HightowerGrape_AnimTrail.P_Pickaxe_HightowerGrape_AnimTrail",
                        Replace = Swaps["Trail"],
                        Type = SwapType.WeaponTrail
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Weapons/FORT_Melee/Pickaxe_HighTower_Grape_Male_1h/FX/NS_Pickaxe_HightowerGrape_1H_Impact.NS_Pickaxe_HightowerGrape_1H_Impact",
                        Replace = Swaps["ImpactFX"],
                        Type = SwapType.WeaponFx
                    }
                }
            }
        };
}
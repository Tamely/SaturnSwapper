using System;
using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Services;

namespace Saturn.Backend.Data.SwapOptions.Pickaxes;

internal sealed class DefaultPickaxeSwap : PickaxeSwap
{
    public DefaultPickaxeSwap(string name, string rarity, string icon, Dictionary<string, string> swaps)
        : base(name, rarity, icon, swaps, EFortRarity.Common)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Weapons/WID_Harvest_Pickaxe_Athena_C_T01",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-DefaultMarkIIIPickaxe.T-Icon-Pickaxes-DefaultMarkIIIPickaxe",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.SmallIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-DefaultMarkIIIPickaxe-L.T-Icon-Pickaxes-DefaultMarkIIIPickaxe-L",
                        Replace = "/",
                        Type = SwapType.LargeIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Default_Mark_III/Meshes/Default_Mark_III_Axe.Default_Mark_III_Axe",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/MarkIIIMale/PickaxeSwing_MarkIIIMale.PickaxeSwing_MarkIIIMale",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/MarkIIIMale/PickaxeReady_MarkIIIMale.PickaxeReady_MarkIIIMale",
                        Replace = Swaps["EquipCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/MarkIIIMale/PickaxeImpactEnemy_MarkIIIMale.PickaxeImpactEnemy_MarkIIIMale",
                        Replace = Swaps["ImpactCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Default_Mark_III/FX/NS_Pickaxe_Defualt_Mark_III_Trail.NS_Pickaxe_Defualt_Mark_III_Trail",
                        Replace = Swaps["Trail"],
                        Type = SwapType.WeaponTrail
                    }
                }
            }
        };
}
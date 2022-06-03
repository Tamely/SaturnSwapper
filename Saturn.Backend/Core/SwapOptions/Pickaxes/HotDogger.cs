using System.Collections.Generic;
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;

namespace Saturn.Backend.Core.SwapOptions.Pickaxes;

internal sealed class HotDoggerSwap : PickaxeSwap
{
    public HotDoggerSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon, swaps, rarityEnum)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Weapons/WID_Harvest_Pickaxe_RelishFemale",
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
                           "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-RelishFPickaxe.T-Icon-Pickaxes-RelishFPickaxe",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.SmallIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                           "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-RelishFPickaxe-L.T-Icon-Pickaxes-RelishFPickaxe-L",
                        Replace = "/",
                        Type = SwapType.LargeIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Relish_Female/Meshes/Relish_Female_Axe.Relish_Female_Axe",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/RelishFemale/PickaxeSwing_RelishFemale.PickaxeSwing_RelishFemale",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                           "/Game/Athena/Sounds/Weapons/PickAxes/RelishFemale/PickaxeReady_RelishFemale.PickaxeReady_RelishFemale",
                        Replace = Swaps["EquipCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                           "/Game/Athena/Sounds/Weapons/PickAxes/RelishFemale/PickaxeImpactEnemy_RelishFemale.PickaxeImpactEnemy_RelishFemale",
                        Replace = Swaps["ImpactCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                           "/Game/Weapons/FORT_Melee/Pickaxe_Relish_Female/FX/NS_Pickaxe_Relish_Female_Idle.NS_Pickaxe_Relish_Female_Idle",
                        Replace = Swaps["NFX"],
                        Type = SwapType.WeaponFx
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Relish_Female/FX/NS_Pickaxe_Relish_Female_AnimTrail.NS_Pickaxe_Relish_Female_AnimTrail",
                        Replace = Swaps["Trail"],
                        Type = SwapType.WeaponTrail
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Weapons/FORT_Melee/Pickaxe_Relish_Female/FX/NS_Pickaxe_Relish_Female_Impact.NS_Pickaxe_Relish_Female_Impact",
                        Replace = Swaps["ImpactFX"],
                        Type = SwapType.WeaponFx
                    }
                }
            }
        };
}
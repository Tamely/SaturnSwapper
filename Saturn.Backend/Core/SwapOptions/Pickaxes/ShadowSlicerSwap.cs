using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.SwapOptions.Pickaxes;

internal sealed class ShadowSlicerSwap : PickaxeSwap
{
    public ShadowSlicerSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon, swaps, rarityEnum)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Athena/Items/Weapons/WID_Harvest_Pickaxe_StreetFashionEclipseFemale",
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
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-StreetFashionEclipsePickaxe.T-Icon-Pickaxes-StreetFashionEclipsePickaxe",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.SmallIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-StreetFashionEclipsePickaxe-L.T-Icon-Pickaxes-StreetFashionEclipsePickaxe-L",
                        Replace = "/",
                        Type = SwapType.LargeIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_StreetFashionRed/Meshes/Demo/SK_Pickaxe_StreetFashionRed_DEMO.SK_Pickaxe_StreetFashionRed_DEMO",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Street_Fashion_Eclipse_Female/Materials/MI_Pickaxe_StreetFashionEclipseFemale.MI_Pickaxe_StreetFashionEclipseFemale",
                        Replace = Swaps["Material"],
                        Type = SwapType.WeaponMaterial
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/Street_Fashion_Red/PA_StreetFashionRed_Swing_Cue.PA_StreetFashionRed_Swing_Cue",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/Street_Fashion_Red/PA_StreetFashionRed_Ready_Cue.PA_StreetFashionRed_Ready_Cue",
                        Replace = Swaps["EquipCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/Street_Fashion_Red/PA_StreetFashionRed_Impact_Cue.PA_StreetFashionRed_Impact_Cue",
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
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.SwapOptions.Pickaxes;

internal sealed class HackAndSmashSwap : PickaxeSwap
{
    public HackAndSmashSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon, swaps, rarityEnum)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Weapons/WID_Harvest_Pickaxe_HenchmanMale1H",
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
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-Henchman1hPickaxes.T-Icon-Pickaxes-Henchman1hPickaxes",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.SmallIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-Henchman1hPickaxes-L.T-Icon-Pickaxes-Henchman1hPickaxes-L",
                        Replace = "/",
                        Type = SwapType.LargeIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Henchman_Male_1h/Meshes/Henchman_Male_1h_Left.Henchman_Male_1h_Left",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Henchman_Male_1h/Meshes/Henchman_Male_1h_Right.Henchman_Male_1h_Right",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/1H_Henchman_Male/PickAxe_Henchman_Male_Cleaver_Swing_Cue.PickAxe_Henchman_Male_Cleaver_Swing_Cue",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/1H_Henchman_Male/PickAxe_Henchman_Male_Hammer_Swing_Cue.PickAxe_Henchman_Male_Hammer_Swing_Cue",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/1H_Henchman_Male/PickAxe_Henchman_Male_Hammer_Ready_Cue.PickAxe_Henchman_Male_Hammer_Ready_Cue",
                        Replace = Swaps["EquipCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/1H_Henchman_Male/PickAxe_Henchman_Male_Hammer_Impact_Cue.PickAxe_Henchman_Male_Hammer_Impact_Cue",
                        Replace = Swaps["ImpactCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/1H_Henchman_Male/PickAxe_Henchman_Male_Cleaver_Impact_Cue.PickAxe_Henchman_Male_Cleaver_Impact_Cue",
                        Replace = Swaps["ImpactCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Effects/Fort_Effects/Effects/Melee/P_Melee_Trail_Generic_Shorter.P_Melee_Trail_Generic_Shorter",
                        Replace = Swaps["Trail"],
                        Type = SwapType.WeaponTrail
                    }
                }
            }
        };
}
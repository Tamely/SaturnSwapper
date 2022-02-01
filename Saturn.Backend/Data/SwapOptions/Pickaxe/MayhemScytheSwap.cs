using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Utils.Swaps;

namespace Saturn.Backend.Data.SwapOptions.Pickaxe;

internal sealed class MayhemScytheSwap : PickaxeSwap
{
    public MayhemScytheSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon, swaps, rarityEnum)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Weapons/WID_Harvest_Pickaxe_MastermindShadow",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[] { 255, 255, 255, 2 }),
                        Replace = System.Convert.ToBase64String(new byte[] { 255, 255, 255, (byte)rarityEnum }),
                        Type = SwapType.Property
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-Mastermind-Shadow-Pickaxe.T-Icon-Pickaxes-Mastermind-Shadow-Pickaxe",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.SmallIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-Mastermind-Shadow-Pickaxe-L.T-Icon-Pickaxes-Mastermind-Shadow-Pickaxe-L",
                        Replace = "/",
                        Type = SwapType.LargeIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Mastermind/Meshes/SK_Pickaxe_Mastermind.SK_Pickaxe_Mastermind",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Mastermind_Shadow/Materials/MI_Pickaxe_Mastermind.MI_Pickaxe_Mastermind",
                        Replace = Swaps["Material"],
                        Type = SwapType.WeaponMaterial
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/Mastermind/PickAxe_MasterMind_Swing_Athena_Cue.PickAxe_MasterMind_Swing_Athena_Cue",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/Mastermind/PickAxe_MasterMind_Ready_Athena_Cue.PickAxe_MasterMind_Ready_Athena_Cue",
                        Replace = Swaps["EquipCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/Mastermind/PickAxe_MasterMind_Impact_Player_Athena_Cue.PickAxe_MasterMind_Impact_Player_Athena_Cue",
                        Replace = Swaps["ImpactCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Mastermind_Shadow/FX/P_Mastermind_Shadow_AnimTrail.P_Mastermind_Shadow_AnimTrail",
                        Replace = Swaps["Trail"],
                        Type = SwapType.WeaponTrail
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Mastermind_Shadow/FX/P_Mastermind_Shadow_Idle.P_Mastermind_Shadow_Idle",
                        Replace = Swaps["FX"],
                        Type = SwapType.WeaponFx
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Mastermind_Shadow/FX/P_Masermind_Shadow_Swing.P_Masermind_Shadow_Swing",
                        Replace = Swaps["SwingFX"],
                        Type = SwapType.WeaponFx
                    }
                }
            }
        };
}
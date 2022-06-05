using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.SwapOptions.Pickaxes;

internal sealed class TorinsLightbladeSwap : PickaxeSwap
{
    public TorinsLightbladeSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon, swaps, rarityEnum)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Weapons/WID_Harvest_Pickaxe_GhostHunterFemale1H",
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
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-GhostHunterPickaxe.T-Icon-Pickaxes-GhostHunterPickaxe",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.SmallIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-GhostHunterPickaxe-L.T-Icon-Pickaxes-GhostHunterPickaxe-L",
                        Replace = "/",
                        Type = SwapType.LargeIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Ghost_Hunter_Female_1h/Meshes/Pickaxe_Ghost_Hunter_Female_1h.Pickaxe_Ghost_Hunter_Female_1h",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/GhostHunterFemale1H/PickaxeSwing_GhostHunterFemale1H.PickaxeSwing_GhostHunterFemale1H",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/GhostHunterFemale1H/PickaxeReady_GhostHunterFemale1H.PickaxeReady_GhostHunterFemale1H",
                        Replace = Swaps["EquipCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/GhostHunterFemale1H/PickaxeImpactEnemy_GhostHunterFemale1H.PickaxeImpactEnemy_GhostHunterFemale1H",
                        Replace = Swaps["ImpactCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Ghost_Hunter_Female_1h/FX/NS_GhostHunter_Pickaxe_Trail.NS_GhostHunter_Pickaxe_Trail",
                        Replace = Swaps["OffhandTrail"],
                        Type = SwapType.WeaponTrail
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Weapons/FORT_Melee/Pickaxe_Ghost_Hunter_Female_1h/FX/NS_Pickaxe_GhostHunter_Pickaxe_Impact.NS_Pickaxe_GhostHunter_Pickaxe_Impact",
                        Replace = Swaps["ImpactFX"],
                        Type = SwapType.WeaponFx
                    }
                }
            }
        };
}
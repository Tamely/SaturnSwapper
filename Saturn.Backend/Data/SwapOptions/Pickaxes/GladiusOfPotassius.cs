using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Pickaxes;

internal sealed class GladiusOfPotassiusSwap : PickaxeSwap
{
    public GladiusOfPotassiusSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon, swaps, rarityEnum)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Weapons/WID_Harvest_Pickaxe_BananaLeader",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[] { 255, 255, 255, 1 }),
                        Replace = System.Convert.ToBase64String(new byte[] { 255, 255, 255, (byte)RarityEnum }),
                        Type = SwapType.Property
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-BananaLeaderPickaxe.T-Icon-Pickaxes-BananaLeaderPickaxe",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.SmallIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-BananaLeaderPickaxe-L.T-Icon-Pickaxes-BananaLeaderPickaxe-L",
                        Replace = "/",
                        Type = SwapType.LargeIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Banana_Leader/Meshes/Banana_Leader_Axe.Banana_Leader_Axe",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/BananaLeader/PickaxeSwing_BananaLeader.PickaxeSwing_BananaLeader",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/BananaLeader/PickaxeReady_BananaLeader.PickaxeReady_BananaLeader",
                        Replace = Swaps["EquipCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/BananaLeader/PickaxeImpactEnemy_BananaLeader.PickaxeImpactEnemy_BananaLeader",
                        Replace = Swaps["ImpactCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Banana_Leader/FX/P_Pickaxe_BananaLeader_Trail.P_Pickaxe_BananaLeader_Trail",
                        Replace = Swaps["Trail"],
                        Type = SwapType.WeaponTrail
                    }
                }
            }
        };
}
using System.Collections.Generic;
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;

namespace Saturn.Backend.Core.SwapOptions.Pickaxes;

internal sealed class PhantasmicPulseSwap : PickaxeSwap
{
    public PhantasmicPulseSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon, swaps, rarityEnum)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Weapons/WID_Harvest_Pickaxe_Elastic1H",
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
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-ElasticPickaxesCosmic.T-Icon-Pickaxes-ElasticPickaxesCosmic",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.SmallIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-ElasticPickaxesCosmic-L.T-Icon-Pickaxes-ElasticPickaxesCosmic-L",
                        Replace = "/",
                        Type = SwapType.LargeIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Elastic/Cosmic/SM_Pickaxe_Elastic1H_Cosmic.SM_Pickaxe_Elastic1H_Cosmic",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/ElasticCosmic/PickaxeSwing_ElasticCosmic_Right.PickaxeSwing_ElasticCosmic_Right",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/ElasticCosmic/PickaxeReady_ElasticCosmic.PickaxeReady_ElasticCosmic",
                        Replace = Swaps["EquipCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/ElasticCosmic/PickaxeImpactEnemy_ElasticCosmic.PickaxeImpactEnemy_ElasticCosmic",
                        Replace = Swaps["ImpactCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Elastic/Cosmic/NS_Pickaxe_Elastic_Cosmic_AnimTrail.NS_Pickaxe_Elastic_Cosmic_AnimTrail",
                        Replace = Swaps["Trail"],
                        Type = SwapType.WeaponTrail
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Weapons/FORT_Melee/Pickaxe_Elastic/Cosmic/NS_Pickaxe_Elastic_Cosmic_Idle.NS_Pickaxe_Elastic_Cosmic_Idle",
                        Replace = Swaps["NFX"],
                        Type = SwapType.WeaponFx
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Weapons/FORT_Melee/Pickaxe_Elastic/Cosmic/NS_Elastic_Cosmic_Impact.NS_Elastic_Cosmic_Impact",
                        Replace = Swaps["ImpactFX"],
                        Type = SwapType.WeaponFx
                    }
                }
            }
        };
}
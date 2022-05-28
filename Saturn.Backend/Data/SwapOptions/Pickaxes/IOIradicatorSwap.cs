using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Pickaxes;

internal sealed class IOIradicatorSwap : PickaxeSwap
{
    public IOIradicatorSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon, swaps, rarityEnum)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Weapons/WID_Harvest_Pickaxe_InnovatorFemale",
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
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-InnovatorPickaxe.T-Icon-Pickaxes-InnovatorPickaxe",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.SmallIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-InnovatorPickaxe-L.T-Icon-Pickaxes-InnovatorPickaxe-L",
                        Replace = "/",
                        Type = SwapType.LargeIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Innovator_Female/Meshes/Innovator_Female.Innovator_Female",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/Innovator_Female/PickaxeSwing_Innovator_Female_Cue.PickaxeSwing_Innovator_Female_Cue",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/Innovator_Female/PickaxeReady_Innovator_Female_Cue.PickaxeReady_Innovator_Female_Cue",
                        Replace = Swaps["EquipCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/Innovator_Female/PickaxeImpactEnemy_Innovator_Female_Cue.PickaxeImpactEnemy_Innovator_Female_Cue",
                        Replace = Swaps["ImpactCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_Innovator_Female/FX/NS_Innovator_Female_AnimTrail.NS_Innovator_Female_AnimTrail",
                        Replace = Swaps["Trail"],
                        Type = SwapType.WeaponTrail
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Weapons/FORT_Melee/Pickaxe_Lone_Wolf_Male/FX/NS_Pickaxe_LoneWolf_Idle.NS_Pickaxe_LoneWolf_Idle",
                        Replace = Swaps["FX"],
                        Type = SwapType.WeaponFx
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Weapons/FORT_Melee/Pickaxe_Skirmish_Female/FX/NS_Pickaxe_Metal_Impact.NS_Pickaxe_Metal_Impact",
                        Replace = Swaps["ImpactFX"],
                        Type = SwapType.WeaponFx
                    }
                }
            }
        };
}
using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Pickaxes;

internal sealed class SnowySwap : PickaxeSwap
{
    public SnowySwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon, swaps, rarityEnum)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Weapons/WID_Harvest_Pickaxe_SweaterWeatherMale",
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
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-SweaterWeatherPickaxe.T-Icon-Pickaxes-SweaterWeatherPickaxe",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.SmallIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-SweaterWeatherPickaxe-L.T-Icon-Pickaxes-SweaterWeatherPickaxe-L",
                        Replace = "/",
                        Type = SwapType.LargeIcon
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_SweaterWeatherMale/Meshes/Sweater_Weather_Male_Axe.Sweater_Weather_Male_Axe",
                        Replace = Swaps["Mesh"],
                        Type = SwapType.WeaponMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/SweaterWeather/PickAxe_SweaterWeatherMale_Swing_Athena_Cue.PickAxe_SweaterWeatherMale_Swing_Athena_Cue",
                        Replace = Swaps["SwingCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/SweaterWeather/PickAxe_SweaterWeatherMale_Ready_Athena_Cue.PickAxe_SweaterWeatherMale_Ready_Athena_Cue",
                        Replace = Swaps["EquipCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Sounds/Weapons/PickAxes/SweaterWeather/PickAxe_SweaterWeatherMale_Impact_Player_Athena_Cue.PickAxe_SweaterWeatherMale_Impact_Player_Athena_Cue",
                        Replace = Swaps["ImpactCue"],
                        Type = SwapType.WeaponSound
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_SweaterWeatherMale/FX/P_Pickaxe_SweaterWeatherMale_Idle.P_Pickaxe_SweaterWeatherMale_Idle",
                        Replace = Swaps["FX"],
                        Type = SwapType.WeaponFx
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Weapons/FORT_Melee/Pickaxe_SweaterWeatherMale/FX/P_Pickaxe_SweaterWeatherMale_Swing.P_Pickaxe_SweaterWeatherMale_Swing",
                        Replace = Swaps["SwingFX"],
                        Type = SwapType.WeaponFx
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Weapons/FORT_Melee/Pickaxe_SweaterWeatherMale/FX/P_Pickaxe_SweaterWeather_Impact.P_Pickaxe_SweaterWeather_Impact",
                        Replace = Swaps["ImpactFX"],
                        Type = SwapType.WeaponFx
                    }
                }
            }
        };
}
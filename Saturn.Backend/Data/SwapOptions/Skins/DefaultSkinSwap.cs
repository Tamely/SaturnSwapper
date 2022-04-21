using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Skins;

internal sealed class DefaultSkinSwap : SkinSwap
{
    private readonly List<string> _cps;
    private readonly string _backblingCp;

    public DefaultSkinSwap(string name, string rarity, string icon, List<string> cps, string backblingCp)
        : base(name, rarity, icon, new MeshDefaultModel())
    {
        _cps = cps;
        _backblingCp = backblingCp;
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Balance/DefaultGameDataCosmetics",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Wslt/Says/We/Didn't/Make/UAssets/But/We/Made/This/One/Which/He/Will/Probably/Steal/Like/He/Always/Does/Cough/Flare/Cough/Also/Solar/Swapper/Is/Literally/Galaxy/But/Wslt/Bans/Me/And/Not/Him??/Ok/I/See/Tamely1.Tamely1",
                        Replace = _cps.Count > 0 ? _cps[0] : "/",
                        Type = SwapType.BodyCharacterPart
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Wslt/Says/We/Didn't/Make/UAssets/But/We/Made/This/One/Which/He/Will/Probably/Steal/Like/He/Always/Does/Cough/Flare/Cough/Also/Solar/Swapper/Is/Literally/Galaxy/But/Wslt/Bans/Me/And/Not/Him??/Ok/I/See/Tamely2.Tamely2",
                        Replace = _cps.Count > 1 ? _cps[1] : "/",
                        Type = SwapType.HeadCharacterPart
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Wslt/Says/We/Didn't/Make/UAssets/But/We/Made/This/One/Which/He/Will/Probably/Steal/Like/He/Always/Does/Cough/Flare/Cough/Also/Solar/Swapper/Is/Literally/Galaxy/But/Wslt/Bans/Me/And/Not/Him??/Ok/I/See/Tamely3.Tamely3",
                        Replace = _cps.Count > 2 ? _cps[2] : "/",
                        Type = SwapType.HatCharacterPart
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Wslt/Says/We/Didn't/Make/UAssets/But/We/Made/This/One/Which/He/Will/Probably/Steal/Like/He/Always/Does/Cough/Flare/Cough/Also/Solar/Swapper/Is/Literally/Galaxy/But/Wslt/Bans/Me/And/Not/Him??/Ok/I/See/Tamely4.Tamely4",
                        Replace = _cps.Count > 3 ? _cps[3] : "/",
                        Type = SwapType.FaceAccessoryCharacterPart
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Wslt/Says/We/Didn't/Make/UAssets/But/We/Made/This/One/Which/He/Will/Probably/Steal/Like/He/Always/Does/Cough/Flare/Cough/Also/Solar/Swapper/Is/Literally/Galaxy/But/Wslt/Bans/Me/And/Not/Him??/Ok/I/See/Tamely5.Tamely5",
                        Replace = _backblingCp,
                        Type = SwapType.BackblingCharacterPart
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_M_Prime",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = "CP_Athena_Body_M_Prime",
                        Replace = "CP_Athena_Body_M_Pr1me",
                        Type = SwapType.BodyCharacterPart
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_M_Prime_G",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = "CP_Athena_Body_M_Prime_G",
                        Replace = "CP_Athena_Body_M_Pr1me_G",
                        Type = SwapType.BodyCharacterPart
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_Prime_A",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = "CP_Athena_Body_F_Prime_A",
                        Replace = "CP_Athena_Body_F_Pr1me_A",
                        Type = SwapType.BodyCharacterPart
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_Prime_B",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = "CP_Athena_Body_F_Prime_B",
                        Replace = "CP_Athena_Body_F_Pr1me_B",
                        Type = SwapType.BodyCharacterPart
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_Prime_C",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = "CP_Athena_Body_F_Prime_C",
                        Replace = "CP_Athena_Body_F_Pr1me_C",
                        Type = SwapType.BodyCharacterPart
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_Prime",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = "CP_Athena_Body_F_Prime",
                        Replace = "CP_Athena_Body_F_Pr1me",
                        Type = SwapType.BodyCharacterPart
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_Prime_E",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = "CP_Athena_Body_F_Prime_E",
                        Replace = "CP_Athena_Body_F_Pr1me_E",
                        Type = SwapType.BodyCharacterPart
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_Prime_G",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = "CP_Athena_Body_F_Prime_G",
                        Replace = "CP_Athena_Body_F_Pr1me_G",
                        Type = SwapType.BodyCharacterPart
                    }
                }
            }
        };
}
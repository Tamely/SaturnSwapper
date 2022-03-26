using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Skins;

internal sealed class DefaultSkinSwap : SkinSwap
{
    private readonly Dictionary<string, string> _cps;
    private readonly string _headOrHat;

    public DefaultSkinSwap(string name, string rarity, string icon, Dictionary<string, string> cps, string headOrHat) 
        : base(name, rarity, icon, new MeshDefaultModel())
    {
        _cps = cps;
        _headOrHat = headOrHat;
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
                            "/Game/Wslt/Says/We/Didn't/Make/UAssets/But/We/Made/This/One/Which/He/Will/Probably/Steal/Like/He/Always/Does/Cough/Flare/Cough/Also/Solar/Swapper/Is/Literally/Galaxy/But/Wslt/Bans/Me/And/Not/Him??/Ok/I/See/Owen1.Owen1",
                        Replace = _cps["Body"],
                        Type = SwapType.BodyCharacterPart
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Wslt/Says/We/Didn't/Make/UAssets/But/We/Made/This/One/Which/He/Will/Probably/Steal/Like/He/Always/Does/Cough/Flare/Cough/Also/Solar/Swapper/Is/Literally/Galaxy/But/Wslt/Bans/Me/And/Not/Him??/Ok/I/See/Owen2.Owen2",
                        Replace = _cps["Head"],
                        Type = SwapType.HeadCharacterPart
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Wslt/Says/We/Didn't/Make/UAssets/But/We/Made/This/One/Which/He/Will/Probably/Steal/Like/He/Always/Does/Cough/Flare/Cough/Also/Solar/Swapper/Is/Literally/Galaxy/But/Wslt/Bans/Me/And/Not/Him??/Ok/I/See/Owen3.Owen3",
                        Replace = _cps["Face"],
                        Type = SwapType.HatCharacterPart
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Wslt/Says/We/Didn't/Make/UAssets/But/We/Made/This/One/Which/He/Will/Probably/Steal/Like/He/Always/Does/Cough/Flare/Cough/Also/Solar/Swapper/Is/Literally/Galaxy/But/Wslt/Bans/Me/And/Not/Him??/Ok/I/See/Owen4.Owen4",
                        Replace = "/",
                        Type = SwapType.OtherCharacterPart
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Wslt/Says/We/Didn't/Make/UAssets/But/We/Made/This/One/Which/He/Will/Probably/Steal/Like/He/Always/Does/Cough/Flare/Cough/Also/Solar/Swapper/Is/Literally/Galaxy/But/Wslt/Bans/Me/And/Not/Him??/Ok/I/See/Owen5.Owen5",
                        Replace = "/",
                        Type = SwapType.OtherCharacterPart
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
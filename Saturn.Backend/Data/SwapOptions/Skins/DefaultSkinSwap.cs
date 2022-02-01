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
                            "/Game/Athena/Heroes/Mesh/Wslt/Will/Skid/This/From/Tamely/Because/He/Always/Does/BodyCharacterPartWithExtraLongLength.BodyCharacterPartWithExtraLongLengthTamelyTamelyTamelyTamelyTamelyTamelyTamelyTamelyTamelyTamelyTamelyTamelyTamelyW",
                        Replace = _cps["Body"],
                        Type = SwapType.BodyCharacterPart
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/CharacterParts/Hopefully/Wslt/Doesnt/Skid/This/From/Me/Like/He/Usually/Does/Because/That/Would/Just/Prove/So/Much/Like/When/He/Said/That/I/Dont/Own/Uassets.ICreateMyOwn.ICreateMyOwnThoughL",
                        Replace = _cps[_headOrHat],
                        Type = SwapType.HeadCharacterPart
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
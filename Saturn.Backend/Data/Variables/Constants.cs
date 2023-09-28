using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.AssetRegistry.Objects;
using CUE4Parse.UE4.Assets.Exports.Texture;
using Saturn.Backend.Data.Plugins;
using Saturn.Backend.Data.SaturnAPI.Models;
using Saturn.Backend.Data.SaturnConfig.Models;
using Saturn.Backend.Data.Swapper.Assets;
using Saturn.Backend.Data.Swapper.Generation;
using Saturn.Backend.Data.Swapper.Swapping.Models;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;

namespace Saturn.Backend.Data.Variables
{
    public class Constants
    {
        public const string USER_VERSION = "2.2.3";

        public static readonly Changelog Changelog = new Changelog()
        {
            Description = "Quality-of-life update",
            Sections = new ChangelogSection[]
            {
                new ChangelogSection()
                {
                    Title = "UI Update",
                    Changes = new ChangelogChange[]
                    {
                        new ChangelogChange()
                        {
                            Summary = "Updated the UI",
                            Description = "Increased functionality by creating an easier to use UI with most steps automated."
                        },
                        new ChangelogChange()
                        {
                            Summary = "Added a changelog",
                            Description = "Added a changelog to the UI to keep users up to date with the latest changes."
                        },
                        new ChangelogChange()
                        {
                            Summary = "Removed backups",
                            Description = "The swapper no longer needs to back up files to swap."
                        },
                        new ChangelogChange()
                        {
                            Summary = "Added asset importer",
                            Description = "You can now import custom files into the game."
                        },
                        new ChangelogChange()
                        {
                            Summary = "Added presets",
                            Description = "You can now swap everything you want at once."
                        },
                        new ChangelogChange()
                        {
                            Summary = "Added plugins",
                            Description = "You can now swap non-traditional items."
                        },
                        new ChangelogChange()
                        {
                            Summary = "Added plugin marketplace",
                            Description = "You can now download plugins straight from the swapper."
                        }
                    }
                },
                new ChangelogSection()
                {
                    Title = "Logic update",
                    Changes = new []
                    {
                        new ChangelogChange()
                        {
                            Summary = "Fixed",
                            Description = "The swapper has been fixed for the new season."
                        },
                        new ChangelogChange()
                        {
                            Summary = "Error Mitigation",
                            Description = "The swapper throws much less errors now."
                        },
                        new ChangelogChange()
                        {
                            Summary = "Added icons to skins",
                            Description = "Skins now swap icons in game (Saturn+ only)."
                        },
                        new ChangelogChange()
                        {
                            Summary = "Added lobby swaps",
                            Description = "Everyone in your lobby can now see your swap (Saturn+ only)."
                        },
                        new ChangelogChange()
                        {
                            Summary = "Added UEFN swaps",
                            Description = "Everyone in your UEFN experience can now see your swap (Saturn+ only)."
                        }
                    }
                }
            }
        };
        
        public static readonly string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Saturn/";

        public static readonly string LogPath = BasePath + "Logs/";
        public static readonly string LogFile = LogPath + "Saturn.log";

        public static readonly string ExternalPath = BasePath + "Externals/";
        public static readonly string APICachePath = BasePath + "APICache/";
        public static readonly string PluginPath = BasePath + "Plugins/";
        
        public static readonly string DataPath = BasePath + "SwapData/";
        public static readonly string MappingsPath = BasePath + "Mappings/";

        public static readonly string OodlePath = ExternalPath + "oo2core_5_win64.dll";
        public static readonly string ConfigPath = BasePath + "Config.json";
        public static readonly string CosmeticsPath = BasePath + "cosmetics.json";

        public static bool isKeyValid = false;
        public static bool isPluginKeyValid = false;
        public static bool isPlus = false;
        public static bool isBeta = false;
        public static bool isClosingCorrectly = false;

        public static readonly Dictionary<string, CharacterPart> EmptyParts = new()
        {
            {
                "Head", new CharacterPart()
                    {
                        Path = "FortniteGame/Content/Characters/CharacterParts/Common/CP_Head_Med_Empty",
                        Enums = new Dictionary<string, string>()
                        {
                            {
                                "CharacterPartType", "Head"
                            }
                        }
                    }
            },
            {
                "Face", new CharacterPart()
                {
                    Path = "FortniteGame/Content/Characters/CharacterParts/FaceAccessories/CP_F_FaceAcc_Empty",
                    Enums = new Dictionary<string, string>()
                    {
                        {
                            "CharacterPartType", "Face"
                        }
                    }
                }
            },
            {
                "Hat", new CharacterPart()
                {
                    Path = "FortniteGame/Content/Characters/CharacterParts/Hats/Empty_None",
                    Enums = new Dictionary<string, string>()
                    {
                        {
                            "CharacterPartType", "Hat"
                        }
                    }
                }
            },
            {
                "MiscOrTail", new CharacterPart()
                {
                    Path = "FortniteGame/Content/Characters/CharacterParts/Common/Empty_Tail",
                    Enums = new Dictionary<string, string>()
                    {
                        {
                            "CharacterPartType", "MiscOrTail"
                        }
                    }
                }
            }
        };
        
        public const string MANIFEST_URL = "launcher/api/public/assets/v2/platform/Windows/namespace/fn/catalogItem/4fe75bbc5a674f4f9b356b5c90567da5/app/Fortnite/label/Live";
        public const string DEFAULTGAMEDATACOSMETICS_PATH = "FortniteGame/Content/Balance/DefaultGameDataCosmetics.uasset";

        // These aren't really constants but :shrug:
        public static string DiscordAvatar = "img/Saturn.png";
        public static string DiscordName = Environment.UserName;

        public static List<string> ConvertedIDs = new();
        public static bool CanSpecialSwap = false;
        public static bool ShouldLobbySwap = false;
        public static bool ShouldCreativeSwap = false;
        public static bool ShouldGlobalSwap = false;
        public static bool IsRemoving = false;
        public static Dictionary<long, byte[]> GlobalSwaps = new();

        public static int CosmeticCount = -1;
        public static int ChunkCount = 0;
        public static List<AssetSelectorItem> Cosmetics = new();

        public static AssetHandler Handler = new();
        public static List<FAssetData> AssetDataBuffers = new();
        public static RarityCollection[] RarityData = new RarityCollection[8];
        public static UTexture2D PlaceholderTexture;
        
        public static SaturnState State = SaturnState.S_Installer;
        public static SaturnState CosmeticState = SaturnState.S_Skin;
        
        public static DefaultFileProvider Provider;
        
        public const int CHUNK_SIZE = 250;
        public static int ChunkIndex = 0;

        public static List<PluginModel> OwnedPlugins = new();
        public static AssetSelectorItem SelectedItem = new();
        public static AssetSelectorItem SelectedOption = new();

        public static List<string> PotentialOptions = new();

        public static List<Swaps> CurrentSwaps = new();
        public static List<ItemModel> CurrentLobbySwaps = new();

        public static Dictionary<string, AssetExportData> AssetCache = new();
    }
}
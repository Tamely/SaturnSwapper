using System;
using System.Collections.Generic;
using CUE4Parse.FileProvider;
using Saturn.Backend.Data.SaturnAPI.Models;
using Saturn.Backend.Data.SaturnConfig.Models;
using Saturn.Backend.Data.Swapper.Swapping.Models;

namespace Saturn.Backend.Data.Variables
{
    public class Constants
    {
        public const string USER_VERSION = "2.2.0";

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
                            Summary = "Added icons to skins",
                            Description = "Skins now swap icons in game (Saturn+ only)."
                        },
                        new ChangelogChange()
                        {
                            Summary = "Added lobby swaps",
                            Description = "Everyone in your lobby can now see your swap (Saturn+ only)."
                        }
                        ,
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
        public static bool isPlus = false;
        public static bool isBeta = true;
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

        // These aren't really constants but :shrug:
        public static string DiscordAvatar = "img/Saturn.png";
        public static string DiscordName = Environment.UserName;
        public static string DiscordDiscriminator = "0000";

        public static List<string> ConvertedIDs = new();
        public static bool CanLobbySwap = false;
        public static bool ShouldLobbySwap = false;
        
        public static SaturnState State = SaturnState.S_Installer;
        public static SaturnState CosmeticState = SaturnState.S_Skin;
        
        public static DefaultFileProvider Provider;
        
        public const int CHUNK_SIZE = 500;
        public static int ChunkIndex = 0;
        
        public static List<List<DisplayItemModel>> ChunkedItems = new();
        public static DisplayItemModel? SelectedDisplayItem = null;
        public static SaturnItemModel? SelectedItem = null;
        public static SaturnItemModel? SelectedOption = null;

        public static List<string> PotentialOptions = new();

        public static List<Swaps> CurrentSwaps = new();
    }
}

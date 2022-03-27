using System;

namespace Saturn.Backend.Data.Utils
{
    public class Config
    {
        public static readonly string BasePath =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Saturn/";

        public static readonly string LogPath = BasePath + "Logs/";
        public static readonly string OodlePath = BasePath + "oo2core_5_win64.dll";
        public static readonly string ConfigPath = BasePath + "Config.dat";
        public static readonly string MappingsFolder = BasePath + "/Mappings/";
        public static readonly string CompressedDataPath = BasePath + "/CompressedData/";
        public static readonly string DecompressedDataPath = BasePath + "/DecompressedData/";
        public static readonly string PluginsPath = BasePath + "/Plugins/";

        public static readonly string LogFile = LogPath + "Saturn.log";

        public static readonly string CloudStoragePath = BasePath + "CloudStorage.ini";
        
        public static readonly string LobbyBackgroundPath = BasePath + "LobbyBackground.png";

        public static readonly string ApplicationPath = AppDomain.CurrentDomain.BaseDirectory;

        public static string MappingsURL = "";
        
        public static bool isBeta = false;
        
        public static bool isMaintenance = false;
    }
}
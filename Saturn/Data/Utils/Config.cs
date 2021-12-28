using Newtonsoft.Json;
using Saturn.Data.Enums;
using Saturn.Data.Models;
using Saturn.Data.Models.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Data.Utils
{
    public class Config
    {
        public static readonly string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Saturn/";

        public static readonly string LogPath = BasePath + "Logs/";
        public static readonly string OodlePath = BasePath + "oo2core_5_win64.dll";
        public static readonly string ConfigPath = BasePath + "Config.dat";
        public static readonly string CompressedDataPath = BasePath + "/CompressedData/";
        public static readonly string DecompressedDataPath = BasePath + "/DecompressedData/";

        public static readonly string LogFile = LogPath + "Saturn.log";
    }
}

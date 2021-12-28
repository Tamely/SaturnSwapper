using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Saturn.Backend.Data.Models.CloudStorage;
using Saturn.Backend.Data.Utils;
using Saturn.Backend.Data.Utils.CloudStorage;

namespace Saturn.Backend.Data.Services
{
    public interface ICloudStorageService
    {
        public string GetChanges(string parentAsset, string changeType);
        public Changes DecodeChanges(string changes);
    }

    public class CloudStorageService : ICloudStorageService
    {
        private readonly ISaturnAPIService _saturnAPIService;
        private readonly IniUtil CloudChanges = new(Config.CloudStoragePath);

        public CloudStorageService(ISaturnAPIService saturnAPIService)
        {
            _saturnAPIService = saturnAPIService;
            Trace.WriteLine("Getting CloudStorage");
            CloudStorage = _saturnAPIService.ReturnEndpoint("api/v1/Saturn/CloudStorage");
            Trace.WriteLine("Done");
            File.WriteAllText(Config.CloudStoragePath, CloudStorage);
        }

        private string CloudStorage { get; }

        public string GetChanges(string parentAsset, string changeType)
            => CloudChanges.Read(parentAsset, changeType);

        public Changes DecodeChanges(string changes)
        {
            var returnChanges = new Changes();
            Trace.WriteLine("Decoding " + changes);
            var data = FileUtil.Base64Decode(changes);
            var changedData = data.Split(":");
            Logger.Log("There are " + changedData.Length + " changes"); 
            returnChanges.SkinName = changedData[0];
            returnChanges.CustomAssetUrl = changedData[1];

            returnChanges.Searches = ConvertStringToList(changedData[2]);
            returnChanges.Replaces = ConvertStringToList(changedData[3]);
            returnChanges.CharacterParts = ConvertStringToList(changedData[4]);
            returnChanges.HatSkins = ConvertStringToList(changedData[5]);

            return returnChanges;
        }

        private List<string> ConvertStringToList(string data)
            => data.Split("->").ToList();

        private string ConvertListToString(IEnumerable<string> data)
            => GetBeforeLast(data.Aggregate("", (current, item) => current + (item + "->")), "->");
            
        // Get string before the last of a substring in a string
        private string GetBeforeLast(string str, string substr)
        {
            var index = str.LastIndexOf(substr);
            return index == -1 ? str : str.Substring(0, index);
        }

        public void SetChanges()
        {
            Logger.Log("CS: " + FileUtil.Base64Encode(
                $"none:none:none:none:none:{ConvertListToString(new[] { "CID_017_Athena_Commando_M", "CID_018_Athena_Commando_M", "CID_019_Athena_Commando_M", "CID_020_Athena_Commando_M", "CID_021_Athena_Commando_F", "CID_022_Athena_Commando_F", "CID_023_Athena_Commando_F", "CID_024_Athena_Commando_F", "CID_025_Athena_Commando_M", "CID_026_Athena_Commando_M", "CID_027_Athena_Commando_F", "CID_032_Athena_Commando_M_Medieval", "CID_033_Athena_Commando_F_Medieval", "CID_034_Athena_Commando_F_Medieval", "CID_035_Athena_Commando_M_Medieval", "CID_036_Athena_Commando_M_WinterCamo", "CID_037_Athena_Commando_F_WinterCamo", "CID_038_Athena_Commando_M_Disco", "CID_042_Athena_Commando_M_Cyberpunk", "CID_046_Athena_Commando_F_HolidaySweater", "CID_047_Athena_Commando_F_HolidayReindeer", "CID_048_Athena_Commando_F_HolidayGingerbread", "CID_050_Athena_Commando_M_HolidayNutcracker", "CID_051_Athena_Commando_M_HolidayElf", "CID_052_Athena_Commando_F_PSBlue", "CID_053_Athena_Commando_M_SkiDude", "CID_054_Athena_Commando_M_SkiDude_USA", "CID_055_Athena_Commando_M_SkiDude_CAN", "CID_056_Athena_Commando_M_SkiDude_GBR", "CID_057_Athena_Commando_M_SkiDude_FRA", "CID_058_Athena_Commando_M_SkiDude_GER", "CID_059_Athena_Commando_M_SkiDude_CHN", "CID_060_Athena_Commando_M_SkiDude_KOR", "CID_071_Athena_Commando_M_Wukong", "CID_072_Athena_Commando_M_Scout", "CID_076_Athena_Commando_F_Sup", "CID_077_Athena_Commando_M_Sup", "CID_082_Athena_Commando_M_Scavenger", "CID_085_Athena_Commando_M_Twitch", "CID_090_Athena_Commando_M_Tactical", "CID_093_Athena_Commando_M_Dinosaur", "CID_095_Athena_Commando_M_Founder", "CID_096_Athena_Commando_F_Founder", "CID_097_Athena_Commando_F_RockerPunk", "CID_098_Athena_Commando_F_StPatty", "CID_113_Athena_Commando_M_BlueAce", "CID_114_Athena_Commando_F_TacticalWoodland", "CID_119_Athena_Commando_F_Candy", "CID_120_Athena_Commando_F_Graffiti", "CID_121_Athena_Commando_M_Graffiti", "CID_122_Athena_Commando_M_Metal", "CID_124_Athena_Commando_F_AuroraGlow", "CID_126_Athena_Commando_M_AuroraGlow", "CID_131_Athena_Commando_M_Warpaint", "CID_134_Athena_Commando_M_Jailbird", "CID_135_Athena_Commando_F_Jailbird", "CID_142_Athena_Commando_M_WWIIPilot", "CID_156_Athena_Commando_F_FuzzyBearInd", "CID_166_Athena_Commando_F_Lifeguard", "CID_173_Athena_Commando_F_StarfishUniform", "CID_179_Athena_Commando_F_Scuba", "CID_187_Athena_Commando_F_FuzzyBearPanda", "CID_199_Athena_Commando_F_BlueSamurai", "CID_216_Athena_Commando_F_Medic", "CID_218_Athena_Commando_M_GreenBeret", "CID_250_Athena_Commando_M_EvilCowboy", "CID_258_Athena_Commando_F_FuzzyBearHalloween", "CID_294_Athena_Commando_F_RedKnightWinter", "CID_346_Athena_Commando_M_DragonNinja", "CID_382_Athena_Commando_M_BaseballKitbash", "CID_391_Athena_Commando_M_HoppityHeist", "CID_490_Athena_Commando_M_BlueBadass", "CID_497_Athena_Commando_F_WildWest", "CID_498_Athena_Commando_M_WildWest", "CID_536_Athena_Commando_F_DurrburgerWorker", "CID_574_Athena_Commando_F_CubeRockerPunk", "CID_653_Athena_Commando_F_UglySweaterFrozen", "CID_666_Athena_Commando_M_ArcticCamo", "CID_667_Athena_Commando_M_ArcticCamo_Dark", "CID_669_Athena_Commando_M_ArcticCamo_Slate", "CID_A_132_Athena_Commando_M_ScavengerFire" })}"));
        }
    }
}
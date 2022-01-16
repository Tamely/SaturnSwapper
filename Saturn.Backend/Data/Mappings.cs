using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider;
using Newtonsoft.Json.Linq;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Services;
using Saturn.Backend.Data.Utils;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Saturn.Backend.Data
{
    public class Mappings
    {
        private readonly IBenBotAPIService _benBotAPIService; 
        private readonly DefaultFileProvider provider; 

        public Mappings(DefaultFileProvider _provider)
        {
            _benBotAPIService = new BenBotAPIService();
            provider = _provider;
        }

        public async Task<bool> Init()
        {
            try
            {
                string json = await _benBotAPIService.ReturnEndpointAsync("mappings");
                Logger.Log("Grabbed mappings, preparing to parse.");
                JArray parsed = JArray.Parse(json);

                foreach (var token in parsed)
                {
                    if ((string)token["meta"]["compressionMethod"] != "Oodle") continue;
                    Logger.Log("Downloading mappings...");
                    File.WriteAllBytesAsync(Config.BasePath + token["fileName"], await _benBotAPIService.ReturnBytesAsync((string)token["url"]));
                    provider.MappingsContainer= new FileUsmapTypeMappingsProvider(Config.BasePath + token["fileName"]);
                }
                Logger.Log("Loaded mappings!");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Unable to parse/load mappings, please contact support! " + ex, LogLevel.Fatal);
                return false;
            }
        }
    }
}

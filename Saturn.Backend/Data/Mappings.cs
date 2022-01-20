using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider;
using Newtonsoft.Json.Linq;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Services;
using Saturn.Backend.Data.Utils;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Saturn.Backend.Data
{
    public class Mappings
    {
        private readonly IBenBotAPIService _benBotAPIService; 
        private readonly DefaultFileProvider provider; 

        public Mappings(DefaultFileProvider _provider, IBenBotAPIService _benbotAPIService)
        {
            _benBotAPIService = _benbotAPIService;
            provider = _provider;
        }

        public async Task Init()
        {
            try
            {
                string json = await _benBotAPIService.ReturnEndpointAsync("mappings");
                Logger.Log("Grabbed mappings, preparing to parse.");
                JArray parsed = JArray.Parse(json);

                if (parsed.Count == 0)
                {
                    Logger.Log("No mappings found, BenBot is probably down. Trying to load old mappings...");

                    Directory.CreateDirectory(Config.MappingsFolder);
                    string newestFile = Directory.GetFiles(Config.MappingsFolder).OrderByDescending(f => new FileInfo(f).LastWriteTime).First();
                    provider.MappingsContainer = new FileUsmapTypeMappingsProvider(newestFile);
                }
                else
                {
                    foreach (var token in parsed)
                    {
                        if (token["meta"]["compressionMethod"].ToString() != "Oodle") continue;
                    
                        Logger.Log("Downloading mappings...");
                    
                        Directory.CreateDirectory(Config.MappingsFolder);
                        if (!File.Exists(Config.MappingsFolder + token["fileName"]))
                            await File.WriteAllBytesAsync(Config.MappingsFolder + token["fileName"],
                                await _benBotAPIService.ReturnBytesAsync(token["url"].ToString()));

                        provider.MappingsContainer =
                            new FileUsmapTypeMappingsProvider(Config.MappingsFolder + token["fileName"]);
                    
                        Logger.Log("Mappings downloaded. Cleaning up the folder...");

                        foreach (var file in new DirectoryInfo(Config.MappingsFolder).GetFiles()
                                     .OrderByDescending(x => x.LastWriteTime).Skip(5))
                            file.Delete();
                    }
                    Logger.Log("Loaded mappings!");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Unable to parse/load mappings, please contact support! " + ex, LogLevel.Fatal);
            }
        }
    }
}

using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Saturn.Backend.Data.Utils
{
    public class Mappings
    {
        private readonly IBenBotAPIService _benBotAPIService; 
        private readonly DefaultFileProvider _provider; 
        private readonly IFortniteAPIService _fortniteAPIService;
        private readonly IJSRuntime _jsRuntime;

        public Mappings(DefaultFileProvider provider, IBenBotAPIService benbotAPIService, IFortniteAPIService fortniteApiService, IJSRuntime jsRuntime)
        {
            _benBotAPIService = benbotAPIService;
            _provider = provider;
            _fortniteAPIService = fortniteApiService;
            _jsRuntime = jsRuntime;
        }

        public async Task Init()
        {
            try
            {
                string json = await _benBotAPIService.ReturnEndpointAsync("mappings?version=" + _fortniteAPIService.GetAES().Build);
                Logger.Log("Grabbed mappings, preparing to parse.");
                JArray tokens = JArray.Parse(json);

                if (tokens.Count == 0)
                {
                    Logger.Log("No mappings found, BenBot is probably down. Trying to load old mappings...");

                    Directory.CreateDirectory(Config.MappingsFolder);
                    string newestFile = Directory.GetFiles(Config.MappingsFolder).OrderByDescending(f => new FileInfo(f).LastWriteTime).First();
                    _provider.MappingsContainer = new FileUsmapTypeMappingsProvider(newestFile);
                }
                else
                {
                    foreach (var token in tokens)
                    {
                        if (token["meta"]["compressionMethod"].ToString() != "Oodle") continue;
                    
                        Logger.Log("Downloading mappings...");
                    
                        Directory.CreateDirectory(Config.MappingsFolder);
                        if (!File.Exists(Config.MappingsFolder + token["fileName"]))
                            await File.WriteAllBytesAsync(Config.MappingsFolder + token["fileName"],
                                                          await _benBotAPIService.ReturnBytesAsync(token["url"].ToString()));

                        _provider.MappingsContainer =
                            new FileUsmapTypeMappingsProvider(Config.MappingsFolder + token["fileName"]);
                    
                        Logger.Log("Mappings downloaded. Cleaning up the folder...");

                        foreach (var file in new DirectoryInfo(Config.MappingsFolder)
                                 .GetFiles().OrderByDescending(x => x.LastWriteTime).Skip(5))
                            file.Delete();
                    }
                    Logger.Log("Loaded mappings!");
                }
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("MessageBox", "There was an error while loading mappings. Please try again or contact support in Tamely's Discord.", "error");
                Logger.Log("Unable to parse/load mappings, please contact support! " + ex, LogLevel.Fatal);
            }
        }
    }
}

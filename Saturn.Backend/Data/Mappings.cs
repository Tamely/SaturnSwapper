using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Services;
using Saturn.Backend.Data.Utils;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Saturn.Backend.Data
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
                if (await _benBotAPIService.IsBenAlive())
                {
                    string json = await _benBotAPIService.ReturnEndpointAsync("mappings?version=" + _fortniteAPIService.GetAES().Build);
                    Logger.Log("Grabbed mappings, preparing to parse.");
                    JArray parsed = JArray.Parse(json);

                    if (parsed.Count == 0)
                    {
                        Logger.Log("No mappings found, BenBot is probably down. Trying to load old mappings...");

                        Directory.CreateDirectory(Config.MappingsFolder);
                        string newestFile = Directory.GetFiles(Config.MappingsFolder).OrderByDescending(f => new FileInfo(f).LastWriteTime).First();
                        _provider.MappingsContainer = new FileUsmapTypeMappingsProvider(newestFile);
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

                            _provider.MappingsContainer =
                                new FileUsmapTypeMappingsProvider(Config.MappingsFolder + token["fileName"]);
                    
                            Logger.Log("Mappings downloaded. Cleaning up the folder...");

                            foreach (var file in new DirectoryInfo(Config.MappingsFolder).GetFiles()
                                         .OrderByDescending(x => x.LastWriteTime).Skip(5))
                                file.Delete();
                        }
                        Logger.Log("Loaded mappings!");
                    }
                }
                else
                {
                    Logger.Log("BenBot is not alive, trying to load old mappings...");
                    if (Directory.Exists(Config.MappingsFolder))
                    {
                        _provider.MappingsContainer =
                            new FileUsmapTypeMappingsProvider(Config.MappingsFolder + _fortniteAPIService.GetAES().Build + "_oo.usmap");
                    
                        Logger.Log("Mappings Loaded. Cleaning up the folder...");

                        foreach (var file in new DirectoryInfo(Config.MappingsFolder).GetFiles()
                                     .OrderByDescending(x => x.LastWriteTime).Skip(5))
                            file.Delete();
                    }
                    else
                    {
                        await _jsRuntime.InvokeVoidAsync("MessageBox", "There was an error with BenBot mappings!",
                            "Unable to connect to BenBot and you don't have a mappings folder. Please contact support in Tamely's Discord to get this fixed right away, or wait until BenBot is back!");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("There was an error loading mappings. We will try to load from old files.");
                if (Directory.Exists(Config.MappingsFolder))
                {
                    _provider.MappingsContainer =
                        new FileUsmapTypeMappingsProvider(Config.MappingsFolder + _fortniteAPIService.GetAES().Build + "_oo.usmap");
                    
                    Logger.Log("Mappings Loaded. Cleaning up the folder...");

                    foreach (var file in new DirectoryInfo(Config.MappingsFolder).GetFiles()
                                 .OrderByDescending(x => x.LastWriteTime).Skip(5))
                        file.Delete();
                }
                else
                {
                    await _jsRuntime.InvokeVoidAsync("MessageBox", "Please restart the swapper", "There was an error while loading mappings. Please try again or contact support in Tamely's Discord.");
                    Logger.Log("Unable to parse/load mappings, please contact support! " + ex, LogLevel.Fatal);
                }
            }
        }
    }
}

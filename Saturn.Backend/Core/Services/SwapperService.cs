#pragma warning disable CA1416, SYSLIB0014 // Disable the warning that says something is deprecated and obsolete

using CUE4Parse;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.Utils;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.CloudStorage;
using Saturn.Backend.Core.Models.FortniteAPI;
using Saturn.Backend.Core.Models.Items;
using Saturn.Backend.Core.Models.SaturnAPI;
using Saturn.Backend.Core.SwapOptions.Backblings;
using Saturn.Backend.Core.SwapOptions.Emotes;
using Saturn.Backend.Core.SwapOptions.Pickaxes;
using Saturn.Backend.Core.SwapOptions.Skins;
using Saturn.Backend.Core.Utils;
using Saturn.Backend.Core.Utils.Compression;
using Saturn.Backend.Core.Utils.FortniteUtils;
using Saturn.Backend.Core.Utils.Swaps;
using Saturn.Backend.Core.Utils.Swaps.Generation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Saturn.Backend.Core.SwapOptions.Gliders;
using Saturn.Backend.Core.Utils.Textures;
using Colors = Saturn.Backend.Core.Enums.Colors;
using Index = Saturn.Backend.Pages.Index;

namespace Saturn.Backend.Core.Services;

public interface ISwapperService
{
    public Task<bool> Convert(Cosmetic item, SaturnItem option, ItemType itemType, bool isAuto = true, bool isRandom = false, Cosmetic random = null);
    public Task<bool> Revert(Cosmetic item, SaturnItem option, ItemType itemType);
    public Task<Dictionary<string, string>> GetCharacterPartsById(string id, Cosmetic? item = null);
    public Task<UObject> GetWIDByID(string id);
    public Task<UObject> GetBackblingCP(string id);
    public Task SwapLobby(Cosmetic item, Cosmetic option);
    public Task<List<Cosmetic>> GetSaturnSkins(bool isLobby = false, bool isOption = false);
    public Task<List<SaturnItem>> GetSkinOptions(Cosmetic item);
    public Task<List<SaturnItem>> GetPickaxeOptions(Cosmetic item);
    public Task<List<SaturnItem>> GetBackblingOptions(Cosmetic item);
    public Task<List<SaturnItem>> GetEmoteOptions(Cosmetic item);
    public Task<List<SaturnItem>> GetGliderOptions(Cosmetic item);
    public Task<List<Cosmetic>> GetSaturnPickaxes(bool isLobby = false, bool isOption = false);
    public Task<List<Cosmetic>> GetSaturnBackblings(bool isLobby = false, bool isOption = false);
    public Task<List<Cosmetic>> GetSaturnGliders();
    public Task<List<Cosmetic>> GetSaturnEmotes(bool isLobby = false, bool isOption = false);
    public Task Swap(Cosmetic item, SaturnItem option, ItemType itemType, List<Cosmetic> Items, bool isAuto = true);
    public DefaultFileProvider Provider { get; }
}

public sealed class SwapperService : ISwapperService
{
    private readonly INotificationService _notificationService;
    private readonly IConfigService _configService;
    private readonly IFortniteAPIService _fortniteAPIService;
    private readonly IDiscordRPCService _discordRPCService;

    private readonly ISaturnAPIService _saturnAPIService;
    private readonly ICloudStorageService _cloudStorageService;

    private readonly IJSRuntime _jsRuntime;

    private bool _halted;
    private readonly DefaultFileProvider _provider;

    public SwapperService(IFortniteAPIService fortniteAPIService, ISaturnAPIService saturnAPIService,
        IConfigService configService, ICloudStorageService cloudStorageService, IJSRuntime jsRuntime,
        IBenBotAPIService benBotApiService, IDiscordRPCService discordRPCService, INotificationService notificationService)
    {
        _notificationService = notificationService;
        _fortniteAPIService = fortniteAPIService;
        _saturnAPIService = saturnAPIService;
        _configService = configService;
        _cloudStorageService = cloudStorageService;
        _jsRuntime = jsRuntime;
        _discordRPCService = discordRPCService;

        var aes = _fortniteAPIService.GetAES();

        Trace.WriteLine("Got AES");

        _provider = new DefaultFileProvider(FortniteUtil.PakPath, SearchOption.TopDirectoryOnly, false, new CUE4Parse.UE4.Versions.VersionContainer(CUE4Parse.UE4.Versions.EGame.GAME_UE5_1));

        _provider.Initialize();

        Trace.WriteLine("Initialized provider");
        
        Config.MappingsURL = JsonConvert.DeserializeObject<IndexModel>(_saturnAPIService.ReturnEndpoint("/")).MappingsLink; // Get the mappings link

        CreateMappings(benBotApiService, fortniteAPIService, jsRuntime);

        Trace.WriteLine("Loaded mappings");

        var keys = new List<KeyValuePair<FGuid, FAesKey>>();
        if (aes.MainKey != null)
            keys.Add(new(new FGuid(), new FAesKey(aes.MainKey)));
        else
            keys.Add(new(new FGuid(), new FAesKey("0000000000000000000000000000000000000000000000000000000000000000")));
        keys.AddRange(from x in aes.DynamicKeys
                      select new KeyValuePair<FGuid, FAesKey>(new FGuid(x.PakGuid), new FAesKey(x.Key)));

        Trace.WriteLine("Set Keys");
        _provider.SubmitKeys(keys);
        Trace.WriteLine("Submitted Keys");
        Trace.WriteLine($"File provider initialized with {_provider.Keys.Count} keys");
        
        foreach (var file in _provider.MountedVfs)
            Logger.Log($"Mounted file: {file.Name}");
    }

    public DefaultFileProvider Provider { get => _provider; }
    private async void CreateMappings(IBenBotAPIService benBotApiService, 
                                      IFortniteAPIService fortniteAPIService, 
                                      IJSRuntime jsRuntime) =>
        await new Mappings(_provider, benBotApiService, fortniteAPIService, jsRuntime).Init();

    public async Task<List<Cosmetic>> GetSaturnSkins(bool isLobby = false, bool isOption = false)
    {
        var skins = new List<Cosmetic>();

        AbstractGeneration Generation = new SkinGeneration(skins, _provider, _configService, this);

        skins = await Generation.Generate();

        Trace.WriteLine($"Deserialized {skins.Count} objects");

        if (isLobby)
        {
            ConvertedItem? convItem = (await _configService.TryGetConvertedItems()).FirstOrDefault(x => x.ItemDefinition.Contains("LOBBY") && x.ItemDefinition.Contains("CID_")) ?? null;
            for (int i = skins.Count - 1; i >= 0; i--)
            {
                if (convItem != null)
                { 
                    skins[i].VariantChannel = convItem.ItemDefinition;
                    skins[i].IsConverted = true;
                    skins[i].PrintColor = Colors.C_GREEN;
                }

                if (!skins[i].Description.Contains("style:")) continue;
                skins.RemoveAt(i);
                i++;
            }
        }
        else
        {
            await _fortniteAPIService.RemoveItems(skins);

            await _fortniteAPIService.AddExtraItems(skins, ItemType.IT_Skin);
            
            for (int i = skins.Count - 1; i >= 0; i--)
            {
                if (skins[i].Description.Contains("style:") && !await _configService.TryGetShouldShowStyles())
                {
                    skins.RemoveAt(i);
                    i++;
                    continue;
                }
            
                if ((await _configService.TryGetConvertedItems()).Any(x => string.Equals(x.Name, skins[i].Name) && string.Equals(x.ItemDefinition, skins[i].Id)))
                    skins[i].IsConverted = true;
            }
        }
        
        if (isOption)
            skins.RemoveAll(x => x.Id.Length > Index.CurrentSkin.Id.Length);

        _discordRPCService.UpdatePresence($"Looking at {skins.Count} different skins");

        if (!FileUtil.CheckIfCppIsInstalled())
        {
            await _notificationService.Error("There was an error decompressing packages with CUE4Parse. Please follow the tutorial that is opening on your browser to fix this, or paste this link in your browser: https://youtu.be/PeETf6ZQnBk");
            await Task.Delay(2000);
            await FileUtil.OpenBrowser("https://youtu.be/PeETf6ZQnBk");
        }
        
        if (skins.Count == 0)
            await _notificationService.Error("There was a mappings error, to fix this. Go to %localappdata%/Saturn/ and delete the folder 'Mappings' then relaunch the swapper.");
        
        return skins;
    }

    public async Task<List<SaturnItem>> GetSkinOptions(Cosmetic item)
    {
        return (await new AddSkins().AddSkinOptions(item, this, _provider)).CosmeticOptions;
    }
    
    public async Task<List<SaturnItem>> GetBackblingOptions(Cosmetic item)
    {
        return (await new AddBackblings().AddBackblingOptions(item, this, _provider, _jsRuntime)).CosmeticOptions;
    }

    public async Task<List<SaturnItem>> GetGliderOptions(Cosmetic item)
    {
        return (await new AddGliders().AddGliderOptions(item, this, _provider)).CosmeticOptions;
    }
    
    public async Task<List<SaturnItem>> GetPickaxeOptions(Cosmetic item)
    {
        return (await new AddPickaxes().AddPickaxeOptions(item, this, _provider)).CosmeticOptions;
    }
    
    public async Task<List<SaturnItem>> GetEmoteOptions(Cosmetic item)
    {
        return (await new AddEmotes().AddEmoteOptions(item, this, _provider)).CosmeticOptions;
    }
    
    public async Task<List<Cosmetic>> GetSaturnBackblings(bool isLobby = false, bool isOption = false)
    {
        var backblings = new List<Cosmetic>();

        AbstractGeneration Generation = new BackblingGeneration(backblings, _provider, _configService, this, _jsRuntime);

        backblings = await Generation.Generate();

        Trace.WriteLine($"Deserialized {backblings.Count} objects");

        if (isLobby)
        {
            ConvertedItem? convItem = (await _configService.TryGetConvertedItems()).FirstOrDefault(x => x.ItemDefinition.Contains("LOBBY") && x.ItemDefinition.Contains("BID_")) ?? null;
            for (int i = backblings.Count - 1; i >= 0; i--)
            {
                if (convItem != null)
                { 
                    backblings[i].VariantChannel = convItem.ItemDefinition;
                    backblings[i].IsConverted = true;
                    backblings[i].PrintColor = Colors.C_GREEN;
                }

                if (!backblings[i].Description.Contains("style:")) continue;
                backblings.RemoveAt(i);
                i++;
            }
        }
        else
        {
            await _fortniteAPIService.RemoveItems(backblings);

            await _fortniteAPIService.AddExtraItems(backblings, ItemType.IT_Backbling);
            
            for (int i = backblings.Count - 1; i >= 0; i--)
            {
                if (backblings[i].Description.Contains("style:") && !await _configService.TryGetShouldShowStyles())
                {
                    backblings.RemoveAt(i);
                    i++;
                    continue;
                }
                
                if ((await _configService.TryGetConvertedItems()).Any(x => string.Equals(x.Name, backblings[i].Name) && string.Equals(x.ItemDefinition, backblings[i].Id)))
                    backblings[i].IsConverted = true;
            }
        }
        
        if (isOption)
            backblings.RemoveAll(x => x.Id.Length > Index.CurrentSkin.Id.Length);

        _discordRPCService.UpdatePresence($"Looking at {backblings.Count} different backblings");

        if (!FileUtil.CheckIfCppIsInstalled())
        {
            await _notificationService.Error(
                "There was an error decompressing packages with CUE4Parse. Please follow the tutorial that is opening on your browser to fix this, or paste this link in your browser: https://youtu.be/PeETf6ZQnBk");
            await Task.Delay(2000);
            await FileUtil.OpenBrowser("https://youtu.be/PeETf6ZQnBk");
        }
        
        if (backblings.Count == 0)
            await _notificationService.Error("There was a mappings error. To fix this, go to %localappdata%/Saturn/ and delete the folder 'Mappings' then relaunch the swapper.");

        return backblings;
    }
    
    public async Task<List<Cosmetic>> GetSaturnGliders()
    {
        var gliders = new List<Cosmetic>();

        AbstractGeneration Generation = new GliderGeneration(gliders, _provider, _configService, this, _jsRuntime);

        gliders = await Generation.Generate();

        Trace.WriteLine($"Deserialized {gliders.Count} objects");
        
        await _fortniteAPIService.RemoveItems(gliders);

        await _fortniteAPIService.AddExtraItems(gliders, ItemType.IT_Glider);

        for (int i = gliders.Count - 1; i >= 0; i--)
        {
            if (gliders[i].Description.Contains("style:") && !await _configService.TryGetShouldShowStyles())
            {
                gliders.RemoveAt(i);
                i++;
                continue;
            }

            if ((await _configService.TryGetConvertedItems()).Any(x =>
                    string.Equals(x.Name, gliders[i].Name) && string.Equals(x.ItemDefinition, gliders[i].Id)))
                gliders[i].IsConverted = true;
        }

        _discordRPCService.UpdatePresence($"Looking at {gliders.Count} different gliders");

        if (!FileUtil.CheckIfCppIsInstalled())
        {
            await _notificationService.Error(
                "There was an error decompressing packages with CUE4Parse. Please follow the tutorial that is opening on your browser to fix this, or paste this link in your browser: https://youtu.be/PeETf6ZQnBk");
            await Task.Delay(2000);
            await FileUtil.OpenBrowser("https://youtu.be/PeETf6ZQnBk");
        }
        
        if (gliders.Count == 0)
            await _notificationService.Error("There was a mappings error. To fix this, go to %localappdata%/Saturn/ and delete the folder 'Mappings' then relaunch the swapper.");

        return gliders;
    }
    
    public async Task<List<Cosmetic>> GetSaturnPickaxes(bool isLobby = false, bool isOption = false)
    {
        var pickaxes = new List<Cosmetic>();

        AbstractGeneration Generation = new PickaxeGeneration(pickaxes, _provider, _configService, this);

        pickaxes = await Generation.Generate();

        Trace.WriteLine($"Deserialized {pickaxes.Count} objects");

        if (isLobby)
        {
            ConvertedItem? convItem = (await _configService.TryGetConvertedItems()).FirstOrDefault(x => x.ItemDefinition.Contains("LOBBY") && !x.ItemDefinition.Contains("CID_") && !x.ItemDefinition.Contains("BID_") && !x.ItemDefinition.Contains("EID_")) ?? null;
            for (int i = pickaxes.Count - 1; i >= 0; i--)
            {
                if (convItem != null)
                { 
                    pickaxes[i].VariantChannel = convItem.ItemDefinition;
                    pickaxes[i].IsConverted = true;
                    pickaxes[i].PrintColor = Colors.C_GREEN;
                }

                if (!pickaxes[i].Description.Contains("style:") || await _configService.TryGetShouldShowStyles()) continue;
                pickaxes.RemoveAt(i);
                i++;
            }
        }
        else
        {
            await _fortniteAPIService.RemoveItems(pickaxes);

            await _fortniteAPIService.AddExtraItems(pickaxes, ItemType.IT_Pickaxe);
            
            for (int i = pickaxes.Count - 1; i >= 0; i--)
            {
                if (pickaxes[i].Description.Contains("style:") && !await _configService.TryGetShouldShowStyles())
                {
                    pickaxes.RemoveAt(i);
                    i++;
                    continue;
                }
                
                if ((await _configService.TryGetConvertedItems()).Any(x => string.Equals(x.Name, pickaxes[i].Name) && string.Equals(x.ItemDefinition, pickaxes[i].Id)))
                    pickaxes[i].IsConverted = true;
            }
        }
        
        if (isOption)
            pickaxes.RemoveAll(x => x.Id.Length > Index.CurrentSkin.Id.Length);

        _discordRPCService.UpdatePresence($"Looking at {pickaxes.Count} different pickaxes");

        if (!FileUtil.CheckIfCppIsInstalled())
        {
            await _notificationService.Error(
                "There was an error decompressing packages with CUE4Parse. Please follow the tutorial that is opening on your browser to fix this, or paste this link in your browser: https://youtu.be/PeETf6ZQnBk");
            await Task.Delay(2000);
            await FileUtil.OpenBrowser("https://youtu.be/PeETf6ZQnBk");
        }
        
        if (pickaxes.Count == 0)
            await _notificationService.Error("There was a mappings error. To fix this, go to %localappdata%/Saturn/ and delete the folder 'Mappings' then relaunch the swapper.");


        return pickaxes;
    }
    
    public async Task<List<Cosmetic>> GetSaturnEmotes(bool isLobby = false, bool isOption = false)
    {
        var emotes = new List<Cosmetic>();

        AbstractGeneration Generation = new EmoteGeneration(emotes, _provider, _configService, this);

        emotes = await Generation.Generate();

        Trace.WriteLine($"Deserialized {emotes.Count} objects");

        if (isLobby)
        {
            ConvertedItem? convItem = (await _configService.TryGetConvertedItems()).FirstOrDefault(x => x.ItemDefinition.Contains("LOBBY") && x.ItemDefinition.Contains("EID_")) ?? null;
            for (int i = emotes.Count - 1; i >= 0; i--)
            {
                if (convItem != null)
                { 
                    emotes[i].VariantChannel = convItem.ItemDefinition;
                    emotes[i].IsConverted = true;
                    emotes[i].PrintColor = Colors.C_GREEN;
                }

                if (!emotes[i].Description.Contains("style:") || await _configService.TryGetShouldShowStyles()) continue;
                emotes.RemoveAt(i);
                i++;
            }
        }
        else
        {
            await _fortniteAPIService.RemoveItems(emotes);

            await _fortniteAPIService.AddExtraItems(emotes, ItemType.IT_Dance);
            
            for (int i = emotes.Count - 1; i >= 0; i--)
            {
                if (emotes[i].Description.Contains("style:") && !await _configService.TryGetShouldShowStyles())
                {
                    emotes.RemoveAt(i);
                    i++;
                    continue;
                }
                
                if ((await _configService.TryGetConvertedItems()).Any(x => string.Equals(x.Name, emotes[i].Name) && string.Equals(x.ItemDefinition, emotes[i].Id)))
                    emotes[i].IsConverted = true;
            }
        }
        
        if (isOption)
            emotes.RemoveAll(x => x.Id.Length > Index.CurrentSkin.Id.Length);

        _discordRPCService.UpdatePresence($"Looking at {emotes.Count} different emotes");

        if (!FileUtil.CheckIfCppIsInstalled())
        {
            await _notificationService.Error(
                "There was an error decompressing packages with CUE4Parse. Please follow the tutorial that is opening on your browser to fix this, or paste this link in your browser: https://youtu.be/PeETf6ZQnBk");
            await Task.Delay(2000);
            await FileUtil.OpenBrowser("https://youtu.be/PeETf6ZQnBk");
        }
        
        if (emotes.Count == 0)
            await _notificationService.Error("There was a mappings error. To fix this, go to %localappdata%/Saturn/ and delete the folder 'Mappings' then relaunch the swapper.");


        return emotes;
    }

    public async Task Swap(Cosmetic item, SaturnItem option, ItemType itemType, List<Cosmetic> Items, bool isAuto = false)
    {
        if (!_halted)
        {
            _halted = true;
            Logger.Log("Checking if item is converted or not!");
            if (item.IsConverted)
            {
                Logger.Log("Item is converted! Reverting!");
                if (!await Revert(item, option, itemType))
                {
                    await ItemUtil.UpdateStatus(item, option,
                        $"There was an error reverting {item.Name}!",
                        Colors.C_RED);
                    Logger.Log($"There was an error reverting {item.Name}!", LogLevel.Error);
                    Process.Start("notepad.exe", Config.LogFile);
                }
                
                if (Config.isMaintenance)
                {
                    await _notificationService.Warn(
                        "Some parts of the swapper might be broken! Watch announcements in his server so you are the first to know when the swapper is 100% working for the newest update!");
                }
            }
            else
            {
                Logger.Log("Item is not converted! Converting!");

                if (item.IsRandom)
                {
                    // Get a random number between two values.
                    var random = new Random();
                    var randomNumber = random.Next(0, Items.Count - 1);

                    while ((Items[randomNumber].HatTypes == HatTypes.HT_FaceACC &&
                            option.Name == "Redline") || (Items[randomNumber].HatTypes == HatTypes.HT_Hat &&
                                                          option.Name != "Redline"))
                        randomNumber = random.Next(0, Items.Count - 1);

                    if (!await Convert(Items[randomNumber], option, itemType, isAuto, true, item))
                    {
                        await ItemUtil.UpdateStatus(item, option,
                            $"There was an error converting {Items[randomNumber].Name}!",
                            Colors.C_RED);
                        Logger.Log($"There was an error converting {Items[randomNumber].Name}!", LogLevel.Error);
                        Process.Start("notepad.exe", Config.LogFile);
                    }
                    else if (Config.isMaintenance)
                    {
                        await _notificationService.Warn(
                            "Some parts of the swapper might be broken! Watch announcements in his server so you are the first to know when the swapper is 100% working for the newest update!");
                    }
                }
                else if (option.Name == "No options!")
                {
                    await _notificationService.Error(
                        $"There are no options for {item.Name}! Can you not read the name and description?");
                }
                else if (option.Name.Contains("Default Skins") || option.Name.Contains("No Backbling"))
                {
                    if (Config.isBeta)
                    {
                        if (option.Name.Contains("No Backbling") && !await _configService.TryGetIsDefaultSwapped())
                        {
                            await ItemUtil.UpdateStatus(item, option,
                                $"There was an error converting {item.Name}!",
                                Colors.C_RED);
                            await _notificationService.Error(
                                "You haven't swapped a skin from default! To add a backbling to default skins (from no backbling), you must swap a skin from default first!");
                        }
                        else
                        {
                            if (!await Convert(item, option, itemType, true))
                            {
                                await ItemUtil.UpdateStatus(item, option,
                                    $"There was an error converting {item.Name}!",
                                    Colors.C_RED);
                                Logger.Log($"There was an error converting {item.Name}!", LogLevel.Error);
                                Process.Start("notepad.exe", Config.LogFile);
                            }
                            else
                            {
                                await _configService.TrySetIsDefaultSwapped(true);
                            }
                        }
                    }
                    else
                    {
                        await _notificationService.Error(
                            "This is a BETA only feature! You have to boost Tamely's server to be able to swap from the default skin due to Saturn using a method no other swapper can offer!");
                        return;
                    }
                }
                else if (!await Convert(item, option, itemType))
                {
                    await ItemUtil.UpdateStatus(item, option,
                        $"There was an error converting {item.Name}!",
                        Colors.C_RED);
                    Logger.Log($"There was an error converting {item.Name}!", LogLevel.Error);
                    Process.Start("notepad.exe", Config.LogFile);
                }
                
                if (Config.isMaintenance)
                {
                    await _notificationService.Warn(
                        "Some parts of the swapper might be broken! Watch announcements in his server so you are the first to know when the swapper is 100% working for the newest update!");
                }

            }

            _halted = false;
        }
    }
    
    public async Task SwapLobby(Cosmetic item, Cosmetic option)
    {
        if (!_halted)
        {
            _halted = true;
            Logger.Log("Checking if item is converted or not!");
            if (item.IsConverted)
            {
                Logger.Log("Item is converted! Reverting!");
                if (!await RevertLobby(item, option))
                {
                    await ItemUtil.UpdateStatus(option, null,
                        $"There was an error reverting {option.Name}!",
                        Colors.C_RED);
                    Logger.Log($"There was an error reverting {option.Name}!", LogLevel.Error);
                    Process.Start("notepad.exe", Config.LogFile);
                }
            }
            else
            {
                Logger.Log("Item is not converted! Converting!");

                if (!await ConvertLobby(item, option))
                {
                    await ItemUtil.UpdateStatus(option, null,
                        $"There was an error converting {option.Name}!",
                        Colors.C_RED);
                    Logger.Log($"There was an error converting {option.Name}!", LogLevel.Error);
                    Process.Start("notepad.exe", Config.LogFile);
                }

            }

            _halted = false;
        }
    }

    private async Task<bool> RevertLobby(Cosmetic item, Cosmetic option)
    {
        try
        {
            await ItemUtil.UpdateStatus(option, null, "Starting...", Colors.C_YELLOW);
            var id = item.VariantChannel;

            var sw = Stopwatch.StartNew();

            await ItemUtil.UpdateStatus(option, null, "Checking config file for item", Colors.C_YELLOW);
            _configService.ConfigFile.ConvertedItems.Any(x =>
            {
                if (x.ItemDefinition != id) return false;
                foreach (var asset in x.Swaps)
                {
                    ItemUtil.UpdateStatus(item, null, "Reading compressed data", Colors.C_YELLOW).GetAwaiter()
                        .GetResult();
                    var data = File.ReadAllBytes(Path.Combine(Config.CompressedDataPath, item.VariantChannel.Replace("LOBBY", ""),
                        Path.GetFileName(asset.ParentAsset)));

                    ItemUtil.UpdateStatus(option, null, "Writing compressed data back to PAK", Colors.C_YELLOW)
                        .GetAwaiter().GetResult();
                    TrySwapAsset(Path.Combine(FortniteUtil.PakPath, "Saturn", asset.File), asset.Offset, data)
                        .GetAwaiter()
                        .GetResult();

                    ItemUtil.UpdateStatus(option, null, "Deleting compressed data", Colors.C_YELLOW).GetAwaiter()
                        .GetResult();

                    Directory.Delete(Path.Combine(Config.CompressedDataPath, item.VariantChannel.Replace("LOBBY", "")), true);
                    Directory.Delete(Path.Combine(Config.DecompressedDataPath, item.VariantChannel.Replace("LOBBY", "")), true);
                }

                return true;
            });


            if (!await _configService.RemoveConvertedItem(id))
                Logger.Log("There was an error removing the item from the config!", LogLevel.Error);
            _configService.SaveConfig();

            sw.Stop();

            item.IsConverted = false;
            if (sw.Elapsed.Seconds > 1)
                await ItemUtil.UpdateStatus(option, null, $"Reverted in {sw.Elapsed.Seconds} seconds!", Colors.C_GREEN);
            else
                await ItemUtil.UpdateStatus(option, null, $"Reverted in {sw.Elapsed.Milliseconds} milliseconds!",
                    Colors.C_GREEN);

            Logger.Log($"Reverted in {sw.Elapsed.Seconds} seconds!");
            Trace.WriteLine($"Reverted in {sw.Elapsed.Seconds} seconds!");
            return true;
        }
        catch (Exception ex)
        {
            await ItemUtil.UpdateStatus(option, null,
                $"There was an error reverting {item.Name}. Please send the log to Tamely on Discord!",
                Colors.C_RED);
            Logger.Log($"There was an error reverting {ex}");
            return false;
        }
    }

    private async Task<bool> ConvertLobby(Cosmetic item, Cosmetic option)
    {
        try
        {
            Directory.CreateDirectory(Config.DecompressedDataPath);
            Directory.CreateDirectory(Config.CompressedDataPath);
            
            await ItemUtil.UpdateStatus(option, null, "Starting...", Colors.C_YELLOW);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            ConvertedItem convertedItem = new ConvertedItem()
            {
                Name = item.Name,
                ItemDefinition = item.Id + "LOBBY",
                Type = "LobbySkin"
            };
            
            await BackupFile("pakchunk0-WindowsClient", option);

            SaturnData.SearchCID = item.Id + "." + item.Id;
            byte[] Search = Encoding.ASCII.GetBytes(item.Id + "." + item.Id);
            byte[] Replace = Encoding.ASCII.GetBytes(option.Id + "." + option.Id);
            
            Logger.Log("Searching ID: " + item.Id);
            Logger.Log("Replacing ID: " + option.Id);
            
            if (_provider.TrySaveAsset("FortniteGame/AssetRegistry.bin", out _))
            {
                if (SaturnData.Block != null)
                {
                    if (Search.Length >= Replace.Length)
                    {
                        FillEnd(ref Replace, Search.Length);

                        byte[] EditedAsset = SaturnData.Block.DecompressedData;
                        
                        AnyLength.SwapNormally(new List<byte[]> { Search }, new List<byte[]> { Replace }, ref EditedAsset);

                        Directory.CreateDirectory(Config.DecompressedDataPath + "//" + item.Id);
                        await File.WriteAllBytesAsync(Config.DecompressedDataPath + "//" + item.Id + "//AssetRegistry.bin", EditedAsset);
                        
                        var compressedData = ZLIB.Compress(EditedAsset);
                        FillEnd(ref compressedData, SaturnData.Block.CompressedData.Length);
                        
                        Directory.CreateDirectory(Config.CompressedDataPath + "//" + item.Id);
                        await File.WriteAllBytesAsync(Config.CompressedDataPath + "//" + item.Id + "//AssetRegistry.bin",
                            compressedData);

                        await TrySwapAsset(Path.Combine(FortniteUtil.PakPath, "Saturn", "pakchunk0-SaturnClient.pak"), SaturnData.Block.Start, compressedData);

                        List<ActiveSwap> swaps = new List<ActiveSwap>()
                        {
                            new ActiveSwap()
                            {
                                File = "pakchunk0-SaturnClient.pak",
                                IsCompressed = true,
                                Offset = SaturnData.Block.Start,
                                Lengths = new Dictionary<long, byte[]>(),
                                ParentAsset = "AssetRegistry.bin"
                            }
                        };
                        
                        sw.Stop();
                        
                        convertedItem.Swaps = swaps;
                        item.IsConverted = true;

                        await _configService.AddConvertedItem(convertedItem);
                        _configService.SaveConfig();

                        if (sw.Elapsed.Minutes > 1)
                        {
                            await ItemUtil.UpdateStatus(option, null, $"Converted in {sw.Elapsed.Minutes} minutes and {sw.Elapsed.Seconds} seconds!",
                                Colors.C_GREEN);
                            await _notificationService.Success(
                                $"Converted in {sw.Elapsed.Minutes} minutes and {sw.Elapsed.Seconds} seconds!", true,
                                "Launch Fortnite");
                        }
                        else if (sw.Elapsed.Seconds > 1)
                        {
                            await ItemUtil.UpdateStatus(option, null, $"Converted in {sw.Elapsed.Seconds} seconds!",
                                Colors.C_GREEN);
                            await _notificationService.Success(
                                $"Converted in {sw.Elapsed.Seconds} seconds!", true,
                                "Launch Fortnite");
                        }
                        else
                        {
                            await ItemUtil.UpdateStatus(option, null, $"Converted in {sw.Elapsed.Milliseconds} milliseconds!",
                                Colors.C_GREEN);
                            await _notificationService.Success(
                                $"Converted in {sw.Elapsed.Milliseconds} milliseconds!", true,
                                "Launch Fortnite");
                        }

                        return true;
                    }
                }
                else
                {
                    Logger.Log("Block is null!", LogLevel.Fatal);
                }
            }
            else
            {
                Logger.Log("Could not find AssetRegistry.bin", LogLevel.Fatal);
            }

            await _notificationService.Error("Failed to convert lobby skin!");
            return false;
        }
        catch (Exception e)
        {
            Logger.Log("There was an error converting a lobby swap! The error was " + e, LogLevel.Fatal);
            return false;
        }
    }
    
    public async Task<bool> Convert(Cosmetic item, SaturnItem option, ItemType itemType, bool isDefault = false, bool isRandom = false, Cosmetic random = null)
    {
        try
        {
            option.Status = null;

            var sw = Stopwatch.StartNew();

            if (isRandom)
                await ItemUtil.UpdateStatus(random, option, "Starting...");
            else
                await ItemUtil.UpdateStatus(item, option, "Starting...");

            ConvertedItem convItem = new()
            {
                Item = item,
                Option = option,
                FromName = option.Name,
                ItemType = itemType,
                IsDefault = isDefault,
                IsRandom = isRandom,
                Random = random,
                Name = item.Name,
                ItemDefinition = item.Id,
                Type = itemType.ToString(),
                Swaps = new List<ActiveSwap>()
            };

            if (isRandom)
            {
                convItem = new ConvertedItem()
                {
                    Item = item,
                    Option = option,
                    FromName = option.Name,
                    ItemType = itemType,
                    IsDefault = isDefault,
                    IsRandom = isRandom,
                    Random = random,
                    Name = item.Name,
                    ItemDefinition = random.Id,
                    Type = itemType.ToString(),
                    Swaps = new List<ActiveSwap>()
                };
            }

            if (isRandom)
                await ItemUtil.UpdateStatus(random, option, "Checking item type");
            else
                await ItemUtil.UpdateStatus(item, option, "Checking item type");
            Changes cloudChanges = new();

            if (isRandom)
                await ItemUtil.UpdateStatus(random, option, "Generating swaps", Colors.C_YELLOW);
            else
                await ItemUtil.UpdateStatus(item, option, "Generating swaps", Colors.C_YELLOW);
            Logger.Log("Generating swaps...");

            if (itemType == ItemType.IT_Glider)
            {
                Logger.Log("Getting glider swaps...");
                string originalMaterial = (await new AddGliders().GetGliderData(option.ItemDefinition, _provider))["Material"];
                string replaceMaterial = option.Swaps["Material"].Split("|")[0];

                Dictionary<string, string> OriginalTextures = new();
                if (_provider.TryLoadObject(originalMaterial, out UObject OriginalMaterial))
                {
                    if (OriginalMaterial.TryGetValue(out FStructFallback[] TextureParameterValues,
                            "TextureParameterValues"))
                    {
                        foreach (var texture in TextureParameterValues)
                        {
                            if (texture.TryGetValue(out FStructFallback ParameterInfo, "ParameterInfo"))
                            {
                                if (ParameterInfo.TryGetValue(out FName Name, "Name"))
                                {
                                    if (texture.TryGetValue(out UObject text, "ParameterValue"))
                                    {
                                        OriginalTextures.Add(Name.Text, text.GetPathName());
                                    }
                                }
                                else
                                {
                                    Logger.Log("Could not get ParameterInfo.Name!", LogLevel.Fatal);
                                    return false;
                                }
                            }
                            else
                            {
                                Logger.Log("Could not get ParameterInfo!", LogLevel.Fatal);
                                return false;
                            }
                        }
                    }
                    else
                    {
                        Logger.Log("Could not find TextureParameterValues!", LogLevel.Fatal);
                        return false;
                    }
                }
                else
                {
                    Logger.Log("Could not find original material!", LogLevel.Fatal);
                    return false;
                }
                
                Logger.Log("Found " + OriginalTextures.Count + " textures for the original material!");
                foreach (var texture in OriginalTextures)
                {
                    Logger.Log("Texture: " + texture.Key + " - " + texture.Value);
                }
                
                Dictionary<string, string> ReplaceTextures = new();
                if (_provider.TryLoadObject(replaceMaterial, out UObject ReplaceMaterial))
                {
                    if (ReplaceMaterial.TryGetValue(out FStructFallback[] TextureParameterValues,
                            "TextureParameterValues"))
                    {
                        foreach (var texture in TextureParameterValues)
                        {
                            if (texture.TryGetValue(out FStructFallback ParameterInfo, "ParameterInfo"))
                            {
                                if (ParameterInfo.TryGetValue(out FName Name, "Name"))
                                {
                                    if (texture.TryGetValue(out UObject text, "ParameterValue"))
                                    {
                                        ReplaceTextures.Add(Name.Text, text.GetPathName());
                                    }
                                }
                                else
                                {
                                    Logger.Log("Could not get ParameterInfo.Name!", LogLevel.Fatal);
                                    return false;
                                }
                            }
                            else
                            {
                                Logger.Log("Could not get ParameterInfo!", LogLevel.Fatal);
                                return false;
                            }
                        }
                    }
                    else
                    {
                        Logger.Log("Could not find TextureParameterValues!", LogLevel.Fatal);
                        return false;
                    }
                }
                else
                {
                    Logger.Log("Could not find replace material!", LogLevel.Fatal);
                    return false;
                }
                
                Logger.Log("Found " + ReplaceTextures.Count + " textures for the replace material!");
                foreach (var texture in ReplaceTextures)
                {
                    Logger.Log("Texture: " + texture.Key + " - " + texture.Value);
                }

                foreach (var texture in OriginalTextures)
                {
                    if (ReplaceTextures.ContainsKey(texture.Key) && texture.Value != ReplaceTextures[texture.Key])
                    {
                        Logger.Log("Found a match for " + texture.Key + "!");
                        
                        if (!TryExportAsset(texture.Value, out _, true))
                        {
                            Logger.Log($"Failed to export \"{texture.Value}\"!", LogLevel.Error);
                            return false;
                        }
                        
                        var file = SaturnData.Path;
                        
                        await BackupFile(file, item, option);
                        
                        var iconResult =
                            await new TextureImporter(Provider).SwapTexture(texture.Value.Split('.')[0] + ".uasset",
                                ReplaceTextures[texture.Key].Split('.')[0] + ".uasset");
                        if (!iconResult.Success) Logger.Log(iconResult.Error);
                        else
                        {
                            Logger.Log("Swapped " + texture.Value + " with " + ReplaceTextures[texture.Key]);
                            convItem.Swaps.Add(new ActiveSwap
                            {
                                File = file,
                                Offset = iconResult.Offset,
                                ParentAsset = texture.Value,
                                IsCompressed = true
                            });
                        }
                    }
                }
                
                sw.Stop();

                if (!await _configService.AddConvertedItem(convItem))
                    Logger.Log("Could not add converted item to config!", LogLevel.Error);
                else
                    Logger.Log($"Added {item.Name} to converted items list in config.");

                _configService.SaveConfig();

                if (sw.Elapsed.Minutes > 1)
                    if (isRandom)
                        await ItemUtil.UpdateStatus(random, option,
                            $"Converted in {sw.Elapsed.Minutes} minutes and {sw.Elapsed.Seconds} seconds!",
                            Colors.C_GREEN);
                    else
                        await ItemUtil.UpdateStatus(item, option,
                            $"Converted in {sw.Elapsed.Minutes} minutes and {sw.Elapsed.Seconds} seconds!",
                            Colors.C_GREEN);
                else if (sw.Elapsed.Seconds > 1)
                    if (isRandom)
                        await ItemUtil.UpdateStatus(random, option, $"Converted in {sw.Elapsed.Seconds} seconds!",
                            Colors.C_GREEN);
                    else
                        await ItemUtil.UpdateStatus(item, option, $"Converted in {sw.Elapsed.Seconds} seconds!",
                            Colors.C_GREEN);
                else if (isRandom)
                    await ItemUtil.UpdateStatus(random, option,
                        $"Converted in {sw.Elapsed.Milliseconds} milliseconds!", Colors.C_GREEN);
                else
                    await ItemUtil.UpdateStatus(item, option,
                        $"Converted in {sw.Elapsed.Milliseconds} milliseconds!", Colors.C_GREEN);
                Trace.WriteLine($"Converted in {sw.Elapsed.Seconds} seconds!");
                Logger.Log($"Converted in {sw.Elapsed.Seconds} seconds!");

                if (await _configService.GetConvertedFileCount() > 2)
                    await _notificationService.Error(
                        "You have more than 2 converted files. This will cause Fortnite to kick you out of your game! Please revert the last item you swapped to prevent this!");
            }
            else
            {
                SaturnOption itemSwap = new();
                if (isDefault)
                    itemSwap = await GenerateDefaultSkins(item);
                else if (option.Options != null)
                    itemSwap = option.Options[0];
                else
                    itemSwap = itemType switch
                    {
                        ItemType.IT_Skin => await GenerateMeshSkins(item, option),
                        ItemType.IT_Dance => await GenerateMeshEmote(item, option),
                        ItemType.IT_Pickaxe => await GenerateMeshPickaxe(item, option),
                        ItemType.IT_Backbling => await GenerateMeshBackbling(item, option),
                        _ => new()
                    };

                if (item.IsCloudAdded)
                    itemSwap = option.Options[0];

                Logger.Log($"There are {itemSwap.Assets.Count} assets to swap...", LogLevel.Info);
                foreach (var asset in itemSwap.Assets)
                {
                    Logger.Log($"Starting swaps for {asset.ParentAsset}");
                    Directory.CreateDirectory(Config.CompressedDataPath);
                    if (isRandom)
                        await ItemUtil.UpdateStatus(random, option, "Exporting asset", Colors.C_YELLOW);
                    else
                        await ItemUtil.UpdateStatus(item, option, "Exporting asset", Colors.C_YELLOW);
                    Logger.Log("Exporting asset");
                    if (!TryExportAsset(asset.ParentAsset, out var data))
                    {
                        Logger.Log($"Failed to export \"{asset.ParentAsset}\"!", LogLevel.Error);
                        return false;
                    }

                    if (isDefault && asset.ParentAsset.Contains("DefaultGameDataCosmetics"))
                        data = new WebClient().DownloadData(
                            "https://cdn.discordapp.com/attachments/754879989614379042/983189162444357642/test.uasset");
                    Logger.Log("Asset exported");
                    Logger.Log($"Starting backup of {Path.GetFileName(SaturnData.Path)}");

                    var file = SaturnData.Path;

                    if (isRandom)
                        await BackupFile(file, random, option);
                    else
                        await BackupFile(file, item, option);

                    foreach (var swaps in asset.Swaps)
                    {
                        Logger.Log(swaps.Search + " :: " + swaps.Replace);
                    }

                    if (!TryIsB64(ref data, asset))
                        Logger.Log($"Cannot swap/determine if '{asset.ParentAsset}' is Base64 or not!",
                            LogLevel.Fatal);

                    var Oodle = new Utils.Compression.Oodle(Config.BasePath);
                    var compressed = SaturnData.isCompressed ? Oodle.Compress(data) : data;

                    Directory.CreateDirectory(Config.DecompressedDataPath);
                    File.SetAttributes(Config.DecompressedDataPath,
                        FileAttributes.Hidden | FileAttributes.System);
                    await File.WriteAllBytesAsync(
                        Config.DecompressedDataPath + Path.GetFileName(asset.ParentAsset).Replace(".uasset", "") +
                        ".uasset", data);


                    file = Path.GetFileName(file.Replace("WindowsClient", "SaturnClient"));

                    var ucas = file;

                    if (isRandom)
                        await ItemUtil.UpdateStatus(random, option, "Adding asset to UCAS", Colors.C_YELLOW);
                    else
                        await ItemUtil.UpdateStatus(item, option, "Adding asset to UCAS", Colors.C_YELLOW);

                    await TrySwapAsset(Path.Combine(FortniteUtil.PakPath, file), SaturnData.Offset,
                        compressed);

                    if (file.Contains("ient_s")) // Check if it's partitioned
                        file = file.Split("ient_s")[0] +
                               "ient"; // Remove the partition from the name because they don't get utocs

                    file = file.Replace("ucas", "utoc");

                    if (!file.Contains("utoc")) // Check if it contains utoc
                        file += ".utoc"; // If not, add it

                    Dictionary<long, byte[]> lengths = new();
                    if (!await CustomAssets.TryHandleOffsets(asset, compressed.Length, data.Length, lengths, file,
                            _saturnAPIService))
                        Logger.Log(
                            $"Unable to apply custom offsets to '{asset.ParentAsset}.' Asset might not have custom assets at all!",
                            LogLevel.Error);

                    if (isRandom)
                        await ItemUtil.UpdateStatus(random, option, "Adding swap to item's config", Colors.C_YELLOW);
                    else
                        await ItemUtil.UpdateStatus(item, option, "Adding swap to item's config", Colors.C_YELLOW);
                    convItem.Swaps.Add(new ActiveSwap
                    {
                        File = ucas,
                        Offset = SaturnData.Offset,
                        ParentAsset = asset.ParentAsset,
                        IsCompressed = SaturnData.isCompressed,
                        Lengths = lengths
                    });
                }

                if (isRandom)
                    random.IsConverted = true;
                else
                    item.IsConverted = true;

                if (option.Type == ItemType.IT_Skin && Config.isBeta)
                {
                    var swapToIcon = await GetIconFromCID(item.Id);
                    if (isDefault)
                    {
                        foreach (var icon in Constants.DefaultIconPaths)
                        {
                            await new TextureImporter(Provider).SwapTexture(icon, swapToIcon);
                        }
                    }
                    else
                    {
                        var iconResult =
                            await new TextureImporter(Provider).SwapTexture(await GetIconFromCID(option.ItemDefinition),
                                swapToIcon);
                        if (!iconResult.Success) Logger.Log(iconResult.Error);
                    }
                }

                sw.Stop();

                if (!await _configService.AddConvertedItem(convItem))
                    Logger.Log("Could not add converted item to config!", LogLevel.Error);
                else
                    Logger.Log($"Added {item.Name} to converted items list in config.");

                _configService.SaveConfig();

                if (sw.Elapsed.Minutes > 1)
                    if (isRandom)
                        await ItemUtil.UpdateStatus(random, option,
                            $"Converted in {sw.Elapsed.Minutes} minutes and {sw.Elapsed.Seconds} seconds!",
                            Colors.C_GREEN);
                    else
                        await ItemUtil.UpdateStatus(item, option,
                            $"Converted in {sw.Elapsed.Minutes} minutes and {sw.Elapsed.Seconds} seconds!",
                            Colors.C_GREEN);
                else if (sw.Elapsed.Seconds > 1)
                    if (isRandom)
                        await ItemUtil.UpdateStatus(random, option, $"Converted in {sw.Elapsed.Seconds} seconds!",
                            Colors.C_GREEN);
                    else
                        await ItemUtil.UpdateStatus(item, option, $"Converted in {sw.Elapsed.Seconds} seconds!",
                            Colors.C_GREEN);
                else if (isRandom)
                    await ItemUtil.UpdateStatus(random, option,
                        $"Converted in {sw.Elapsed.Milliseconds} milliseconds!", Colors.C_GREEN);
                else
                    await ItemUtil.UpdateStatus(item, option,
                        $"Converted in {sw.Elapsed.Milliseconds} milliseconds!", Colors.C_GREEN);
                Trace.WriteLine($"Converted in {sw.Elapsed.Seconds} seconds!");
                Logger.Log($"Converted in {sw.Elapsed.Seconds} seconds!");

                if (await _configService.GetConvertedFileCount() > 2)
                    await _notificationService.Error(
                        "You have more than 2 converted files. This will cause Fortnite to kick you out of your game! Please revert the last item you swapped to prevent this!");
            }


            return true;
        }
        catch (Exception ex)
        {
            await ItemUtil.UpdateStatus(item, option,
                $"There was an error converting {item.Name}. Please send the log to Tamely on Discord!",
                Colors.C_RED);
            Logger.Log($"There was an error converting {ex}");
            
            if (ex.ToString()
                .Contains(
                    "Win32.SafeHandles.SafeFileHandle.CreateFile(String fullPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)"))
            {
                await _notificationService.Error(
                    "Couldn't create file backups! Please follow the tutorial that is opening on your browser to fix this, or paste this link in your browser: https://youtu.be/YXXj31G7QKg");
                await Task.Delay(2000);
                await FileUtil.OpenBrowser("https://youtu.be/YXXj31G7QKg");
            }
            return false;
        }
    }

    public async Task<bool> Revert(Cosmetic item, SaturnItem option, ItemType itemType)
    {
        try
        {
            await ItemUtil.UpdateStatus(item, option, "Starting...", Colors.C_YELLOW);
            var id = item.Id;

            var sw = Stopwatch.StartNew();

            await ItemUtil.UpdateStatus(item, option, "Checking config file for item", Colors.C_YELLOW);
            _configService.ConfigFile.ConvertedItems.Any(x =>
            {
                if (x.ItemDefinition != id) return false;
                foreach (var asset in x.Swaps)
                {
                    ItemUtil.UpdateStatus(item, option, "Reading compressed data", Colors.C_YELLOW).GetAwaiter()
                        .GetResult();
                    var data = File.ReadAllBytes(Path.Combine(Config.CompressedDataPath,
                        Path.GetFileName(asset.ParentAsset).Replace(".uasset", "") + ".uasset"));

                    ItemUtil.UpdateStatus(item, option, "Writing compressed data back to UCAS", Colors.C_YELLOW)
                        .GetAwaiter().GetResult();
                    TrySwapAsset(Path.Combine(FortniteUtil.PakPath, asset.File), asset.Offset, data)
                        .GetAwaiter()
                        .GetResult();

                    ItemUtil.UpdateStatus(item, option, "Checking for customs", Colors.C_YELLOW).GetAwaiter()
                        .GetResult();

                    string file = asset.File;
                    if (file.Contains("ient_s")) // Check if it's partitioned
                        file = file.Split("ient_s")[0] + "ient.ucas"; // Remove the partition from the name because they don't get utocs
                    
                    if (asset.Lengths != new Dictionary<long, byte[]>())
                        foreach (var (key, value) in asset.Lengths)
                            TrySwapAsset(
                                Path.Combine(FortniteUtil.PakPath, file.Replace("ucas", "utoc")),
                                key, value).GetAwaiter().GetResult();

                    ItemUtil.UpdateStatus(item, option, "Deleting compressed data", Colors.C_YELLOW).GetAwaiter()
                        .GetResult();
                    File.Delete(Path.Combine(Config.CompressedDataPath,
                        Path.GetFileName(asset.ParentAsset).Replace(".uasset", "") + ".uasset"));
                    File.Delete(Path.Combine(Config.DecompressedDataPath,
                        Path.GetFileName(asset.ParentAsset).Replace(".uasset", "") + ".uasset"));
                }

                return true;
            });

            if (option.Type == ItemType.IT_Skin && Config.isBeta)
            {
                var path = await GetIconFromCID(item.Id); // Swapping icon path
                if (option.Name.Contains("Default Skins"))
                {
                    foreach (var icon in Constants.DefaultIconPaths)
                    {
                        await new TextureImporter(Provider).SwapTexture(null, path,
                            await File.ReadAllBytesAsync(Path.Combine(Config.CompressedDataPath, icon.SubstringAfterLast('/'))));
                        File.Delete(icon.SubstringAfterLast('/'));
                    }
                }
                else
                {
                    var iconResult = await new TextureImporter(Provider).SwapTexture(
                        path, null,
                        await File.ReadAllBytesAsync(Path.Combine(Config.CompressedDataPath, path.SubstringAfterLast('/'))));
                    if (!iconResult.Success) Logger.Log(iconResult.Error);
                }
            }

            if (!await _configService.RemoveConvertedItem(id))
                Logger.Log("There was an error removing the item from the config!", LogLevel.Error);
            _configService.SaveConfig();

            sw.Stop();

            item.IsConverted = false;
            if (sw.Elapsed.Seconds > 1)
            {
                await ItemUtil.UpdateStatus(item, option, $"Reverted in {sw.Elapsed.Seconds} seconds!", Colors.C_GREEN);
                await _notificationService.Success($"Reverted in {sw.Elapsed.Seconds} seconds!", true,
                    "Launch Fortnite");
            }
            else
            {
                await ItemUtil.UpdateStatus(item, option, $"Reverted in {sw.Elapsed.Milliseconds} milliseconds!",
                    Colors.C_GREEN);
                await _notificationService.Success($"Reverted in {sw.Elapsed.Milliseconds} milliseconds!", true,
                    "Launch Fortnite");
            }

            Logger.Log($"Reverted in {sw.Elapsed.Seconds} seconds!");
            Trace.WriteLine($"Reverted in {sw.Elapsed.Seconds} seconds!");
            return true;
        }
        catch (Exception ex)
        {
            await ItemUtil.UpdateStatus(item, option,
                $"There was an error reverting {item.Name}. Please send the log to Tamely on Discord!",
                Colors.C_RED);
            Logger.Log($"There was an error reverting {ex}");
            return false;
        }
    }

    /// <summary>
    /// Gets the WID from the pickaxes ID
    /// </summary>
    /// <param name="id">The pickaxes ID</param>
    /// <returns>UObject: The WID of the pickaxe specified in the arguments.</returns>
    public async Task<UObject> GetWIDByID(string id)
    {
        UObject export = new UObject(); // Create a new UObject to hold the export
        await Task.Run(() => // Run the following code on a separate thread because mappings hang on the main one
        {
            if (_provider.TryLoadObject(Constants.PidPath + id, out UObject PID))
                PID.TryGetValue(out export, "WeaponDefinition"); // Get the WID from the PID
            else if (_provider.TryLoadObject(Constants.NewPidPath + id, out PID))

                PID.TryGetValue(out export, "WeaponDefinition"); // Get the WID from the PID

        });

        return export; // Return the WID
    }
    
    /// <summary>
    /// Gets the CP from the BID
    /// </summary>
    /// <param name="id">The backbling's ID</param>
    /// <returns>UObject: The CP of the backbling ID specified in the arguments.</returns>
    public async Task<UObject> GetBackblingCP(string id)
    {
        UObject[] export = Array.Empty<UObject>(); // Create a new UObject to hold the export
        await Task.Run(() => // Run the following code on a separate thread because mappings hang on the main one
        {
            if (_provider.TryLoadObject(Constants.BidPath + id, out UObject BID))
                BID.TryGetValue(out export, "CharacterParts"); // Get the CPs from the BID
            else if (_provider.TryLoadObject(Constants.NewBidPath + id, out BID))
                BID.TryGetValue(out export, "CharacterParts"); // Get the CPs from the BID
            else if (_provider.TryLoadObject(Constants.ConstructorBidPath + id, out BID))
                BID.TryGetValue(out export, "CharacterParts"); // Get the CPs from the BID
            else if (_provider.TryLoadObject(Constants.OutlanderBidPath + id, out BID))
                BID.TryGetValue(out export, "CharacterParts"); // Get the CPs from the BID
            else if (_provider.TryLoadObject(Constants.NinjaBidPath + id, out BID))
                BID.TryGetValue(out export, "CharacterParts"); // Get the CPs from the BID
            else if (_provider.TryLoadObject(Constants.CommandoBidPath + id, out BID))
                BID.TryGetValue(out export, "CharacterParts"); // Get the CPs from the BID

        });

        if (export.Length > 0)
            return export[0]; // Return the base CP

        Logger.Log("Unable to find backbling character part for " + id, LogLevel.Error);
        return new UObject(); // Return an empty UObject
    }

    public async Task<Dictionary<string, string>> GetCharacterPartsById(string id, Cosmetic? item = null)
    {
        Dictionary<string, string> cps = new();
            
        if (_provider.TryLoadObject(Constants.CidPath + id, out var CharacterItemDefinition))
        {
            if (CharacterItemDefinition.TryGetValue(out UObject[] CharacterParts, "BaseCharacterParts"))
            {
                if (item is {VariantChannel: { }})
                    if (item.VariantChannel.ToLower().Contains("parts") || item.VariantChannel.ToLower().Contains("material") || item.VariantTag == null)
                    {
                        if (CharacterItemDefinition.TryGetValue(out UObject[] ItemVariants, "ItemVariants"))
                        {
                            foreach (var style in ItemVariants)
                            {
                                if (style.TryGetValue(out FStructFallback[] PartOptions, "PartOptions"))
                                    foreach (var PartOption in PartOptions)
                                    {
                                        if (PartOption.TryGetValue(out FText VariantName, "VariantName"))
                                        {
                                            if (VariantName.Text != item.Name && item.VariantTag != null)
                                            {
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            Logger.Log("No VariantName found");
                                            continue;
                                        }
                                
                                        if (PartOption.TryGetValue(out UObject[] Parts, "VariantParts"))
                                        {
                                            CharacterParts = CharacterParts.Concat(Parts).ToArray();
                                        }
                                    }
                            
                                if (style.TryGetValue(out FStructFallback[] MaterialOptions, "MaterialOptions"))
                                    foreach (var MaterialOption in MaterialOptions)
                                    {
                                        if (MaterialOption.TryGetValue(out FText VariantName, "VariantName"))
                                        {
                                            if (VariantName.Text != item.Name && item.VariantTag != null)
                                            {
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            Logger.Log("No VariantName found");
                                            continue;
                                        }
                                
                                        if (MaterialOption.TryGetValue(out UObject[] Parts, "VariantParts"))
                                        {
                                            CharacterParts = CharacterParts.Concat(Parts).ToArray();
                                        }
                                    }
                            }
                        }
                    }
                    else
                        Logger.Log("Item style doesn't contain parts");

                foreach (var characterPart in CharacterParts)
                {
                    if (characterPart == null)
                        continue;
                    if (!characterPart.TryGetValue(out EFortCustomPartType CustomPartType, "CharacterPartType"))
                    {
                        CustomPartType = EFortCustomPartType.Head;
                    }

                    if (cps.ContainsKey(CustomPartType.ToString()))
                        cps.Remove(CustomPartType.ToString());
                    
                    cps.Add(CustomPartType.ToString(), characterPart.GetPathName());
                }
            }
            else
            {
                Logger.Log("Failed to load Character Parts...");
            }
                
        }
        else
        {
            Logger.Log("Failed to Load Character...", LogLevel.Fatal);
        }

        return cps;
    }

    private async Task<SaturnOption> GenerateMeshGlider(Cosmetic item, SaturnItem option)
    {
        return new SaturnOption();
    }

    #region GenerateBackbling
    private async Task<SaturnOption> GenerateMeshBackbling(Cosmetic item, SaturnItem option)
    {
        return option.ItemDefinition switch
        {
            "BID_695_StreetFashionEclipse" => new BlackoutBagBackblingSwap(item.Name,
                                                                           item.Images.SmallIcon,
                                                                           item.Rarity.BackendValue,
                                                                           option.Swaps).ToSaturnOption(),
            "BID_600_HightowerTapas" => new ThorsCloakBackblingSwap(item.Name,
                                                                    item.Images.SmallIcon,
                                                                    item.Rarity.BackendValue,
                                                                    option.Swaps).ToSaturnOption(),
            "BID_678_CardboardCrewHolidayMale" => new WrappingCaperBackblingSwap(item.Name,
                                                                                 item.Images.SmallIcon,
                                                                                 item.Rarity.BackendValue,
                                                                                 option.Swaps).ToSaturnOption(),
            "BID_430_GalileoSpeedBoat_9RXE3" => new TheSithBackblingSwap(item.Name,
                                                                         item.Images.SmallIcon,
                                                                         item.Rarity.BackendValue,
                                                                         option.Swaps).ToSaturnOption(),
            "BID_545_RenegadeRaiderFire" => new FirestarterBackblingSwap(item.Name,
                                                                        item.Images.SmallIcon,
                                                                        item.Rarity.BackendValue,
                                                                        option.Swaps).ToSaturnOption(),
            "BID_562_CelestialFemale" => new NucleusBackblingSwap(item.Name,
                                                                item.Images.SmallIcon,
                                                                item.Rarity.BackendValue,
                                                                option.Swaps).ToSaturnOption(),
            "BID_289_Banner" => new BannerCapeBackblingSwap(item.Name,
                                                                item.Images.SmallIcon,
                                                                item.Rarity.BackendValue,
                                                                option.Swaps).ToSaturnOption(),
            "BID_266_BunkerMan" => new NanaCapeBackblingSwap(item.Name,
                                                                item.Images.SmallIcon,
                                                                item.Rarity.BackendValue,
                                                                option.Swaps).ToSaturnOption(),
            "BID_121_RedRiding" => new FabledCapeBackblingSwap(item.Name,
                                                            item.Images.SmallIcon,
                                                            item.Rarity.BackendValue,
                                                            option.Swaps).ToSaturnOption(),
            "BID_122_HalloweenTomato" => new NightCloakBackblingSwap(item.Name,
                                                                    item.Images.SmallIcon,
                                                                    item.Rarity.BackendValue,
                                                                    option.Swaps).ToSaturnOption(),
            "BID_073_DarkViking" => new FrozenShroudBackblingSwap(item.Name,
                                                                    item.Images.SmallIcon,
                                                                    item.Rarity.BackendValue,
                                                                    option.Swaps).ToSaturnOption(),
            "BID_167_RedKnightWinterFemale" => new FrozenRedShieldBackblingSwap(item.Name,
                                                                                item.Images.SmallIcon,
                                                                                item.Rarity.BackendValue,
                                                                                option.Swaps).ToSaturnOption(),
            "BID_003_RedKnight" => new RedShieldBackblingSwap(item.Name,
                                                            item.Images.SmallIcon,
                                                            item.Rarity.BackendValue,
                                                            option.Swaps).ToSaturnOption(),
            "BID_343_CubeRedKnight" => new DarkShieldBackblingSwap(item.Name,
                                                                item.Images.SmallIcon,
                                                                item.Rarity.BackendValue,
                                                                option.Swaps).ToSaturnOption(),
            "BID_388_DevilRockMale" => new FlameSigilBackblingSwap(item.Name,
                                                                item.Images.SmallIcon,
                                                                item.Rarity.BackendValue,
                                                                option.Swaps).ToSaturnOption(),   
            "BID_069_DecoMale" => new VenturionCapeBackblingSwap(item.Name,
                                                               item.Images.SmallIcon,
                                                               item.Rarity.BackendValue,
                                                               option.Swaps).ToSaturnOption(),
            "BID_070_DecoFemale" => new VenturaCapeBackblingSwap(item.Name,
                                                                 item.Images.SmallIcon,
                                                                 item.Rarity.BackendValue,
                                                                 option.Swaps).ToSaturnOption(),
            "BID_319_StreetRacerDriftRemix" => new AtmosphereBackblingswap(item.Name,
                                                                        item.Images.SmallIcon,
                                                                        item.Rarity.BackendValue,
                                                                        option.Swaps).ToSaturnOption(),
            _ => new SaturnOption()
        };
    }
    #endregion

    #region GenerateMeshDefaults
    private async Task<SaturnOption> GenerateDefaultSkins(Cosmetic item)
    {
        Logger.Log($"Getting character parts for {item.Name}");
        Logger.Log(item.Id);

        bool isBackblingSwap = item.Id.StartsWith("BID_");
        var cps = new List<string>();

        if (isBackblingSwap)
        {
            foreach (var _item in await _configService.TryGetConvertedItems())
            {
                if (!_item.Swaps[0].ParentAsset.Contains("DefaultGameDataCosmetics")) continue;
                cps = (await GetCharacterPartsById(_item.ItemDefinition)).Values.Where(x => x != "/").ToList();
                break;
            }
        }

        var characterParts = isBackblingSwap ? cps : (await GetCharacterPartsById(item.Id)).Values.Where(x => x != "/").ToList();

        return new DefaultSkinSwap(item.Name,
                                   item.Rarity.BackendValue,
                                   item.Images.SmallIcon,
                                   characterParts,
                                   item.Id.StartsWith("BID_") ? (await GetBackblingCP(item.Id)).GetPathName() : "/").ToSaturnOption();
    }

    private async Task<SaturnOption> GenerateMeshSkins(Cosmetic item, SaturnItem option)
    {
        Logger.Log($"Getting character parts for {item.Name}");
        Logger.Log(Constants.CidPath + item.Id);

        Logger.Log("Generating swaps");

        return option.ItemDefinition switch
        {
            "CID_162_Athena_Commando_F_StreetRacer" => new RedlineSkinSwap(item.Name, 
                                                                           item.Rarity.BackendValue, 
                                                                           item.Images.SmallIcon, 
                                                                           option.SwapModel).ToSaturnOption(),
            "CID_653_Athena_Commando_F_UglySweaterFrozen" => new FrozenNogOpsSkinSwap(item.Name,
                                                                                      item.Rarity.BackendValue,
                                                                                      item.Images.SmallIcon,
                                                                                      option.SwapModel).ToSaturnOption(),
            "CID_784_Athena_Commando_F_RenegadeRaiderFire" => new BlazeSkinSwap(item.Name,
                                                                                item.Rarity.BackendValue,
                                                                                item.Images.SmallIcon,
                                                                                option.SwapModel).ToSaturnOption(),
            "CID_970_Athena_Commando_F_RenegadeRaiderHoliday" => new GingerbreadRaiderSkinSwap(item.Name,
                                                                                               item.Rarity.BackendValue,
                                                                                               item.Images.SmallIcon,
                                                                                               option.SwapModel).ToSaturnOption(),
            "CID_A_322_Athena_Commando_F_RenegadeRaiderIce" => new PermafrostRaiderSkinSwap(item.Name,
                                                                                            item.Rarity.BackendValue,
                                                                                            item.Images.SmallIcon,
                                                                                            option.SwapModel).ToSaturnOption(),
            "CID_936_Athena_Commando_F_RaiderSilver" => new DiamondDivaSkinSwap(item.Name,
                                                                                item.Rarity.BackendValue,
                                                                                item.Images.SmallIcon,
                                                                                option.SwapModel).ToSaturnOption(),
            "CID_A_007_Athena_Commando_F_StreetFashionEclipse" => new RubyShadowsSkinSwap(item.Name,
                                                                                          item.Rarity.BackendValue,
                                                                                          item.Images.SmallIcon,
                                                                                          option.SwapModel).ToSaturnOption(),
            "CID_A_311_Athena_Commando_F_ScholarFestiveWinter" => new BlizzabelleSkinSwap(item.Name,
                                                                                          item.Rarity.BackendValue,
                                                                                          item.Images.SmallIcon,
                                                                                          option.SwapModel).ToSaturnOption(),
            "CID_294_Athena_Commando_F_RedKnightWinter" => new FrozenRedKnightSkinSwap(item.Name,
                                                                                       item.Rarity.BackendValue,
                                                                                       item.Images.SmallIcon,
                                                                                       option.SwapModel).ToSaturnOption(),
            "CID_231_Athena_Commando_F_RedRiding" => new FableSkinSwap(item.Name,
                                                                       item.Rarity.BackendValue,
                                                                       item.Images.SmallIcon,
                                                                       option.SwapModel).ToSaturnOption(),
            "CID_380_Athena_Commando_F_DarkViking_Fire" => new MoltenValkyrieSkinSwap(item.Name,
                                                                                      item.Rarity.BackendValue,
                                                                                      item.Images.SmallIcon,
                                                                                      option.SwapModel).ToSaturnOption(),
            "CID_029_Athena_Commando_F_Halloween" => new GhoulTrooperSwap(item.Name,
                                                                          item.Rarity.BackendValue,
                                                                          item.Images.SmallIcon,
                                                                          option.SwapModel).ToSaturnOption(),
            "CID_124_Athena_Commando_F_AuroraGlow" => new NiteliteSwap(item.Name,
                                                                       item.Rarity.BackendValue,
                                                                       item.Images.SmallIcon,
                                                                       option.SwapModel).ToSaturnOption(),
            "CID_242_Athena_Commando_F_Bullseye" => new BullseyeSwap(item.Name,
                                                                     item.Rarity.BackendValue,
                                                                     item.Images.SmallIcon,
                                                                     option.SwapModel).ToSaturnOption(),
            "CID_A_420_Athena_Commando_F_NeonGraffitiLava" => new TectonicKomplexSkinSwap(item.Name,
                                                                                          item.Rarity.BackendValue,
                                                                                          item.Images.SmallIcon,
                                                                                          option.SwapModel).ToSaturnOption(),
            "CID_A_203_Athena_Commando_F_PunkKoi" => new CharlotteSkinSwap(item.Name,
                                                                           item.Rarity.BackendValue,
                                                                           item.Images.SmallIcon,
                                                                           option.SwapModel).ToSaturnOption(),
            "CID_A_210_Athena_Commando_F_RenegadeSkull" => new SkeletaraSkinSwap(item.Name,
                                                                                 item.Rarity.BackendValue,
                                                                                 item.Images.SmallIcon,
                                                                                 option.SwapModel).ToSaturnOption(),
            _ => new SaturnOption()
        };
    }
    #endregion

    #region GenerateEmoteSwaps
    private async Task<SaturnOption> GenerateMeshEmote(Cosmetic item, SaturnItem option)
    {
        if (option.Swaps == new Dictionary<string, string>())
        {
            await ItemUtil.UpdateStatus(item, option, $"Failed to find data for \"{item.Id}\"!",
                Colors.C_YELLOW);
            Logger.Log($"Failed to find data for \"{item.Id}\"!", LogLevel.Error);
            return new SaturnOption();
        }

        await _notificationService.Warn(
            "Don't put this emote in your selected emotes! If you are going to use it in-game, favorite the emote and select it from your favorites! Fortnite will kick you if it's in your 6 selections!");

        Logger.Log("CMM: " + option.Swaps["CMM"]);
        Logger.Log("CMF: " + option.Swaps["CMF"]);

        return option.ItemDefinition switch
        {
            "EID_DanceMoves" => new DanceMovesEmoteSwap(item.Name,
                                                        item.Rarity.BackendValue,
                                                        item.Images.SmallIcon,
                                                        option.Swaps).ToSaturnOption(),
            "EID_BoogieDown" => new BoogieDownEmoteSwap(item.Name,
                                                        item.Rarity.BackendValue,
                                                        item.Images.SmallIcon,
                                                        option.Swaps).ToSaturnOption(),
            "EID_Roving" => new LilRoverEmoteSwap(item.Name,
                                                  item.Rarity.BackendValue,
                                                  item.Images.SmallIcon,
                                                  option.Swaps).ToSaturnOption(),
            "EID_Laugh" => new LaughItUpEmoteSwap(item.Name,
                                                  item.Rarity.BackendValue,
                                                  item.Images.SmallIcon,
                                                  option.Swaps).ToSaturnOption(),
            "EID_Saucer" => new LilSaucerEmoteSwap(item.Name,
                                                   item.Rarity.BackendValue,
                                                   item.Images.SmallIcon,
                                                   option.Swaps).ToSaturnOption(),
            "EID_Believer" => new Ska_stra_terrestrialEmoteSwap(item.Name,
                                                                item.Rarity.BackendValue,
                                                                item.Images.SmallIcon,
                                                                option.Swaps).ToSaturnOption(),
            "EID_Custodial" => new CleanSweepEmoteSwap(item.Name,
                                                       item.Rarity.BackendValue,
                                                       item.Images.SmallIcon,
                                                       option.Swaps).ToSaturnOption(),
            "EID_WatchThis" => new ReadyWhenYouAreEmoteSwap(item.Name,
                                                            item.Rarity.BackendValue,
                                                            item.Images.SmallIcon,
                                                            option.Swaps).ToSaturnOption(),
            "EID_Division" => new NailedItEmoteSwap(item.Name,
                                                    item.Rarity.BackendValue,
                                                    item.Images.SmallIcon,
                                                    option.Swaps).ToSaturnOption(),
            "EID_HighActivity" => new KickBackEmoteSwap(item.Name,
                                                        item.Rarity.BackendValue,
                                                        item.Images.SmallIcon,
                                                        option.Swaps).ToSaturnOption(),
            "EID_Terminal" => new VulcanSaluteEmoteSwap(item.Name,
                                                        item.Rarity.BackendValue,
                                                        item.Images.SmallIcon,
                                                        option.Swaps).ToSaturnOption(),
            "EID_WIR" => new HotMaratEmoteSwap(item.Name,
                                               item.Rarity.BackendValue,
                                               item.Images.SmallIcon,
                                               option.Swaps).ToSaturnOption(),
            _ => new SaturnOption()
        };
    }
    #endregion
    
    private async Task<SaturnOption> GenerateMeshPickaxe(Cosmetic item, SaturnItem option)
    {
        Logger.Log("Generating swaps");
        EFortRarity Rarity = (EFortRarity)int.Parse(option.Swaps["Rarity"]);

        return option.ItemDefinition switch
        {
            "Pickaxe_ID_408_MastermindShadow" => new MayhemScytheSwap(item.Name,
                                                                      item.Rarity.Value,
                                                                      item.Images.SmallIcon,
                                                                      option.Swaps,
                                                                      Rarity).ToSaturnOption(),
            "DefaultPickaxe" => new DefaultPickaxeSwap(item.Name,
                                                       item.Rarity.Value,
                                                       item.Images.SmallIcon,
                                                       option.Swaps).ToSaturnOption(),
            "Pickaxe_ID_541_StreetFashionEclipseFemale" => new ShadowSlicerSwap(item.Name,
                                                                                item.Rarity.Value,
                                                                                item.Images.SmallIcon,
                                                                                option.Swaps,
                                                                                Rarity).ToSaturnOption(),
            "Pickaxe_ID_713_GumballMale" => new GumBrawlerSwap(item.Name,
                                                               item.Rarity.Value,
                                                               item.Images.SmallIcon,
                                                               option.Swaps,
                                                               Rarity).ToSaturnOption(),
            "Pickaxe_ID_143_FlintlockWinter" => new FrozenAxeSwap(item.Name,
                                                                  item.Rarity.Value,
                                                                  item.Images.SmallIcon,
                                                                  option.Swaps,
                                                                  Rarity).ToSaturnOption(),
            "Pickaxe_ID_616_InnovatorFemale" => new IOIradicatorSwap(item.Name,
                                                                     item.Rarity.Value,
                                                                     item.Images.SmallIcon,
                                                                     option.Swaps,
                                                                     Rarity).ToSaturnOption(),
            "Pickaxe_ID_671_GhostHunterFemale1H" => new TorinsLightbladeSwap(item.Name,
                                                                             item.Rarity.Value,
                                                                             item.Images.SmallIcon,
                                                                             option.Swaps,
                                                                             Rarity).ToSaturnOption(),
            "Pickaxe_ID_715_LoneWolfMale" => new BladeOfTheWaningMoonSwap(item.Name,
                                                                          item.Rarity.Value,
                                                                          item.Images.SmallIcon,
                                                                          option.Swaps,
                                                                          Rarity).ToSaturnOption(),
            "Pickaxe_ID_508_HistorianMale_6BQSW" => new LeviathanAxeSwap(item.Name,
                                                                         item.Rarity.Value,
                                                                         item.Images.SmallIcon,
                                                                         option.Swaps,
                                                                         Rarity).ToSaturnOption(),
            "Pickaxe_ID_542_TyphoonFemale1H_CTEVQ" => new CombatKnifeSwap(item.Name,
                                                                          item.Rarity.Value,
                                                                          item.Images.SmallIcon,
                                                                          option.Swaps,
                                                                          Rarity).ToSaturnOption(),
            "Pickaxe_ID_457_HightowerSquash1H" => new HandOfLightningSwap(item.Name,
                                                                          item.Rarity.Value,
                                                                          item.Images.SmallIcon,
                                                                          option.Swaps,
                                                                          Rarity).ToSaturnOption(),
            "Pickaxe_ID_463_Elastic1H" => new PhantasmicPulseSwap(item.Name,
                                                                  item.Rarity.Value,
                                                                  item.Images.SmallIcon,
                                                                  option.Swaps,
                                                                  Rarity).ToSaturnOption(),
            "Pickaxe_ID_454_HightowerGrapeMale1H" => new GrootsSapAxesSwap(item.Name,
                                                                           item.Rarity.Value,
                                                                           item.Images.SmallIcon,
                                                                           option.Swaps,
                                                                           Rarity).ToSaturnOption(),
            "Pickaxe_ID_361_HenchmanMale1H" => new HackAndSmashSwap(item.Name,
                                                                    item.Rarity.Value,
                                                                    item.Images.SmallIcon,
                                                                    option.Swaps,
                                                                    Rarity).ToSaturnOption(),
            "Pickaxe_ID_284_CrazyEight1H" => new BankShotsSwap(item.Name,
                                                               item.Rarity.Value,
                                                               item.Images.SmallIcon,
                                                               option.Swaps,
                                                               Rarity).ToSaturnOption(),
            "Pickaxe_ID_334_SweaterWeatherMale" => new SnowySwap(item.Name,
                                                                 item.Rarity.Value,
                                                                 item.Images.SmallIcon,
                                                                 option.Swaps,
                                                                 Rarity).ToSaturnOption(),
            "Pickaxe_ID_568_ObsidianFemale" => new AxetralFormSwap(item.Name,
                                                                   item.Rarity.Value,
                                                                   item.Images.SmallIcon,
                                                                   option.Swaps,
                                                                   Rarity).ToSaturnOption(),
            "Pickaxe_ID_313_ShiitakeShaolinMale" => new CrescentShroomSwap(item.Name,
                                                               item.Rarity.Value,
                                                               item.Images.SmallIcon,
                                                               option.Swaps,
                                                               Rarity).ToSaturnOption(),
            "Pickaxe_ID_545_CrushFemale1H" => new LovestruckStrikerSwap(item.Name,
                                                               item.Rarity.Value,
                                                               item.Images.SmallIcon,
                                                               option.Swaps,
                                                               Rarity).ToSaturnOption(),
            "Pickaxe_ID_480_PoisonFemale" => new ForsakenStrikeSwap(item.Name,
                                                               item.Rarity.Value,
                                                               item.Images.SmallIcon,
                                                               option.Swaps,
                                                               Rarity).ToSaturnOption(),
            "Pickaxe_ID_690_RelishFemale_DC74M" => new HotDoggerSwap(item.Name,
                                                                item.Rarity.Value,
                                                                item.Images.SmallIcon,
                                                                option.Swaps,
                                                                Rarity).ToSaturnOption(),
            "Pickaxe_ID_721_RustyBoltSliceMale_V3A4N" => new ButcherCleaverSwap(item.Name,
                                                                item.Rarity.Value,
                                                                item.Images.SmallIcon,
                                                                option.Swaps,
                                                                Rarity).ToSaturnOption(), 
           /* "807_NeonGraffitiLavaFemale" => new SulfuricStreetShineSwap(item.Name,
                                                                item.Rarity.Value,
                                                                item.Images.SmallIcon,
                                                                option.Swaps,
                                                                Rarity).ToSaturnOption(), */ //Removed, waiting for a fix
            "Pickaxe_ID_612_AntiqueMale" => new ChopChopSwap(item.Name,
                                                             item.Rarity.Value,
                                                             item.Images.SmallIcon,
                                                             option.Swaps,
                                                             Rarity).ToSaturnOption(),
            "Pickaxe_ID_766_BinaryFemale" => new TheImaginedBladePickaxeSwap(item.Name,
                                                                      item.Rarity.Value,
                                                                      item.Images.SmallIcon,
                                                                      option.Swaps,
                                                                      Rarity).ToSaturnOption(),
            "Pickaxe_ID_613_BelieverFemale" => new TheFretBasherSwap(item.Name,
                                                               item.Rarity.Value,
                                                               item.Images.SmallIcon,
                                                               option.Swaps,
                                                               Rarity).ToSaturnOption(),
            _ => new SaturnOption()
            
        };
    }


    public async Task BackupFile(string sourceFile, Cosmetic item, SaturnItem? option = null)
    {
        await ItemUtil.UpdateStatus(item, option, "Backing up files", Colors.C_YELLOW);
        
        await Task.Run(async () =>
        {
            var fileName = Path.GetFileNameWithoutExtension(sourceFile);
            var fileExts = new[]
            {
                ".pak",
                ".sig",
                ".utoc",
                ".ucas"
            };
            
            if (fileName.Contains("ient_s")) // Check if it's partitioned
                        fileName = fileName.Split("ient_s")[0] + "ient"; // Remove the partition from the name because they don't get utocs
            
            foreach (var (fileExt, path) in from fileExt in fileExts
                     let path = Path.Combine(FortniteUtil.PakPath, fileName + fileExt)
                     select (fileExt, path))
            {
                if (sourceFile == "pakchunk0-WindowsClient")
                    Directory.CreateDirectory(Path.Combine(FortniteUtil.PakPath, "Saturn"));
                
                if (!File.Exists(path))
                {
                    Logger.Log($"File \"{fileName + fileExt}\" doesn't exist!", LogLevel.Warning);
                    return;
                }
            
                if (fileExt is ".ucas")
                {
                    for (int i = 0; i < 20; i++)
                    {
                        try
                        {
                            var partitionPath = i > 0 ? string.Concat(fileName, "_s", i, ".ucas") : string.Concat(fileName, ".ucas");
                            partitionPath = Path.Combine(FortniteUtil.PakPath, partitionPath);
                                    
                            if (!File.Exists(partitionPath))
                                break;
                                    
                            if (File.Exists(partitionPath.Replace("WindowsClient", "SaturnClient")))
                            {
                                Logger.Log($"File \"{partitionPath}\" already exists!", LogLevel.Warning);
                                continue;
                            }
                                    
                            var bufferLength = 262144;
                            var readBuffer = new Byte[bufferLength];
                            var writeBuffer = new Byte[bufferLength];
                            var readSize = -1;
            
                            IAsyncResult writeResult;
                            IAsyncResult readResult;
            
                            await using var sourceStream = new FileStream(partitionPath, FileMode.Open, FileAccess.Read);
                            
                            if (sourceFile == "pakchunk0-WindowsClient")
                                partitionPath = Path.GetDirectoryName(partitionPath) + "//Saturn//" + Path.GetFileName(partitionPath);
                            
                            
                            await using (var destinationStream = new FileStream(partitionPath.Replace("WindowsClient", "SaturnClient"), FileMode.Create, FileAccess.Write, FileShare.None, 8, FileOptions.Asynchronous | FileOptions.SequentialScan))
                            {
                                destinationStream.SetLength(sourceStream.Length);
                                readSize = sourceStream.Read(readBuffer, 0, readBuffer.Length);
                                readBuffer = Interlocked.Exchange(ref writeBuffer, readBuffer);
            
                                while (readSize > 0)
                                {
                                    writeResult = destinationStream.BeginWrite(writeBuffer, 0, readSize, null, null);
                                    readResult = sourceStream.BeginRead(readBuffer, 0, readBuffer.Length, null, null);
                                    destinationStream.EndWrite(writeResult);
                                    readSize = sourceStream.EndRead(readResult);
                                    readBuffer = Interlocked.Exchange(ref writeBuffer, readBuffer);
                                }
            
                                sourceStream.Close();
                                destinationStream.Close();
                            }
            
                            Logger.Log($"Successfully copied container part {i} for {fileName}");
                        }
                        catch (Exception e)
                        {
                            Logger.Log($"There was an error copying container part {i} for {fileName}: " + e.ToString(), LogLevel.Error);
                            throw new FileLoadException($"Failed to open container partition {i} for {fileName}", e);
                        }
                    }
                }
                else
                {
                    var newPath = path.Replace("WindowsClient", "SaturnClient");
                    
                    if (sourceFile == "pakchunk0-WindowsClient")
                        newPath = Path.GetDirectoryName(newPath) + "//Saturn//" + Path.GetFileName(newPath);
                    
                    if (File.Exists(newPath))
                    {
                        Logger.Log($"Duplicate for \"{fileName + fileExt}\" already exists!", LogLevel.Warning);
                        continue;
                    }
            
                    await using var source = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    await using var destination = File.Create(newPath);
                    await source.CopyToAsync(destination);
                }
                Logger.Log($"Duplicated file \"{fileName + fileExt}\"");
            }
        });
       
    }

    private bool TryIsB64(ref byte[] data, SaturnAsset asset)
    {
        List<byte[]> Searches = new();
        List<byte[]> Replaces = new();

        try
        {
            if (!asset.ParentAsset.Contains("WID") 
                && !asset.ParentAsset.Contains("Rarity") 
                && !asset.ParentAsset.Contains("ID_") 
                && !asset.ParentAsset.ToLower().Contains("backpack") 
                && !asset.ParentAsset.ToLower().Contains("gameplay")
                && !asset.ParentAsset.ToLower().Contains("defaultgamedatacosmetics")
                && !asset.ParentAsset.ToLower().Contains("prime"))
            {
                Searches.Add(Encoding.ASCII.GetBytes(asset.ParentAsset.Replace(".uasset", "").Replace("FortniteGame/Content/", "/Game/")));
                Replaces.Add(Encoding.ASCII.GetBytes("/"));
            }

            // My hardcoded fixes for assets that oodle doesnt like
            if (asset.ParentAsset.ToLower().Contains("backpack") && asset.ParentAsset.ToLower().Contains("eclipse"))
            {
                Searches.Add(new byte[] { 128, 137, 125, 52, 112, 160, 41, 136, 85, 24, 105, 64, 86, 153, 101, 207, 105, 255, 255, 255, 255, 255, 255, 255, 255, 227, 34, 88, 165, 109, 85 });
                Replaces.Add(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                Searches.Add(new byte[] { 23, 4, 128, 155, 5, 152, 34, 65, 79, 78, 195, 37, 32, 110, 183, 112, 126, 162, 66, 99, 16, 131, 122, 115 });
                Replaces.Add(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                Searches.Add(new byte[] { 67, 117, 115, 116, 111, 109, 67, 104, 97, 114, 97, 99, 116, 101, 114, 66, 97, 99, 107, 112, 97, 99, 107, 68, 97, 116, 97 });
                Replaces.Add(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            }

            foreach (var swap in asset.Swaps)
            {
                if (swap.Search.StartsWith("hex="))
                {
                    swap.Search = System.Convert.ToBase64String(FileUtil.HexToBytes(swap.Search.Substring(4)));
                    swap.Type = SwapType.Base64;
                }
                
                if (swap.Replace.StartsWith("hex="))
                {
                    swap.Replace = System.Convert.ToBase64String(FileUtil.HexToBytes(swap.Replace.Substring(4)));
                    swap.Type = SwapType.Property;
                }


                switch (swap)
                {
                    case { Type: SwapType.Base64 }:
                        Searches.Add(System.Convert.FromBase64String(swap.Search));
                        Replaces.Add(Encoding.ASCII.GetBytes(swap.Replace));
                        break;
                    case { Type: SwapType.Property }:
                        Searches.Add(System.Convert.FromBase64String(swap.Search));
                        Replaces.Add(System.Convert.FromBase64String(swap.Replace));
                        break;
                    default:
                        Searches.Add(Encoding.ASCII.GetBytes(swap.Search));
                        Replaces.Add(Encoding.ASCII.GetBytes(swap.Replace));
                        break;
                }
            }

            if (asset.ParentAsset.Contains("WID"))
                AnyLength.TrySwap(ref data, Searches, Replaces, true);
            else if (asset.ParentAsset.Contains("DefaultGameDataCosmetics") || asset.ParentAsset.Contains("Prime"))
                AnyLength.SwapNormally(Searches, Replaces, ref data);
            else
                AnyLength.TrySwap(ref data, Searches, Replaces);


            var ClassNames = new List<byte[]>()
            {
                new byte[] { 67,117,115,116,111,109,67,104,97,114,97,99,116,101,114,72,101,97,100,68,97,116,97 },
                new byte[] { 67,117,115,116,111,109,67,104,97,114,97,99,116,101,114,72,97,116,68,97,116,97 },
                new byte[] { 70,97,99,101,67,117,115,116,111,109,67,104,97,114,97,99,116,101,114,72,97,116,68,97,116,97 },
                new byte[] { 67,117,115,116,111,109,67,104,97,114,97,99,116,101,114,66,111,100,121,80,97,114,116,68,97,116,97 }
            };

            var NullBytes = new List<byte[]>()
            {
                new byte[] { 0 },
                new byte[] { 0 },
                new byte[] { 0 },
                new byte[] { 0 }
            };

            AnyLength.SwapNormally(ClassNames, NullBytes, ref data);

            return true;
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message, LogLevel.Error);
            return false;
        }
    }

    private bool TryExportAsset(string asset, out byte[] data, bool isUbulk = false)
    {
        data = Array.Empty<byte>();
        try
        {
            Logger.Log($"Saving asset \"{asset.Split('.')[0]}\"");
            if (!_provider.TrySavePackage(asset.Split('.')[0], out var pkg))
            {
                Logger.Log($"Failed to export asset \"{asset}\"!", LogLevel.Warning);
                return false;
            }

            Logger.Log("Getting data");
            data = isUbulk
                ? pkg.FirstOrDefault(x => x.Key.Contains("ubulk")).Value
                : pkg.FirstOrDefault(x => x.Key.Contains("uasset")).Value;

            if (!isUbulk)
            {
                Logger.Log($"UAsset path is {SaturnData.UAssetPath}", LogLevel.Debug);
                File.WriteAllBytes(
                    Path.Combine(Config.CompressedDataPath, $"{Path.GetFileName(SaturnData.UAssetPath)}"),
                    SaturnData.CompressedData);
            }

            Logger.Log($"Successfully exported asset \"{asset}\" and cached compressed data");

            return true;
        }
        catch (Exception e)
        {
            Logger.Log($"Failed to export asset \"{asset}\"! Reason: {e.Message}", LogLevel.Error);
            return false;
        }
    }

    private async Task TrySwapAsset(string path, long offset, byte[] data)
    {
        try
        {
            await using var writer =
                new BinaryWriter(File.Open(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite));
            writer.BaseStream.Seek(offset, SeekOrigin.Begin);
            writer.Write(data);
            writer.Close();

            Logger.Log($"Successfully swapped asset in file {Path.GetFileName(path)}");
        }
        catch (Exception e)
        {
            Logger.Log($"Failed to swap asset in file {Path.GetFileName(path)}! Reason: {e.Message}",
                LogLevel.Error);
        }
    }
    
    private static void FillEnd(ref byte[] buffer, int len) => Array.Resize(ref buffer, len);

    private async Task<string> GetIconFromCID(string id)
    {
        if (Provider.TryLoadObject("FortniteGame/Content/Athena/Items/Cosmetics/Characters/" + id, out var cidObj) 
            && cidObj.TryGetValue(out UObject export, "HeroDefinition") 
            && export.TryGetValue(out FSoftObjectPath SPI, "SmallPreviewImage"))
        {
                    var imageAsset = SPI.AssetPathName.Text.Replace("/Game/", "FortniteGame/Content/").Split('.')[0] + ".uasset";
                    return imageAsset;
        }

        return string.Empty;
    }
}

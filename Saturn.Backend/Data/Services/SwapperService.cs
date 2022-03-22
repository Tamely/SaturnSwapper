#pragma warning disable CA1416, SYSLIB0014 // Disable the warning that says something is deprecated and obsolete

using CUE4Parse;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Objects.UObject;
using Microsoft.JSInterop;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.CloudStorage;
using Saturn.Backend.Data.Models.FortniteAPI;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Utils;
using Saturn.Backend.Data.Utils.FortniteUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CUE4Parse.UE4.Objects.Core.i18N;
using Newtonsoft.Json;
using Saturn.Backend.Data.SwapOptions.Pickaxes;
using Saturn.Backend.Data.SwapOptions.Skins;
using Colors = Saturn.Backend.Data.Enums.Colors;
using Saturn.Backend.Data.SwapOptions.Backblings;
using Saturn.Backend.Data.SwapOptions.Emotes;
using Saturn.Backend.Data.Utils.Swaps;
using Saturn.Backend.Data.Utils.Swaps.Generation;
using System.IO.Compression;
using Saturn.Backend.Data.Models.SaturnAPI;

namespace Saturn.Backend.Data.Services;

public interface ISwapperService
{
    public Task<bool> Convert(Cosmetic item, SaturnItem option, ItemType itemType, bool isAuto = true, bool isRandom = false, Cosmetic random = null);
    public Task<bool> Revert(Cosmetic item, SaturnItem option, ItemType itemType);
    public Task<Dictionary<string, string>> GetCharacterPartsById(string id, Cosmetic? item = null);
    public Task<UObject> GetWIDByID(string id);
    public Task<UObject> GetBackblingCP(string id);
    public Task<List<Cosmetic>> GetSaturnSkins();
    public Task<List<SaturnItem>> GetSkinOptions(Cosmetic item);
    public Task<List<SaturnItem>> GetPickaxeOptions(Cosmetic item);
    public Task<List<SaturnItem>> GetBackblingOptions(Cosmetic item);
    public Task<List<SaturnItem>> GetEmoteOptions(Cosmetic item);
    public Task<List<Cosmetic>> GetSaturnPickaxes();
    public Task<List<Cosmetic>> GetSaturnBackblings();
    public Task<List<Cosmetic>> GetSaturnEmotes();
    public Task Swap(Cosmetic item, SaturnItem option, ItemType itemType, List<Cosmetic> Items, bool isAuto = true);
    public DefaultFileProvider Provider { get; }
}

public sealed class SwapperService : ISwapperService
{
    private readonly IConfigService _configService;
    private readonly IFortniteAPIService _fortniteAPIService;
    private readonly IDiscordRPCService _discordRPCService;

    private readonly ISaturnAPIService _saturnAPIService;
    private readonly ICloudStorageService _cloudStorageService;

    private readonly IJSRuntime _jsRuntime;

    private bool _halted;
    private readonly DefaultFileProvider _provider;

    public SwapperService(IFortniteAPIService fortniteAPIService, ISaturnAPIService saturnAPIService,
        IConfigService configService, ICloudStorageService cloudStorageService, IJSRuntime jsRuntime, IBenBotAPIService benBotApiService, IDiscordRPCService discordRPCService)
    {
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
        keys.AddRange(from x in aes.DynamicKeys
                      select new KeyValuePair<FGuid, FAesKey>(new FGuid(x.PakGuid), new FAesKey(x.Key)));

        Trace.WriteLine("Set Keys");
        _provider.SubmitKeys(keys);
        Trace.WriteLine("Submitted Keys");
        Trace.WriteLine($"File provider initialized with {_provider.Keys.Count} keys");
    }

    public DefaultFileProvider Provider { get => _provider; }
    private async void CreateMappings(IBenBotAPIService benBotApiService, 
                                      IFortniteAPIService fortniteAPIService, 
                                      IJSRuntime jsRuntime) =>
        await new Mappings(_provider, benBotApiService, fortniteAPIService, jsRuntime).Init();

    public async Task<List<Cosmetic>> GetSaturnSkins()
    {
        var skins = new List<Cosmetic>();

        AbstractGeneration Generation = new SkinGeneration(skins, _provider, _configService, this);

        skins = await Generation.Generate();

        Generation.WriteItems(skins);

        Trace.WriteLine($"Deserialized {skins.Count} objects");
        
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

        _discordRPCService.UpdatePresence($"Looking at {skins.Count} different skins");

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
    
    public async Task<List<SaturnItem>> GetPickaxeOptions(Cosmetic item)
    {
        return (await new AddPickaxes().AddPickaxeOptions(item, this, _provider)).CosmeticOptions;
    }
    
    public async Task<List<SaturnItem>> GetEmoteOptions(Cosmetic item)
    {
        return (await new AddEmotes().AddEmoteOptions(item, this, _provider)).CosmeticOptions;
    }
    
    public async Task<List<Cosmetic>> GetSaturnBackblings()
    {
        var backblings = new List<Cosmetic>();

        AbstractGeneration Generation = new BackblingGeneration(backblings, _provider, _configService, this, _jsRuntime);

        backblings = await Generation.Generate();

        Generation.WriteItems(backblings);

        Trace.WriteLine($"Deserialized {backblings.Count} objects");
        
        foreach (var item in backblings)
        {
            if ((await _configService.TryGetConvertedItems()).Any(x => string.Equals(x.Name, item.Name) && string.Equals(x.ItemDefinition, item.Id)))
                item.IsConverted = true;
        }

        _discordRPCService.UpdatePresence($"Looking at {backblings.Count} different backblings");

        return backblings;
    }
    
    public async Task<List<Cosmetic>> GetSaturnPickaxes()
    {
        var pickaxes = new List<Cosmetic>();

        AbstractGeneration Generation = new PickaxeGeneration(pickaxes, _provider, _configService, this);

        pickaxes = await Generation.Generate();

        Generation.WriteItems(pickaxes);

        Trace.WriteLine($"Deserialized {pickaxes.Count} objects");
        
        foreach (var item in pickaxes)
        {
            if ((await _configService.TryGetConvertedItems()).Any(x => string.Equals(x.Name, item.Name) && string.Equals(x.ItemDefinition, item.Id)))
                item.IsConverted = true;
        }

        _discordRPCService.UpdatePresence($"Looking at {pickaxes.Count} different pickaxes");

        return pickaxes;
    }
    
    public async Task<List<Cosmetic>> GetSaturnEmotes()
    {
        var emotes = new List<Cosmetic>();

        AbstractGeneration Generation = new EmoteGeneration(emotes, _provider, _configService, this);

        emotes = await Generation.Generate();

        Generation.WriteItems(emotes);

        Trace.WriteLine($"Deserialized {emotes.Count} objects");
        
        foreach (var item in emotes)
        {
            if ((await _configService.TryGetConvertedItems()).Any(x => string.Equals(x.Name, item.Name) && string.Equals(x.ItemDefinition, item.Id)))
                item.IsConverted = true;
        }

        _discordRPCService.UpdatePresence($"Looking at {emotes.Count} different emotes");

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
                    await _jsRuntime.InvokeVoidAsync("MessageBox", "Some parts of the swapper may be broken!",
                        "Tamely hasn't updated the swapper's offsets to the current Fortnite build. Watch announcements in his server so you are the first to know when he does!",
                        "warning");
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
                        await _jsRuntime.InvokeVoidAsync("MessageBox", "Some parts of the swapper may be broken!",
                            "Tamely hasn't updated the swapper's offsets to the current Fortnite build. Watch announcements in his server so you are the first to know when he does!",
                            "warning");
                    }
                }
                else if (option.Name == "No options!")
                {
                    await _jsRuntime.InvokeVoidAsync("MessageBox", "No options!",
                        $"There are no options for {item.Name}! Can you not read the name and description?",
                        "warning");
                }
                else if (option.Name == "Default")
                {
                    if (Config.isBeta)
                    {
                        if (!await Convert(item, option, itemType, true))
                        {
                            await ItemUtil.UpdateStatus(item, option,
                                $"There was an error converting {item.Name}!",
                                Colors.C_RED);
                            Logger.Log($"There was an error converting {item.Name}!", LogLevel.Error);
                            Process.Start("notepad.exe", Config.LogFile);
                        }
                    }
                    else
                    {
                        await _jsRuntime.InvokeVoidAsync("MessageBox", "This is a BETA feature!",
                            "You have to boost Tamely's server to be able to swap from the default skin due to Saturn using a method no other swapper can offer!",
                            "warning");
                        return;
                    }
                }
                else if (!await Convert(item, option, itemType, false))
                {
                    await ItemUtil.UpdateStatus(item, option,
                        $"There was an error converting {item.Name}!",
                        Colors.C_RED);
                    Logger.Log($"There was an error converting {item.Name}!", LogLevel.Error);
                    Process.Start("notepad.exe", Config.LogFile);
                }
                
                if (Config.isMaintenance)
                {
                    await _jsRuntime.InvokeVoidAsync("MessageBox", "Some parts of the swapper may be broken!",
                        "Tamely hasn't updated the swapper's offsets to the current Fortnite build. Watch announcements in his server so you are the first to know when he does!",
                        "warning");
                }

            }

            _halted = false;
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

            ConvertedItem convItem = new();

            if (isRandom)
            {
                convItem = new ConvertedItem()
                {
                    Name = item.Name,
                    ItemDefinition = random.Id,
                    Type = itemType.ToString(),
                    Swaps = new List<ActiveSwap>()
                };
            }
            else
            {
                convItem = new ConvertedItem()
                {
                    Name = item.Name,
                    ItemDefinition = item.Id,
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

            SaturnOption itemSwap = new();
            if (isDefault)
            {
                itemSwap = await GenerateDefaultSkins(item, option);
            }
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

            try
            {
                var changes = _cloudStorageService.GetChanges(option.ItemDefinition, item.Id);
                cloudChanges = _cloudStorageService.DecodeChanges(changes);

                // Really shouldn't be a list but I'm too lazy to change it right now
                itemSwap.Assets = option.Options[0].Assets;
            }
            catch
            {
                Logger.Log("There was no hotfix found for this item!", LogLevel.Warning);
            }

            if (item.IsCloudAdded)
                itemSwap = option.Options[0];

            if (option.ItemDefinition == "CID_A_272_Athena_Commando_F_Prime")
            {
                var search = "/Game/Athena/Items/Cosmetics/Characters/CID_A_272_Athena_Commando_F_Prime.CID_A_272_Athena_Commando_F_Prime";
                SaturnData.SearchString = search;

                if (!TryExportAsset("FortniteGame/AssetRegistry.bin", out _))
                {
                    Logger.Log("Failed to export asset registry!", LogLevel.Error);
                    return false;
                }

                if (SaturnData.Block.Data == null)
                {
                    Logger.Log("Failed to find string in asset registry!", LogLevel.Error);
                    return false;
                }

                byte[] uncompressed = SaturnData.Block.Data;
                if (!AnyLength.SwapNormally(new List<byte[]>() { Encoding.ASCII.GetBytes(search) }, 
                    new List<byte[]>() { Encoding.ASCII.GetBytes(search.Replace("CID_A_272_Athena_Commando_F_Prime", "CID_A_272_AthenaOWEN_IS_SO_SMART")) },
                    ref uncompressed))
                {
                    Logger.Log("Failed to swap in uncompressed registry block!", LogLevel.Error);
                    return false;
                }

                var mem = new MemoryStream();
                using (ZLibStream zLibStream = new ZLibStream(mem, CompressionMode.Compress))
                {
                    zLibStream.Write(uncompressed, 0, uncompressed.Length);
                }

                byte[] compressed = mem.ToArray();

                await TrySwapAsset(Path.Combine(FortniteUtil.PakPath, "pakchunk0-WindowsClient.pak"), SaturnData.Block.Offset, FillEnd(compressed, SaturnData.Block.CompressedLength));
            }

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
                        "https://cdn.discordapp.com/attachments/770991313490280478/943307827357823007/TamelysDefaultGameData.uasset");
                Logger.Log("Asset exported");
                Logger.Log($"Starting backup of {Path.GetFileName(SaturnData.Path)}");

                var file = SaturnData.Path;

                if (isRandom)
                    await BackupFile(file, random, option);
                else
                    await BackupFile(file, item, option);

                foreach (var swwaps in asset.Swaps)
                {
                    Logger.Log(swwaps.Search + " :: " + swwaps.Replace);
                }

                if (!TryIsB64(ref data, asset))
                    Logger.Log($"Cannot swap/determine if '{asset.ParentAsset}' is Base64 or not!",
                        LogLevel.Fatal);

                var compressed = SaturnData.isCompressed ? Utils.Oodle.Compress(data) : data;

                Directory.CreateDirectory(Config.DecompressedDataPath);
                File.SetAttributes(Config.DecompressedDataPath,
                    FileAttributes.Hidden | FileAttributes.System);
                await File.WriteAllBytesAsync(
                    Config.DecompressedDataPath + Path.GetFileName(asset.ParentAsset).Replace(".uasset", "") + ".uasset", data);


                file = Path.GetFileName(file.Replace("WindowsClient", "SaturnClient"));

                if (isRandom)
                    await ItemUtil.UpdateStatus(random, option, "Adding asset to UCAS", Colors.C_YELLOW);
                else
                    await ItemUtil.UpdateStatus(item, option, "Adding asset to UCAS", Colors.C_YELLOW);

                await TrySwapAsset(Path.Combine(FortniteUtil.PakPath, file), SaturnData.Offset,
                    compressed);

                if (file.Contains("ient_s")) // Check if it's partitioned
                    file = file.Split("ient_s")[0] + "ient"; // Remove the partition from the name because they don't get utocs
                
                file = file.Replace("ucas", "utoc");

                Dictionary<long, byte[]> lengths = new();
                if (!await CustomAssets.TryHandleOffsets(asset, compressed.Length, data.Length, lengths, file, _saturnAPIService))
                    Logger.Log(
                        $"Unable to apply custom offsets to '{asset.ParentAsset}.' Asset might not have custom assets at all!",
                        LogLevel.Error);

                if (isRandom)
                    await ItemUtil.UpdateStatus(random, option, "Adding swap to item's config", Colors.C_YELLOW);
                else
                    await ItemUtil.UpdateStatus(item, option, "Adding swap to item's config", Colors.C_YELLOW);
                convItem.Swaps.Add(new ActiveSwap
                {
                    File = file.Replace("utoc", "ucas"),
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

            sw.Stop();

            if (!await _configService.AddConvertedItem(convItem))
                Logger.Log("Could not add converted item to config!", LogLevel.Error);
            else
                Logger.Log($"Added {item.Name} to converted items list in config.");

            _configService.SaveConfig();
            
            if (sw.Elapsed.Minutes > 1)
                if (isRandom)
                    await ItemUtil.UpdateStatus(random, option, $"Converted in {sw.Elapsed.Minutes} minutes and {sw.Elapsed.Seconds} seconds!",
                        Colors.C_GREEN);
                else
                    await ItemUtil.UpdateStatus(item, option, $"Converted in {sw.Elapsed.Minutes} minutes and {sw.Elapsed.Seconds} seconds!",
                        Colors.C_GREEN);
            else if (sw.Elapsed.Seconds > 1)
                if (isRandom)
                    await ItemUtil.UpdateStatus(random, option, $"Converted in {sw.Elapsed.Seconds} seconds!",
                        Colors.C_GREEN);
                else
                    await ItemUtil.UpdateStatus(item, option, $"Converted in {sw.Elapsed.Seconds} seconds!",
                        Colors.C_GREEN);
            else
            if (isRandom)
                await ItemUtil.UpdateStatus(random, option,
                    $"Converted in {sw.Elapsed.Milliseconds} milliseconds!", Colors.C_GREEN);
            else
                await ItemUtil.UpdateStatus(item, option,
                    $"Converted in {sw.Elapsed.Milliseconds} milliseconds!", Colors.C_GREEN);
            Trace.WriteLine($"Converted in {sw.Elapsed.Seconds} seconds!");
            Logger.Log($"Converted in {sw.Elapsed.Seconds} seconds!");

            if (await _configService.GetConvertedFileCount() > 2)
                await _jsRuntime.InvokeVoidAsync("MessageBox", "You might want to revert the last item you swapped!", "If you go ingame with your currently swapped items, you will be kicked from Fortnite.", "warning");


            return true;
        }
        catch (Exception ex)
        {
            await ItemUtil.UpdateStatus(item, option,
                $"There was an error converting {item.Name}. Please send the log to Tamely on Discord!",
                Colors.C_RED);
            Logger.Log($"There was an error converting {ex.StackTrace}");

            if (ex.StackTrace.Contains("CUE4Parse.UE4.Assets.Exports.PropertyUtil"))
                await _jsRuntime.InvokeVoidAsync("MessageBox", "There was a common error with CUE4Parse that occured.",
                    "Restart the swapper to fix it!", "error");
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
                    if (asset.Lengths != new Dictionary<long, byte[]>())
                        foreach (var (key, value) in asset.Lengths)
                            TrySwapAsset(
                                Path.Combine(FortniteUtil.PakPath, asset.File.Replace("ucas", "utoc")),
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


            if (!await _configService.RemoveConvertedItem(id))
                Logger.Log("There was an error removing the item from the config!", LogLevel.Error);
            _configService.SaveConfig();

            sw.Stop();

            item.IsConverted = false;
            if (sw.Elapsed.Seconds > 1)
                await ItemUtil.UpdateStatus(item, option, $"Reverted in {sw.Elapsed.Seconds} seconds!", Colors.C_GREEN);
            else
                await ItemUtil.UpdateStatus(item, option, $"Reverted in {sw.Elapsed.Milliseconds} milliseconds!",
                    Colors.C_GREEN);

            Logger.Log($"Reverted in {sw.Elapsed.Seconds} seconds!");
            Trace.WriteLine($"Reverted in {sw.Elapsed.Seconds} seconds!");
            return true;
        }
        catch (Exception ex)
        {
            await ItemUtil.UpdateStatus(item, option,
                $"There was an error reverting {item.Name}. Please send the log to Tamely on Discord!",
                Colors.C_RED);
            Logger.Log($"There was an error reverting {ex.StackTrace}");
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
            "BID_319_StreetRacerDriftRemix" => new AtmosphereBackblingswap(item.Name,
                                                                        item.Images.SmallIcon,
                                                                        item.Rarity.BackendValue,
                                                                        option.Swaps).ToSaturnOption(),
            _ => new SaturnOption()
        };
    }
    #endregion

    #region GenerateMeshDefaults
    private async Task<SaturnOption> GenerateDefaultSkins(Cosmetic item, SaturnItem option)
    {
        Logger.Log($"Getting character parts for {item.Name}");
        Logger.Log(Constants.CidPath + item.Id);
        
        var characterParts = Task.Run(() => GetCharacterPartsById(item.Id)).GetAwaiter().GetResult();

        if (characterParts == new Dictionary<string, string>())
            return null;

        string headOrHat = _configService.ConfigFile.HeadOrHatCharacterPart;
        Logger.Log("Hat or head is set to: " + headOrHat);
        if (headOrHat == "Hat" && !characterParts.ContainsKey("Hat"))
            headOrHat = "Face";

        if (headOrHat == "Face" && !characterParts.ContainsKey("Face"))
            headOrHat = "Head";

        if (headOrHat == "Head" && !characterParts.ContainsKey("Head"))
            headOrHat = "Hat";

        if (headOrHat == "Hat" && !characterParts.ContainsKey("Hat"))
            headOrHat = "Face";

        // Fallback, 2 body cps so nothing goes invalid because there isnt a head or hat
        if (headOrHat == "Face" && !characterParts.ContainsKey("Face"))
            headOrHat = "Body";


        Logger.Log("Hat or head is swapping as: " + headOrHat);

        if (characterParts.Count > 2)
            option.Status = "This item might not be perfect!";

        return new DefaultSkinSwap(item.Name,
                                   item.Rarity.BackendValue,
                                   item.Images.SmallIcon,
                                   characterParts,
                                   headOrHat).ToSaturnOption();
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
            "CID_082_Athena_Commando_M_Scavenger" => new RustLordSkinSwap(item.Name,
                                                                          item.Rarity.BackendValue,
                                                                          item.Images.SmallIcon,
                                                                          option.SwapModel).ToSaturnOption(),
            "CID_A_132_Athena_Commando_M_ScavengerFire" => new RoastLordSkinSwap(item.Name,
                                                                                 item.Rarity.BackendValue,
                                                                                 item.Images.SmallIcon,
                                                                                 option.SwapModel).ToSaturnOption(),
            "CID_380_Athena_Commando_F_DarkViking_Fire" => new MoltenValkyrieSkinSwap(item.Name,
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

        await _jsRuntime.InvokeVoidAsync("MessageBox", "Don't put this emote in your selected emotes!",
            "If you are going to use it in-game, favorite the emote and select it from your favorites! Fortnite will kick you if it's in your 6 selections!",
            "warning");

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
        
        List<byte[]> SeriesBytes = new List<byte[]>();

        switch (option.Name)
        {
            case "Default Pickaxe":
                break;
            default:
                if (option.Swaps["Series"] != "/" && await _configService.TryGetShouldSeriesConvert())
                {
                    Logger.Log(option.Swaps["Series"]);
                    Rarity = EFortRarity.Transcendent;
                    SeriesBytes = await FileUtil.GetColorsFromSeries(option.Swaps["Series"], _provider);
                }
                break;
        }

        var output = option.ItemDefinition switch
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
            _ => new SaturnOption()
            
        };

        #region Default Pickaxe Rarity and Series Swaps

        if (SeriesBytes.Count > 0 && await _configService.TryGetShouldSeriesConvert() && option.ItemDefinition != "DefaultPickaxe")
        {
            output.Assets.Add(
                new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Balance/RarityData",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[]
                            { 0, 0, 0, 0, 34, 84, 53, 63, 186, 245, 118, 63, 0, 0, 128, 63 }),
                        Replace = System.Convert.ToBase64String(SeriesBytes[0]),
                        Type = SwapType.Property
                    },
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[]
                            { 0, 0, 0, 0, 0, 0, 128, 63, 251, 121, 211, 62, 0, 0, 128, 63 }),
                        Replace = System.Convert.ToBase64String(SeriesBytes[1]),
                        Type = SwapType.Property
                    },
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[]
                            { 19, 129, 58, 62, 254, 95, 5, 63, 0, 0, 128, 63, 0, 0, 128, 63 }),
                        Replace = System.Convert.ToBase64String(SeriesBytes[2]),
                        Type = SwapType.Property
                    },
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[]
                            { 143, 170, 22, 62, 18, 192, 77, 61, 54, 32, 130, 62, 0, 0, 128, 63 }),
                        Replace = System.Convert.ToBase64String(SeriesBytes[3]),
                        Type = SwapType.Property
                    },
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[]
                            { 20, 151, 35, 61, 35, 75, 102, 60, 10, 215, 35, 61, 0, 0, 128, 63 }),
                        Replace = System.Convert.ToBase64String(SeriesBytes[4]),
                        Type = SwapType.Property
                    }
                }
            });
        }
        else if (SeriesBytes.Count > 0 && await _configService.TryGetShouldSeriesConvert() &&
                 option.ItemDefinition == "DefaultPickaxe")
        {
            output.Assets.RemoveAll(x => x.ParentAsset == "FortniteGame/Content/Balance/RarityData");
            option.Status = !string.IsNullOrEmpty(option.Status) ? $"All common items are going to be a series and {option.Status}" : "All common items are going to be a series";
            output.Assets.Add(
                new SaturnAsset()
                {
                    ParentAsset = "FortniteGame/Content/Balance/RarityData",
                    Swaps = new List<SaturnSwap>()
                    {
                        new SaturnSwap()
                        {
                            Search = System.Convert.ToBase64String(new byte[]
                                { 152, 161, 49, 63, 152, 161, 49, 63, 152, 161, 49, 63, 0, 0, 128, 63 }),
                            Replace = System.Convert.ToBase64String(SeriesBytes[0]),
                            Type = SwapType.Property
                        },
                        new SaturnSwap()
                        {
                            Search = System.Convert.ToBase64String(new byte[]
                                { 168, 114, 242, 62, 254, 95, 5, 63, 95, 239, 14, 63, 0, 0, 128, 63 }),
                            Replace = System.Convert.ToBase64String(SeriesBytes[1]),
                            Type = SwapType.Property
                        },
                        new SaturnSwap()
                        {
                            Search = System.Convert.ToBase64String(new byte[]
                                { 170, 214, 50, 62, 241, 73, 71, 62, 170, 10, 93, 62, 0, 0, 128, 63 }),
                            Replace = System.Convert.ToBase64String(SeriesBytes[2]),
                            Type = SwapType.Property
                        },
                        new SaturnSwap()
                        {
                            Search = System.Convert.ToBase64String(new byte[]
                                { 113, 255, 81, 61, 22, 221, 122, 61, 130, 253, 151, 61, 0, 0, 128, 63 }),
                            Replace = System.Convert.ToBase64String(SeriesBytes[3]),
                            Type = SwapType.Property
                        },
                        new SaturnSwap()
                        {
                            Search = System.Convert.ToBase64String(new byte[]
                                { 30, 53, 166, 60, 120, 211, 173, 60, 26, 168, 12, 61, 0, 0, 128, 63 }),
                            Replace = System.Convert.ToBase64String(SeriesBytes[4]),
                            Type = SwapType.Property
                        }
                    }
                });
        }
        else if (option.ItemDefinition == "DefaultPickaxe" && await _configService.TryGetShouldRarityConvert())
        {
            output.Assets.RemoveAll(x => x.ParentAsset == "FortniteGame/Content/Balance/RarityData");
            option.Status = !string.IsNullOrEmpty(option.Status) ? $"All common items are going to be {Rarity} and {option.Status}" : $"All common items are going to be {Rarity}";
            output.Assets.Add(
                Rarity switch
                {
                    EFortRarity.Uncommon => new SaturnAsset()
                    {
                        ParentAsset = "FortniteGame/Content/Balance/RarityData",
                        Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {152,161,49,63,152,161,49,63,152,161,49,63,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {16,122,182,62,220,184,125,63,0,0,0,0,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {168,114,242,62,254,95,5,63,95,239,14,63,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {87,208,244,61,254,95,5,63,0,0,0,0,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {170,214,50,62,241,73,71,62,170,10,93,62,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {0,0,0,0,170,10,93,62,161,247,198,58,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {113,255,81,61,22,221,122,61,130,253,151,61,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {21,31,31,58,129,32,160,61,45,208,110,58,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {30,53,166,60,120,211,173,60,26,168,12,61,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {21,31,31,58,28,153,7,61,21,31,31,58,0,0,128,63}),
                                Type = SwapType.Property
                            }
                        }
                    },
                    EFortRarity.Rare => new SaturnAsset()
                    {
                        ParentAsset = "FortniteGame/Content/Balance/RarityData",
                        Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {152,161,49,63,152,161,49,63,152,161,49,63,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {0,0,0,0,0,0,128,63,169,245,118,63,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {168,114,242,62,254,95,5,63,95,239,14,63,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {0,0,0,0,65,125,219,62,0,0,128,63,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {170,214,50,62,241,73,71,62,170,10,93,62,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {0,0,0,0,177,219,199,61,254,95,5,63,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {113,255,81,61,22,221,122,61,130,253,151,61,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {0,0,0,0,206,193,115,61,54,32,130,62,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {30,53,166,60,120,211,173,60,26,168,12,61,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {41,63,41,60,92,171,189,60,125,150,39,61,0,0,128,63}),
                                Type = SwapType.Property
                            }
                        }
                    },
                    EFortRarity.Epic => new SaturnAsset()
                    {
                        ParentAsset = "FortniteGame/Content/Balance/RarityData",
                        Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {152,161,49,63,152,161,49,63,152,161,49,63,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {207,47,86,63,10,215,163,60,0,0,128,63,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {168,114,242,62,254,95,5,63,95,239,14,63,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {71,1,30,63,217,151,204,61,0,0,128,63,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {170,214,50,62,241,73,71,62,170,10,93,62,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {9,166,58,62,190,79,213,60,160,166,38,63,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {113,255,81,61,22,221,122,61,130,253,151,61,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {88,3,148,61,212,68,31,60,154,210,74,62,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {30,53,166,60,120,211,173,60,26,168,12,61,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {94,247,214,60,135,166,108,60,125,150,39,61,0,0,128,63}),
                                Type = SwapType.Property
                            }
                        }
                    },
                    EFortRarity.Legendary => new SaturnAsset()
                    {
                        ParentAsset = "FortniteGame/Content/Balance/RarityData",
                        Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {152,161,49,63,152,161,49,63,152,161,49,63,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {186,245,118,63,68,136,11,63,56,107,0,62,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {168,114,242,62,254,95,5,63,95,239,14,63,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {0,0,128,63,10,215,131,62,10,215,35,60,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {170,214,50,62,241,73,71,62,170,10,93,62,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {254,95,5,63,149,12,160,61,0,0,0,0,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {113,255,81,61,22,221,122,61,130,253,151,61,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {54,32,130,62,16,233,55,61,55,53,80,60,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {30,53,166,60,120,211,173,60,26,168,12,61,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {10,215,35,61,20,63,70,60,188,116,19,60,0,0,128,63}),
                                Type = SwapType.Property
                            }
                        }
                    },
                    EFortRarity.Mythic => new SaturnAsset()
                    {
                        ParentAsset = "FortniteGame/Content/Balance/RarityData",
                        Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {152,161,49,63,152,161,49,63,152,161,49,63,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {0,0,128,63,232,17,95,63,41,61,195,62,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {168,114,242,62,254,95,5,63,95,239,14,63,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {242,149,72,63,169,48,18,63,16,120,160,61,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {170,214,50,62,241,73,71,62,170,10,93,62,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {254,95,5,63,2,99,149,62,213,174,137,60,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {113,255,81,61,22,221,122,61,130,253,151,61,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {205,204,76,62,235,224,224,61,245,160,160,60,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {30,53,166,60,120,211,173,60,26,168,12,61,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {125,150,39,61,248,84,206,60,41,63,41,60,0,0,128,63}),
                                Type = SwapType.Property
                            }
                        }
                    },
                    EFortRarity.Transcendent or EFortRarity.Unattainable => new SaturnAsset()
                    {
                        ParentAsset = "FortniteGame/Content/Balance/RarityData",
                        Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {152,161,49,63,152,161,49,63,152,161,49,63,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {0,0,0,0,34,84,53,63,186,245,118,63,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {168,114,242,62,254,95,5,63,95,239,14,63,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {0,0,0,0,0,0,128,63,251,121,211,62,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {170,214,50,62,241,73,71,62,170,10,93,62,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {19,129,58,62,254,95,5,63,0,0,128,63,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {113,255,81,61,22,221,122,61,130,253,151,61,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {143,170,22,62,18,192,77,61,54,32,130,62,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {30,53,166,60,120,211,173,60,26,168,12,61,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {20,151,35,61,35,75,102,60,10,215,35,61,0,0,128,63}),
                                Type = SwapType.Property
                            }
                        }
                    },
                    EFortRarity.NumRarityValues or EFortRarity.EFortRarity_MAX => new SaturnAsset()
                    {
                        ParentAsset = "FortniteGame/Content/Balance/RarityData",
                        Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {152,161,49,63,152,161,49,63,152,161,49,63,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {95,11,186,61,35,134,21,63,177,54,198,59,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {168,114,242,62,254,95,5,63,95,239,14,63,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {95,11,186,61,35,134,21,63,177,54,198,59,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {170,214,50,62,241,73,71,62,170,10,93,62,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {95,11,186,61,35,134,21,63,177,54,198,59,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {113,255,81,61,22,221,122,61,130,253,151,61,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {95,11,186,61,35,134,21,63,177,54,198,59,0,0,128,63}),
                                Type = SwapType.Property
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {30,53,166,60,120,211,173,60,26,168,12,61,0,0,128,63}),
                                Replace = System.Convert.ToBase64String(new byte[] {95,11,186,61,35,134,21,63,177,54,198,59,0,0,128,63}),
                                Type = SwapType.Property
                            }
                        }
                    },
                    _ => throw new Exception("Rarity not found")
                });
        }

        #endregion

        return output;
    }

    private async Task BackupFile(string sourceFile, Cosmetic item, SaturnItem option)
    {
        await ItemUtil.UpdateStatus(item, option, "Backing up files", Colors.C_YELLOW);
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
                        var paritionPath = i > 0 ? string.Concat(fileName, "_s", i, ".ucas") : string.Concat(fileName, ".ucas");
                        paritionPath = Path.Combine(FortniteUtil.PakPath, paritionPath);
                        
                        if (!File.Exists(paritionPath))
                            break;
                        
                        if (File.Exists(paritionPath.Replace("WindowsClient", "SaturnClient")))
                        {
                            Logger.Log($"File \"{paritionPath}\" already exists!", LogLevel.Warning);
                            continue;
                        }
                        
                        await using var paritionSource = File.Open(paritionPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        await using var paritionDestination = File.Create(paritionPath.Replace("WindowsClient", "SaturnClient"));
                        await paritionSource.CopyToAsync(paritionDestination);
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
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message, LogLevel.Error);
            return false;
        }
    }

    private bool TryExportAsset(string asset, out byte[] data)
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
            data = pkg.FirstOrDefault(x => x.Key.Contains("uasset")).Value;

            Logger.Log($"UAsset path is {SaturnData.UAssetPath}", LogLevel.Debug);
            File.WriteAllBytes(
                Path.Combine(Config.CompressedDataPath, $"{Path.GetFileName(SaturnData.UAssetPath)}"),
                SaturnData.CompressedData);

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

    private byte[] FillEnd(byte[] buffer, int len)
    {
        List<byte> result = new List<byte>(buffer);
        result.AddRange(Enumerable.Repeat((byte)0, len - buffer.Length));
        return result.ToArray();
    }
}

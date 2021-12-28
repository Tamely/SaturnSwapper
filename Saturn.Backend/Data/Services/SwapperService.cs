using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CUE4Parse;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Assets.Exports.Material.Parameters;
using CUE4Parse.UE4.Objects.Core.Misc;
using Microsoft.AspNetCore.Components.Web;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.CloudStorage;
using Saturn.Backend.Data.Models.FortniteAPI;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Models.SaturnAPI;
using Saturn.Backend.Data.Utils;
using Saturn.Backend.Data.Utils.FortniteUtils;
using Serilog;

namespace Saturn.Backend.Data.Services
{
    public interface ISwapperService
    {
        public Task<bool> Convert(Cosmetic item, SaturnItem option, ItemType itemType, bool isAuto = true);
        public Task<bool> Revert(Cosmetic item, SaturnItem option, ItemType itemType);
        public Task Swap(Cosmetic item, SaturnItem option, ItemType itemType, bool isAuto = true);
    }

    public class SwapperService : ISwapperService
    {
        private readonly IConfigService _configService;
        private readonly IFortniteAPIService _fortniteAPIService;

        private readonly ISaturnAPIService _saturnAPIService;
        private readonly ICloudStorageService _cloudStorageService;

        private bool _halted;
        private readonly DefaultFileProvider _provider;


        public SwapperService(IFortniteAPIService fortniteAPIService, ISaturnAPIService saturnAPIService,
            IConfigService configService, ICloudStorageService cloudStorageService)
        {
            _fortniteAPIService = fortniteAPIService;
            _saturnAPIService = saturnAPIService;
            _configService = configService;
            _cloudStorageService = cloudStorageService;

            var _aes = _fortniteAPIService.GetAES();

            Trace.WriteLine("Got AES");

            _provider = new DefaultFileProvider(FortniteUtil.PakPath, SearchOption.TopDirectoryOnly, false, new CUE4Parse.UE4.Versions.VersionContainer(CUE4Parse.UE4.Versions.EGame.GAME_UE5_LATEST));
            _provider.Initialize();
            Trace.WriteLine("Initialized provider");


            var keys = new List<KeyValuePair<FGuid, FAesKey>>();
            if (_aes.MainKey != null)
                keys.Add(new(new FGuid(), new FAesKey(_aes.MainKey)));
            keys.AddRange(_aes.DynamicKeys.Select(x =>
                new KeyValuePair<FGuid, FAesKey>(new FGuid(x.PakGuid), new FAesKey(x.Key))));

            Trace.WriteLine("Set Keys");
            _provider.SubmitKeys(keys);
            Trace.WriteLine("Submitted Keys");
            Trace.WriteLine($"File provider initialized with {_provider.Keys.Count} keys");
        }

        public async Task Swap(Cosmetic item, SaturnItem option, ItemType itemType, bool isAuto = true)
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
                    }
                        
                }
                else
                {
                    Logger.Log("Item is not converted! Converting!");
                    if (!await Convert(item, option, itemType, isAuto))
                    {
                        await ItemUtil.UpdateStatus(item, option,
                        $"There was an error converting {item.Name}!",
                        Colors.C_RED);
                        Logger.Log($"There was an error converting {item.Name}!", LogLevel.Error);
                    }    
                        
                }

                _halted = false;
            }
        }

        public async Task<bool> Convert(Cosmetic item, SaturnItem option, ItemType itemType, bool isDefault = true)
        {
            try
            {
                var itemCheck = await IsTypeConverted(itemType);
                if (itemCheck != null)
                {
                    await ItemUtil.UpdateStatus(item, option,
                        $"You already have {itemCheck} converted! Revert it before converting another item of the same type.",
                        Colors.C_RED);
                    return false;
                }

                var sw = Stopwatch.StartNew();

                await ItemUtil.UpdateStatus(item, option, "Starting...");

                ConvertedItem convItem = new()
                {
                    Name = item.Name,
                    ItemDefinition = item.Id,
                    Type = itemType.ToString(),
                    Swaps = new List<ActiveSwap>()
                };

                await ItemUtil.UpdateStatus(item, option, "Checking item type");
                Changes cloudChanges = new();

                if (isDefault)
                    switch (itemType)
                    {
                        case ItemType.IT_Skin:

                            #region AutoSkins

                            await ItemUtil.UpdateStatus(item, option, "Generating swaps", Colors.C_YELLOW);
                            var skin = await GenerateSwaps(item);

                            foreach (var asset in skin.Assets)
                            {
                                Directory.CreateDirectory(Config.CompressedDataPath);
                                await ItemUtil.UpdateStatus(item, option, "Exporting asset", Colors.C_YELLOW);
                                if (!TryExportAsset(asset.ParentAsset, out var data))
                                {
                                    Logger.Log($"Failed to export \"{asset.ParentAsset}\"!", LogLevel.Error);
                                    return false;
                                }

                                var file = SaturnData.Path.Replace("utoc", "ucas");

                                await BackupFile(file, item, option);

                                
                                if (asset.ParentAsset.ToLower().Contains("defaultgamedatacosmetics"))
                                    data = new WebClient().DownloadData(new Uri(
                                        await _saturnAPIService.GetDownloadUrl(
                                            Path.GetFileNameWithoutExtension(asset.ParentAsset))));

                                try
                                {
                                    var changes = _cloudStorageService.GetChanges(Path.GetFileNameWithoutExtension(asset.ParentAsset), item.Id);
                                    cloudChanges = _cloudStorageService.DecodeChanges(changes);
                                }
                                catch
                                {
                                    Logger.Log("There was no hotfix found for this item!", LogLevel.Warning);
                                }

                                if (cloudChanges.SkinName == skin.Name)
                                {
                                    if (cloudChanges.Searches[0] != "none")
                                    {
                                        Trace.WriteLine("Searches are not empty");
                                        skin.Assets[skin.Assets.IndexOf(asset)].Swaps = new();
                                        foreach (var search in cloudChanges.Searches)
                                        {
                                            skin.Assets[skin.Assets.IndexOf(asset)].Swaps[cloudChanges.Searches.IndexOf(search)].Search = search;
                                            skin.Assets[skin.Assets.IndexOf(asset)].Swaps[cloudChanges.Searches.IndexOf(search)].Replace = cloudChanges.Replaces[cloudChanges.Searches.IndexOf(search)];
                                        }
                                    }
                                    
                                    if (cloudChanges.CharacterParts[0] != "none")
                                    {
                                        Trace.WriteLine("CharacterParts are not empty");
                                        foreach (var swap in skin.Assets[skin.Assets.IndexOf(asset)].Swaps)
                                            swap.Replace = "/Game/Tamely";
                                        foreach (var characterPart in cloudChanges.CharacterParts)
                                            skin.Assets[skin.Assets.IndexOf(asset)].Swaps[cloudChanges.CharacterParts.IndexOf(characterPart)].Replace = characterPart;
                                    }
                                        
                                }

                                cloudChanges = new();
                                

                                if (!TryIsB64(ref data, asset))
                                    Logger.Log($"Cannot swap/determine if '{asset.ParentAsset}' is Base64 or not!",
                                        LogLevel.Fatal);
                                
                                var compressed = SaturnData.isCompressed ? Oodle.Compress(data) : data;

                                Directory.CreateDirectory(Config.DecompressedDataPath);
                                File.SetAttributes(Config.DecompressedDataPath,
                                    FileAttributes.Hidden | FileAttributes.System);
                                await File.WriteAllBytesAsync(
                                    Config.DecompressedDataPath + Path.GetFileName(asset.ParentAsset).Replace(".uasset", "") + ".uasset", data);


                                file = file.Replace("WindowsClient", "SaturnClient");

                                await ItemUtil.UpdateStatus(item, option, "Adding asset to UCAS", Colors.C_YELLOW);

                                await TrySwapAsset(Path.Combine(FortniteUtil.PakPath, file), SaturnData.Offset,
                                    compressed);

                                file = file.Replace("ucas", "utoc");

                                await ItemUtil.UpdateStatus(item, option, "Checking for customs", Colors.C_YELLOW);
                                Dictionary<long, byte[]> lengths = new();
                                if (!await CustomAssets.TryHandleOffsets(asset, compressed.Length, data.Length, lengths, file, _saturnAPIService))
                                    Logger.Log(
                                        $"Unable to apply custom assets to '{asset.ParentAsset}.' Asset might not have custom assets at all!",
                                        LogLevel.Error);

                                await ItemUtil.UpdateStatus(item, option, "Adding swap to item's config", Colors.C_YELLOW);
                                convItem.Swaps.Add(new ActiveSwap
                                {
                                    File = file.Replace("utoc", "ucas"),
                                    Offset = SaturnData.Offset,
                                    ParentAsset = asset.ParentAsset,
                                    Lengths = lengths
                                });
                            }

                            item.IsConverted = true;

                            sw.Stop();

                            if (!await _configService.AddConvertedItem(convItem))
                                Logger.Log("Could not add converted item to config!", LogLevel.Error);
                            else
                                Logger.Log($"Added {item.Name} to converted items list in config.");

                            _configService.SaveConfig();

                            if (sw.Elapsed.Seconds > 1)
                                await ItemUtil.UpdateStatus(item, option, $"Converted in {sw.Elapsed.Seconds} seconds!",
                                    Colors.C_GREEN);
                            else
                                await ItemUtil.UpdateStatus(item, option,
                                    $"Converted in {sw.Elapsed.Milliseconds} milliseconds!", Colors.C_GREEN);
                            Trace.WriteLine($"Converted in {sw.Elapsed.Seconds} seconds!");
                            Logger.Log($"Converted in {sw.Elapsed.Seconds} seconds!");


                            break;

                        #endregion

                        case ItemType.IT_Backbling:
                            itemCheck = await IsSkinStillConverted();
                            if (itemCheck == null)
                            {
                                await ItemUtil.UpdateStatus(item, option,
                                    "You need to have a skin converted before you can add a backpack to it!",
                                    Colors.C_RED);
                                return false;
                            }

                            #region AutoBackblings

                            await ItemUtil.UpdateStatus(item, option, "Generating swaps", Colors.C_YELLOW);
                            var backbling = await GenerateBackbling(item, option);

                            foreach (var asset in backbling.Assets)
                            {
                                await ItemUtil.UpdateStatus(item, option, "Exporting asset", Colors.C_YELLOW);
                                var decompressedData = await File.ReadAllBytesAsync(Config.DecompressedDataPath +
                                    "DefaultGameDataCosmetics.uasset");

                                if (!TryIsB64(ref decompressedData, asset))
                                    Logger.Log($"Cannot swap/determine if '{asset.ParentAsset}' is Base64 or not!",
                                        LogLevel.Fatal);

                                var compressedData = Oodle.Compress(decompressedData);

                                var keyValuePair = await GetFileNameAndOffsetFromConvertedItems(backbling);
                                var file = keyValuePair.Keys.FirstOrDefault();
                                var offset = keyValuePair.Values.FirstOrDefault();

                                await ItemUtil.UpdateStatus(item, option, "Adding asset to UCAS", Colors.C_YELLOW);

                                await TrySwapAsset(Path.Combine(FortniteUtil.PakPath, file), offset, compressedData);

                                file = file.Replace("ucas", "utoc");

                                await ItemUtil.UpdateStatus(item, option, "Checking for customs", Colors.C_YELLOW);
                                Dictionary<long, byte[]> lengths = new();
                                if (!await CustomAssets.TryHandleOffsets(asset, compressedData.Length, decompressedData.Length,
                                    lengths, file, _saturnAPIService))
                                    Logger.Log(
                                        $"Unable to apply custom assets to '{asset.ParentAsset}.' Asset might not have custom assets at all!",
                                        LogLevel.Error);

                                await ItemUtil.UpdateStatus(item, option, "Adding swap to item's config", Colors.C_YELLOW);
                                convItem.Swaps.Add(new ActiveSwap
                                {
                                    File = file.Replace("utoc", "ucas"),
                                    Offset = offset,
                                    ParentAsset = asset.ParentAsset,
                                    Lengths = lengths
                                });
                            }

                            item.IsConverted = true;

                            sw.Stop();

                            if (!await _configService.AddConvertedItem(convItem))
                                Logger.Log("Could not add converted item to config!", LogLevel.Error);
                            else
                                Logger.Log($"Added {item.Name} to converted items list in config.");

                            _configService.SaveConfig();

                            if (sw.Elapsed.Seconds > 1)
                                await ItemUtil.UpdateStatus(item, option, $"Converted in {sw.Elapsed.Seconds} seconds!",
                                    Colors.C_GREEN);
                            else
                                await ItemUtil.UpdateStatus(item, option,
                                    $"Converted in {sw.Elapsed.Milliseconds} milliseconds!", Colors.C_GREEN);
                            Trace.WriteLine($"Converted in {sw.Elapsed.Seconds} seconds!");
                            Logger.Log($"Converted in {sw.Elapsed.Seconds} seconds!");

                            break;

                        #endregion
                        
                        case ItemType.IT_Dance:
                            itemCheck = await IsTypeConverted(ItemType.IT_Dance);
                            if (itemCheck != null)
                            {
                                await ItemUtil.UpdateStatus(item, option,
                                    $"You need to revert {itemCheck} before you can swap {item.Name}!",
                                    Colors.C_RED);
                                return false;
                            }

                            var emote = await GenerateEmote(item, option);
                            
                            foreach (var asset in emote.Assets)
                            {
                                Directory.CreateDirectory(Config.CompressedDataPath);
                                await ItemUtil.UpdateStatus(item, option, "Exporting asset", Colors.C_YELLOW);
                                if (!TryExportAsset(asset.ParentAsset, out var data))
                                {
                                    Logger.Log($"Failed to export \"{asset.ParentAsset}\"!", LogLevel.Error);
                                    return false;
                                }

                                var file = SaturnData.Path.Replace("utoc", "ucas");

                                await BackupFile(file, item, option);


                                try
                                {
                                    data = new WebClient().DownloadData(new Uri(
                                        await _saturnAPIService.GetDownloadUrl(
                                            Path.GetFileNameWithoutExtension(asset.ParentAsset))));
                                }
                                catch
                                {
                                    Logger.Log("There was no custom asset found for this item!", LogLevel.Warning);
                                }

                                try
                                {
                                    var changes = _cloudStorageService.GetChanges(Path.GetFileNameWithoutExtension(asset.ParentAsset), item.Id);
                                    cloudChanges = _cloudStorageService.DecodeChanges(changes);
                                }
                                catch
                                {
                                    Logger.Log("There was no hotfix found for this item!", LogLevel.Warning);
                                }

                                if (cloudChanges.SkinName == emote.Name)
                                {
                                    if (cloudChanges.Searches[0] != "none")
                                    {
                                        Trace.WriteLine("Searches are not empty");
                                        emote.Assets[emote.Assets.IndexOf(asset)].Swaps = new();
                                        foreach (var search in cloudChanges.Searches)
                                        {
                                            emote.Assets[emote.Assets.IndexOf(asset)].Swaps[cloudChanges.Searches.IndexOf(search)].Search = search;
                                            emote.Assets[emote.Assets.IndexOf(asset)].Swaps[cloudChanges.Searches.IndexOf(search)].Replace = cloudChanges.Replaces[cloudChanges.Searches.IndexOf(search)];
                                        }
                                    }
                                }

                                cloudChanges = new();
                                

                                if (!TryIsB64(ref data, asset))
                                    Logger.Log($"Cannot swap/determine if '{asset.ParentAsset}' is Base64 or not!",
                                        LogLevel.Fatal);

                                var compressed = Oodle.Compress(data);


                                file = file.Replace("WindowsClient", "SaturnClient");

                                await ItemUtil.UpdateStatus(item, option, "Adding asset to UCAS", Colors.C_YELLOW);

                                await TrySwapAsset(Path.Combine(FortniteUtil.PakPath, file), SaturnData.Offset,
                                    compressed);

                                file = file.Replace("ucas", "utoc");

                                await ItemUtil.UpdateStatus(item, option, "Checking for customs", Colors.C_YELLOW);
                                Dictionary<long, byte[]> lengths = new();
                                if (!await CustomAssets.TryHandleOffsets(asset, compressed.Length, data.Length, lengths, file, _saturnAPIService))
                                    Logger.Log(
                                        $"Unable to apply custom assets to '{asset.ParentAsset}.' Asset might not have custom assets at all!",
                                        LogLevel.Error);

                                await ItemUtil.UpdateStatus(item, option, "Adding swap to item's config", Colors.C_YELLOW);
                                convItem.Swaps.Add(new ActiveSwap
                                {
                                    File = file.Replace("utoc", "ucas"),
                                    Offset = SaturnData.Offset,
                                    ParentAsset = asset.ParentAsset,
                                    Lengths = lengths
                                });
                            }

                            item.IsConverted = true;

                            sw.Stop();

                            if (!await _configService.AddConvertedItem(convItem))
                                Logger.Log("Could not add converted item to config!", LogLevel.Error);
                            else
                                Logger.Log($"Added {item.Name} to converted items list in config.");

                            _configService.SaveConfig();

                            if (sw.Elapsed.Seconds > 1)
                                await ItemUtil.UpdateStatus(item, option, $"Converted in {sw.Elapsed.Seconds} seconds!",
                                    Colors.C_GREEN);
                            else
                                await ItemUtil.UpdateStatus(item, option,
                                    $"Converted in {sw.Elapsed.Milliseconds} milliseconds!", Colors.C_GREEN);
                            Trace.WriteLine($"Converted in {sw.Elapsed.Seconds} seconds!");
                            Logger.Log($"Converted in {sw.Elapsed.Seconds} seconds!");

                            break;
                    }
                else
                    switch (itemType)
                    {
                        case ItemType.IT_Skin:

                            #region AutoSkins

                            await ItemUtil.UpdateStatus(item, option, "Generating swaps", Colors.C_YELLOW);
                            Logger.Log("Generating swaps...");
                            var skin = await GenerateMeshSkins(item, option);

                            Logger.Log($"There are {skin.Assets.Count} assets to swap...", LogLevel.Info);
                            foreach (var asset in skin.Assets)
                            {
                                Logger.Log($"Starting swaps for {asset.ParentAsset}");
                                Directory.CreateDirectory(Config.CompressedDataPath);
                                await ItemUtil.UpdateStatus(item, option, "Exporting asset", Colors.C_YELLOW);
                                Logger.Log("Exporting asset");
                                if (!TryExportAsset(asset.ParentAsset, out var data))
                                {
                                    Logger.Log($"Failed to export \"{asset.ParentAsset}\"!", LogLevel.Error);
                                    return false;
                                }
                                Logger.Log("Asset exported");
                                Logger.Log($"Starting backup of {SaturnData.Path}");

                                var file = SaturnData.Path.Replace("utoc", "ucas");

                                await BackupFile(file, item, option);

                                try
                                {
                                    var changes = _cloudStorageService.GetChanges(Path.GetFileNameWithoutExtension(asset.ParentAsset), item.Id);
                                    cloudChanges = _cloudStorageService.DecodeChanges(changes);
                                }
                                catch
                                {
                                    Logger.Log("There was no hotfix found for this item!", LogLevel.Warning);
                                }

                                if (cloudChanges.SkinName == skin.Name)
                                {
                                    if (cloudChanges.Searches[0] != "none")
                                    {
                                        Trace.WriteLine("Searches are not empty");
                                        skin.Assets[skin.Assets.IndexOf(asset)].Swaps = new();
                                        foreach (var search in cloudChanges.Searches)
                                        {
                                            skin.Assets[skin.Assets.IndexOf(asset)].Swaps[cloudChanges.Searches.IndexOf(search)].Search = search;
                                            skin.Assets[skin.Assets.IndexOf(asset)].Swaps[cloudChanges.Searches.IndexOf(search)].Replace = cloudChanges.Replaces[cloudChanges.Searches.IndexOf(search)];
                                        }
                                    }
                                    
                                    if (cloudChanges.CharacterParts[0] != "none")
                                    {
                                        Trace.WriteLine("CharacterParts are not empty");
                                        foreach (var swap in skin.Assets[skin.Assets.IndexOf(asset)].Swaps)
                                            swap.Replace = "/Game/Tamely";
                                        foreach (var characterPart in cloudChanges.CharacterParts)
                                            skin.Assets[skin.Assets.IndexOf(asset)].Swaps[cloudChanges.CharacterParts.IndexOf(characterPart)].Replace = characterPart;
                                    }
                                        
                                }

                                cloudChanges = new();
                                

                                if (!TryIsB64(ref data, asset))
                                    Logger.Log($"Cannot swap/determine if '{asset.ParentAsset}' is Base64 or not!",
                                        LogLevel.Fatal);
                                
                                var compressed = SaturnData.isCompressed ? Oodle.Compress(data) : data;

                                Directory.CreateDirectory(Config.DecompressedDataPath);
                                File.SetAttributes(Config.DecompressedDataPath,
                                    FileAttributes.Hidden | FileAttributes.System);
                                await File.WriteAllBytesAsync(
                                    Config.DecompressedDataPath + Path.GetFileName(asset.ParentAsset).Replace(".uasset", "") + ".uasset", data);


                                file = file.Replace("WindowsClient", "SaturnClient");

                                await ItemUtil.UpdateStatus(item, option, "Adding asset to UCAS", Colors.C_YELLOW);

                                await TrySwapAsset(Path.Combine(FortniteUtil.PakPath, file), SaturnData.Offset,
                                    compressed);

                                file = file.Replace("ucas", "utoc");
                                
                                Dictionary<long, byte[]> lengths = new();
                                if (!await CustomAssets.TryHandleOffsets(asset, compressed.Length, data.Length, lengths, file, _saturnAPIService))
                                    Logger.Log(
                                        $"Unable to apply custom offsets to '{asset.ParentAsset}.' Asset might not have custom assets at all!",
                                        LogLevel.Error);
                                
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

                            item.IsConverted = true;

                            sw.Stop();

                            if (!await _configService.AddConvertedItem(convItem))
                                Logger.Log("Could not add converted item to config!", LogLevel.Error);
                            else
                                Logger.Log($"Added {item.Name} to converted items list in config.");

                            _configService.SaveConfig();

                            if (sw.Elapsed.Seconds > 1)
                                await ItemUtil.UpdateStatus(item, option, $"Converted in {sw.Elapsed.Seconds} seconds!",
                                    Colors.C_GREEN);
                            else
                                await ItemUtil.UpdateStatus(item, option,
                                    $"Converted in {sw.Elapsed.Milliseconds} milliseconds!", Colors.C_GREEN);
                            Trace.WriteLine($"Converted in {sw.Elapsed.Seconds} seconds!");
                            Logger.Log($"Converted in {sw.Elapsed.Seconds} seconds!");


                            break;

                        #endregion
                    }

                return true;
            }
            catch (Exception ex)
            {
                await ItemUtil.UpdateStatus(item, option,
                    $"There was an error converting {item.Name}. Please send the log to Tamely on Discord!",
                    Colors.C_RED);
                Logger.Log($"There was an error converting {ex.StackTrace}");
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

                switch (itemType)
                {
                    case ItemType.IT_Skin:
                        var itemCheck = await IsTypeConverted(ItemType.IT_Backbling);
                        Trace.WriteLine(itemCheck);
                        if (itemCheck != null)
                        {
                            await ItemUtil.UpdateStatus(item, option,
                                $"You need to revert {itemCheck} before you can revert {item.Name}!", Colors.C_RED);
                            return false;
                        }

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
                        break;
                    case ItemType.IT_Backbling:
                        await ItemUtil.UpdateStatus(item, option, "Checking config file for item", Colors.C_YELLOW);
                        _configService.ConfigFile.ConvertedItems.Any(x =>
                        {
                            if (x.ItemDefinition != id) return false;
                            foreach (var asset in x.Swaps)
                            {
                                ItemUtil.UpdateStatus(item, option, "Reading compressed data", Colors.C_YELLOW).GetAwaiter()
                                    .GetResult();
                                var data = File.ReadAllBytes(Path.Combine(Config.DecompressedDataPath,
                                    Path.GetFileName(asset.ParentAsset).Replace(".uasset", "") + ".uasset"));

                                var compressed = Oodle.Compress(data);

                                ItemUtil.UpdateStatus(item, option, "Writing compressed data back to UCAS", Colors.C_YELLOW)
                                    .GetAwaiter().GetResult();
                                TrySwapAsset(Path.Combine(FortniteUtil.PakPath, asset.File), asset.Offset, compressed)
                                    .GetAwaiter()
                                    .GetResult();

                                ItemUtil.UpdateStatus(item, option, "Checking for customs", Colors.C_YELLOW).GetAwaiter()
                                    .GetResult();
                                if (asset.Lengths != new Dictionary<long, byte[]>())
                                    foreach (var (key, value) in asset.Lengths)
                                        TrySwapAsset(
                                            Path.Combine(FortniteUtil.PakPath, asset.File.Replace("ucas", "utoc")),
                                            key, value).GetAwaiter().GetResult();
                            }

                            return true;
                        });
                        break;
                    case ItemType.IT_Dance:
                        await ItemUtil.UpdateStatus(item, option, "Checking config file for item", Colors.C_YELLOW);
                        _configService.ConfigFile.ConvertedItems.Any(x =>
                        {
                            if (x.ItemDefinition != id) return false;
                            foreach (var asset in x.Swaps)
                            {
                                ItemUtil.UpdateStatus(item, option, "Reading compressed data", Colors.C_YELLOW).GetAwaiter()
                                    .GetResult();
                                var data = File.ReadAllBytes(Path.Combine(Config.CompressedDataPath,
                                    Path.GetFileName(asset.ParentAsset)));

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
                                    Path.GetFileName(asset.ParentAsset)));
                            }

                            return true;
                        });
                        break;
                }


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

        public async Task<Dictionary<string, string>> GetEmoteDataByItem(Cosmetic item)
        {
            Dictionary<string, string> data = new();

            var strs = await FileUtil.GetStringsFromAsset(Constants.EidPath + item.Id, _provider);

            string cmf = "";
            string cmm = "";
            string sIcon = "";
            string lIcon = "";

            foreach (var str in strs)
            {
                if (str.ToLower().Contains("cmf") && str.ToLower().Contains("animation"))
                    if (str.Contains('.'))
                        cmf = str;
                if (str.ToLower().Contains("cmm") && str.ToLower().Contains("animation"))
                    if (str.Contains('.'))
                        cmm = str;
                if (str.ToLower().Contains("icon"))
                    if (str.Contains('.') && !str.ToLower().Contains("-l"))
                        sIcon = str;
                if (str.ToLower().Contains("icon"))
                    if (str.Contains('.') && str.ToLower().Contains("-l"))
                        lIcon = str;
            }
            
            data.Add("CMF", cmf);
            data.Add("CMM", cmm);
            data.Add("SmallIcon", sIcon);
            data.Add("LargeIcon", lIcon);

            data.Add("Name", item.Name);
            data.Add("Description", item.Description);

            if (data["CMF"] == null)
                data.Add("CMF", data["CMM"]);

            if (data["LargeIcon"] == null)
                data.Add("LargeIcon", data["SmallIcon"]);

            return data;
        }

        public async Task<string> GetBackblingCharacterPart(Cosmetic item)
        {
            var strs = await FileUtil.GetStringsFromAsset(Constants.BidPath + item.Id, _provider);
            return strs.FirstOrDefault(x => x.ToLower().Contains("characterpart")).Split('.')[0]
                   + '.' + Path.GetFileNameWithoutExtension(
                       strs.FirstOrDefault(x => x.ToLower().Contains("characterpart")).Split('.')[0]);
        }

        public async Task<string> IsSkinStillConverted()
        {
            foreach (var convItem in (await _configService.TryGetConvertedItems()).Where(x =>
                x.Type == ItemType.IT_Skin.ToString()))
                return convItem.Name;
            return null;
        }

        public async Task<string> IsTypeConverted(ItemType itemType)
        {
            foreach (var convItem in (await _configService.TryGetConvertedItems()).Where(x =>
                x.Type == itemType.ToString()))
                return convItem.Name;
            return null;
        }


        public async Task<Dictionary<string, string>> GetCharacterPartsById(string id)
        {
            Dictionary<string, string> cps = new();
            List<string> bodies = new();
            List<string> heads = new();
            List<string> faceAccs = new();
            List<string> miscOrTail = new();
            List<string> other = new();
            var strs = await FileUtil.GetStringsFromAsset(Constants.CidPath + id, _provider);

            foreach (var str in strs.Where(str => str.Contains("/Game/Athena/Heroes/Meshes/Bodies/") ||
                                                  str.Contains("/Game/Athena/Heroes/Meshes/Heads/") ||
                                                  str.Contains("CharacterParts")))
                if (str.ToLower().Contains("bod"))
                {
                    if (str.Contains('.')) bodies.Add(str);
                }
                else if (str.ToLower().Contains("/heads/") && !str.ToLower().Contains("hat") &&
                         !str.ToLower().Contains("faceacc"))
                {
                    if (str.Contains('.')) heads.Add(str);
                }
                else if (str.ToLower().Contains("face") || str.ToLower().Contains("hat"))
                {
                    if (str.Contains('.')) faceAccs.Add(str);
                }
                else if (str.ToLower().Contains("charm"))
                {
                    if (str.Contains('.')) miscOrTail.Add(str);
                }
                else
                {
                    if (str.Contains('.')) other.Add(str);
                }


            /* Trying to counteract CP-Type Variants */
            cps.Add("Body", bodies.Count > 0 ? FileUtil.GetShortest(bodies) : "/Game/Tamely");
            cps.Add("Head", heads.Count > 0 ? FileUtil.GetShortest(heads) : "/Game/Tamely");
            cps.Add("FaceACC", faceAccs.Count > 0 ? FileUtil.GetShortest(faceAccs) : "/Game/Tamely");
            cps.Add("MiscOrTail", miscOrTail.Count > 0 ? FileUtil.GetShortest(miscOrTail) : "/Game/Tamely");
            cps.Add("Other", other.Count > 0 ? FileUtil.GetShortest(other) : "/Game/Tamely");


            foreach (var characterPart in cps)
                Logger.Log(characterPart.Key + " : " + characterPart.Value);


            return cps;
        }
        
        #region GenerateMeshDefaults


        private async Task<SaturnOption> GenerateMeshSkins(Cosmetic item, SaturnItem option)
        {
            Logger.Log($"Getting character parts for {item.Name}");
            var characterParts = await GetCharacterPartsById(item.Id);
            if (characterParts == new Dictionary<string, string>())
                return null;
            
            Logger.Log("Creating swap model");
            
            MeshDefaultModel swapModel = new()
            {
                HairMaterial = "/Game/Tamely",
                HeadMaterial = "/Game/Tamely",
                HeadHairColor = "/Game/Tamely",
                HeadFX = "/Game/Tamely",
                HeadSkinColor = "/Game/Tamely",
                HeadPartModifierBP = "/Game/Tamely",
                HeadMesh = "/Game/Tamely",
                HeadABP = "/Game/Tamely",
                BodyFX = "/Game/Tamely",
                BodyPartModifierBP = "/Game/Tamely",
                BodyABP = "/Game/Tamely",
                BodyMesh = "/Game/Tamely",
                BodyMaterial = "/Game/Tamely",
                BodySkeleton = "/Game/Tamely",
                FaceACCMaterial = "/Game/Tamely",
                FaceACCMaterial2 = "/Game/Tamely",
                FaceACCMaterial3 = "/Game/Tamely",
                FaceACCMesh = "/Game/Tamely",
                FaceACCABP = "/Game/Tamely",
                FaceACCFX = "/Game/Tamely",
                FaceACCPartModifierBP = "/Game/Tamely"
            };


            Logger.Log("Looping through character parts");

            foreach (var characterPart in characterParts)
            {
                Logger.Log($"Getting strings in asset: {characterPart.Value}");
                var assetStrings = await FileUtil.GetStringsFromAsset(characterPart.Value.Split('.')[0], _provider);
                
                foreach (var i in assetStrings)
                    Logger.Log(i);
                
                switch (characterPart.Key)
                {
                    case "Body":
                        Logger.Log("Character part is type: Body");
                        foreach (var assetString in assetStrings)
                        {
                            if ((assetString.ToLower().Contains("material") || assetString.ToLower().Contains("skins")) && assetString.Contains('.'))
                                swapModel.BodyMaterial = assetString;
                            if ((assetString.ToLower().Contains("mesh") && !assetString.ToLower().Contains("anim") && !assetString.ToLower().Contains("abp")) && assetString.Contains('.'))
                                swapModel.BodyMesh = assetString;
                            if (assetString.ToLower().Contains("skeleton") && !assetString.ToLower().Contains("anim") && !assetString.ToLower().Contains("abp") && assetString.Contains('.'))
                                swapModel.BodySkeleton = assetString;
                            if ((assetString.ToLower().Contains("anim") || assetString.ToLower().Contains("abp")) && assetString.Contains('.'))
                                swapModel.BodyABP = assetString;
                            if (assetString.ToLower().Contains("/blueprint") && assetString.Contains('.'))
                                swapModel.BodyPartModifierBP = assetString;
                            if (assetString.ToLower().Contains("/effect") && assetString.Contains('.'))
                                swapModel.BodyFX = assetString;
                        }
                        
                        break;
                    case "Head":
                        Logger.Log("Character part is type: Head");
                        foreach (var assetString in assetStrings)
                        {
                            if ((assetString.ToLower().Contains("material") || assetString.ToLower().Contains("skins")) && !assetString.ToLower().Contains("hair") && assetString.Contains('.'))
                                swapModel.HeadMaterial = assetString;
                            if ((assetString.ToLower().Contains("material") || assetString.ToLower().Contains("skins")) && assetString.ToLower().Contains("hair") && assetString.Contains('.'))
                                swapModel.HairMaterial = assetString;
                            if ((assetString.ToLower().Contains("hair") && assetString.ToLower().Contains("color")) && assetString.Contains('.'))
                                swapModel.HeadHairColor = assetString;
                            if ((assetString.ToLower().Contains("swatch") && assetString.ToLower().Contains("color")) &&
                                assetString.Contains('.'))
                                swapModel.HeadSkinColor = assetString;
                            if ((assetString.ToLower().Contains("mesh") && !assetString.ToLower().Contains("anim") && !assetString.ToLower().Contains("abp")) && assetString.Contains('.'))
                                swapModel.HeadMesh = assetString;
                            if ((assetString.ToLower().Contains("anim") || assetString.ToLower().Contains("abp")) && assetString.Contains('.'))
                                swapModel.HeadABP = assetString;
                            if (assetString.ToLower().Contains("/blueprint") && assetString.Contains('.'))
                                swapModel.HeadPartModifierBP = assetString;
                            if (assetString.ToLower().Contains("/effect") && assetString.Contains('.'))
                                swapModel.HeadFX = assetString;
                        }
                        
                        break;
                    case "FaceACC":
                        Logger.Log("Character part is type: FaceACC");
                        foreach (var assetString in assetStrings)
                        {
                            if ((assetString.ToLower().Contains("material") || assetString.ToLower().Contains("skins")) && assetString.Contains('.') && swapModel.FaceACCMaterial2 == "/Game/Tamely")
                                swapModel.FaceACCMaterial = assetString;
                            else if ((assetString.ToLower().Contains("material") || assetString.ToLower().Contains("skins")) && assetString.Contains('.') && swapModel.FaceACCMaterial3 == "/Game/Tamely")
                                swapModel.FaceACCMaterial2 = assetString;
                            else if ((assetString.ToLower().Contains("material") || assetString.ToLower().Contains("skins")) && assetString.Contains('.'))
                                swapModel.FaceACCMaterial3 = assetString;
                            if ((assetString.ToLower().Contains("mesh") && !assetString.ToLower().Contains("anim") && !assetString.ToLower().Contains("abp")) && assetString.Contains('.'))
                                swapModel.FaceACCMesh = assetString;
                            if ((assetString.ToLower().Contains("anim") || assetString.ToLower().Contains("abp")) && assetString.Contains('.'))
                                swapModel.FaceACCABP = assetString;
                            if (assetString.ToLower().Contains("/blueprint") && assetString.Contains('.'))
                                swapModel.FaceACCPartModifierBP = assetString;
                            if (assetString.ToLower().Contains("/effect") && assetString.Contains('.'))
                                swapModel.FaceACCFX = assetString;
                        }
                        
                        break;
                    case "MiscOrTail":
                        Logger.Log("Character part is type: MiscOrTail");
                        break;
                    case "Other":
                        Logger.Log("Character part is type: Other");
                        break;
                }

            }
            
            Logger.Log($"Head hair color: {swapModel.HeadHairColor}");
            Logger.Log($"Head skin color: {swapModel.HeadSkinColor}");
            Logger.Log($"Head part modifier bp: {swapModel.HeadPartModifierBP}");
            Logger.Log($"Head FX: {swapModel.HeadFX}");
            Logger.Log($"Head mesh: {swapModel.HeadMesh}");
            Logger.Log($"Head ABP: {swapModel.HeadABP}");
            Logger.Log($"Body ABP: {swapModel.BodyABP}");
            Logger.Log($"Body mesh: {swapModel.BodyMesh}");
            Logger.Log($"Body material: {swapModel.BodyMaterial}");
            Logger.Log($"Body skeleton: {swapModel.BodySkeleton}");
            Logger.Log($"Body part modifier BP: {swapModel.BodyPartModifierBP}");
            Logger.Log($"Body FX: {swapModel.BodyFX}");
            Logger.Log($"Face ACC material: {swapModel.FaceACCMaterial}");
            Logger.Log($"Face ACC mesh: {swapModel.FaceACCMesh}");
            Logger.Log($"Face ACC ABP: {swapModel.FaceACCABP}");
            Logger.Log($"Face ACC material 2: {swapModel.FaceACCMaterial2}");
            Logger.Log($"Face ACC material 3: {swapModel.FaceACCMaterial3}");
            Logger.Log($"Face ACC part modifier BP: {swapModel.FaceACCPartModifierBP}");
            Logger.Log($"Face ACC FX: {swapModel.FaceACCFX}");
            

            Logger.Log("Generating swaps");

            if (swapModel.HeadMesh.ToLower().Contains("ramirez") || swapModel.HeadMesh.ToLower().Contains("starfish"))
            {
                (swapModel.HeadMaterial, swapModel.HairMaterial) = (swapModel.HairMaterial, swapModel.HeadMaterial);
            }

            return option.ItemDefinition switch
            {
                "CID_A_311_Athena_Commando_F_ScholarFestiveWinter" => new SaturnOption()
                {
                    Name = item.Name,
                    Icon = item.Images.SmallIcon,
                    Rarity = item.Rarity.BackendValue,
                    Assets = new()
                    {
                        new SaturnAsset()
                        {
                            ParentAsset =
                                "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_F_ScholarFestiveWinter",
                            Swaps = new List<SaturnSwap>()
                            {
                                new()
                                {
                                    Search =
                                        "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Skins/Winter/Materials/F_MED_Scholar_FestiveWinter_Body.F_MED_Scholar_FestiveWinter_Body",
                                    Replace = swapModel.BodyMaterial,
                                    Type = SwapType.BodyMaterial
                                },
                                new()
                                {
                                    Search =
                                        "/Game/Characters/Player/Female/Medium/Base/SK_M_Female_Base_Skeleton.SK_M_Female_Base_Skeleton",
                                    Replace = swapModel.BodySkeleton,
                                    Type = SwapType.BodySkeleton
                                },
                                new()
                                {
                                    Search =
                                        "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Meshes/F_MED_Scholar.F_MED_Scholar",
                                    Replace = swapModel.BodyMesh,
                                    Type = SwapType.BodyMesh
                                },
                                new()
                                {
                                    Search =
                                        "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Meshes/F_MED_Scholar_AnimBP.F_MED_Scholar_AnimBP_C",
                                    Replace = swapModel.BodyABP,
                                    Type = SwapType.BodyAnim
                                }
                            }
                        },
                        new SaturnAsset()
                        {
                            ParentAsset =
                                "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_ScholarFestiveWinter",
                            Swaps = new List<SaturnSwap>()
                            {
                                new()
                                {
                                    Search =
                                        "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Skins/Winter/Materials/F_MED_Scholar_FestiveWinter_Head.F_MED_Scholar_FestiveWinter_Head",
                                    Replace = swapModel.HeadMaterial,
                                    Type = SwapType.HeadMaterial
                                },
                                new()
                                {
                                    Search =
                                        "/Game/Characters/Player/Female/Medium/Heads/F_MED_CAU_Jane_Head_01/Meshes/F_MED_CAU_Jane_Head_01.F_MED_CAU_Jane_Head_01",
                                    Replace = swapModel.HeadMesh,
                                    Type = SwapType.BodyMesh
                                },
                                new()
                                {
                                    Search =
                                        "/Game/Characters/Player/Female/Medium/Heads/F_MED_CAU_Jane_Head_01/Meshes/F_MED_CAU_Jane_Head_01_AnimBP_Child.F_MED_CAU_Jane_Head_01_AnimBP_Child_C",
                                    Replace = swapModel.HeadABP,
                                    Type = SwapType.HeadAnim
                                }
                            }
                        },
                        new SaturnAsset()
                        {
                            ParentAsset =
                                "FortniteGame/Content/Characters/CharacterParts/FaceAccessories/CP_F_MED_ScholarFestiveWinter_FaceAcc",
                            Swaps = new List<SaturnSwap>()
                            {
                                new()
                                {
                                    Search =
                                        "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Skins/Winter/Materials/F_MED_Scholar_FestiveWinter_Hair.F_MED_Scholar_FestiveWinter_Hair",
                                    Replace = swapModel.FaceACCMaterial,
                                    Type = SwapType.FaceAccessoryMaterial
                                },
                                new()
                                {
                                    Search =
                                        "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Skins/Ghoul/Materials/F_MED_Scholar_Glass_Ghoul_FaceAcc.F_MED_Scholar_Glass_Ghoul_FaceAcc",
                                    Replace = swapModel.FaceACCMaterial2,
                                    Type = SwapType.FaceAccessoryMaterial
                                },
                                new()
                                {
                                    Search =
                                        "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Skins/Winter/Materials/F_MED_Scholar_FestiveWinter_FaceAcc.F_MED_Scholar_FestiveWinter_FaceAcc",
                                    Replace = swapModel.FaceACCMaterial3,
                                    Type = SwapType.FaceAccessoryMaterial
                                },
                                new()
                                {
                                    Search =
                                        "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Meshes/Parts/F_MED_Scholar.F_MED_Scholar",
                                    Replace = swapModel.FaceACCMesh,
                                    Type = SwapType.FaceAccessoryMesh
                                },
                                new()
                                {
                                    Search =
                                        "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Meshes/Parts/F_MED_Scholar_AnimBP.F_MED_Scholar_AnimBP_C",
                                    Replace = swapModel.FaceACCABP,
                                    Type = SwapType.FaceAccessoryAnim
                                }
                            }
                        }
                    }
                },
                _ => new SaturnOption()
            };
        }
        #endregion

        #region GenerateCPSwaps

        private async Task<SaturnOption> GenerateSwaps(Cosmetic item)
        {
            var characterParts = await GetCharacterPartsById(item.Id);
            if (characterParts == new Dictionary<string, string>())
            {
                Logger.Log($"Failed to find character parts for \"{item.Id}\"!", LogLevel.Error);
                return new SaturnOption();
            }

            return new SaturnOption
            {
                Name = item.Name,
                Icon = item.Images.SmallIcon,
                Rarity = item.Rarity.BackendValue,
                Assets = new List<SaturnAsset>
                {
                    new SaturnAsset
                    {
                        ParentAsset = "FortniteGame/Content/Balance/DefaultGameDataCosmetics.uasset",
                        Swaps = new List<SaturnSwap>
                        {
                            new()
                            {
                                Search =
                                    "/Game/Athena/Heroes/Wslt/Is/A/Skid/And/Will/Probably/Steal/This/CustomCharacterParts/PlaceholderCustomCharacterPartNumber1UsedForSwappingByTamely.PlaceholderCustomCharacterPartNumber1UsedForSwappingByTamely",
                                Replace = characterParts["Body"],
                                Type = SwapType.BodyCharacterPart
                            },
                            new()
                            {
                                Search =
                                    "/Game/Athena/Heroes/Wslt/Is/A/Skid/And/Will/Probably/Steal/This/CustomCharacterParts/PlaceholderCustomCharacterPartNumber2UsedForSwappingByTamely.PlaceholderCustomCharacterPartNumber2UsedForSwappingByTamely",
                                Replace = characterParts["Head"],
                                Type = SwapType.HeadCharacterPart
                            },
                            new()
                            {
                                Search =
                                    "/Game/Athena/Heroes/Wslt/Is/A/Skid/And/Will/Probably/Steal/This/CustomCharacterParts/PlaceholderCustomCharacterPartNumber3UsedForSwappingByTamely.PlaceholderCustomCharacterPartNumber3UsedForSwappingByTamely",
                                Replace = characterParts["FaceACC"],
                                Type = SwapType.FaceAccessoryCharacterPart
                            },
                            new()
                            {
                                Search =
                                    "/Game/Athena/Heroes/Wslt/Is/A/Skid/And/Will/Probably/Steal/This/CustomCharacterParts/PlaceholderCustomCharacterPartNumber4UsedForSwappingByTamely.PlaceholderCustomCharacterPartNumber4UsedForSwappingByTamely",
                                Replace = characterParts["MiscOrTail"],
                                Type = SwapType.MiscOrTailCharacterPart
                            },
                            new()
                            {
                                Search =
                                    "/Game/Athena/Heroes/Wslt/Is/A/Skid/And/Will/Probably/Steal/This/CustomCharacterParts/PlaceholderCustomCharacterPartNumber5UsedForSwappingByTamely.PlaceholderCustomCharacterPartNumber5UsedForSwappingByTamely",
                                Replace = characterParts["Other"],
                                Type = SwapType.OtherCharacterPart
                            },
                            new()
                            {
                                Search =
                                    "/Game/Athena/Heroes/Wslt/Is/A/Skid/And/Will/Probably/Steal/This/CustomCharacterParts/PlaceholderCustomCharacterPartNumber6UsedForSwappingByTamely.PlaceholderCustomCharacterPartNumber6UsedForSwappingByTamely",
                                Replace = "/Game/Tamely",
                                Type = SwapType.OtherCharacterPart
                            },
                            new()
                            {
                                Search =
                                    "/Game/Athena/Heroes/Wslt/Is/A/Skid/And/Will/Probably/Steal/This/CustomCharacterParts/PlaceholderCustomBackpackCharacterPartUsedByTamelyForSwapping.PlaceholderCustomBackpackCharacterPartUsedByTamelyForSwapping",
                                Replace = "/Game/Tamely",
                                Type = SwapType.OtherCharacterPart
                            }
                        }
                    },
                    new SaturnAsset
                    {
                        ParentAsset =
                            "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_F_RebirthDefaultA.uasset",
                        Swaps = new List<SaturnSwap>
                        {
                            new()
                            {
                                Search = "\"CP_Body_Commando_F_RebirthDefaultA",
                                Replace = "Tamely",
                                Type = SwapType.BodyCharacterPart
                            }
                        }
                    },
                    new SaturnAsset
                    {
                        ParentAsset =
                            "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_M_RebirthSoldier.uasset",
                        Swaps = new List<SaturnSwap>
                        {
                            new()
                            {
                                Search = System.Convert.ToBase64String(new byte[]
                                {
                                    0, 31, 67, 80, 95, 65, 116, 104, 101, 110, 97, 95, 66, 111, 100, 121, 95, 77, 95,
                                    82, 101, 98, 105, 114, 116, 104, 83, 111, 108, 100, 105, 101, 114
                                }),
                                Replace = "Tamely",
                                Type = SwapType.Base64
                            }
                        }
                    }
                }
            };
        }

        #endregion

        #region GenerateBackblingCPSwaps

        private async Task<SaturnOption> GenerateBackbling(Cosmetic item, SaturnItem option)
        {
            var characterPart = await GetBackblingCharacterPart(item);
            if (characterPart == null)
            {
                await ItemUtil.UpdateStatus(item, option, $"Failed to find character parts for \"{item.Id}\"!",
                    Colors.C_YELLOW);
                Logger.Log($"Failed to find character parts for \"{item.Id}\"!", LogLevel.Error);
                return new SaturnOption();
            }

            return new SaturnOption
            {
                Name = item.Name,
                Icon = item.Images.SmallIcon,
                Rarity = item.Rarity.BackendValue,
                Assets = new List<SaturnAsset>
                {
                    new SaturnAsset
                    {
                        ParentAsset = "FortniteGame/Content/Balance/DefaultGameDataCosmetics.uasset",
                        Swaps = new List<SaturnSwap>
                        {
                            new()
                            {
                                Search = "/Game/Tamely",
                                Replace = characterPart,
                                Type = SwapType.BackblingCharacterPart
                            }
                        }
                    }
                }
            };
        }

        #endregion
        
        #region GenerateEmoteSwaps

        private async Task<SaturnOption> GenerateEmote(Cosmetic item, SaturnItem option)
        {
            var swaps = await GetEmoteDataByItem(item);
            if (swaps == new Dictionary<string, string>())
            {
                await ItemUtil.UpdateStatus(item, option, $"Failed to find data for \"{item.Id}\"!",
                    Colors.C_YELLOW);
                Logger.Log($"Failed to find data for \"{item.Id}\"!", LogLevel.Error);
                return new SaturnOption();
            }

            return new SaturnOption
            {
                Name = item.Name,
                Icon = item.Images.SmallIcon,
                Rarity = item.Rarity.BackendValue,
                Assets = new List<SaturnAsset>
                {
                    new SaturnAsset
                    {
                        ParentAsset = "FortniteGame/Content/Athena/Items/Cosmetics/Dances/EID_DanceMoves.uasset",
                        Swaps = new List<SaturnSwap>
                        {
                            new()
                            {
                                Search = "/Game/Athena/Heroes/Wslt/Is/A/Skid/And/Will/Probably/Steal/This/Animations/PlaceholderEmoteOrAnythingAnimationUsedForSwappingByTamely_CMF.PlaceholderEmoteOrAnythingAnimationUsedForSwappingByTamely_CMF",
                                Replace = swaps["CMF"],
                                Type = SwapType.BodyAnim
                            },
                            new()
                            {
                                Search = "/Game/Athena/Heroes/Wslt/Is/A/Skid/And/Will/Probably/Steal/This/Animations/PlaceholderEmoteOrAnythingAnimationUsedForSwappingByTamely_CMM.PlaceholderEmoteOrAnythingAnimationUsedForSwappingByTamely_CMM",
                                Replace = swaps["CMM"],
                                Type = SwapType.BodyAnim
                            },
                            new()
                            {
                                Search = "/Game/Athena/Heroes/Wslt/Is/A/Skid/And/Will/Probably/Steal/This/Icons/PlaceholderCosmeticIconUsedForSwappingByTamely.PlaceholderCosmeticIconUsedForSwappingByTamely",
                                Replace = swaps["SmallIcon"],
                                Type = SwapType.Modifier
                            },
                            new()
                            {
                                Search = "/Game/Athena/Heroes/Wslt/Is/A/Skid/And/Will/Probably/Steal/This/Icons/PlaceholderCosmeticIconUsedForSwappingByTamely-L.PlaceholderCosmeticIconUsedForSwappingByTamely-L",
                                Replace = swaps["LargeIcon"],
                                Type = SwapType.Modifier
                            },
                            new()
                            {
                                Search = "Emote Name For Tamely's Emote Assets",
                                Replace = item.Name,
                                Type = SwapType.Modifier
                            },
                            new()
                            {
                                Search = "Emote Description For Tamely's Emote Assets But I Need To Make This Longer So It Fits Every Single Description.",
                                Replace = item.Description,
                                Type = SwapType.Modifier
                            }
                        }
                    }
                }
            };
        }

        #endregion

        private async Task<Dictionary<string, long>> GetFileNameAndOffsetFromConvertedItems(SaturnOption item)
        {
            var ret = new Dictionary<string, long>();
            foreach (var convertedItem in await _configService.TryGetConvertedItems())
                if (convertedItem.Type == ItemType.IT_Skin.ToString())
                    foreach (var swap in convertedItem.Swaps)
                        if (swap.ParentAsset == item.Assets[0].ParentAsset)
                            ret.Add(swap.File, swap.Offset);
            return ret;
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

            foreach (var fileExt in fileExts)
            {
                var path = Path.Combine(FortniteUtil.PakPath, fileName + fileExt);
                if (!File.Exists(path))
                {
                    Logger.Log($"File \"{fileName + fileExt}\" doesn't exist!", LogLevel.Warning);
                    return;
                }

                var newPath = path.Replace("WindowsClient", "SaturnClient");
                if (File.Exists(newPath))
                {
                    Logger.Log($"Duplicate for \"{fileName + fileExt}\" already exists!", LogLevel.Warning);
                    continue;
                }

                using var source = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var destination = File.Create(newPath);
                await source.CopyToAsync(destination);

                Logger.Log($"Duplicated file \"{fileName + fileExt}\"");
            }
        }

        public bool TryIsB64(ref byte[] data, SaturnAsset asset)
        {
            List<byte[]> Searches = new();
            List<byte[]> Replaces = new();
            
            try
            {
                                
                Searches.Add(Encoding.ASCII.GetBytes(asset.ParentAsset.Replace(".uasset", "").Replace("FortniteGame/Content/", "/Game/")));
                Replaces.Add(Encoding.ASCII.GetBytes("/Game/Tamely"));
                foreach (var swap in asset.Swaps)
                    switch (swap.Type)
                    {
                        case SwapType.Base64:
                            Searches.Add(System.Convert.FromBase64String(swap.Search));
                            Replaces.Add(Encoding.ASCII.GetBytes(swap.Replace));
                            break;
                        case SwapType.Property:
                            Searches.Add(System.Convert.FromBase64String(swap.Search));
                            Replaces.Add(System.Convert.FromBase64String(swap.Replace));
                            break;
                        default:
                            Searches.Add(Encoding.ASCII.GetBytes(swap.Search));
                            Replaces.Add(Encoding.ASCII.GetBytes(swap.Replace));
                            break;
                    }

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
            data = null;
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

        private static void AddInvalidBytes(ref byte[] bytes, int len)
        {
            var newBytes = new List<byte>();
            newBytes.AddRange(bytes);
            for (var i = 0; i < len; i++)
                newBytes.Add(0);

            bytes = newBytes.ToArray();
        }
    }
}
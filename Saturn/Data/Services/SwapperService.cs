using CUE4Parse;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Saturn.Data.Enums;
using Saturn.Data.Models.FortniteAPI;
using Saturn.Data.Models.Items;
using Saturn.Data.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Data.Services
{

    public interface ISwapperService
    {
        public Task<bool> Convert(Cosmetic item, ItemType itemType, bool isAuto = true);
        public Task<bool> Revert(Cosmetic item, ItemType itemType);
        public Task Swap(Cosmetic item, ItemType itemType, bool isAuto = true);
    }
    public class SwapperService : ISwapperService
    {
        DefaultFileProvider _provider;

        private readonly ISaturnAPIService _saturnAPIService;
        private readonly IFortniteAPIService _fortniteAPIService;
        private readonly IConfigService _configService;


        public SwapperService(IFortniteAPIService fortniteAPIService, ISaturnAPIService saturnAPIService, IConfigService configService)
        {
            _fortniteAPIService = fortniteAPIService;
            _saturnAPIService = saturnAPIService;
            _configService = configService;

            var _aes = _fortniteAPIService.GetAES();

            Trace.WriteLine("Got AES");

            _provider = new DefaultFileProvider(FortniteUtil.PakPath, SearchOption.TopDirectoryOnly);
            _provider.Initialize();
            Trace.WriteLine("Initialized provider");

            var keys = new List<KeyValuePair<FGuid, FAesKey>>
            {
                new(new FGuid(), new FAesKey(_aes.MainKey))
            };
            keys.AddRange(_aes.DynamicKeys.Select(x =>
                new KeyValuePair<FGuid, FAesKey>(new FGuid(x.PakGuid), new FAesKey(x.Key))));

            Trace.WriteLine("Set Keys");
            _provider.SubmitKeys(keys);
            Trace.WriteLine("Submitted Keys");
            Trace.WriteLine("EXE LOC: " + Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase));
            Trace.WriteLine($"File provider initialized with {_provider.Keys.Count} keys");
            
        }

        public async Task Swap(Cosmetic item, ItemType itemType, bool isAuto = true)
        {
            Logger.Log("Checking if item is converted or not!");
            if (item.IsConverted)
            {
                Logger.Log("Item is converted! Reverting!");
                if (!await Revert(item, itemType))
                    Logger.Log($"There was an error reverting {item.Name}!", LogLevel.Error);
            }
            else
            {
                Logger.Log("Item is not converted! Converting!");
                if (!await Convert(item, itemType, isAuto))
                    Logger.Log($"There was an error converting {item.Name}!", LogLevel.Error);
            }
                
        }

        public async Task<bool> Convert(Cosmetic item, ItemType itemType, bool isAuto = true)
        {
            try
            {
                var itemCheck = await IsTypeConverted(itemType);
                if (itemCheck != null)
                {
                    await ItemUtil.UpdateStatus(item, $"You already have {itemCheck} converted! Revert it before converting another item of the same type.", Colors.C_RED);
                    return false;
                }
                var sw = Stopwatch.StartNew();

                await ItemUtil.UpdateStatus(item, "Starting...");

                ConvertedItem convItem = new()
                {
                    Name = item.Name,
                    ItemDefinition = item.Id,
                    Type = itemType.ToString(),
                    Swaps = new List<ActiveSwap>()
                };

                await ItemUtil.UpdateStatus(item, "Checking item type");

                if (isAuto)
                    switch (itemType)
                    {
                        case ItemType.IT_Skin:
                            #region AutoSkins
                            await ItemUtil.UpdateStatus(item, "Generating swaps", Colors.C_YELLOW);
                            var skin = await GenerateSwaps(item);

                            foreach (var asset in skin.Assets)
                            {
                                Directory.CreateDirectory(Config.CompressedDataPath);
                                await ItemUtil.UpdateStatus(item, "Exporting asset", Colors.C_YELLOW);
                                if (!TryExportAsset(asset.ParentAsset, out var data))
                                {
                                    Logger.Log($"Failed to export \"{asset.ParentAsset}\"!", LogLevel.Error);
                                    return false;
                                }

                                var file = SaturnData.Path.Replace("utoc", "ucas");

                                await BackupFile(file, item);

                                if (asset.ParentAsset.ToLower().Contains("defaultgamedatacosmetics"))
                                {
                                    data = new WebClient().DownloadData(new Uri(
                                        await _saturnAPIService.GetDownloadUrl(Path.GetFileNameWithoutExtension(asset.ParentAsset))));
                                }

                                if (!TryIsB64(ref data, asset))
                                    Logger.Log($"Cannot swap/determine if '{asset.ParentAsset}' is Base64 or not!", LogLevel.Fatal);

                                var compressed = Oodle.Compress(data);

                                Directory.CreateDirectory(Config.DecompressedDataPath);
                                File.SetAttributes(Config.DecompressedDataPath, FileAttributes.Hidden | FileAttributes.System);
                                await File.WriteAllBytesAsync(Config.DecompressedDataPath + Path.GetFileName(asset.ParentAsset), data);


                                file = file.Replace("WindowsClient", "SaturnClient");

                                await ItemUtil.UpdateStatus(item, "Adding asset to UCAS", Colors.C_YELLOW);

                                await TrySwapAsset(Path.Combine(FortniteUtil.PakPath, file), SaturnData.Offset, compressed);

                                file = file.Replace("ucas", "utoc");

                                await ItemUtil.UpdateStatus(item, "Checking for customs", Colors.C_YELLOW);
                                Dictionary<long, byte[]> lengths = new();
                                if (!await TryHandleOffsets(asset, compressed.Length, data.Length, lengths, file))
                                    Logger.Log($"Unable to apply custom assets to '{asset.ParentAsset}.' Asset might not have custom assets at all!", LogLevel.Error);

                                await ItemUtil.UpdateStatus(item, "Adding swap to item's config", Colors.C_YELLOW);
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
                                await ItemUtil.UpdateStatus(item, $"Converted in {sw.Elapsed.Seconds} seconds!", Colors.C_GREEN);
                            else
                                await ItemUtil.UpdateStatus(item, $"Converted in {sw.Elapsed.Milliseconds} milliseconds!", Colors.C_GREEN);
                            Trace.WriteLine($"Converted in {sw.Elapsed.Seconds} seconds!");
                            Logger.Log($"Converted in {sw.Elapsed.Seconds} seconds!");


                            break;
                            #endregion
                        case ItemType.IT_Backbling:
                            itemCheck = await IsSkinStillConverted();
                            if (itemCheck == null)
                            {
                                await ItemUtil.UpdateStatus(item, $"You need to have a skin converted before you can add a backpack to it!", Colors.C_RED);
                                return false;
                            }
                            #region AutoBackblings

                            await ItemUtil.UpdateStatus(item, "Generating swaps", Colors.C_YELLOW);
                            var backbling = await GenerateBackbling(item);

                            foreach (var asset in backbling.Assets)
                            {
                                await ItemUtil.UpdateStatus(item, "Exporting asset", Colors.C_YELLOW);
                                var decompressedData = await File.ReadAllBytesAsync(Config.DecompressedDataPath + "DefaultGameDataCosmetics.uasset");

                                if (!TryIsB64(ref decompressedData, asset))
                                    Logger.Log($"Cannot swap/determine if '{asset.ParentAsset}' is Base64 or not!", LogLevel.Fatal);

                                var compressedData = Oodle.Compress(decompressedData);

                                var keyValuePair = await GetFileNameAndOffsetFromConvertedItems(backbling);
                                var file = keyValuePair.Keys.FirstOrDefault();
                                var offset = keyValuePair.Values.FirstOrDefault();

                                await ItemUtil.UpdateStatus(item, "Adding asset to UCAS", Colors.C_YELLOW);

                                await TrySwapAsset(Path.Combine(FortniteUtil.PakPath, file), offset, compressedData);

                                file = file.Replace("ucas", "utoc");

                                await ItemUtil.UpdateStatus(item, "Checking for customs", Colors.C_YELLOW);
                                Dictionary<long, byte[]> lengths = new();
                                if (!await TryHandleOffsets(asset, compressedData.Length, decompressedData.Length, lengths, file))
                                    Logger.Log($"Unable to apply custom assets to '{asset.ParentAsset}.' Asset might not have custom assets at all!", LogLevel.Error);

                                await ItemUtil.UpdateStatus(item, "Adding swap to item's config", Colors.C_YELLOW);
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
                                await ItemUtil.UpdateStatus(item, $"Converted in {sw.Elapsed.Seconds} seconds!", Colors.C_GREEN);
                            else
                                await ItemUtil.UpdateStatus(item, $"Converted in {sw.Elapsed.Milliseconds} milliseconds!", Colors.C_GREEN);
                            Trace.WriteLine($"Converted in {sw.Elapsed.Seconds} seconds!");
                            Logger.Log($"Converted in {sw.Elapsed.Seconds} seconds!");

                            break;
                            #endregion
                    }

                return true;
            }
            catch (Exception ex)
            {
                await ItemUtil.UpdateStatus(item, $"There was an error converting {item.Name}. Please send the log to Tamely on Discord!", Colors.C_RED);
                Logger.Log($"There was an error converting {ex.StackTrace}");
                return false;
            }
            
        }

        public async Task<bool> Revert(Cosmetic item, ItemType itemType)
        {
            try
            {
                await ItemUtil.UpdateStatus(item, "Starting...", Colors.C_YELLOW);
                var id = item.Id;

                var sw = Stopwatch.StartNew();

                switch (itemType)
                {
                    case ItemType.IT_Skin:
                        await ItemUtil.UpdateStatus(item, "Checking config file for item", Colors.C_YELLOW);
                        _configService.ConfigFile.ConvertedItems.Any(x =>
                        {
                            if (x.ItemDefinition != id) return false;
                            foreach (var asset in x.Swaps)
                            {
                                ItemUtil.UpdateStatus(item, "Reading compressed data", Colors.C_YELLOW).GetAwaiter().GetResult();
                                var data = File.ReadAllBytes(Path.Combine(Config.CompressedDataPath,
                                    Path.GetFileName(asset.ParentAsset)));

                                ItemUtil.UpdateStatus(item, "Writing compressed data back to UCAS", Colors.C_YELLOW).GetAwaiter().GetResult();
                                TrySwapAsset(Path.Combine(FortniteUtil.PakPath, asset.File), asset.Offset, data).GetAwaiter()
                                    .GetResult();

                                ItemUtil.UpdateStatus(item, "Checking for customs", Colors.C_YELLOW).GetAwaiter().GetResult();
                                if (asset.Lengths != new Dictionary<long, byte[]>())
                                    foreach (var (key, value) in asset.Lengths)
                                        TrySwapAsset(Path.Combine(FortniteUtil.PakPath, asset.File.Replace("ucas", "utoc")),
                                            key, value).GetAwaiter().GetResult();

                                ItemUtil.UpdateStatus(item, "Deleting compressed data", Colors.C_YELLOW).GetAwaiter().GetResult();
                                File.Delete(Path.Combine(Config.CompressedDataPath,
                                    Path.GetFileName(asset.ParentAsset)));
                            }

                            return true;
                        });
                        break;
                    case ItemType.IT_Backbling:
                        await ItemUtil.UpdateStatus(item, "Checking config file for item", Colors.C_YELLOW);
                        _configService.ConfigFile.ConvertedItems.Any(x =>
                        {
                            if (x.ItemDefinition != id) return false;
                            foreach (var asset in x.Swaps)
                            {
                                ItemUtil.UpdateStatus(item, "Reading compressed data", Colors.C_YELLOW).GetAwaiter().GetResult();
                                var data = File.ReadAllBytes(Path.Combine(Config.DecompressedDataPath,
                                    Path.GetFileName(asset.ParentAsset)));

                                var compressed = Oodle.Compress(data);

                                ItemUtil.UpdateStatus(item, "Writing compressed data back to UCAS", Colors.C_YELLOW).GetAwaiter().GetResult();
                                TrySwapAsset(Path.Combine(FortniteUtil.PakPath, asset.File), asset.Offset, compressed).GetAwaiter()
                                    .GetResult();

                                ItemUtil.UpdateStatus(item, "Checking for customs", Colors.C_YELLOW).GetAwaiter().GetResult();
                                if (asset.Lengths != new Dictionary<long, byte[]>())
                                    foreach (var (key, value) in asset.Lengths)
                                        TrySwapAsset(Path.Combine(FortniteUtil.PakPath, asset.File.Replace("ucas", "utoc")),
                                            key, value).GetAwaiter().GetResult();
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
                    await ItemUtil.UpdateStatus(item, $"Reverted in {sw.Elapsed.Seconds} seconds!", Colors.C_GREEN);
                else
                    await ItemUtil.UpdateStatus(item, $"Reverted in {sw.Elapsed.Milliseconds} milliseconds!", Colors.C_GREEN);

                Logger.Log($"Reverted in {sw.Elapsed.Seconds} seconds!");
                Trace.WriteLine($"Reverted in {sw.Elapsed.Seconds} seconds!");
                return true;
            }
            catch (Exception ex)
            {
                await ItemUtil.UpdateStatus(item, $"There was an error reverting {item.Name}. Please send the log to Tamely on Discord!", Colors.C_RED);
                Logger.Log($"There was an error reverting {ex.StackTrace}");
                return false;
            }
            
        }

        public async Task<Dictionary<string, string>> GetEmoteDataByItem(Cosmetic item)
        {
            Dictionary<string, string> data = new();

            var strs = await FileUtil.GetStringsFromAsset(Constants.EidPath + item.Id, _provider);

            foreach (var str in strs)
            {
                if (str.ToLower().Contains("cmf"))
                    if (str.Contains('.')) data.Add("CMF", str);
                if (str.ToLower().Contains("cmm"))
                    if (str.Contains('.')) data.Add("CMM", str);
                if (str.ToLower().Contains("icon"))
                    if (str.Contains('.') && !str.ToLower().Contains("-l")) data.Add("SmallIcon", str);
                if (str.ToLower().Contains("icon"))
                    if (str.Contains('.') && str.ToLower().Contains("-l")) data.Add("LargeIcon", str);
            }

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
                + '.' + Path.GetFileNameWithoutExtension(strs.FirstOrDefault(x => x.ToLower().Contains("characterpart")).Split('.')[0]);
        }

        public async Task<string> IsSkinStillConverted()
        {
            foreach (var convItem in await _configService.TryGetConvertedItems())
                if (convItem.Type == ItemType.IT_Skin.ToString())
                    return convItem.Name;
            return null;
        }

        public async Task<string> IsTypeConverted(ItemType itemType)
        {
            foreach (var convItem in await _configService.TryGetConvertedItems())
                if (convItem.Type == itemType.ToString())
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
                else if (str.ToLower().Contains("/heads/") && !str.ToLower().Contains("hat") && !str.ToLower().Contains("faceacc"))
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
                Assets = new()
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
        private async Task<SaturnOption> GenerateBackbling(Cosmetic item)
        {
            var characterPart = await GetBackblingCharacterPart(item);
            if (characterPart == null)
            {
                await ItemUtil.UpdateStatus(item, $"Failed to find character parts for \"{item.Id}\"!", Colors.C_YELLOW);
                Logger.Log($"Failed to find character parts for \"{item.Id}\"!", LogLevel.Error);
                return new SaturnOption();
            }

            return new SaturnOption
            {
                Name = item.Name,
                Icon = item.Images.SmallIcon,
                Rarity = item.Rarity.BackendValue,
                Assets = new()
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

        private async Task BackupFile(string sourceFile, Cosmetic item)
        {
            ItemUtil.UpdateStatus(item, "Backing up files", Colors.C_YELLOW).GetAwaiter().GetResult();
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
            try
            {
                foreach (var swap in asset.Swaps)
                    if (swap.Type == SwapType.Base64)
                        EditAsset(ref data, System.Convert.FromBase64String(swap.Search), swap.Replace);
                    else if (swap.Type == SwapType.BackblingCharacterPart)
                        EditCustomAsset(ref data, swap.Search, swap.Replace);
                    else
                        EditAsset(ref data, swap.Search, swap.Replace);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public async Task<bool> TryHandleOffsets(SaturnAsset asset, int compressedLength, int decompressedLength, Dictionary<long, byte[]> lengths, string file)
        {
            try
            {
                if (asset.ParentAsset.Contains("DefaultGameDataCosmetics"))
                {
                    var assetData = await _saturnAPIService.GetOffsets("DefaultGameDataCosmetics");

                    var n = 1;

                    #region Handling compressed offsets

                    foreach (var compressedOffset in assetData.CompressedOffsets)
                        if (n == 1)
                        {
                            lengths.Add(compressedOffset,
                                FileUtil.GetBytes(
                                    await File.ReadAllBytesAsync(Path.Combine(FortniteUtil.PakPath, file)),
                                    compressedOffset, 2));

                            FileUtil.WriteIntToFile(Path.Combine(FortniteUtil.PakPath, file),
                                compressedOffset, compressedLength);

                            n++;
                        }
                        else
                        {
                            lengths.Add(compressedOffset - 2,
                                FileUtil.GetBytes(
                                    await File.ReadAllBytesAsync(Path.Combine(FortniteUtil.PakPath, file)),
                                    compressedOffset - 2, 2));

                            FileUtil.WriteHexToFile(Path.Combine(FortniteUtil.PakPath, file),
                                compressedOffset - 2, FileUtil.IntToHex(compressedLength));

                            n++;
                        }

                    #endregion

                    n = 1;

                    #region Handling decompressed offsets

                    foreach (var decompressedOffset in assetData.DecompressedOffsets)
                        if (n == 1)
                        {
                            lengths.Add(decompressedOffset,
                                FileUtil.GetBytes(
                                    await File.ReadAllBytesAsync(Path.Combine(FortniteUtil.PakPath, file)),
                                    decompressedOffset, 2));

                            FileUtil.WriteIntToFile(Path.Combine(FortniteUtil.PakPath, file),
                                decompressedOffset, decompressedLength);

                            n++;
                        }
#if !DEBUG
								    else if (n == 2)
								    {
									    lengths.Add(decompressedOffset - 2,
							                FileUtil.GetBytes(
								                await File.ReadAllBytesAsync(Path.Combine(FortniteUtil.PakPath, file)),
								                decompressedOffset - 2, 2));

						                FileUtil.WriteHexToFile(Path.Combine(FortniteUtil.PakPath, file),
							                decompressedOffset - 2, FileUtil.IntToHex(decompressedLength + 20));

						                n++;
								    }
#endif
                        else
                        {
                            lengths.Add(decompressedOffset - 2,
                                FileUtil.GetBytes(
                                    await File.ReadAllBytesAsync(Path.Combine(FortniteUtil.PakPath, file)),
                                    decompressedOffset - 2, 2));

                            FileUtil.WriteHexToFile(Path.Combine(FortniteUtil.PakPath, file),
                                decompressedOffset - 2, FileUtil.IntToHex(decompressedLength));

                            n++;
                        }

                    #endregion
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

        }
        private bool TryExportAsset(string asset, out byte[] data)
        {
            data = null;
            try
            {
                if (!_provider.TrySavePackage(asset, out var pkg))
                {
                    Logger.Log($"Failed to export asset \"{asset}\"!", LogLevel.Warning);
                    return false;
                }

                data = pkg.FirstOrDefault(x => x.Key.Contains("uasset")).Value;

                File.WriteAllBytes(Path.Combine(Config.CompressedDataPath, $"{Path.GetFileName(SaturnData.UAssetPath)}"),
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

        private static void EditCustomAsset(ref byte[] data, string search, string replace)
        {
            var searchBytes = Encoding.Default.GetBytes(search);
            var replaceBytes = Encoding.Default.GetBytes(replace);

            var offset = 0;

            while (offset < data.Length)
            {
                var found = true;
                for (var i = 0; i < searchBytes.Length; i++)
                {
                    if (data[offset + i] == searchBytes[i])
                        continue;

                    found = false;
                    break;
                }

                if (!found)
                {
                    offset++;
                    continue;
                }

                for (var i = 0; i < replaceBytes.Length; i++)
                    data[offset + i] = replaceBytes[i];

                break;
            }
        }

        private static void EditAsset(ref byte[] data, string search, string replace)
        {
            var searchBytes = Encoding.Default.GetBytes(search);
            var replaceBytes = Encoding.Default.GetBytes(replace);

            var diff = search.Length - replace.Length;
            if (diff < 0)
            {
                Logger.Log("Difference is less than 0!", LogLevel.Warning);
                AnyLength.ReplaceAnyLength(ref data, searchBytes, replaceBytes);
                return;
            }

            AddInvalidBytes(ref replaceBytes, diff);

            var offset = 0;

            while (offset < data.Length)
            {
                var found = true;
                for (var i = 0; i < searchBytes.Length; i++)
                {
                    if (data[offset + i] == searchBytes[i])
                        continue;

                    found = false;
                    break;
                }

                if (!found)
                {
                    offset++;
                    continue;
                }

                for (var i = 0; i < replaceBytes.Length; i++)
                    data[offset + i] = replaceBytes[i];

                break;
            }
        }

        private static void EditAsset(ref byte[] data, byte[] searchBytes, string replace)
        {
            var replaceBytes = Encoding.Default.GetBytes(replace);

            var diff = searchBytes.Length - replace.Length;
            if (diff < 0)
            {
                Logger.Log("Difference is less than 0!", LogLevel.Warning);
                AnyLength.ReplaceAnyLength(ref data, searchBytes, replaceBytes);
                return;
            }

            AddInvalidBytes(ref replaceBytes, diff);

            var offset = 0;

            while (offset < data.Length)
            {
                var found = true;
                for (var i = 0; i < searchBytes.Length; i++)
                {
                    if (data[offset + i] == searchBytes[i])
                        continue;

                    found = false;
                    break;
                }

                if (!found)
                {
                    offset++;
                    continue;
                }

                for (var i = 0; i < replaceBytes.Length; i++)
                    data[offset + i] = replaceBytes[i];

                break;
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
                Logger.Log($"Failed to swap asset in file {Path.GetFileName(path)}! Reason: {e.Message}", LogLevel.Error);
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

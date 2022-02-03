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
using System.Windows.Media;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse_Conversion.Textures;
using Saturn.Backend.Data.SwapOptions.Pickaxes;
using Saturn.Backend.Data.SwapOptions.Skins;
using Colors = Saturn.Backend.Data.Enums.Colors;
using Saturn.Backend.Data.SwapOptions.Backblings;
using Saturn.Backend.Data.SwapOptions.Emotes;
using SharpGLTF.Schema2;
using SkiaSharp;

namespace Saturn.Backend.Data.Services;

public interface ISwapperService
{
    public Task<bool> Convert(Cosmetic item, SaturnItem option, ItemType itemType, bool isAuto = true, bool isRandom = false, Cosmetic random = null);
    public Task<bool> Revert(Cosmetic item, SaturnItem option, ItemType itemType);
    public Task<List<Cosmetic>> GetSaturnSkins();
    public Task Swap(Cosmetic item, SaturnItem option, ItemType itemType, List<Cosmetic> Items, bool isAuto = true);
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

        var _aes = _fortniteAPIService.GetAES();

        Trace.WriteLine("Got AES");

        _provider = new DefaultFileProvider(FortniteUtil.PakPath, SearchOption.TopDirectoryOnly, false, new CUE4Parse.UE4.Versions.VersionContainer(CUE4Parse.UE4.Versions.EGame.GAME_UE5_1));
        _provider.Initialize();

        Trace.WriteLine("Initialized provider");

        new Mappings(_provider, benBotApiService, fortniteAPIService, jsRuntime).Init();

        Trace.WriteLine("Loaded mappings");

        var keys = new List<KeyValuePair<FGuid, FAesKey>>();
        if (_aes.MainKey != null)
            keys.Add(new(new FGuid(), new FAesKey(_aes.MainKey)));
        keys.AddRange(from x in _aes.DynamicKeys
                      select new KeyValuePair<FGuid, FAesKey>(new FGuid(x.PakGuid), new FAesKey(x.Key)));

        Trace.WriteLine("Set Keys");
        _provider.SubmitKeys(keys);
        Trace.WriteLine("Submitted Keys");
        Trace.WriteLine($"File provider initialized with {_provider.Keys.Count} keys");
    }

    public async Task<List<Cosmetic>> GetSaturnSkins()
    {
        var Skins = new List<Cosmetic>();

        await Task.Run(() =>
        {
            foreach (var (assetPath, assetValue) in _provider.Files)
            {
                if (!assetPath.Contains("/CID_")) continue;
                if (_provider.TryLoadObject(assetPath.Split('.')[0], out var asset))
                {
                    Cosmetic skin = new();

                    skin.Name = asset.TryGetValue(out FText DisplayName, "DisplayName") ? DisplayName.Text : "TBD";
                    skin.Description = asset.TryGetValue(out FText Description, "Description") ? Description.Text : "To be determined...";

                    skin.Id = FileUtil.SubstringFromLast(assetPath, '/').Split('.')[0];
                    
                    if (skin.Name.ToLower() is "null" or "tbd" or "hero" || skin.Id.ToLower().Contains("cid_vip_"))
                        continue;
                    
                    skin.Rarity = new Rarity
                    {
                        Value = asset.TryGetValue(out EFortRarity Rarity, "Rarity") ? Rarity.ToString().Split("::")[0] : "Uncommon"
                    };

                    if (skin.Name is "Recruit" or "Random")
                        skin.Rarity.Value = "Common";

                    if (skin.Name is "Random")
                        skin.IsRandom = true;

                    skin.Series = asset.TryGetValue(out UObject Series, "Series")
                        ? new Series()
                        {
                            BackendValue = FileUtil.SubstringFromLast(Series.GetFullName(), '/').Split('.')[0]
                        } : null;
                    
                    skin.Images = new Images();

                    if (File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/skins/" + skin.Id + ".png")))
                        skin.Images.SmallIcon = "skins/" + skin.Id + ".png";
                    else
                    {
                        if (asset.TryGetValue(out UObject HID, "HeroDefinition"))
                        {
                            if (HID.TryGetValue(out UTexture2D smallIcon, "SmallPreviewImage"))
                            {
                                using var ms = new MemoryStream();
                                smallIcon.Decode().Encode().SaveTo(ms);
                            
                                Directory.CreateDirectory(Path.Combine(Config.ApplicationPath, "wwwroot/skins/"));
                                if (!File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/skins/" + skin.Id + ".png")))
                                    File.WriteAllBytes(Path.Combine(Config.ApplicationPath, "wwwroot/skins/" + skin.Id + ".png"), ms.ToArray());

                                skin.Images.SmallIcon = "skins/" + skin.Id + ".png";
                            }
                            else
                            {
                                Logger.Log("Cannot parse the small icon for " + skin.Id);
                                continue;
                            }

                        }
                        else
                        {
                            Logger.Log("Cannot parse the HID for " + skin.Id);
                            continue;
                        }
                    }
                    
                    Skins.Add(skin.AddSkinOptions());
                }
                else
                {
                    Logger.Log($"Failed to load {assetPath}");
                }
                
                // sort skins by alphabetical order
                Skins = Skins.OrderBy(x => x.Id).ToList();

                // Remove items from the array that are duplicates
                for (var i = 0; i < Skins.Count; i++)
                {
                    for (var j = i + 1; j < Skins.Count; j++)
                    {
                        if (Skins[i].Name == Skins[j].Name
                            && Skins[i].Images.SmallIcon == Skins[j].Images.SmallIcon
                            && Skins[i].Description == Skins[j].Description)
                        {
                            Skins.RemoveAt(j);
                            j--;
                        }
                    }
                }
            }
        });

        Trace.WriteLine($"Deserialized {Skins.Count} objects");

        _discordRPCService.UpdatePresence($"Looking at {Skins.Count} different skins");

        return Skins;

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
                        "https://cdn.discordapp.com/attachments/770991313490280478/936001299697242182/TamelysDefaultGameData.uasset");
                Logger.Log("Asset exported");
                Logger.Log($"Starting backup of {SaturnData.Path}");

                var file = SaturnData.Path.Replace("utoc", "ucas");

                if (isRandom)
                    await BackupFile(file, random, option);
                else
                    await BackupFile(file, item, option);


                if (!TryIsB64(ref data, asset))
                    Logger.Log($"Cannot swap/determine if '{asset.ParentAsset}' is Base64 or not!",
                        LogLevel.Fatal);

                var compressed = SaturnData.isCompressed ? Utils.Oodle.Compress(data) : data;

                Directory.CreateDirectory(Config.DecompressedDataPath);
                File.SetAttributes(Config.DecompressedDataPath,
                    FileAttributes.Hidden | FileAttributes.System);
                await File.WriteAllBytesAsync(
                    Config.DecompressedDataPath + Path.GetFileName(asset.ParentAsset).Replace(".uasset", "") + ".uasset", data);


                file = file.Replace("WindowsClient", "SaturnClient");

                if (isRandom)
                    await ItemUtil.UpdateStatus(random, option, "Adding asset to UCAS", Colors.C_YELLOW);
                else
                    await ItemUtil.UpdateStatus(item, option, "Adding asset to UCAS", Colors.C_YELLOW);

                await TrySwapAsset(Path.Combine(FortniteUtil.PakPath, file), SaturnData.Offset,
                    compressed);

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

            if (sw.Elapsed.Seconds > 1)
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
                _jsRuntime.InvokeVoidAsync("MessageBox", "You might want to revert the last item you swapped!", "If you go ingame with your currently swapped items, you will be kicked from Fortnite.", "warning");


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

        if (cmm == "")
            cmm = strs.FirstOrDefault(x => x.StartsWith("/Game/Animation/Game/MainPlayer/"));

        data.Add("CMF", cmf);
        data.Add("CMM", cmm);
        data.Add("SmallIcon", sIcon);
        data.Add("LargeIcon", lIcon);

        data.Add("Name", item.Name);
        data.Add("Description", item.Description);

        if (data["CMF"] == "")
        {
            data.Remove("CMF");
            data.Add("CMF", data["CMM"]);
        }

        if (data["LargeIcon"] != "") return data;
        data.Remove("LargeIcon");
        data.Add("LargeIcon", data["SmallIcon"]);

        return data;
    }
    
    public async Task<string> GetBackblingCharacterPart(Cosmetic item)
    {
        string characterPart = "";
        await Task.Run(() =>
        {
            if (_provider.TryLoadObject(Constants.BidPath + item.Id, out var BID))
            {
                BID.TryGetValue(out UObject[] BackblingCharacterPart, "CharacterParts");
                if (BackblingCharacterPart.Length > 0)
                    characterPart = BackblingCharacterPart[0].GetPathName();
                
            }
            else
            {
                Logger.Log("There was no backbling character part found!", LogLevel.Error);
                characterPart = "No character part found!";
            }
        });

        return characterPart;
    }

    private async Task<Dictionary<string, string>> GetDataFromBackblingCharacterPart(string backblingCharacterPart)
    {
        var output = new Dictionary<string, string>();

        await Task.Run(() =>
        {
            if (_provider.TryLoadObject(backblingCharacterPart.Split('.')[0], out var bCP))
            {
                output.Add("Mesh",
                    bCP.TryGetValue(out FSoftObjectPath SkeletalMesh, "SkeletalMesh") 
                        ? SkeletalMesh.AssetPathName.Text 
                        : "/");

                output.Add("FX",
                    bCP.TryGetValue(out FSoftObjectPath IdleEffectNiagara, "IdleEffectNiagara")
                        ? IdleEffectNiagara.AssetPathName.Text 
                        : "/");
                
                if (output["FX"] == "/")
                {
                    output.Remove("FX");
                    output.Add("FX",
                        bCP.TryGetValue(out FSoftObjectPath IdleEffect, "IdleEffect")
                            ? IdleEffect.AssetPathName.Text 
                            : "/");
                }

                output.Add("PartModifierBP",
                    bCP.TryGetValue(out FSoftObjectPath PartModifierBlueprint, "PartModifierBlueprint")
                        ? PartModifierBlueprint.AssetPathName.Text 
                        : "/");

                if (bCP.TryGetValue(out FStructFallback[] MaterialOverrides, "MaterialOverrides"))
                {
                    foreach (var materialOverride in MaterialOverrides)
                    {
                        if (materialOverride.Get<int>("MaterialOverrideIndex") != 0)
                            continue;
                        output.Add("Material", materialOverride.Get<FSoftObjectPath>("OverrideMaterial").AssetPathName.Text);
                    }
                }
                else
                    output.Add("Material", "/");

                if (bCP.TryGetValue(out UObject AdditonalData, "AdditionalData"))
                    output.Add("ABP",
                        AdditonalData.TryGetValue(out FSoftObjectPath AnimClass, "AnimClass") 
                            ? AnimClass.AssetPathName.Text 
                            : null);
            }
            else
                Logger.Log($"Couldn't process backbling character part! {backblingCharacterPart.Split('.')[0]}", LogLevel.Error);
        });


        foreach (var input in output)
            Logger.Log($"{input.Key}: {input.Value}", LogLevel.Debug);
        
        return output;
    }

    public async Task<Dictionary<string, string>> GetAssetsFromWID(string wid)
    {
        var output = new Dictionary<string, string>();

        UObject? export = await _provider.TryLoadObjectAsync(wid);

        Logger.Log("Getting WeaponMeshOverride");
        export.TryGetValue(out FSoftObjectPath Mesh, "WeaponMeshOverride");
        Logger.Log("Getting WeaponMaterialOverrides");
        export.TryGetValue(out FSoftObjectPath[] Material, "WeaponMaterialOverrides");
        Logger.Log("Getting SmallPreviewImage");
        export.TryGetValue(out FSoftObjectPath SmallIcon, "SmallPreviewImage");
        Logger.Log("Getting LargePreviewImage");
        export.TryGetValue(out FSoftObjectPath LargeIcon, "LargePreviewImage");
        Logger.Log("Getting IdleEffect");
        export.TryGetValue(out FSoftObjectPath FX, "IdleEffect");
        Logger.Log("Getting SwingFX");
        export.TryGetValue(out FSoftObjectPath SwingFX, "SwingEffect");
        Logger.Log("Getting Offhand SwingFX");
        export.TryGetValue(out FSoftObjectPath OffhandSwingFX, "SwingEffectOffhandNiagara");
        FPropertyTagType? ImpactCue = null;
        if (export.TryGetValue(out UScriptMap ImpactPhysicalSurfaceSoundsMap, "ImpactPhysicalSurfaceSoundsMap"))
            ImpactPhysicalSurfaceSoundsMap.Properties.TryGetValue(ImpactPhysicalSurfaceSoundsMap.Properties.Keys.First(), out ImpactCue);
        FPropertyTagType? EquipCue = null;
        if (export.TryGetValue(out UScriptMap ReloadSoundsMap, "ReloadSoundsMap"))
            ReloadSoundsMap.Properties.TryGetValue(ReloadSoundsMap.Properties.Keys.First(), out EquipCue);
        FPropertyTagType? SwingCue = null;
        if (export.TryGetValue(out UScriptMap PrimaryFireSoundMap, "PrimaryFireSoundMap"))
            PrimaryFireSoundMap.Properties.TryGetValue(PrimaryFireSoundMap.Properties.Keys.First(), out SwingCue);
        Logger.Log("Getting WeaponActorClass");
        export.TryGetValue(out FSoftObjectPath ActorClass, "WeaponActorClass");
        Logger.Log("Getting AnimTrails");
        export.TryGetValue(out FSoftObjectPath Trail, "AnimTrails");
        export.TryGetValue(out FSoftObjectPath OffhandTrail, "AnimTrailsOffhand");
        Logger.Log("Getting Rarity");
        output.Add("Rarity", export.TryGetValue(out EFortRarity Rarity, "Rarity") 
            ? ((int)Rarity).ToString() 
            : "1");

        Logger.Log("Getting Series");
        string Series = "/";
        if (export.TryGetValue(out UObject SeriesObject, "Series"))
            Series = SeriesObject.GetPathName();

        output.Add("Mesh", Mesh.AssetPathName.Text);
        output.Add("Material", Material != null ? Material[0].AssetPathName.Text : "/");
        output.Add("SmallIcon", SmallIcon.AssetPathName.Text);
        output.Add("LargeIcon", LargeIcon.AssetPathName.Text);
        output.Add("SwingFX", SwingFX.AssetPathName.Text);
        output.Add("OffhandSwingFX", OffhandSwingFX.AssetPathName.Text);
        output.Add("FX", FX.AssetPathName.Text);
        output.Add("SwingCue", SwingCue != null ? ((FSoftObjectPath)SwingCue.GenericValue).AssetPathName.Text : "/");
        output.Add("EquipCue", EquipCue != null ? ((FSoftObjectPath)EquipCue.GenericValue).AssetPathName.Text : "/");
        output.Add("ImpactCue", ImpactCue != null ? ((FSoftObjectPath)ImpactCue.GenericValue).AssetPathName.Text : "/");
        output.Add("ActorClass", ActorClass.AssetPathName.Text);
        output.Add("Trail", Trail.AssetPathName.Text);
        output.Add("OffhandTrail", OffhandTrail.AssetPathName.Text);
        output.Add("Series", Series);

        
        foreach (var str in output)
        {
            if (String.IsNullOrEmpty(str.Value))
                output[str.Key] = null;
            
            Logger.Log(str.Key + ": " + str.Value ?? "Null");
        }

        return output;
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
                                                Logger.Log("Skipping " + VariantName.Text);
                                                continue;
                                            }

                                            Logger.Log("Found Item: " + VariantName.Text);
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
                                                Logger.Log("Skipping " + VariantName.Text);
                                                continue;
                                            }

                                            Logger.Log("Found Item: " + VariantName.Text);
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
                    characterPart.TryGetValue(out EFortCustomPartType CustomPartType, "CharacterPartType");

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

        foreach (var cp in cps)
        {
            Logger.Log(cp.Key + ": " + cp.Value, LogLevel.Debug);
        }

        return cps;
    }

    #region GenerateBackbling
    private async Task<SaturnOption> GenerateMeshBackbling(Cosmetic item, SaturnItem option)
    {
        Logger.Log($"Getting cp for {item.Name}");
        var characterPart = await GetBackblingCharacterPart(item);
        Logger.Log("Backbling character part: " + characterPart);

        try
        {
            var changes = _cloudStorageService.GetChanges(item.Id, "CharacterPartReplacements");
            var cloudChanges = _cloudStorageService.DecodeChanges(changes);

            characterPart = cloudChanges.CharacterPartsReplace[0];
        }
        catch
        {
            // Ignored
        }

        var data = await GetDataFromBackblingCharacterPart(characterPart);

        Logger.Log("Generating swaps");

        switch (option.ItemDefinition)
        {
            case "BID_430_GalileoSpeedBoat_9RXE3":
                if (data["Material"] != "/")
                    option.Status = "This item might not be perfect!";
                break;
            case "BID_678_CardboardCrewHolidayMale":
                if (data["Material"] != "/" || data["FX"] != "/")
                    option.Status = "This item might not be perfect!";
                break;
            case "BID_695_StreetFashionEclipse":
                if (data["FX"] != "/")
                    option.Status = "This item might not be perfect!";
                break;
        }


        return option.ItemDefinition switch
        {
            "BID_695_StreetFashionEclipse" => new BlackoutBagBackblingSwap(item.Name,
                                                                           item.Images.SmallIcon,
                                                                           item.Rarity.BackendValue,
                                                                           data).ToSaturnOption(),
            "BID_600_HightowerTapas" => new ThorsCloakBackblingSwap(item.Name,
                                                                    item.Images.SmallIcon,
                                                                    item.Rarity.BackendValue,
                                                                    data).ToSaturnOption(),
            "BID_678_CardboardCrewHolidayMale" => new WrappingCaperBackblingSwap(item.Name,
                                                                                 item.Images.SmallIcon,
                                                                                 item.Rarity.BackendValue,
                                                                                 data).ToSaturnOption(),
            "BID_430_GalileoSpeedBoat_9RXE3" => new TheSithBackblingSwap(item.Name,
                                                                         item.Images.SmallIcon,
                                                                         item.Rarity.BackendValue,
                                                                         data).ToSaturnOption(),
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

        var characterParts = Task.Run(() => GetCharacterPartsById(item.Id, item)).GetAwaiter().GetResult();

        if (characterParts == new Dictionary<string, string>())
            return null;

        Logger.Log("Creating swap model");

        MeshDefaultModel swapModel = new()
        {
            HeadMaterials = new Dictionary<int, string>(),
            HeadHairColor = "/Game/Tamely",
            HeadFX = "/Game/Tamely",
            HeadSkinColor = "/Game/Tamely",
            HeadPartModifierBP = "/Game/Tamely",
            HeadMesh = "/Game/Tamely",
            HeadABP = null,
            BodyFX = "/Game/Tamely",
            BodyPartModifierBP = "/Game/Tamely",
            BodyABP = null,
            BodyMesh = "/Game/Tamely",
            BodyMaterials = new Dictionary<int, string>(),
            BodySkeleton = "/Game/Tamely",
            FaceACCMaterials = new Dictionary<int, string>(),
            FaceACCMesh = "/Game/Tamely",
            FaceACCABP = null,
            FaceACCFX = "/Game/Tamely",
            FaceACCPartModifierBP = "/Game/Tamely",
            HatType = ECustomHatType.ECustomHatType_None
        };

        Dictionary<string, string> MaterialReplacements = new Dictionary<string, string>();
        await Task.Run(() =>
        {
            if (item is {VariantChannel: { }})
            {
                if (item.VariantChannel.ToLower() != "material" &&
                    item.VariantChannel.ToLower() != "parts" &&
                    item.VariantTag != null ||
                    !_provider.TryLoadObject(Constants.CidPath + item.Id, out var CharacterItemDefinition) ||
                    !CharacterItemDefinition.TryGetValue(out UObject[] ItemVariants, "ItemVariants")) 
                    return;
                foreach (var style in ItemVariants)
                {
                    if (style.TryGetValue(out FStructFallback[] PartOptions, "PartOptions"))
                        foreach (var PartOption in PartOptions)
                        {
                            if (PartOption.TryGetValue(out FText VariantName, "VariantName"))
                            {
                                Logger.Log("Found Item: " + VariantName.Text);
                                if (VariantName.Text != item.Name && item.VariantTag != null)
                                {
                                    Logger.Log("Skipping " + VariantName.Text);
                                    continue;
                                }

                                Logger.Log("Found Item: " + VariantName.Text);

                            
                                if (PartOption.TryGetValue(out FStructFallback[] VariantMaterials,"VariantMaterials"))
                                    foreach (var variantMaterial in VariantMaterials)
                                    {
                                        var matOverride = variantMaterial.Get<FSoftObjectPath>("OverrideMaterial").AssetPathName.Text;
                                        var MaterialToSwap = variantMaterial.Get<FSoftObjectPath>("MaterialToSwap").AssetPathName.Text;

                                        Logger.Log("Original material: " + MaterialToSwap);
                                        Logger.Log("Override material: " + matOverride);
                                        MaterialReplacements.Add(MaterialToSwap, matOverride);
                                    }
                            }
                            else
                            {
                                Logger.Log("No VariantName found");
                            }
                        }
                    else
                        Logger.Log("No PartOptions found");
                    
                    
                    if (style.TryGetValue(out FStructFallback[] MaterialOptions, "MaterialOptions"))
                        foreach (var MaterialOption in MaterialOptions)
                        {
                            if (MaterialOption.TryGetValue(out FText VariantName, "VariantName"))
                            {
                                if (VariantName.Text != item.Name && item.VariantTag != null)
                                {
                                    Logger.Log("Skipping " + VariantName.Text);
                                    continue;
                                }

                                Logger.Log("Found Item: " + VariantName.Text);

                            
                                if (MaterialOption.TryGetValue(out FStructFallback[] VariantMaterials,"VariantMaterials"))
                                    foreach (var variantMaterial in VariantMaterials)
                                    {
                                        var matOverride = variantMaterial.Get<FSoftObjectPath>("OverrideMaterial").AssetPathName.Text;
                                        var MaterialToSwap = variantMaterial.Get<FSoftObjectPath>("MaterialToSwap").AssetPathName.Text;

                                        Logger.Log("Original material: " + MaterialToSwap);
                                        Logger.Log("Override material: " + matOverride);
                                        MaterialReplacements.Add(MaterialToSwap, matOverride);
                                    }
                            }
                            else
                            {
                                Logger.Log("No VariantName found");
                            }
                        }
                    else
                        Logger.Log("No MaterialOptions found");
                }
            }
        });

        Dictionary<int, string> OGHeadMaterials = new();
        var optionsParts = Task.Run(() => GetCharacterPartsById(option.ItemDefinition)).GetAwaiter()
            .GetResult();

        await Task.Run(() =>
        {
            if (!_provider.TryLoadObject(optionsParts["Head"].Split('.')[0], out var part) ||
                !part.TryGetValue(out FStructFallback[] MaterialOverride, "MaterialOverrides")) return;
            foreach (var (material, matIndex) in from materialOverride in MaterialOverride
                     let material = materialOverride.Get<FSoftObjectPath>("OverrideMaterial").AssetPathName.ToString()
                     let matIndex = materialOverride.Get<int>("MaterialOverrideIndex")
                     select (material, matIndex))
            {
                OGHeadMaterials.Add(matIndex, material);
            }
        });

        Logger.Log("Looping through character parts");
        foreach (var characterPart in characterParts)
        {
            Logger.Log($"Getting strings in asset: {characterPart.Value}");
            switch (characterPart.Key)
            {
                case "Body":
                    Logger.Log("Character part is type: Body");

                    await Task.Run(() =>
                    {
                        if (_provider.TryLoadObject(characterPart.Value.Split('.')[0], out var part))
                        {
                            if (part.TryGetValue(out FSoftObjectPath mesh, "SkeletalMesh"))
                                swapModel.BodyMesh = mesh.AssetPathName.Text;

                            swapModel.BodySkeleton =
                                part.Get<FSoftObjectPath[]>("MasterSkeletalMeshes")[0].AssetPathName.ToString();

                            if (part.TryGetValue(out UObject AdditionalData, "AdditionalData"))
                            {
                                FSoftObjectPath AnimClass = AdditionalData.GetOrDefault("AnimClass",
                                    new FSoftObjectPath(), StringComparison.OrdinalIgnoreCase);
                                swapModel.BodyABP = AnimClass.AssetPathName.ToString();
                            }


                            if (part.TryGetValue(out FStructFallback[] MaterialOverride, "MaterialOverrides"))
                            {
                                foreach (var materialOverride in MaterialOverride)
                                {
                                    var material = materialOverride.Get<FSoftObjectPath>("OverrideMaterial")
                                        .AssetPathName.ToString();

                                    if (MaterialReplacements.ContainsKey(material))
                                    {
                                        string temp = material;
                                        material = MaterialReplacements[material];
                                        MaterialReplacements.Remove(temp);
                                    }

                                    var matIndex = materialOverride.Get<int>("MaterialOverrideIndex");
                                    swapModel.BodyMaterials.Add(matIndex, material);
                                }
                            }


                            swapModel.BodyFX =
                                part.TryGetValue(out FSoftObjectPath IdleEffectNiagara, "IdleEffectNiagara")
                                    ? IdleEffectNiagara.AssetPathName.ToString()
                                    : "/";

                            if (part.TryGetValue(out FSoftObjectPath IdleEffect, "IdleEffect") &&
                                swapModel.BodyFX == "/")
                                swapModel.BodyFX = IdleEffect.AssetPathName.ToString();

                            if (part.TryGetValue(out FSoftObjectPath BodyPartModifierBP, "PartModifierBlueprint"))
                                swapModel.BodyPartModifierBP = BodyPartModifierBP.AssetPathName.ToString();
                        }
                    });
                    break;
                    
                case "Head":
                    Logger.Log("Character part is type: Head");

                    await Task.Run(() =>
                    {
                        if (_provider.TryLoadObject(characterPart.Value.Split('.')[0], out var part))
                        {
                            swapModel.HeadMesh = part.Get<FSoftObjectPath>("SkeletalMesh").AssetPathName.Text;

                            if (part.TryGetValue(out UObject AdditionalData, "AdditionalData"))
                            {
                                if (AdditionalData.TryGetValue(out FSoftObjectPath AnimClass, "AnimClass"))
                                    swapModel.HeadABP = AnimClass.AssetPathName.Text;

                                swapModel.HeadHairColor = AdditionalData.TryGetValue(out FSoftObjectPath HairColorSwatch, "HairColorSwatch") 
                                    ? HairColorSwatch.AssetPathName.Text 
                                    : "/";
                                    
                                swapModel.HeadSkinColor = AdditionalData.TryGetValue(out FSoftObjectPath SkinColorSwatch, "SkinColorSwatch") 
                                    ? SkinColorSwatch.AssetPathName.Text
                                    : "/";
                            }
                                
                                
                            if (part.TryGetValue(out FStructFallback[] MaterialOverride, "MaterialOverrides"))
                            {
                                foreach (var materialOverride in MaterialOverride)
                                {
                                    var material = materialOverride.Get<FSoftObjectPath>("OverrideMaterial").AssetPathName.Text;
                                    
                                    if (MaterialReplacements.ContainsKey(material))
                                    {
                                        string temp = material;
                                        material = MaterialReplacements[material];
                                        MaterialReplacements.Remove(temp);
                                    }
                                    
                                    var matIndex = materialOverride.Get<int>("MaterialOverrideIndex");
                                    swapModel.HeadMaterials.Add(matIndex, material);
                                }
                            }

                            swapModel.HeadFX =
                                part.TryGetValue(out FSoftObjectPath IdleEffectNiagara, "IdleEffectNiagara")
                                    ? IdleEffectNiagara.AssetPathName.ToString()
                                    : "/";

                            if (part.TryGetValue(out FSoftObjectPath IdleEffect, "IdleEffect") && swapModel.HeadFX == "/")
                                swapModel.HeadFX = IdleEffect.AssetPathName.Text;
                                
                            if (part.TryGetValue(out FSoftObjectPath BodyPartModifierBP, "PartModifierBlueprint"))
                                swapModel.HeadPartModifierBP = BodyPartModifierBP.AssetPathName.Text;
                        }
                    });
                    break;
                    
                case "Face":
                case "Hat":
                    Logger.Log("Character part is type: Hat or FaceACC");

                    await Task.Run(() =>
                    {
                        if (_provider.TryLoadObject(characterPart.Value.Split('.')[0], out var part))
                        {
                            swapModel.FaceACCMesh = part.Get<FSoftObjectPath>("SkeletalMesh").AssetPathName.Text;

                            // This is for skins like ghoul trooper and maven
                            if (swapModel.FaceACCMesh.ToLower().Contains("glasses"))
                            {
                                swapModel.FaceACCMesh = "/";
                                swapModel.HatType = ECustomHatType.ECustomHatType_None;
                                return;
                            }

                            if (part.TryGetValue(out UObject AdditionalData, "AdditionalData"))
                            {
                                swapModel.FaceACCABP = AdditionalData.GetOrDefault("AnimClass", new FSoftObjectPath(),
                                    StringComparison.OrdinalIgnoreCase).AssetPathName.Text;

                                swapModel.HatType = AdditionalData.GetOrDefault("HatType",
                                    ECustomHatType.ECustomHatType_None, StringComparison.OrdinalIgnoreCase);
                            }

                            if (part.TryGetValue(out FStructFallback[] MaterialOverride, "MaterialOverrides"))
                            {
                                foreach (var materialOverride in MaterialOverride)
                                {
                                    var material = materialOverride.Get<FSoftObjectPath>("OverrideMaterial").AssetPathName.ToString();
                                    
                                    if (MaterialReplacements.ContainsKey(material))
                                    {
                                        string temp = material;
                                        material = MaterialReplacements[material];
                                        MaterialReplacements.Remove(temp);
                                    }
                                    
                                    var matIndex = materialOverride.Get<int>("MaterialOverrideIndex");
                                    swapModel.FaceACCMaterials.Add(matIndex, material);
                                }
                            }


                            swapModel.FaceACCFX =
                                part.TryGetValue(out FSoftObjectPath IdleEffectNiagara, "IdleEffectNiagara")
                                    ? IdleEffectNiagara.AssetPathName.ToString()
                                    : "/";
                            
                            if (part.TryGetValue(out FSoftObjectPath IdleEffect, "IdleEffect") && swapModel.FaceACCFX == "/")
                                swapModel.FaceACCFX = IdleEffect.AssetPathName.Text;
                                
                            if (part.TryGetValue(out FSoftObjectPath FaceACCPartModifierBP, "PartModifierBlueprint"))
                                swapModel.FaceACCPartModifierBP = FaceACCPartModifierBP.AssetPathName.Text;
                        }
                    });
                    break;
            }
        }
        
        foreach (var (material, value) in MaterialReplacements)
        {
            if (material.ToLower().Contains("hat") || material.ToLower().Contains("helmet") ||
                material.ToLower().Contains("faceacc") || material.ToLower().Contains("mask"))
            {
                int i = 0;
                while (swapModel.FaceACCMaterials.ContainsKey(i)) i++;
                swapModel.FaceACCMaterials.Add(i, value);
            }
            else if (material.ToLower().Contains("head") || material.ToLower().Contains("hair"))
            {
                int i = 0;
                while (swapModel.HeadMaterials.ContainsKey(i)) i++;
                swapModel.HeadMaterials.Add(i, value);
            }
            else if (material.ToLower().Contains("body") || material.ToLower().Contains("bodies"))
            {
                int i = 0;
                while (swapModel.BodyMaterials.ContainsKey(i)) i++;
                swapModel.BodyMaterials.Add(i, value);
            }
        }

        if ((swapModel.HeadMesh.ToLower().Contains("ramirez")) &&
            !swapModel.HeadMesh.ToLower().Contains("/parts/"))
        {
            foreach (var material in swapModel.HeadMaterials)
            {
                if (!material.Value.ToLower().Contains("hair") ||
                    !OGHeadMaterials[material.Key].ToLower().Contains("hair") ||
                    material.Value.ToLower().Contains("hide")) continue;
                foreach (var ogMaterial in OGHeadMaterials.Where(ogMaterial 
                             => ogMaterial.Value.ToLower().Contains("hair")))
                {
                    (swapModel.HeadMaterials[material.Key], swapModel.HeadMaterials[ogMaterial.Key]) = (
                        swapModel.HeadMaterials[ogMaterial.Key], swapModel.HeadMaterials[material.Key]);
                }
            }
        }
        
        if (option.Name == "Blizzabelle")
        {
            if (swapModel.HeadMaterials.Count > 1 && swapModel.FaceACCMaterials.Count < 2)
            {
                (swapModel.FaceACCMesh, swapModel.HeadMesh) = (swapModel.HeadMesh, swapModel.FaceACCMesh);
                (swapModel.FaceACCABP, swapModel.HeadABP) = (swapModel.HeadABP, swapModel.FaceACCABP);
                (swapModel.FaceACCMaterials, swapModel.HeadMaterials) = (swapModel.HeadMaterials, swapModel.FaceACCMaterials);
                (swapModel.HeadFX, swapModel.FaceACCFX) = (swapModel.FaceACCFX, swapModel.HeadFX);
                (swapModel.HeadPartModifierBP, swapModel.FaceACCPartModifierBP) = (swapModel.FaceACCPartModifierBP, swapModel.HeadPartModifierBP);
            }
        }
            
        if (swapModel.BodyMaterials == new Dictionary<int, string>() || swapModel.BodyMaterials.Count < 5)
            for (int i = swapModel.BodyMaterials.Count; i < 5; i++)
                swapModel.BodyMaterials.Add(i, "/");
            
        if (swapModel.HeadMaterials == new Dictionary<int, string>() || swapModel.HeadMaterials.Count < 5)
            for (int i = swapModel.HeadMaterials.Count; i < 5; i++)
                if (swapModel.HeadMaterials.ContainsKey(i))
                    swapModel.HeadMaterials.Add(i - 1, "/");
                else
                 swapModel.HeadMaterials.Add(i, "/");
            
        if (swapModel.FaceACCMaterials == new Dictionary<int, string>() || swapModel.FaceACCMaterials.Count < 5)
            for (int i = swapModel.FaceACCMaterials.Count; i < 5; i++)
                swapModel.FaceACCMaterials.Add(i, "/");

        if (swapModel.FaceACCABP == "None")
            swapModel.FaceACCABP = null;
        if (swapModel.HeadABP == "None")
            swapModel.HeadABP = null;
        if (swapModel.BodyABP == "None")
            swapModel.BodyABP = null;

        if (string.IsNullOrEmpty(swapModel.BodyABP))
            swapModel.BodyABP = null;
        if (string.IsNullOrEmpty(swapModel.HeadABP))
            swapModel.HeadABP = null;
        if (string.IsNullOrEmpty(swapModel.FaceACCABP))
            swapModel.FaceACCABP = null;

        Logger.Log($"Head hair color: {swapModel.HeadHairColor}");
        Logger.Log($"Head skin color: {swapModel.HeadSkinColor}");
        Logger.Log($"Head part modifier bp: {swapModel.HeadPartModifierBP}");
        Logger.Log($"Head FX: {swapModel.HeadFX}");
        Logger.Log($"Head mesh: {swapModel.HeadMesh}");
        Logger.Log($"Head ABP: {swapModel.HeadABP ?? "Null"}");
        Logger.Log($"Head Materials:");
        foreach (var material in swapModel.HeadMaterials)
            Logger.Log($"\t{material.Key}: {material.Value}");
        Logger.Log($"Body ABP: {swapModel.BodyABP ?? "Null"}");
        Logger.Log($"Body mesh: {swapModel.BodyMesh}");
        Logger.Log($"Body materials:");
        foreach (var material in swapModel.BodyMaterials)
            Logger.Log($"\t{material.Key}: {material.Value}");
        Logger.Log($"Body skeleton: {swapModel.BodySkeleton}");
        Logger.Log($"Body part modifier BP: {swapModel.BodyPartModifierBP}");
        Logger.Log($"Body FX: {swapModel.BodyFX}");
        Logger.Log($"Face ACC materials:");
        foreach (var material in swapModel.FaceACCMaterials)
            Logger.Log($"\t{material.Key}: {material.Value}");
        Logger.Log($"Face ACC mesh: {swapModel.FaceACCMesh}");
        Logger.Log($"Face ACC ABP: {swapModel.FaceACCABP ?? "Null"}");
        Logger.Log($"Face ACC part modifier BP: {swapModel.FaceACCPartModifierBP}");
        Logger.Log($"Face ACC FX: {swapModel.FaceACCFX}");

        Logger.Log("Generating swaps");

        return option.ItemDefinition switch
        {
            "CID_162_Athena_Commando_F_StreetRacer" => new RedlineSkinSwap(item.Name, 
                                                                           item.Rarity.BackendValue, 
                                                                           item.Images.SmallIcon, 
                                                                           swapModel).ToSaturnOption(),
            "CID_653_Athena_Commando_F_UglySweaterFrozen" => new FrozenNogOpsSkinSwap(item.Name,
                                                                                      item.Rarity.BackendValue,
                                                                                      item.Images.SmallIcon,
                                                                                      swapModel).ToSaturnOption(),
            "CID_784_Athena_Commando_F_RenegadeRaiderFire" => new BlazeSkinSwap(item.Name,
                                                                                item.Rarity.BackendValue,
                                                                                item.Images.SmallIcon,
                                                                                swapModel).ToSaturnOption(),
            "CID_970_Athena_Commando_F_RenegadeRaiderHoliday" => new GingerbreadRaiderSkinSwap(item.Name,
                                                                                               item.Rarity.BackendValue,
                                                                                               item.Images.SmallIcon,
                                                                                               swapModel).ToSaturnOption(),
            "CID_A_322_Athena_Commando_F_RenegadeRaiderIce" => new PermafrostRaiderSkinSwap(item.Name,
                                                                                            item.Rarity.BackendValue,
                                                                                            item.Images.SmallIcon,
                                                                                            swapModel).ToSaturnOption(),
            "CID_936_Athena_Commando_F_RaiderSilver" => new DiamondDivaSkinSwap(item.Name,
                                                                                item.Rarity.BackendValue,
                                                                                item.Images.SmallIcon,
                                                                                swapModel).ToSaturnOption(),
            "CID_A_007_Athena_Commando_F_StreetFashionEclipse" => new RubyShadowsSkinSwap(item.Name,
                                                                                          item.Rarity.BackendValue,
                                                                                          item.Images.SmallIcon,
                                                                                          swapModel).ToSaturnOption(),
            "CID_A_311_Athena_Commando_F_ScholarFestiveWinter" => new BlizzabelleSkinSwap(item.Name,
                                                                                          item.Rarity.BackendValue,
                                                                                          item.Images.SmallIcon,
                                                                                          swapModel).ToSaturnOption(),
            "CID_294_Athena_Commando_F_RedKnightWinter" => new FrozenRedKnightSkinSwap(item.Name,
                                                                                       item.Rarity.BackendValue,
                                                                                       item.Images.SmallIcon,
                                                                                       swapModel).ToSaturnOption(),
            _ => new SaturnOption()
        };
    }
    #endregion

    #region GenerateEmoteSwaps
    private async Task<SaturnOption> GenerateMeshEmote(Cosmetic item, SaturnItem option)
    {
        var swaps = await GetEmoteDataByItem(item);
        if (swaps == new Dictionary<string, string>())
        {
            await ItemUtil.UpdateStatus(item, option, $"Failed to find data for \"{item.Id}\"!",
                Colors.C_YELLOW);
            Logger.Log($"Failed to find data for \"{item.Id}\"!", LogLevel.Error);
            return new SaturnOption();
        }

        await _jsRuntime.InvokeVoidAsync("MessageBox", "Don't put this emote in your selected emotes!",
            "If you are going to use it in-game, favorite the emote and select it from your favorites! Fortnite will kick you if it's in your 6 selections!",
            "warning");

        Logger.Log("CMM: " + swaps["CMM"]);

        return option.ItemDefinition switch
        {
            "EID_DanceMoves" => new DanceMovesEmoteSwap(item.Name,
                                                        item.Rarity.BackendValue,
                                                        item.Images.SmallIcon,
                                                        swaps).ToSaturnOption(),
            "EID_BoogieDown" => new BoogieDownEmoteSwap(item.Name,
                                                        item.Rarity.BackendValue,
                                                        item.Images.SmallIcon,
                                                        swaps).ToSaturnOption(),
            _ => new SaturnOption()
        };
    }
    #endregion
    
    private async Task<SaturnOption> GenerateMeshPickaxe(Cosmetic item, SaturnItem option)
    {
        Logger.Log($"Getting wid for {item.Name}");
        var swaps = await GetAssetsFromWID(item.DefinitionPath);
        
        Logger.Log("Generating swaps");
        EFortRarity Rarity = (EFortRarity)int.Parse(swaps["Rarity"]);
        
        List<byte[]> SeriesBytes = new List<byte[]>();

        if (swaps["FX"] == "None")
            swaps["FX"] = "/";

        switch (option.ItemDefinition)
        {
            case "DefaultPickaxe":
                if (swaps["FX"] != "/" || swaps["Material"] != "/" || swaps["ActorClass"] != "/Game/Weapons/FORT_Melee/Blueprints/B_Athena_Pickaxe_Generic.B_Athena_Pickaxe_Generic_C")
                    option.Status = "This item might not be perfect!";
                break;
            case "Pickaxe_ID_541_StreetFashionEclipseFemale":
                if (swaps["FX"] != "/" || swaps["ActorClass"] != "/Game/Weapons/FORT_Melee/Blueprints/B_Athena_Pickaxe_Generic.B_Athena_Pickaxe_Generic_C")
                    option.Status = "This item might not be perfect!";
                if (swaps["Series"] != "/" && await _configService.TryGetShouldSeriesConvert())
                {
                    Rarity = EFortRarity.Transcendent;
                    SeriesBytes = await FileUtil.GetColorsFromSeries(swaps["Series"], _provider);
                }
                break;
            case "Pickaxe_ID_408_MastermindShadow":
                if (swaps["ActorClass"] != "/Game/Weapons/FORT_Melee/Blueprints/B_Athena_Pickaxe_Generic.B_Athena_Pickaxe_Generic_C")
                    option.Status = "This item might not be perfect!";
                if (swaps["Series"] != "/" && await _configService.TryGetShouldSeriesConvert())
                {
                    Rarity = EFortRarity.Transcendent;
                    SeriesBytes = await FileUtil.GetColorsFromSeries(swaps["Series"], _provider);
                }
                break;
            case "Pickaxe_ID_713_GumballMale":
                if (swaps["ActorClass"] != "/Game/Weapons/FORT_Melee/Blueprints/Impact/B_Athena_Pickaxe_Sythe1H.B_Athena_Pickaxe_Sythe1H_C")
                    option.Status = "This item might not be perfect!";
                if (swaps["Series"] != "/" && await _configService.TryGetShouldSeriesConvert())
                {
                    Rarity = EFortRarity.Transcendent;
                    SeriesBytes = await FileUtil.GetColorsFromSeries(swaps["Series"], _provider);
                }
                break;
        }

        var output = option.ItemDefinition switch
        {
            "Pickaxe_ID_408_MastermindShadow" => new MayhemScytheSwap(
                                                    item.Name,
                                                    item.Rarity.Value,
                                                    item.Images.SmallIcon,
                                                    swaps,
                                                    Rarity).ToSaturnOption(),
            "DefaultPickaxe" => new DefaultPickaxeSwap(
                                    item.Name,
                                    item.Rarity.Value,
                                    item.Images.SmallIcon,
                                    swaps).ToSaturnOption(),
            "Pickaxe_ID_541_StreetFashionEclipseFemale" => new ShadowSlicerSwap(
                                                                item.Name,
                                                                item.Rarity.Value,
                                                                item.Images.SmallIcon,
                                                                swaps,
                                                                Rarity).ToSaturnOption(),
            "Pickaxe_ID_713_GumballMale" => new GumBrawlerSwap(
                                                item.Name,
                                                item.Rarity.Value,
                                                item.Images.SmallIcon,
                                                swaps,
                                                Rarity).ToSaturnOption(),
            _ => new SaturnOption()
            
        };

        #region Default Pickaxe Rarity and Series Swaps

                if (SeriesBytes != new List<byte[]>() && await _configService.TryGetShouldSeriesConvert() && option.ItemDefinition != "DefaultPickaxe")
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
        else if (SeriesBytes != new List<byte[]>() && await _configService.TryGetShouldSeriesConvert() &&
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

        foreach (var (fileExt, path) in from fileExt in fileExts
                                        let path = Path.Combine(FortniteUtil.PakPath, fileName + fileExt)
                                        select (fileExt, path))
        {
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
}
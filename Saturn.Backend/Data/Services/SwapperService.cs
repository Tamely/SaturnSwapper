using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
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
using Index = Saturn.Backend.Pages.Index;

namespace Saturn.Backend.Data.Services
{
    public interface ISwapperService
    {
        public Task<bool> Convert(Cosmetic item, SaturnItem option, ItemType itemType, bool isAuto = true, bool isRandom = false, Cosmetic random = null);
        public Task<bool> Revert(Cosmetic item, SaturnItem option, ItemType itemType);
        public Task Swap(Cosmetic item, SaturnItem option, ItemType itemType, List<Cosmetic> Items, bool isAuto = true);
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

        public async Task Swap(Cosmetic item, SaturnItem option, ItemType itemType, List<Cosmetic> Items, bool isAuto = true)
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
                        }   
                    }
                    else if (!await Convert(item, option, itemType, isAuto))
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

        public async Task<bool> Convert(Cosmetic item, SaturnItem option, ItemType itemType, bool isDefault = true, bool isRandom = false, Cosmetic random = null)
        {
            try
            {
                option.Status = null;
                var itemCheck = await IsTypeConverted(itemType);
                if (itemCheck != null)
                {
                    option.Status =
                            $"Revert {itemCheck} before converting another item of the same type.";
                    return false;
                }

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

                    var itemSwap = itemType switch
                    {
                        ItemType.IT_Skin => await GenerateMeshSkins(item, option),
                        ItemType.IT_Dance => await GenerateMeshEmote(item, option),
                        ItemType.IT_Pickaxe => await GenerateMeshPickaxe(item, option),
                        _ => new()
                    };

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
                        Logger.Log("Asset exported");
                        Logger.Log($"Starting backup of {SaturnData.Path}");

                        var file = SaturnData.Path.Replace("utoc", "ucas");

                        if (isRandom)
                            await BackupFile(file, random, option);
                        else
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

                        if (cloudChanges.SkinName == itemSwap.Name)
                        {
                            if (cloudChanges.Searches[0] != "none")
                            {
                                Trace.WriteLine("Searches are not empty");
                                itemSwap.Assets[itemSwap.Assets.IndexOf(asset)].Swaps = new();
                                foreach (var search in cloudChanges.Searches)
                                {
                                    itemSwap.Assets[itemSwap.Assets.IndexOf(asset)].Swaps[cloudChanges.Searches.IndexOf(search)].Search = search;
                                    itemSwap.Assets[itemSwap.Assets.IndexOf(asset)].Swaps[cloudChanges.Searches.IndexOf(search)].Replace = cloudChanges.Replaces[cloudChanges.Searches.IndexOf(search)];
                                }
                            }
                                    
                            if (cloudChanges.CharacterParts[0] != "none")
                            {
                                Trace.WriteLine("CharacterParts are not empty");
                                foreach (var swap in itemSwap.Assets[itemSwap.Assets.IndexOf(asset)].Swaps)
                                    swap.Replace = "/Game/Tamely";
                                foreach (var characterPart in cloudChanges.CharacterParts)
                                    itemSwap.Assets[itemSwap.Assets.IndexOf(asset)].Swaps[cloudChanges.CharacterParts.IndexOf(characterPart)].Replace = characterPart;
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

        public async Task<string> IsTypeConverted(ItemType itemType)
        {
            foreach (var convItem in (await _configService.TryGetConvertedItems()).Where(x =>
                x.Type == itemType.ToString()))
                return convItem.Name;
            return null;
        }

        public async Task<Dictionary<string, string>> GetAssetsFromWID(string wid)
        {
            var output = new Dictionary<string, string>();
            
            
            var strs = await FileUtil.GetStringsFromAsset(wid, _provider);

            
            string Mesh = "/";
            string Material = "/";
            string SmallIcon = "/";
            string LargeIcon = "/";
            string FX = "/";
            string SwingCue = "/";
            string EquipCue = "/";
            string ImpactCue = "/";
            string ActorClass = "/";
            string Trail = "/";
            
            foreach (var str in strs)
            {
                if (str.Contains('.'))
                {
                    if (str.ToLower().Contains("meshes"))
                        Mesh = str;
                    if (str.ToLower().Contains("material"))
                        Material = str;
                    if (str.ToLower().Contains("impact") && !str.ToLower().Contains("fx"))
                        ImpactCue = str;
                    if (str.ToLower().Contains("swing") && !str.ToLower().Contains("fx"))
                        SwingCue = str;
                    if ((str.ToLower().Contains("ready") || str.ToLower().Contains("equip")) &&
                        !str.ToLower().Contains("fx"))
                        EquipCue = str;
                    if (str.ToLower().Contains("icon") && str.ToLower().Contains("-l"))
                        LargeIcon = str;
                    if (str.ToLower().Contains("icon") && !str.ToLower().Contains("-l"))
                        SmallIcon = str;
                    if (str ==
                        "/Game/Weapons/FORT_Melee/Blueprints/B_Athena_Pickaxe_Generic.B_Athena_Pickaxe_Generic_C")
                        ActorClass = str;
                    if (str.ToLower().Contains("fx") && !str.ToLower().Contains("trail"))
                        FX = str;
                    if (str.ToLower().Contains("fx") && str.ToLower().Contains("trail"))
                        Trail = str;
                }
            }
            
            output.Add("Mesh", Mesh);
            output.Add("Material", Material);
            output.Add("SmallIcon", SmallIcon);
            output.Add("LargeIcon", LargeIcon);
            output.Add("FX", FX);
            output.Add("SwingCue", SwingCue);
            output.Add("EquipCue", EquipCue);
            output.Add("ImpactCue", ImpactCue);
            output.Add("ActorClass", ActorClass);
            output.Add("Trail", Trail);

            return output;
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
            
            Logger.Log("CMM: " + swaps["CMM"]);

            return new SaturnOption
            {
                Name = item.Name,
                Icon = item.Images.SmallIcon,
                Rarity = item.Rarity.BackendValue,
                Assets = new List<SaturnAsset>
                {
                    new SaturnAsset
                    {
                        ParentAsset = "FortniteGame/Content/Athena/Items/Cosmetics/Dances/EID_DanceMoves",
                        Swaps = new List<SaturnSwap>
                        {
                            new()
                            {
                                Search = "/Game/Animation/Game/MainPlayer/Montages/Emotes/Emote_DanceMoves.Emote_DanceMoves",
                                Replace = swaps["CMM"],
                                Type = SwapType.BodyAnim
                            },
                            new()
                            {
                                Search = "/Game/UI/Foundation/Textures/Icons/Emotes/T-Icon-Emotes-E-Dance.T-Icon-Emotes-E-Dance",
                                Replace = swaps["SmallIcon"],
                                Type = SwapType.Modifier
                            },
                            new()
                            {
                                Search = "/Game/UI/Foundation/Textures/Icons/Emotes/T-Icon-Emotes-E-Dance-L.T-Icon-Emotes-E-Dance-L",
                                Replace = "/",
                                Type = SwapType.Modifier
                            }
                        }
                    }
                }
            };
        }

        #endregion
        
        #region GeneratePickaxeSwaps
        
        private async Task<SaturnOption> GenerateMeshPickaxe(Cosmetic item, SaturnItem option)
        {
            Logger.Log($"Getting wid for {item.Name}");
            var swaps = await GetAssetsFromWID(item.DefinitionPath);
            
            if (swaps["FX"] != "/" || swaps["Material"] != "/" || swaps["ActorClass"] == "/")
                option.Status = "This item might not be perfect!";
            

            Logger.Log("Generating swaps");

            var output = new SaturnOption()
            {
                Name = item.Name,
                Icon = item.Images.SmallIcon,
                Rarity = item.Rarity.BackendValue,
                Assets = new List<SaturnAsset>()
                {
                    new SaturnAsset()
                    {
                        ParentAsset = "FortniteGame/Content/Athena/Items/Weapons/WID_Harvest_Pickaxe_Athena_C_T01",
                        Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search =
                                    "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-DefaultMarkIIIPickaxe.T-Icon-Pickaxes-DefaultMarkIIIPickaxe",
                                Replace = swaps["SmallIcon"],
                                Type = SwapType.SmallIcon
                            },
                            new SaturnSwap()
                            {
                                Search =
                                    "/Game/UI/Foundation/Textures/Icons/Weapons/Items/T-Icon-Pickaxes-DefaultMarkIIIPickaxe-L.T-Icon-Pickaxes-DefaultMarkIIIPickaxe-L",
                                Replace = swaps["LargeIcon"],
                                Type = SwapType.LargeIcon
                            },
                            new SaturnSwap()
                            {
                                Search =
                                    "/Game/Weapons/FORT_Melee/Pickaxe_Default_Mark_III/Meshes/Default_Mark_III_Axe.Default_Mark_III_Axe",
                                Replace = swaps["Mesh"],
                                Type = SwapType.WeaponMesh
                            },
                            new SaturnSwap()
                            {
                                Search =
                                    "/Game/Athena/Sounds/Weapons/PickAxes/MarkIIIMale/PickaxeSwing_MarkIIIMale.PickaxeSwing_MarkIIIMale",
                                Replace = swaps["SwingCue"],
                                Type = SwapType.WeaponSound
                            },
                            new SaturnSwap()
                            {
                                Search =
                                    "/Game/Athena/Sounds/Weapons/PickAxes/MarkIIIMale/PickaxeReady_MarkIIIMale.PickaxeReady_MarkIIIMale",
                                Replace = swaps["EquipCue"],
                                Type = SwapType.WeaponSound
                            },
                            new SaturnSwap()
                            {
                                Search =
                                    "/Game/Athena/Sounds/Weapons/PickAxes/MarkIIIMale/PickaxeImpactEnemy_MarkIIIMale.PickaxeImpactEnemy_MarkIIIMale",
                                Replace = swaps["ImpactCue"],
                                Type = SwapType.WeaponSound
                            },
                            new SaturnSwap()
                            {
                                Search =
                                    "/Game/Weapons/FORT_Melee/Pickaxe_Default_Mark_III/FX/NS_Pickaxe_Defualt_Mark_III_Trail.NS_Pickaxe_Defualt_Mark_III_Trail",
                                Replace = swaps["Trail"],
                                Type = SwapType.WeaponTrail
                            }
                        }
                    }
                }
            };


            var Rarity = await FileUtil.GetRarityFromAsset(item.DefinitionPath, _provider);
            if (Rarity != EFortRarity.Common)
            {
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

            return output;
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

        private bool TryIsB64(ref byte[] data, SaturnAsset asset)
        {
            List<byte[]> Searches = new();
            List<byte[]> Replaces = new();
            
            try
            {
                                
                Searches.Add(Encoding.ASCII.GetBytes(asset.ParentAsset.Replace(".uasset", "").Replace("FortniteGame/Content/", "/Game/")));
                Replaces.Add(Encoding.ASCII.GetBytes("/"));
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
    }
}
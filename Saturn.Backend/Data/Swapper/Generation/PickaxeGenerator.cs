using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CUE4Parse;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.Utils;
using Saturn.Backend.Data.Compression;
using Saturn.Backend.Data.SaturnAPI.Models;
using Saturn.Backend.Data.Swapper.Swapping;
using Saturn.Backend.Data.Variables;

namespace Saturn.Backend.Data.Swapper.Generation;

public class PickaxeGenerator : Generator
{
    private CompressionBase compressor = new Oodle();
    private async Task<DisplayItemModel> GetDisplayCharacterInfo(DefaultFileProvider provider, string path)
    {
        DisplayItemModel item = new DisplayItemModel();

        var uasset = await provider.TryLoadObjectAsync(path);
        if (uasset == null)
        {
            Logger.Log($"Failed to load uasset with path \"{path}\"", LogLevel.Warning);
            return null;
        }

        item.ID = Path.GetFileNameWithoutExtension(path);
        item.Name = uasset.TryGetValue(out FText DisplayName, "DisplayName") ? DisplayName.Text : "TBD";
        item.Description = uasset.TryGetValue(out FText Description, "Description")
            ? Description.Text
            : "To be determined...";

        return item;
    }

    private async Task<SaturnItemModel> GetCharacterInfo(DefaultFileProvider provider, string path)
    {
        SaturnItemModel item = new SaturnItemModel();

        var uasset = await provider.TryLoadObjectAsync(path);
        if (uasset == null)
        {
            Logger.Log($"Failed to load uasset with path \"{path}\"", LogLevel.Warning);
            return null;
        }
        
        var wid = uasset.Get<UObject>("WeaponDefinition");

        item.ID = Path.GetFileNameWithoutExtension(path);
        item.Name = uasset.TryGetValue(out FText DisplayName, "DisplayName") ? DisplayName.Text : "TBD";
        item.Description = uasset.TryGetValue(out FText Description, "Description")
            ? Description.Text
            : "To be determined...";

        Dictionary<string, string> Enums = new();
        Enums.Add("Series", wid.TryGetValue(out UObject Series, "Series") ? Series.GetPathName() : "None");
        Enums.Add("PrimaryFireAbility", wid.TryGetValue(out UObject PrimaryFireAbility, "PrimaryFireAbility") ? PrimaryFireAbility.GetPathName() : "None");

        item.CharacterParts = new Dictionary<string, CharacterPart>()
        {
            {
                "Pickaxe", new CharacterPart()
                {
                    Path = wid.GetPathName(),
                    Enums = Enums
                }
            }
        };

        return item;
    }
    
    public async Task<List<DisplayItemModel>> Generate()
    {
        List<DisplayItemModel> items = new();

        foreach (var file in Constants.Provider.Files.Keys)
        {
            if (!file.Contains("fortnitegame/content/athena/items/cosmetics/pickaxes/")) continue;

            var item = await GetDisplayCharacterInfo(Constants.Provider, file.Split('.')[0]);
            if (item == null) continue;
            if (item.Name == "TBD" || string.IsNullOrWhiteSpace(item.Name)) item.Name = item.ID;
            items.Add(item);
        }

        return items;
    }

    public async Task<SaturnItemModel> GetItemData(DisplayItemModel item)
    {
        List<SaturnItemModel> options = new();

        string file = Constants.Provider.Files.First(x => x.Key.Contains(item.ID + ".uasset") && x.Key.Contains("fortnitegame/content/athena/items/cosmetics/pickaxes/")).Key;
        var pickaxe = await GetCharacterInfo(Constants.Provider, file.Split('.')[0]);

        foreach (var optionId in Constants.PotentialOptions)
        {
            if (!optionId.ToLower().Contains("pickaxe")) continue;
            file = Constants.Provider.Files.FirstOrDefault(x => x.Key.Contains(optionId.ToLower() + ".uasset") && x.Key.Contains("fortnitegame/content/athena/items/cosmetics/pickaxes/"), new KeyValuePair<string, GameFile>()).Key;
            if (string.IsNullOrWhiteSpace(file)) continue;
            
            var option = await GetCharacterInfo(Constants.Provider, file.Split('.')[0]);
            
            if (option.CharacterParts["Pickaxe"].Enums["Series"] != pickaxe.CharacterParts["Pickaxe"].Enums["Series"]) continue;
            if (option.CharacterParts["Pickaxe"].Enums["PrimaryFireAbility"] != pickaxe.CharacterParts["Pickaxe"].Enums["PrimaryFireAbility"]) continue;
            
            int diffSize = (pickaxe.CharacterParts["Pickaxe"].Path.Split('.')[0].Length + Path.GetFileNameWithoutExtension(pickaxe.CharacterParts["Pickaxe"].Path).Length) -
                           (option.CharacterParts["Pickaxe"].Path.Split('.')[0].Length + Path.GetFileNameWithoutExtension(option.CharacterParts["Pickaxe"].Path).Length);

            IoPackage.NameToBeSearching.Add(pickaxe.CharacterParts["Pickaxe"].Path.SubstringAfterLast('/').Split('.')[0]);
            IoPackage.NameToBeSearching.Add(option.CharacterParts["Pickaxe"].Path.SubstringAfterLast('/').Split('.')[0]);

            if (Constants.CosmeticState == SaturnState.S_Pickaxe)
                SaturnData.IsPickaxe = true;

            IoPackage.ClearHeaders();
            IoPackage oldObj =
                (IoPackage)await Constants.Provider.LoadPackageAsync(option.CharacterParts["Pickaxe"].Path.Split('.')[0] + ".uasset");
                
            long originalLength = oldObj.TotalSize;
            uint originalCompressedSize = SaturnData.CompressedSize;

            SaturnData.Clear();

            if (Constants.CosmeticState == SaturnState.S_Pickaxe)
                SaturnData.IsPickaxe = true;

            IoPackage.ClearHeaders();
            IoPackage newObj =
                (IoPackage)await Constants.Provider.LoadPackageAsync(pickaxe.CharacterParts["Pickaxe"].Path.Split('.')[0] + ".uasset");

            if (oldObj.TotalSize < newObj.TotalSize + diffSize) continue;

            var obj = oldObj.Swap(newObj);
            byte[] serializedData = obj.Serialize();
            
            if (serializedData.Length > originalLength) continue;
            
            byte[] swap = new byte[originalLength];
            Buffer.BlockCopy(serializedData, 0, swap, 0, serializedData.Length);

            IoPackage.NameToBeSearching.Clear();

            byte[] data = compressor.Compress(swap);
            byte[] noScriptData = compressor.Compress(FileLogic.RemoveClassNames(swap));

            Logger.Log($"Asset: '{option.CharacterParts["Pickaxe"].Path} | Compressed Length: {originalCompressedSize} | Script Length: {data.Length} | No Script Length: {noScriptData.Length}");
                
            if (data.Length > originalCompressedSize && noScriptData.Length > originalCompressedSize) continue;

            options.Add(option);
        }

        pickaxe.Options = options;
        return pickaxe;
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CUE4Parse.FileProvider;
using CUE4Parse.FileProvider.Objects;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Objects.Core.i18N;
using Saturn.Backend.Data.Compression;
using Saturn.Backend.Data.SaturnAPI.Models;
using Saturn.Backend.Data.Variables;

namespace Saturn.Backend.Data.Swapper.Generation;

public class SkinGenerator : Generator
{
    CompressionBase compressor = new Oodle();
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
        Dictionary<string, CharacterPart> characterParts = new();

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

        if (uasset.TryGetValue(out UObject[] CharacterParts, "BaseCharacterParts"))
        {
            foreach (var cp in CharacterParts)
            {
                if (cp == null) continue;

                Dictionary<string, string> Enums = new();
                EFortCustomPartType CustomPartType = cp.GetOrDefault("CharacterPartType", EFortCustomPartType.Head);

                if (CustomPartType == EFortCustomPartType.Face)
                    CustomPartType = EFortCustomPartType.Hat;

                if (characterParts.ContainsKey(CustomPartType.ToString()) &&
                    CustomPartType == EFortCustomPartType.Hat)
                {
                    CustomPartType = EFortCustomPartType.Face;
                    if (characterParts.ContainsKey(CustomPartType.ToString()))
                        continue;
                }

                string PartType = cp.GetOrDefault("CharacterPartType", EFortCustomPartType.Head).ToString();
                Enums.Add("CharacterPartType", PartType);
                
                if (cp.TryGetValue(out UObject AdditionalData, "AdditionalData"))
                {
                    string CustomHatType = AdditionalData.GetOrDefault("HatType", ECustomHatType.ECustomHatType_None).ToString();
                    Enums.Add("HatType", CustomHatType);
                }
                
                characterParts.Add(CustomPartType.ToString(), new CharacterPart()
                {
                    Path = cp.GetPathName(),
                    Enums = Enums
                });
            }
        }
        
        item.CharacterParts = characterParts;

        return item;

    }
    
    public async Task<List<DisplayItemModel>> Generate()
    {
        List<DisplayItemModel> items = new();

        foreach (var file in Constants.Provider.Files.Keys)
        {
            if (!file.Contains("fortnitegame/content/athena/items/cosmetics/characters/") && !file.Contains("fortnitegame/plugins/gamefeatures/brcosmetics/content/athena/items/cosmetics/characters/")) continue;

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

        string file = Constants.Provider.Files.First(x => x.Key.Contains(item.ID + ".uasset") && (x.Key.Contains("fortnitegame/content/athena/items/cosmetics/characters/") || x.Key.Contains("fortnitegame/plugins/gamefeatures/brcosmetics/content/athena/items/cosmetics/characters/"))).Key;
        var character = await GetCharacterInfo(Constants.Provider, file.Split('.')[0]);

        foreach (var optionId in Constants.PotentialOptions)
        {
            if (!optionId.ToLower().Contains("character_") && !optionId.ToLower().Contains("cid_")) continue;
            file = Constants.Provider.Files.FirstOrDefault(x => x.Key.Contains(optionId.ToLower() + ".uasset") && (x.Key.Contains("fortnitegame/content/athena/items/cosmetics/characters/") || x.Key.Contains("fortnitegame/plugins/gamefeatures/brcosmetics/content/athena/items/cosmetics/characters/")), new KeyValuePair<string, GameFile>()).Key;
            if (string.IsNullOrWhiteSpace(file)) continue;
            
            var option = await GetCharacterInfo(Constants.Provider, file.Split('.')[0]);

            if (option.CharacterParts.Count < character.CharacterParts.Count) continue;

            bool bShouldBreak = false;
            foreach (var characterPart in character.CharacterParts)
            {
                if (!option.CharacterParts.ContainsKey(characterPart.Key))
                {
                    if (option.CharacterParts.ContainsKey(characterPart.Key == "Hat"
                            ? "Face"
                            : "Hat"))
                    {
                        if (option.CharacterParts[characterPart.Key == "Hat"
                                ? "Face"
                                : "Hat"].Enums["HatType"] == "ECustomHatType_None")
                        {
                            bShouldBreak = true;
                            break;
                        }

                        (option.CharacterParts[characterPart.Key], option.CharacterParts[
                            characterPart.Key == "Hat"
                                ? "Face"
                                : "Hat"]) = (option.CharacterParts[characterPart.Key == "Hat"
                            ? "Face"
                            : "Hat"], option.CharacterParts[characterPart.Key]);
                    }
                    else
                    {
                        bShouldBreak = true;
                        break;
                    }
                }
                
                if (!option.CharacterParts.ContainsKey(characterPart.Key))
                {
                    bShouldBreak = true;
                    break;
                }
                
                if (option.CharacterParts[characterPart.Key].Enums["HatType"] == "ECustomHatType_None" &&
                    characterPart.Value.Enums["HatType"] != "ECustomHatType_None")
                {
                    bShouldBreak = true;
                    break;
                }
            }

            if (bShouldBreak) continue;

            options.Add(option);
        }

        character.Options = options;
        return character;
    }
}
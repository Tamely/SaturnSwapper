using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse_Conversion.Textures;
using Newtonsoft.Json;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.FortniteAPI;
using Saturn.Backend.Data.Services;
using Saturn.Backend.Data.SwapOptions.Skins;
using SkiaSharp;

namespace Saturn.Backend.Data.Utils.Swaps.Generation;

internal sealed class SkinGeneration : AbstractGeneration
{
    private List<Cosmetic> _skins { get; set; }
    private DefaultFileProvider _provider { get; set; }
    private IConfigService _configService { get; set; }
    private ISwapperService _swapperService { get; set; }

    /// <summary>
    /// The constructor for the skin generation class which helps with methods to generate skins.
    /// </summary>
    /// <param name="skins">The list of skins (should be empty if you are just generating) that we will be adding to</param>
    /// <param name="provider">The default file provider from CUE4Parse</param>
    /// <param name="configService">The config service from the MainWindow class in Saturn.Client</param>
    /// <param name="swapperService">The swapper service class which calls this class</param>
    public SkinGeneration(List<Cosmetic> skins, 
                            DefaultFileProvider provider, 
                            IConfigService configService, 
                            ISwapperService swapperService) : base(ItemType.IT_Skin)
    {
        _skins = skins; // Set the skins list
        _provider = provider; // Set the file provider
        _configService = configService; // Set the config service
        _swapperService = swapperService; // Set the swapper service
    }

    /// <summary>
    /// Generates the skins for the user to be added to the swapper.
    /// </summary>
    /// <returns>List of Cosmetic: Fully generated skins (with swaps)</returns>
    public override async Task<List<Cosmetic>> Generate()
    {
        foreach (var (assetPath, _) in _provider.Files) // For every file in Fortnite
        {
            if (!assetPath.Contains("/CID_")) continue; // If the file is not a skin, skip it
                
            // doing a foreach instead of LINQ because LINQ is fucking stupid and slow
            bool any = false; // Set the any variable to false because we will need to check if the skin was already added
            foreach (var x in _skins) // For every skin in the skins list from the cache file
            {
                if (x.Id != FileUtil.SubstringFromLast(assetPath, '/').Split('.')[0]) continue; // If the skin id is not the same as the file name, skip it
                any = true; // Set the any variable to true because the skin was already added
                break; // Break the loop
            }
            if (any) continue; // If the skin was already added, skip it

            List<Cosmetic> CosmeticsToInsert = new(); // Create a new list of cosmetics to insert
                
            if (_provider.TryLoadObject(assetPath.Split('.')[0], out var asset)) // Try to load the CID
            {
                Cosmetic skin = new(); // Create a new cosmetic to add to the list

                skin.Name = asset.TryGetValue(out FText DisplayName, "DisplayName") ? DisplayName.Text : "TBD"; // Get the display name
                skin.Description = asset.TryGetValue(out FText Description, "Description") ? Description.Text : "To be determined..."; // Get the description

                skin.Id = FileUtil.SubstringFromLast(assetPath, '/').Split('.')[0]; // Get the skin id
                    
                if (skin.Name.ToLower() is "null" or "tbd" or "hero" || skin.Id.ToLower().Contains("cid_vip_")) continue; // If the skin name is null, skip it

                skin.Rarity = new Rarity // Make a new rarity instance
                {
                    Value = asset.TryGetValue(out EFortRarity Rarity, "Rarity") ? Rarity.ToString().Split("::")[0] : "Uncommon" // Get the rarity
                }; // End the rarity instance

                if (skin.Name is "Recruit" or "Random") // If the skin name is recruit or random
                    skin.Rarity.Value = "Common"; // Set the rarity to common

                if (skin.Name is "Random") // If the skin name is random
                    skin.IsRandom = true; // Set the random variable to true

                skin.Series = asset.TryGetValue(out UObject Series, "Series") // Get the series
                    ? new Series() // If the series is null, make a new instance of series
                    {
                        BackendValue = FileUtil.SubstringFromLast(Series.GetFullName(), '/').Split('.')[0] // Get the backend value
                    } : null; // End the series instance
                    
                skin.Images = new Images(); // Make a new images instance

                if (File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/skins/" + skin.Id + ".png"))) // If the skin image exists in the wwwroot folder
                    skin.Images.SmallIcon = "skins/" + skin.Id + ".png"; // Set the small icon to the skin image
                else // Otherwise
                {
                    if (asset.TryGetValue(out UObject HID, "HeroDefinition")) // If the HID is readable
                    {
                        if (HID.TryGetValue(out UTexture2D smallIcon, "SmallPreviewImage")) // If the small icon is readable
                        {
                            await using var ms = new MemoryStream(); // Create a new memory stream
                            smallIcon.Decode()?.Encode(ms, SKEncodedImageFormat.Png, 30); // Encode the small icon to the memory stream

                            Directory.CreateDirectory(Path.Combine(Config.ApplicationPath, "wwwroot/skins/")); // Create the wwwroot/skins folder if it doesn't exist
                            if (!File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/skins/" + skin.Id + ".png"))) // If the skin image doesn't exist in the wwwroot folder
                                await File.WriteAllBytesAsync(Path.Combine(Config.ApplicationPath, "wwwroot/skins/" + skin.Id + ".png"), ms.ToArray()); // Write the skin image to the wwwroot folder

                            skin.Images.SmallIcon = "skins/" + skin.Id + ".png"; // Set the small icon to the skin image
                        }
                        else // Otherwise
                        {
                            continue; // Skip the skin
                        }
                    }
                    else // Otherwise
                    {
                        continue; // Skip the skin
                    }
                }

                if (!skin.Id.ToLower().Contains("hardwood") && !skin.Id.ToLower().Contains("football")) // If the skin id isnt the basketball or football skins
                {
                    if (asset.TryGetValue(out UObject[] variant, "ItemVariants")) 
                    {
                        foreach (var variants in variant) // For every variant in the CIDs variants
                        {
                            if (variants.TryGetValue(out FStructFallback FVariantChannelTag, 
                                    "VariantChannelTag")) // If the variant channel tag is readable
                            {
                                FVariantChannelTag.TryGetValue(out FName VariantChannelTag, "TagName"); // Get the variant channel tag name
                                if (!VariantChannelTag.Text.Contains("Material") && !VariantChannelTag.Text.Contains("Parts")) // If the variant channel tag name doesn't contain material or parts
                                    continue; // Skip the variant
                                    
                                    
                                if (variants.TryGetValue(out FStructFallback[] MaterialOptions, "MaterialOptions")) // If the material options are readable
                                    foreach (var MaterialOption in MaterialOptions) // For every material option in material options
                                    {
                                        if (MaterialOption.TryGetValue(out FStructFallback[] MaterialParams, "VariantMaterialParams") && MaterialParams.Length != 0) // If the material params are readable and not empty
                                            continue; // Skip the material option because those won't be perfect

                                        if (MaterialOption.TryGetValue(out FText VariantName, "VariantName")) // If the variant name is readable
                                        {
                                            FName TagName = new FName(); // Create a new FName

                                            if (MaterialOption.TryGetValue(out FStructFallback CustomizationVariantTag,
                                                    "CustomizationVariantTag")) // If the customization variant tag is readable
                                                CustomizationVariantTag.TryGetValue(out TagName, "TagName"); // Get the customization variant tag name

                                            if (string.IsNullOrWhiteSpace(VariantName.Text) || VariantName.Text.ToLower() == "default" || VariantName.Text.ToLower() == skin.Name.ToLower()) // If the variant name is empty or is the same as the skin name or is the same as the skin name in lowercase
                                                continue; // Skip the variant
                                                
                                            if (!File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/skins/" + skin.Id + "_" + VariantName.Text.Replace(" ","_").Replace("\\","").Replace("/","") + ".png") + ".png")) // If the skin image doesn't exist
                                            {
                                                if (MaterialOption.TryGetValue(out UTexture2D PreviewImage,
                                                        "PreviewImage")) // If the preview image for the variant is readable
                                                {
                                                    await using var ms = new MemoryStream(); // Create a new memory stream
                                                    PreviewImage.Decode()?.Encode(ms, SKEncodedImageFormat.Png, 30); // Encode the preview image to the memory stream
                            
                                                    Directory.CreateDirectory(Path.Combine(Config.ApplicationPath, "wwwroot/skins/")); // Create the skin directory if it doesn't exist
                                                    if (!File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/skins/" + skin.Id + "_" + VariantName.Text.Replace(" ","_").Replace("\\","").Replace("/","") + ".png") + ".png")) // If the skin image doesn't exist
                                                        await File.WriteAllBytesAsync(Path.Combine(Config.ApplicationPath, "wwwroot/skins/" + skin.Id + "_" + VariantName.Text.Replace(" ","_").Replace("\\","").Replace("/","") + ".png"), ms.ToArray()); // Write the skin image to the wwwroot/skins directory
                                                }
                                            }

                                            CosmeticsToInsert.Add(new Cosmetic() // Create a new cosmetic instance
                                            {
                                                Name = VariantName.Text, // Set the cosmetic name
                                                Description = skin.Name + " style: " + skin.Description, // Set the cosmetic description with the name of the skin it is a style of in the first part
                                                Id = skin.Id, // Set the cosmetic id
                                                Rarity = skin.Rarity, // Set the cosmetic rarity
                                                Series = skin.Series, // Set the cosmetic series
                                                Images = new Images() // Create a new images instance
                                                {
                                                    SmallIcon = "skins/" + skin.Id  + "_" + VariantName.Text.Replace(" ","_").Replace("\\","").Replace("/","") + ".png" // Set the small icon image
                                                }, // End the images instance
                                                VariantChannel = VariantChannelTag.Text, // Set the variant channel
                                                VariantTag = TagName.Text // Set the variant tag
                                            });
                                        }
                                    }
                            }
                        }
                    }
                }

                foreach (var value in CosmeticsToInsert) // For each cosmetic to insert
                    _skins.Add(value); // Add the cosmetic to the skins list

                _skins.Add(skin);  // Add the skin to the skins list
            }
            else // Otherwise
                Logger.Log($"Failed to load {assetPath}"); // Log that the asset failed to load
        }
        
        _skins = _skins.OrderBy(x => x.Id).ToList(); // sort skins by alphabetical order

        // Remove items from the array that are duplicates
        for (var i = 0; i < _skins.Count; i++) // For every item in the skins list
        for (var j = i + 1; j < _skins.Count; j++) // We want to loop through the skins list again to get each skin compared to each skin
        {
            if (_skins[i].Name != _skins[j].Name ||  // If the skins names are not the same
                _skins[i].Images.SmallIcon != _skins[j].Images.SmallIcon || // If the small icons are not the same
                _skins[i].Description != _skins[j].Description) continue; // If the descriptions are not the same, skip them
            _skins.RemoveAt(j); // Remove the duplicate skin
            j--; // Decrement the j value to fix the list
        }

        return _skins; // Return the skins list
    }
}
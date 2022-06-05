using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse_Conversion.Textures;
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.FortniteAPI;
using Saturn.Backend.Core.Services;
using SkiaSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Saturn.Backend.Core.Utils.Swaps.Generation;

internal class PickaxeGeneration : AbstractGeneration
{
    public PickaxeGeneration(List<Cosmetic> pickaxes,
                            DefaultFileProvider _provider,
                            IConfigService _configService,
                            ISwapperService _swapperService) : base(ItemType.IT_Pickaxe)
    {
        this.pickaxes = pickaxes;
        this._provider = _provider;
        this._configService = _configService;
        this._swapperService = _swapperService;
    }

    public override async Task<List<Cosmetic>> Generate()
    {
        foreach (var (assetPath, _) in _provider.Files) // For every file in Fortnite
        {
            if (!assetPath.ToLower().Contains("items/cosmetics/pickaxes")) continue; // If the file is not a pickaxe, skip it
                
            // doing a foreach instead of LINQ because LINQ is fucking stupid and slow
            bool any = false; // Set the any variable to false because we will need to check if the pickaxe was already added
            foreach (var x in pickaxes) // For every pickaxe in the pickaxes list from the cache file
            {
                if (x.Id != FileUtil.SubstringFromLast(assetPath, '/').Split('.')[0]) continue; // If the pickaxe id is not the same as the file name, skip it
                any = true; // Set the any variable to true because the pickaxe was already added
                break; // Break the loop
            }
            if (any) continue; // If the pickaxe was already added, skip it

            if (_provider.TryLoadObject(assetPath.Split('.')[0], out var asset)) // Try to load the PID
            {
                Cosmetic pickaxe = new(); // Create a new cosmetic to add to the list

                pickaxe.Name = asset.TryGetValue(out FText DisplayName, "DisplayName") ? DisplayName.Text : "TBD"; // Get the display name
                pickaxe.Description = asset.TryGetValue(out FText Description, "Description") ? Description.Text : "To be determined..."; // Get the description

                pickaxe.Id = FileUtil.SubstringFromLast(assetPath, '/').Split('.')[0]; // Get the pickaxe id
                    
                if (pickaxe.Name.ToLower() is "null" or "tbd" or "hero") continue; // If the pickaxe name is null, skip it

                pickaxe.Rarity = new Rarity // Make a new rarity instance
                {
                    Value = asset.TryGetValue(out EFortRarity Rarity, "Rarity") ? Rarity.ToString().Split("::")[0] : "Uncommon" // Get the rarity
                }; // End the rarity instance

                if (pickaxe.Name is "Default Pickaxe" or "Random") // If the pickaxe name is default or random
                    pickaxe.Rarity.Value = "Common"; // Set the rarity to common

                if (pickaxe.Name is "Random") // If the pickaxe name is random
                    pickaxe.IsRandom = true; // Set the random variable to true

                pickaxe.Series = asset.TryGetValue(out UObject Series, "Series") // Get the series
                    ? new Series() // If the series is null, make a new instance of series
                    {
                        BackendValue = FileUtil.SubstringFromLast(Series.GetFullName(), '/').Split('.')[0] // Get the backend value
                    } : null; // End the series instance
                    
                pickaxe.Images = new Images(); // Make a new images instance

                if (File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/pickaxes/" + pickaxe.Id + ".png"))) // If the pickaxe image exists in the wwwroot folder
                    pickaxe.Images.SmallIcon = "pickaxes/" + pickaxe.Id + ".png"; // Set the small icon to the pickaxe image
                else // Otherwise
                {
                    UObject WID = await _swapperService.GetWIDByID(pickaxe.Id); // Gets the WID of the pickaxe from the ID

                    if (WID == new UObject()) // Check if it was successful
                    {
                        Logger.Log("Cannot get the WID for " + pickaxe.Id); // Log the error
                        continue; // Skip the pickaxe
                    }
                    
                    if (WID.TryGetValue(out UTexture2D smallIcon, "SmallPreviewImage")) // If the small icon is readable
                    {
                        await using var ms = new MemoryStream(); // Create a new memory stream
                        smallIcon.Decode()?.Encode(ms, SKEncodedImageFormat.Png, 30); // Encode the small icon to the memory stream

                        Directory.CreateDirectory(Path.Combine(Config.ApplicationPath, "wwwroot/pickaxes/")); // Create the wwwroot/pickaxes folder if it doesn't exist
                        if (!File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/pickaxes/" + pickaxe.Id + ".png"))) // If the pickaxe image doesn't exist in the wwwroot folder
                            await File.WriteAllBytesAsync(Path.Combine(Config.ApplicationPath, "wwwroot/pickaxes/" + pickaxe.Id + ".png"), ms.ToArray()); // Write the pickaxe image to the wwwroot folder

                        pickaxe.Images.SmallIcon = "pickaxes/" + pickaxe.Id + ".png"; // Set the small icon to the pickxe image
                    }
                    else // Otherwise
                    {
                        Logger.Log("Cannot parse the small icon for " + pickaxe.Id); // Log the error
                        continue; // Skip the pickaxe
                    }
                }

                pickaxes.Add(pickaxe); // Add the pickaxe to the list
            }
            else // Otherwise
                Logger.Log($"Failed to load {assetPath}"); // Log that the asset failed to load
        }
        
        pickaxes = pickaxes.OrderBy(x => x.Name).ToList(); // sort pickaxes by alphabetical order

        // Remove items from the array that are duplicates
        for (var i = 0; i < pickaxes.Count; i++) // For every item in the pickaxes list
        for (var j = i + 1; j < pickaxes.Count; j++) // We want to loop through the pickaxes list again to get each pickaxe compared to each pickaxe
        {
            if (pickaxes[i].Name != pickaxes[j].Name ||  // If the pickaxes names are not the same
                pickaxes[i].Images.SmallIcon != pickaxes[j].Images.SmallIcon || // If the small icons are not the same
                pickaxes[i].Description != pickaxes[j].Description) continue; // If the descriptions are not the same, skip them
            pickaxes.RemoveAt(j); // Remove the duplicate pickaxe
            j--; // Decrement the j value to fix the list
        }

        return pickaxes; // Return the pickaxes list
    }

    private List<Cosmetic> pickaxes { get; set; }
    private DefaultFileProvider _provider { get; set; }
    private IConfigService _configService { get; set; }
    private ISwapperService _swapperService { get; set; }
}
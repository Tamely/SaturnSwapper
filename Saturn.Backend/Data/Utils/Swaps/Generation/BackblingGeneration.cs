using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse_Conversion.Textures;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.FortniteAPI;
using Saturn.Backend.Data.Services;
using Saturn.Backend.Data.SwapOptions.Backblings;
using SkiaSharp;

namespace Saturn.Backend.Data.Utils.Swaps.Generation;

internal class BackblingGeneration : AbstractGeneration
{
    public BackblingGeneration(List<Cosmetic> backBlings,
                            DefaultFileProvider _provider,
                            IConfigService _configService,
                            ISwapperService _swapperService,
                            IJSRuntime _jsRuntime) : base(ItemType.IT_Backbling)
    {
        this.backBlings = backBlings;
        this._provider = _provider;
        this._configService = _configService;
        this._swapperService = _swapperService;
        this._jsRuntime = _jsRuntime;
    }

    public override async Task<List<Cosmetic>> Generate()
    {
        if (File.Exists(Config.BackblingCache)) // If the cache file exists
            backBlings = JsonConvert.DeserializeObject<List<Cosmetic>>(await File.ReadAllTextAsync(Config.BackblingCache)); // Deserialize the cache file

        foreach (var (assetPath, _) in _provider.Files) // For every file in Fortnite
        {
            if (!assetPath.Contains("/BID_")) continue; // If the file is not a backbling, skip it
                
            // doing a foreach instead of LINQ because LINQ is fucking stupid and slow
            bool any = false; // Set the any variable to false because we will need to check if the skin was already added
            foreach (var x in backBlings) // For every backbling in the backblings list from the cache file
            {
                if (x.Id != FileUtil.SubstringFromLast(assetPath, '/').Split('.')[0]) continue; // If the backbling id is not the same as the file name, skip it
                any = true; // Set the any variable to true because the skin was already added
                break; // Break the loop
            }
            if (any) continue; // If the backbling was already added, skip it

            if (_provider.TryLoadObject(assetPath.Split('.')[0], out var asset)) // Try to load the BID
            {
                Cosmetic backbling = new(); // Create a new cosmetic to add to the list

                backbling.Name = asset.TryGetValue(out FText DisplayName, "DisplayName") ? DisplayName.Text : "TBD"; // Get the display name
                backbling.Description = asset.TryGetValue(out FText Description, "Description") ? Description.Text : "To be determined..."; // Get the description

                backbling.Id = FileUtil.SubstringFromLast(assetPath, '/').Split('.')[0]; // Get the backbling id
                    
                if (backbling.Name.ToLower() is "null" or "tbd" or "hero") continue; // If the backbling name is null, skip it

                backbling.Rarity = new Rarity // Make a new rarity instance
                {
                    Value = asset.TryGetValue(out EFortRarity Rarity, "Rarity") ? Rarity.ToString().Split("::")[0] : "Uncommon" // Get the rarity
                }; // End the rarity instance

                if (backbling.Name is "Random") // If the pickaxe name is random
                {
                    backbling.Rarity.Value = "Common"; // Set the rarity to common
                    backbling.IsRandom = true; // Set the random variable to true
                }

                backbling.Series = asset.TryGetValue(out UObject Series, "Series") // Get the series
                    ? new Series() // If the series is null, make a new instance of series
                    {
                        BackendValue = FileUtil.SubstringFromLast(Series.GetFullName(), '/').Split('.')[0] // Get the backend value
                    } : null; // End the series instance
                    
                backbling.Images = new Images(); // Make a new images instance

                if (File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/backblings/" + backbling.Id + ".png"))) // If the backbling image exists in the wwwroot folder
                    backbling.Images.SmallIcon = "backblings/" + backbling.Id + ".png"; // Set the small icon to the backbling image
                else // Otherwise
                {
                    if (asset.TryGetValue(out UTexture2D smallIcon, "SmallPreviewImage")) // If the small icon is readable
                    {
                        await using var ms = new MemoryStream(); // Create a new memory stream
                        smallIcon.Decode()?.Encode(ms, SKEncodedImageFormat.Png, 30); // Encode the small icon to the memory stream

                        Directory.CreateDirectory(Path.Combine(Config.ApplicationPath, "wwwroot/backblings/")); // Create the wwwroot/backblings folder if it doesn't exist
                        if (!File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/backblings/" + backbling.Id + ".png"))) // If the backbling image doesn't exist in the wwwroot folder
                            await File.WriteAllBytesAsync(Path.Combine(Config.ApplicationPath, "wwwroot/backblings/" + backbling.Id + ".png"), ms.ToArray()); // Write the backbling image to the wwwroot folder

                        backbling.Images.SmallIcon = "backblings/" + backbling.Id + ".png"; // Set the small icon to the skin image
                    }
                    else // Otherwise
                    {
                        Logger.Log("Cannot parse the small icon for " + backbling.Id); // Log the error
                        continue; // Skip the backbling
                    }
                }
                
                backBlings.Add(backbling); // Add the backbling to the list
            }
            else // Otherwise
                Logger.Log($"Failed to load {assetPath}"); // Log that the asset failed to load
        }
        
        backBlings = backBlings.OrderBy(x => x.Id).ToList(); // sort pickaxes by alphabetical order

        // Remove items from the array that are duplicates
        for (var i = 0; i < backBlings.Count; i++) // For every item in the backbling list
        for (var j = i + 1; j < backBlings.Count; j++) // We want to loop through the backblings list again to get each pickaxe compared to each backbling
        {
            if (backBlings[i].Name != backBlings[j].Name ||  // If the backblings names are not the same
                backBlings[i].Images.SmallIcon != backBlings[j].Images.SmallIcon || // If the small icons are not the same
                backBlings[i].Description != backBlings[j].Description) continue; // If the descriptions are not the same, skip them
            backBlings.RemoveAt(j); // Remove the duplicate backbling
            j--; // Decrement the j value to fix the list
        }

        return backBlings; // Return the backblings list
    }

    private List<Cosmetic> backBlings { get; set; }
    private DefaultFileProvider _provider { get; set; }
    private IConfigService _configService { get; set; }
    private ISwapperService _swapperService { get; set; }
    private IJSRuntime _jsRuntime { get; set; }
}
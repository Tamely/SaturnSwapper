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
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.FortniteAPI;
using Saturn.Backend.Core.Services;
using SkiaSharp;

namespace Saturn.Backend.Core.Utils.Swaps.Generation;

internal class GliderGeneration : AbstractGeneration
{
    public GliderGeneration(List<Cosmetic> gliders,
                            DefaultFileProvider _provider,
                            IConfigService _configService,
                            ISwapperService _swapperService,
                            IJSRuntime _jsRuntime) : base(ItemType.IT_Glider)
    {
        this.gliders = gliders;
        this._provider = _provider;
        this._configService = _configService;
        this._swapperService = _swapperService;
        this._jsRuntime = _jsRuntime;
    }

    public override async Task<List<Cosmetic>> Generate()
    {
        foreach (var (assetPath, _) in _provider.Files) // For every file in Fortnite
        {
            if (!assetPath.ToLower().Contains("/items/cosmetics/gliders")) continue; // If the file is not a glider, skip it
                
            // doing a foreach instead of LINQ because LINQ is fucking stupid and slow
            bool any = false; // Set the any variable to false because we will need to check if the skin was already added
            foreach (var x in gliders) // For every glider in the gliders list from the cache file
            {
                if (x.Id != FileUtil.SubstringFromLast(assetPath, '/').Split('.')[0]) continue; // If the glider id is not the same as the file name, skip it
                any = true; // Set the any variable to true because the skin was already added
                break; // Break the loop
            }
            if (any) continue; // If the glider was already added, skip it

            if (_provider.TryLoadObject(assetPath.Split('.')[0], out var asset)) // Try to load the Glider ID
            {
                Cosmetic glider = new(); // Create a new cosmetic to add to the list

                glider.Name = asset.TryGetValue(out FText DisplayName, "DisplayName") ? DisplayName.Text : "TBD"; // Get the display name
                glider.Description = asset.TryGetValue(out FText Description, "Description") ? Description.Text : "To be determined..."; // Get the description

                glider.Id = FileUtil.SubstringFromLast(assetPath, '/').Split('.')[0]; // Get the backbling id
                    
                if (glider.Name.ToLower() is "null" or "tbd" or "hero") continue; // If the glider name is null, skip it

                glider.Rarity = new Rarity // Make a new rarity instance
                {
                    Value = asset.GetOrDefault("Rarity", EFortRarity.Uncommon).ToString() // Get the rarity
                }; // End the rarity instance

                if (glider.Name is "Random") // If the pickaxe name is random
                {
                    glider.Rarity.Value = "Common"; // Set the rarity to common
                    glider.IsRandom = true; // Set the random variable to true
                }

                glider.Series = asset.TryGetValue(out UObject Series, "Series") // Get the series
                    ? new Series() // If the series is null, make a new instance of series
                    {
                        BackendValue = FileUtil.SubstringFromLast(Series.GetFullName(), '/').Split('.')[0] // Get the backend value
                    } : null; // End the series instance
                    
                glider.Images = new Images(); // Make a new images instance

                if (File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/gliders/" + glider.Id + ".png"))) // If the glider image exists in the wwwroot folder
                    glider.Images.SmallIcon = "gliders/" + glider.Id + ".png"; // Set the small icon to the glider image
                else // Otherwise
                {
                    if (asset.TryGetValue(out UTexture2D smallIcon, "SmallPreviewImage")) // If the small icon is readable
                    {
                        await using var ms = new MemoryStream(); // Create a new memory stream
                        smallIcon.Decode()?.Encode(ms, SKEncodedImageFormat.Png, 30); // Encode the small icon to the memory stream

                        Directory.CreateDirectory(Path.Combine(Config.ApplicationPath, "wwwroot/gliders/")); // Create the wwwroot/backblings folder if it doesn't exist
                        if (!File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/gliders/" + glider.Id + ".png"))) // If the glider image doesn't exist in the wwwroot folder
                            await File.WriteAllBytesAsync(Path.Combine(Config.ApplicationPath, "wwwroot/gliders/" + glider.Id + ".png"), ms.ToArray()); // Write the glider image to the wwwroot folder

                        glider.Images.SmallIcon = "gliders/" + glider.Id + ".png"; // Set the small icon to the skin image
                    }
                    else // Otherwise
                    {
                        Logger.Log("Cannot parse the small icon for " + glider.Id); // Log the error
                        continue; // Skip the glider
                    }
                }

                gliders.Add(glider); // Add the glider to the list
            }
            else // Otherwise
                Logger.Log($"Failed to load {assetPath}"); // Log that the asset failed to load
        }
        
        gliders = gliders.OrderBy(x => x.Id).ToList(); // sort gliders by alphabetical order

        // Remove items from the array that are duplicates
        for (var i = 0; i < gliders.Count; i++) // For every item in the glider list
        for (var j = i + 1; j < gliders.Count; j++) // We want to loop through the glider list again to get each pickaxe compared to each glider
        {
            if (gliders[i].Name != gliders[j].Name ||  // If the glider names are not the same
                gliders[i].Images.SmallIcon != gliders[j].Images.SmallIcon || // If the small icons are not the same
                gliders[i].Description != gliders[j].Description) continue; // If the descriptions are not the same, skip them
            gliders.RemoveAt(j); // Remove the duplicate glider
            j--; // Decrement the j value to fix the list
        }

        return gliders; // Return the glider list
    }

    private List<Cosmetic> gliders { get; set; }
    private DefaultFileProvider _provider { get; set; }
    private IConfigService _configService { get; set; }
    private ISwapperService _swapperService { get; set; }
    private IJSRuntime _jsRuntime { get; set; }
}

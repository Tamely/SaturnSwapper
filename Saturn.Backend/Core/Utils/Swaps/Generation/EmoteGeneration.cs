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

internal class EmoteGeneration : AbstractGeneration
{
    public EmoteGeneration(List<Cosmetic> emotes,
                            DefaultFileProvider _provider,
                            IConfigService _configService,
                            ISwapperService _swapperService) : base(ItemType.IT_Dance)
    {
        this.emotes = emotes;
        this._provider = _provider;
        this._configService = _configService;
        this._swapperService = _swapperService;
    }

    public override async Task<List<Cosmetic>> Generate()
    {
        foreach (var (assetPath, _) in _provider.Files) // For every file in Fortnite
        {
            if (!assetPath.Contains("Dances/EID_")) continue; // If the file is not an emote, skip it
                
            // doing a foreach instead of LINQ because LINQ is fucking stupid and slow
            bool any = false; // Set the any variable to false because we will need to check if the emote was already added
            foreach (var x in emotes) // For every emote in the emotes list from the cache file
            {
                if (x.Id != FileUtil.SubstringFromLast(assetPath, '/').Split('.')[0]) continue; // If the emote id is not the same as the file name, skip it
                any = true; // Set the any variable to true because the emote was already added
                break; // Break the loop
            }
            if (any) continue; // If the emote was already added, skip it

            if (_provider.TryLoadObject(assetPath.Split('.')[0], out var asset)) // Try to load the EID
            {
                Cosmetic emote = new(); // Create a new cosmetic to add to the list

                emote.Name = asset.TryGetValue(out FText DisplayName, "DisplayName") ? DisplayName.Text : "TBD"; // Get the display name
                emote.Description = asset.TryGetValue(out FText Description, "Description") ? Description.Text : "To be determined..."; // Get the description

                emote.Id = FileUtil.SubstringFromLast(assetPath, '/').Split('.')[0]; // Get the emote id
                    
                if (emote.Name.ToLower() is "null" or "tbd" or "hero") continue; // If the emote name is null, skip it

                emote.Rarity = new Rarity // Make a new rarity instance
                {
                    Value = asset.TryGetValue(out EFortRarity Rarity, "Rarity") ? Rarity.ToString().Split("::")[0] : "Uncommon" // Get the rarity
                }; // End the rarity instance

                if (emote.Name is "Dance Moves" or "Random") // If the emote name is dance moves or random
                    emote.Rarity.Value = "Common"; // Set the rarity to common

                if (emote.Name is "Random") // If the emote name is random
                    emote.IsRandom = true; // Set the random variable to true

                emote.Series = asset.TryGetValue(out UObject Series, "Series") // Get the series
                    ? new Series() // If the series is null, make a new instance of series
                    {
                        BackendValue = FileUtil.SubstringFromLast(Series.GetFullName(), '/').Split('.')[0] // Get the backend value
                    } : null; // End the series instance
                    
                emote.Images = new Images(); // Make a new images instance

                if (File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/emotes/" + emote.Id + ".png"))) // If the emote image exists in the wwwroot folder
                    emote.Images.SmallIcon = "emotes/" + emote.Id + ".png"; // Set the small icon to the emote image
                else // Otherwise
                {
                    if (asset.TryGetValue(out UTexture2D smallIcon, "SmallPreviewImage")) // If the small icon is readable
                    {
                        await using var ms = new MemoryStream(); // Create a new memory stream
                        smallIcon.Decode()?.Encode(ms, SKEncodedImageFormat.Png, 30); // Encode the small icon to the memory stream

                        Directory.CreateDirectory(Path.Combine(Config.ApplicationPath, "wwwroot/emotes/")); // Create the wwwroot/emotes folder if it doesn't exist
                        if (!File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/emotes/" + emote.Id + ".png"))) // If the emote image doesn't exist in the wwwroot folder
                            await File.WriteAllBytesAsync(Path.Combine(Config.ApplicationPath, "wwwroot/emotes/" + emote.Id + ".png"), ms.ToArray()); // Write the emote image to the wwwroot folder

                        emote.Images.SmallIcon = "emotes/" + emote.Id + ".png"; // Set the small icon to the emote image
                    }
                    else // Otherwise
                    {
                        Logger.Log("Cannot parse the small icon for " + emote.Id); // Log the error
                        continue; // Skip the emote
                    }
                }
                
                emotes.Add(emote);
            }
            else // Otherwise
                Logger.Log($"Failed to load {assetPath}"); // Log that the asset failed to load
        }
        
        emotes = emotes.OrderBy(x => x.Name).ToList(); // sort emotes by alphabetical order

        // Remove items from the array that are duplicates
        for (var i = 0; i < emotes.Count; i++) // For every item in the emotes list
        for (var j = i + 1; j < emotes.Count; j++) // We want to loop through the emotes list again to get each emote compared to each emote
        {
            if (emotes[i].Name != emotes[j].Name ||  // If the emotes names are not the same
                emotes[i].Images.SmallIcon != emotes[j].Images.SmallIcon || // If the small icons are not the same
                emotes[i].Description != emotes[j].Description) continue; // If the descriptions are not the same, skip them
            emotes.RemoveAt(j); // Remove the duplicate emote
            j--; // Decrement the j value to fix the list
        }

        return emotes; // Return the emotes list
    }

    private List<Cosmetic> emotes { get; set; }
    private DefaultFileProvider _provider { get; set; }
    private IConfigService _configService { get; set; }
    private ISwapperService _swapperService { get; set; }
}
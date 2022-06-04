using Newtonsoft.Json;
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.FortniteAPI;
using Saturn.Backend.Core.Models.Items;
using Saturn.Backend.Core.Models.SaturnAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Asset = Saturn.Backend.Core.Models.SaturnAPI.Asset;
using Swap = Saturn.Backend.Core.Models.SaturnAPI.Swap;

namespace Saturn.Backend.Core.Utils.ReadPlugins;

public class DotSaturn
{
    public static async Task<Cosmetic> Read(string filePath)
    {
        if (Path.GetExtension(filePath) == ".json")
        {
            Logger.Log("Detected different swapper type plugin!");

            try
            {
                var plugin = JsonConvert.DeserializeObject<GalaxyPlugin>(await File.ReadAllTextAsync(filePath));

                Logger.Log("Plugin is of type Galaxy!");
                Logger.Log("Converting Galaxy Swapper plugin to Saturn plugin format...");

                var saturnTypePlugin = await ConvertGalaxyToSaturn(plugin);

                Logger.Log("Conversion complete!");
                Logger.Log("Converting Saturn plugin to Saturn item.");

                return await ConvertPluginToItem(saturnTypePlugin);
            }
            catch
            {
                try
                {
                    var plugin = JsonConvert.DeserializeObject<LelePlugin>(await File.ReadAllTextAsync(filePath));

                    Logger.Log("Plugin is of type Lele!");
                    Logger.Log("Converting Lele Swapper plugin to Saturn plugin format...");

                    var saturnTypePlugin = await ConvertLeleToSaturn(plugin);

                    Logger.Log("Conversion complete!");
                    Logger.Log("Converting Saturn plugin to Saturn item.");

                    return await ConvertPluginToItem(saturnTypePlugin);
                }
                catch
                {
                    Logger.Log("There was an error parsing plugin file! Name: " + Path.GetFileName(filePath), LogLevel.Error);
                    return new Cosmetic()
                    {
                        Name = "Error",
                        Id = "BROKEN",
                        Description = $"There was an error parsing {Path.GetFileNameWithoutExtension(filePath)}! Report this to Tamely!",
                        Rarity = new Rarity()
                        {
                            Value = "Epic"
                        },
                        Images = new Images()
                        {
                            SmallIcon =
                                "https://fortnite-api.com/images/cosmetics/br/bid_npc_hightowerdate/smallicon.png"
                        }
                    };
                }

            }
        }

        return new Cosmetic();
    }
    
    private static async Task<Cosmetic> ConvertPluginToItem(PluginModel plugin)
    {
        Cosmetic item = new Cosmetic();
        try
        {
            item.Name = plugin.Name.Split(" to ")[1];
            item.Id = plugin.Name;
            item.Description = plugin.Message ?? "No message provided.";
            item.Images = new Images()
            {
                SmallIcon = plugin.Icon
            };
            item.Rarity = new Rarity()
            {
                Value = "Epic"
            };
            SaturnItem saturnItem = new SaturnItem
            {
                Name = plugin.Name.Split(" to ")[0],
                ItemDefinition = plugin.Name,
                Description = plugin.Message ?? "No message provided.",
                Icon = plugin.SwapIcon,
                Type = ItemType.IT_Misc,
                Rarity = "Epic",
                Options = new List<SaturnOption>()
                {
                    new SaturnOption()
                    {
                        Name = plugin.Name.Split(" to ")[1],
                        Icon = plugin.SwapIcon,
                        Rarity = "Epic",
                        Assets = new()
                    }
                }
            };

            foreach (var swap in plugin.Assets)
            {
                List<SaturnSwap> swaps = swap.Swaps.Select(itemSwap => new SaturnSwap() { Search = itemSwap.Search, Replace = itemSwap.Replace }).ToList();

                saturnItem.Options[0].Assets.Add(new SaturnAsset()
                {
                    ParentAsset = swap.AssetPath,
                    Swaps = swaps
                });
            }
        
            item.CosmeticOptions = new List<SaturnItem>()
            {
                saturnItem
            };
        }
        catch (Exception e)
        {
            Logger.Log(e.ToString());
        }


        return item;
    }
    
    public static async Task<PluginModel> ConvertGalaxyToSaturn(GalaxyPlugin galaxyPlugin)
    {
        var pluginModel = new PluginModel
        {
            Name = galaxyPlugin.Name.Replace(" To ", " to ").Replace(" TO ", " to ").Replace(" tO ", " to "),
            Icon = galaxyPlugin.Icon,
            SwapIcon = galaxyPlugin.swappedIcon,
            Message = galaxyPlugin.Message
        };


        List<Asset> assets = new List<Asset>();

        foreach (var asset in galaxyPlugin.Assets)
        {
            List<Swap> swaps = new List<Swap>();
            foreach (var swap in asset.Swaps)
            {
                if (swap.Type.ToLower() == "string")
                {
                    swaps.Add(new Swap()
                    {
                        Search = swap.Search,
                        Replace = swap.Replace
                    });
                }
                else
                {
                    swaps.Add(new Swap()
                    {
                        Search = "hex=" + swap.Search,
                        Replace = "hex=" + swap.Replace
                    });
                }
            }
            
            
            assets.Add(new Asset()
            {
                AssetPath = asset.Path,
                Swaps = swaps
            });
        }
        
        pluginModel.Assets = assets;

        return pluginModel;
    }
    
    public static async Task<PluginModel> ConvertLeleToSaturn(LelePlugin lelePlugin)
    {
        string message = "No message provided.";
        if (lelePlugin.Messages.Count > 0)
        {
            if (lelePlugin.Messages[0].localization.Count > 0)
            {
                message = (lelePlugin.Messages[0].localization.Find(e => e.languageId.ToLower() == "en") ?? lelePlugin.Messages[0].localization[0]).message;
            }
        }
        
        var pluginModel = new PluginModel
        {
            Name = lelePlugin.DefaultName + " to " + lelePlugin.SwappedName,
            Icon = lelePlugin.SwappedIcon,
            SwapIcon = lelePlugin.DefaultIcon,
            Message = message
        };


        List<Asset> assets = new List<Asset>();

        foreach (var asset in lelePlugin.Swaps)
        {
            List<Swap> swaps = new List<Swap>();
            foreach (var swap in asset.Swaps)
            {
                swaps.Add(new Swap()
                {
                    Search = swap.Key,
                    Replace = swap.Value.ToString()
                });
            }
            
            
            assets.Add(new Asset()
            {
                AssetPath = asset.AssetPath,
                Swaps = swaps
            });
        }
        
        pluginModel.Assets = assets;

        return pluginModel;
    }


    public static void Write(string filePath, string json)
    {
        
    }

    public static ulong GenerateSecurityCheck(uint compressedSize, uint decompressedSize)
        => compressedSize ^ (ulong) decompressedSize;


}
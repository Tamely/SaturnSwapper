using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DiscordRPC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.FortniteAPI;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Models.Items.Galaxy;
using Saturn.Backend.Data.Models.Items.Lele;
using Saturn.Backend.Data.Models.SaturnAPI;
using Asset = Saturn.Backend.Data.Models.SaturnAPI.Asset;
using Swap = Saturn.Backend.Data.Models.SaturnAPI.Swap;

namespace Saturn.Backend.Data.Utils.ReadPlugins;

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
                var plugin = JsonConvert.DeserializeObject<LelePlugin>(await File.ReadAllTextAsync(filePath));

                Logger.Log("Plugin is of type Lele!");
                Logger.Log("Converting Lele Swapper plugin to Saturn plugin format...");

                var saturnTypePlugin = await ConvertLeleToSaturn(plugin);

                Logger.Log("Conversion complete!");
                Logger.Log("Converting Saturn plugin to Saturn item.");

                return await ConvertPluginToItem(saturnTypePlugin);
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
            Name = galaxyPlugin.Name,
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
        var pluginModel = new PluginModel
        {
            Name = lelePlugin.DefaultName + " to " + lelePlugin.SwappedName,
            Icon = lelePlugin.SwappedIcon,
            SwapIcon = lelePlugin.DefaultIcon,
            Message = lelePlugin.Messages[0].localization[0].message
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
        => (ulong) compressedSize ^ (ulong) decompressedSize;


}
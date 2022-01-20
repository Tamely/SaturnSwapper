using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Models.SaturnAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Saturn.Backend.Data.Services;

public interface IPluginService
{
    public Task<PluginModel> ConvertGalaxyToSaturn(string galaxyPlugin);
    public Task<PluginModel> LoadPlugin(string saturnPlugin);
    public Task<SaturnItem> ConvertPluginToItem(PluginModel plugin);
}

public class PluginService : IPluginService
{
    public async Task<SaturnItem> ConvertPluginToItem(PluginModel plugin)
    {
        SaturnItem item = new SaturnItem
        {
            Name = plugin.Name.Split(" to ")[0],
            Description = "Plugin",
            Icon = plugin.Icon,
            Type = ItemType.IT_Misc,
            Rarity = "Legendary",
            Options = new List<SaturnOption>()
            {
                new SaturnOption()
                {
                    Name = plugin.Name.Split(" to ")[1],
                    Icon = plugin.SwapIcon,
                    Rarity = "Legendary"
                }
            }
        };

        foreach (var swap in plugin.Assets)
        {
            List<SaturnSwap> swaps = swap.Swaps.Select(itemSwap => new SaturnSwap() { Search = itemSwap.Search, Replace = itemSwap.Replace }).ToList();

            item.Options[0].Assets.Add(new SaturnAsset()
            {
                ParentAsset = swap.AssetPath,
                Swaps = swaps
            });
        }

        return item;
    }
    
    
    public async Task<PluginModel> LoadPlugin(string saturnPlugin)
    {
        return JsonConvert.DeserializeObject<PluginModel>(saturnPlugin) ?? new PluginModel();
    }
    
    public async Task<PluginModel> ConvertGalaxyToSaturn(string galaxyPlugin)
    {
        dynamic plugin = JObject.Parse(galaxyPlugin);
        var pluginModel = new PluginModel
        {
            Name = plugin.Name,
            Icon = plugin.Icon,
            SwapIcon = plugin.Swapicon,
            Message = plugin.Message
        };


        List<Asset> assets = new List<Asset>();

        foreach (var asset in plugin.Assets)
        {
            List<Swap> swaps = new List<Swap>();
            foreach (var swap in asset.Swaps)
            {
                if (swap.type.ToLower().ToString() == "string")
                {
                    swaps.Add(new Swap()
                    {
                        Search = swap.search.ToString(),
                        Replace = swap.replace.ToString()
                    });
                }
                else
                {
                    swaps.Add(new Swap()
                    {
                        Search = "hex=" + swap.search.ToString(),
                        Replace = "hex=" + swap.replace.ToString()
                    });
                }
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
}
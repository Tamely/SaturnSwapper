using System;
using Newtonsoft.Json;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Saturn.Backend.Data.Services
{
    public interface IConfigService
    {
        public Configuration ConfigFile { get; set; }
        public Task<List<ConvertedItem>> TryGetConvertedItems();
        public Task<bool> RemoveConvertedItem(string id);
        public Task<bool> AddConvertedItem(ConvertedItem item);
        public Task<bool> TryGetShouldRarityConvert();
        public Task<bool> TrySetShouldRarityConvert(bool shouldConvert);
        public Task<bool> TryGetShouldSeriesConvert();
        public Task<bool> TrySetShouldSeriesConvert(bool shouldConvert);
        public Task<bool> TryGetShouldShowStyles();
        public Task<bool> TrySetShouldShowStyles(bool shouldShow);
        public Task<string> TryGetFortniteVersion();
        public Task<bool> TrySetFortniteVersion(string fortniteBuild);
        public Task<int> GetConvertedFileCount();
        public void SaveConfig();
    }

    public class ConfigService : IConfigService
    {
        private readonly IFortniteAPIService _fortniteAPIService;
        public ConfigService(IFortniteAPIService fortniteAPIService)
        {
            _fortniteAPIService = fortniteAPIService;
            
            if (!TryGetConfig())
                Logger.Log("There was an error parsing the config. Generating new one!", LogLevel.Warning);
            if (!TrySetFortniteLocation().GetAwaiter().GetResult())
                Logger.Log("There was an error settings Fortnite's install location!", LogLevel.Error);
        }

        public Configuration ConfigFile { get; set; }

        // Saves the config. Used on exit.
        public void SaveConfig()
        {
            var json = JsonConvert.SerializeObject(ConfigFile, Formatting.Indented);
            File.WriteAllText(Config.ConfigPath, json);
        }

        // Returns true if the item was added, false if it couldn't be added
        public async Task<bool> AddConvertedItem(ConvertedItem item)
        {
            try
            {
                ConfigFile.ConvertedItems.Add(item);
                return true;
            }
            catch
            {
                Logger.Log($"There was an error adding {item.Name} to the converted items list! Moving on.",
                    LogLevel.Error);
                return false;
            }
        }

        // Returns true if the item was removed, false if it wasn't found or item couldn't be removed
        public async Task<bool> RemoveConvertedItem(string id)
        {
            return ConfigFile.ConvertedItems.Any(x =>
            {
                if (x.ItemDefinition != id) return false;
                try
                {
                    ConfigFile.ConvertedItems.Remove(x);
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        // Returns converted items if they exist, otherwise returns new list
        public async Task<List<ConvertedItem>> TryGetConvertedItems()
        {
            try
            {
                return await GetConvertedItems();
            }
            catch
            {
                Logger.Log("There was an error getting the converted items! Setting them to none.", LogLevel.Error);
                return new List<ConvertedItem>();
            }
        }

        // Returns true if it sets the configs location, false if it can't.
        public async Task<bool> TrySetFortniteLocation()
        {
            try
            {
                ConfigFile.InstallLocation = FortniteUtil.GetFortnitePath();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryGetConfig()
        {
            try
            {
                ConfigFile =
                    JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(Config.ConfigPath));
                return true;
            }
            catch
            {
                ConfigFile = new Configuration()
                {
                    FortniteBuild = _fortniteAPIService.GetAES().Build
                };
                return false;
            }
        }

        public async Task<int> GetConvertedFileCount()
        {
            List<string> convertedFiles = new List<string>();
            foreach (var swap in from item in ConfigFile.ConvertedItems from swap in item.Swaps where convertedFiles.IndexOf(swap.File) == -1 select swap)
                convertedFiles.Add(swap.File);
            return convertedFiles.Count;
        }

        public async Task<string> TryGetFortniteVersion()
        {
            try
            {
                return ConfigFile.FortniteBuild;
            }
            catch
            {
                return "Null or error";
            }
        }

        public async Task<bool> TrySetFortniteVersion(string fortniteBuild)
        {
            try
            {
                ConfigFile.FortniteBuild = fortniteBuild;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TryGetShouldRarityConvert()
        {
            try
            {
                return ConfigFile.ShouldPickaxeSwapRarity;
            }
            catch
            {
                ConfigFile.ShouldPickaxeSwapRarity = true;
                return true;
            }
        }
        
        public async Task<bool> TrySetShouldRarityConvert(bool shouldConvert)
        {
            try
            {
                ConfigFile.ShouldPickaxeSwapRarity = shouldConvert;
                SaveConfig();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TryGetShouldSeriesConvert()
        {
            try
            {
                return ConfigFile.ShouldPickaxeSwapSeries;
            }
            catch
            {
                ConfigFile.ShouldPickaxeSwapSeries = true;
                return true;
            }
        }
        
        public async Task<bool> TrySetShouldSeriesConvert(bool shouldConvert)
        {
            try
            {
                ConfigFile.ShouldPickaxeSwapSeries = shouldConvert;
                SaveConfig();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TryGetShouldShowStyles()
        {
            try
            {
                return ConfigFile.ShouldShowStyles;
            }
            catch
            {
                ConfigFile.ShouldShowStyles = true;
                return true;
            }
        }

        public async Task<bool> TrySetShouldShowStyles(bool shouldShow)
        {
            try
            {
                ConfigFile.ShouldShowStyles = shouldShow;
                SaveConfig();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<List<ConvertedItem>> GetConvertedItems()
        {
            return ConfigFile.ConvertedItems;
        }
    }
}
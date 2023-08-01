using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CUE4Parse;
using Saturn.Backend.Data.Asset;
using Saturn.Backend.Data.Fortnite;
using Saturn.Backend.Data.SaturnAPI.Models;
using Saturn.Backend.Data.Swapper.Assets;
using Saturn.Backend.Data.Swapper.Core.Models;
using Saturn.Backend.Data.Swapper.Swapping;
using Saturn.Backend.Data.Variables;
using Serilog;

namespace Saturn.Backend.Data.Plugins.TEMPORARY;

public class PluginLogic
{
    private static Dictionary<string, List<string>> Downloads = new();
    public static async Task Convert(Plugin plugin)
    {
        plugin.Description = $"Starting to convert: {plugin.Name}";
        Downloads[plugin.Name] = new List<string>();
        List<SwapData> swapData = new();
        
        foreach (var url in plugin.Downloads ?? new())
        {
            Downloads[plugin.Name].Add(url);
        }
        
        foreach (var asset in plugin.Assets)
        {
            plugin.Description = $"Converting asset: {Path.GetFileNameWithoutExtension(asset.AssetPath)}";
            Logger.Log($"Converting asset: {Path.GetFileNameWithoutExtension(asset.AssetPath)}");

            var pkg = await Constants.Provider.SavePackageAsync(asset.AssetPath.Split('.')[0] + ".uasset");
            Deserializer deserializer = new Deserializer(pkg.Values.First());
            deserializer.Deserialize();
            
            Logger.Log("Deserialized");
            
            var data = SaturnData.ToNonStatic();
            SaturnData.Clear();
            
            if (!string.IsNullOrWhiteSpace(asset.AssetSwap))
            {
                Logger.Log("Asset swap isnt null");
                if (asset.AssetSwap.ToLower() is "invalidate")
                {
                    deserializer.Invalidate();
                    Logger.Log("Invalidated");
                }
                else
                {
                    var newPkg = await Constants.Provider.SavePackageAsync(asset.AssetSwap.Split('.')[0] + ".uasset");
                    Deserializer newDeserializer = new Deserializer(newPkg.Values.First());
                    newDeserializer.Deserialize();

                    deserializer = deserializer.Swap(newDeserializer);
                }
            }

            foreach (var swap in asset.SoftObjectSwaps ?? new())
            {
                Logger.Log($"Searching: {swap.Search} and replacing with {swap.Replace}");
                deserializer.SwapNameMap(swap.Search, swap.Replace);
            }

            Logger.Log("Serializing");
            byte[] assetBytes = new Serializer(deserializer).Serialize();

            foreach (var swap in asset.ByteSwaps ?? new())
            {
                Logger.Log("byte[] repalcement");
                if (swap.Search.Length < swap.Replace.Length)
                    throw new Exception("Search length is greater than replace length for byte[] swap!");

                byte[] replaceData = Enumerable.Repeat((byte)0x00, swap.Search.Length).ToArray();
                Buffer.BlockCopy(swap.Replace, 0, replaceData, 0, swap.Replace.Length); // Fill the length with 0 (in case it's mismatched)

                int offset = Utilities.IndexOfSequence(assetBytes, swap.Search);
                if (offset == -1) continue;

                Buffer.BlockCopy(replaceData, 0, assetBytes, offset, replaceData.Length);
            }

            Logger.Log("adding to swap data");
            swapData.Add(new SwapData
            {
                SaturnData = data,
                Data = assetBytes
            });

            SaturnData.Clear();
        }
        
        Constants.SelectedOption = new AssetSelectorItem()
        {
            DisplayName = "Plugin",
            ID = "Plugin"
        };
        
        Constants.SelectedItem = new AssetSelectorItem()
        {
            DisplayName = plugin.Name,
            ID = plugin.Name
        };
        
        Logger.Log("converting");
        await FileLogic.Convert(swapData);

        Constants.SelectedOption = new();
        Constants.SelectedItem = new();

        Logger.Log("Filling out downloads");
        foreach (var url in Downloads[plugin.Name])
        {
            WebClient wc = new WebClient();
            wc.DownloadFile(url, Constants.BasePath + "temp.zip");
            ZipFile.ExtractToDirectory(Constants.BasePath + "temp.zip", DataCollection.GetGamePath());
            File.Delete(Constants.BasePath + "temp.zip");
        }
        
        Logger.Log("finished");
        plugin.Description = $"Finished converting: {plugin.Name}";
    }

    public static async Task Revert(Plugin plugin)
    {
        plugin.Description = $"Reverting: {plugin.Name}";
        
        if (File.Exists(Constants.DataPath + plugin.Name + ".json"))
        {
            File.Delete(Constants.DataPath + plugin.Name + ".json");
        }
        
        foreach (var url in Downloads[plugin.Name])
        {
            WebClient wc = new WebClient();
            wc.DownloadFile(url, Constants.BasePath + "temp.zip");
            Directory.CreateDirectory(Constants.BasePath + "TempDirectory");
            ZipFile.ExtractToDirectory(Constants.BasePath + "temp.zip", Constants.BasePath + "TempDirectory");

            foreach (var file in Directory.EnumerateFiles(Constants.BasePath + "TempDirectory"))
            {
                File.Delete(Path.Join(DataCollection.GetGamePath(), Path.GetFileName(file)));
            }

            Directory.Delete(Constants.BasePath + "TempDirectory", true);
        }
        
        plugin.Description = $"Successfully reverted: {plugin.Name}";
    }
}
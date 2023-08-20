using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CUE4Parse.UE4.Assets.Objects;
using Saturn.Backend.Data.Swapper.Assets;
using Saturn.Backend.Data.Variables;

namespace Saturn.Backend.Data.Swapper.Generation;

public class OptionHandler
{
    public List<AssetSelectorItem> PerfectOptions { get; set; }
    public List<AssetSelectorItem> Options { get; set; }

    public async Task<AssetExportData> GenerateOptionDataWithFix(AssetSelectorItem option)
    {
        AssetExportData exportData;
        if (Constants.AssetCache.TryGetValue(option.ID, out var value))
        {
            exportData = value;
        }
        else
        {
            exportData = await AssetExportData.Create(option.Asset, option.Type, Array.Empty<FStructFallback>());
            FixPartData(exportData);
            Constants.AssetCache.Add(option.ID, exportData);
        }
        return exportData;
    }

    private static void FixPartData(AssetExportData exportData)
    {
        if (exportData.ExportParts.Any(part => Enum.Parse<EFortCustomPartType>(part.Part) == EFortCustomPartType.Face)
            && exportData.ExportParts.All(part => Enum.Parse<EFortCustomPartType>(part.Part) != EFortCustomPartType.Hat))
        {
            exportData.ExportParts.First(part => Enum.Parse<EFortCustomPartType>(part.Part) == EFortCustomPartType.Face).Part = EFortCustomPartType.Hat.ToString();
        }
        
        if (exportData.ExportParts.Any(part => Enum.Parse<EFortCustomPartType>(part.Part) == EFortCustomPartType.Face)
            && exportData.ExportParts.Any(part => Enum.Parse<EFortCustomPartType>(part.Part) == EFortCustomPartType.Hat))
        {
            if (exportData.ExportParts
                    .First(part => Enum.Parse<EFortCustomPartType>(part.Part) == EFortCustomPartType.Face)
                    .MorphName is "None"
                && exportData.ExportParts
                    .First(part => Enum.Parse<EFortCustomPartType>(part.Part) == EFortCustomPartType.Hat)
                    .MorphName is not "None")
            {
                exportData.ExportParts.First(part => Enum.Parse<EFortCustomPartType>(part.Part) == EFortCustomPartType.Face).Part = EFortCustomPartType.EFortCustomPartType_MAX.ToString();
                exportData.ExportParts.First(part => Enum.Parse<EFortCustomPartType>(part.Part) == EFortCustomPartType.Hat).Part = EFortCustomPartType.Face.ToString();
                exportData.ExportParts.First(part => Enum.Parse<EFortCustomPartType>(part.Part) == EFortCustomPartType.EFortCustomPartType_MAX).Part = EFortCustomPartType.Hat.ToString();
            }
        }

        foreach (var stylePart in exportData.StyleExportParts)
        {
            if (exportData.ExportParts.Any(part => part.Part == stylePart.Part))
            {
                exportData.ExportParts.RemoveAll(part => part.Part == stylePart.Part);
            }
            exportData.ExportParts.Add(stylePart);
        }
    }

    public static async Task<OptionHandler> CreateAssetOptions(AssetSelectorItem item, List<FStructFallback> styles)
    {
        OptionHandler data = new()
        {
            PerfectOptions = new List<AssetSelectorItem>(),
            Options = new List<AssetSelectorItem>()
        };
        
        await Constants.Handler.Reset();

        AssetExportData exportData = await AssetExportData.Create(item.Asset, item.Type, styles.ToArray());
        
        if (Constants.ShouldGlobalSwap)
        {
            List<AssetSelectorItem> options = await Constants.Handler.Handler.ExecuteWithFileBias(Constants.PotentialOptions);
            await Parallel.ForEachAsync(options, async (option, token) =>
            {
                data.PerfectOptions.Add(option);
                data.Options.Add(option);
            });
            
            var random = data.PerfectOptions.First(x => x.IsRandom);
            data.PerfectOptions.RemoveAll(x => x.IsRandom);
            data.Options.RemoveAll(x => x.IsRandom);
            
            data.PerfectOptions.Insert(0, random);
            data.Options.Insert(0, random);
        }
        else
        {
            await Constants.Handler.Reset();
            List<AssetSelectorItem> options = await Constants.Handler.Handler.ExecuteWithFileBias(Constants.PotentialOptions);
            await Parallel.ForEachAsync(options, async (option, token) =>
            {
                if (option.IsRandom) return;
                if ((option.Series ?? "").Equals(item.Series ?? ""))
                {
                    if (!string.IsNullOrWhiteSpace(exportData.WeaponActorClass))
                    {
                        AssetExportData optionExportData;
                        if (Constants.AssetCache.TryGetValue(option.ID, out var value))
                        {
                            optionExportData = value;
                        }
                        else
                        {
                            optionExportData = await AssetExportData.Create(option.Asset, option.Type, Array.Empty<FStructFallback>());
                            Constants.AssetCache.Add(option.ID, optionExportData);
                        }
                        
                        if (!(optionExportData.WeaponActorClass ?? "").Equals(exportData.WeaponActorClass ?? "", StringComparison.InvariantCultureIgnoreCase))
                            return;
                    }
                    
                    data.PerfectOptions.Add(option);
                    data.Options.Add(option);
                }
            });
        }

        return data;
    }
    
    public static async Task<OptionHandler> CreateCharacterPartOptions(AssetSelectorItem item, List<FStructFallback> styles)
    {
        OptionHandler data = new()
        {
            PerfectOptions = new List<AssetSelectorItem>(),
            Options = new List<AssetSelectorItem>()
        };
        
        await Constants.Handler.Reset();

        if (Constants.ShouldGlobalSwap)
        {
            List<AssetSelectorItem> options = await Constants.Handler.Handler.ExecuteWithFileBias(Constants.PotentialOptions);

            await Parallel.ForEachAsync(options, async (option, token) =>
            {
                data.PerfectOptions.Add(option);
                data.Options.Add(option);
            });
            
            var random = data.PerfectOptions.First(x => x.IsRandom);
            data.PerfectOptions.RemoveAll(x => x.IsRandom);
            data.Options.RemoveAll(x => x.IsRandom);
            
            data.PerfectOptions.Insert(0, random);
            data.Options.Insert(0, random);
        }
        else
        {
            AssetExportData exportData = await AssetExportData.Create(item.Asset, item.Type, styles.ToArray());
            FixPartData(exportData);

            Constants.AssetCache[item.ID] = exportData;

            await Constants.Handler.Reset();
            List<AssetSelectorItem> options = await Constants.Handler.Handler.ExecuteWithFileBias(Constants.PotentialOptions);
            await Parallel.ForEachAsync(options, async (option, token) =>
            {
                if (option.IsRandom) return;
                bool isPerfect = true;
            
                AssetExportData optionExportData;
                if (Constants.AssetCache.TryGetValue(option.ID, out var value))
                {
                    optionExportData = value;
                }
                else
                {
                    optionExportData = await AssetExportData.Create(option.Asset, option.Type, Array.Empty<FStructFallback>());
                    FixPartData(optionExportData);
                    Constants.AssetCache.Add(option.ID, optionExportData);
                }

                if (exportData.ExportParts.Any(part => optionExportData.ExportParts.All(optionPart => optionPart.Part != part.Part)))
                {
                    return;
                }

                foreach (var _ in from part in exportData.ExportParts 
                         let optionPart = optionExportData.ExportParts.First(optionPart => part.Part == optionPart.Part) 
                         where part.MorphName != "None" 
                         where part.MorphName != "None" && optionPart.MorphName == "None" 
                         select optionPart)
                {
                    isPerfect = false;
                }

                if (isPerfect)
                {
                    data.PerfectOptions.Add(option);
                }

                data.Options.Add(option);
            });
        }
        
        return data;
    }
}
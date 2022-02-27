using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse_Conversion.Textures;
using Newtonsoft.Json;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.FortniteAPI;
using Saturn.Backend.Data.Services;
using Saturn.Backend.Data.SwapOptions.Skins;
using SkiaSharp;

namespace Saturn.Backend.Data.Utils.Swaps;

public static class CosmeticGeneration
{
    public static async Task GenerateSkins(this List<Cosmetic> skins, 
                                            DefaultFileProvider _provider, 
                                            IConfigService _configService, 
                                            ISwapperService _swapperService)
    {
        bool shouldShowStyles = await _configService.TryGetShouldShowStyles();

        if (File.Exists(Config.SkinsCache))
                skins = JsonConvert.DeserializeObject<List<Cosmetic>>(await File.ReadAllTextAsync(Config.SkinsCache));

        foreach (var (assetPath, _) in _provider.Files)
        {
            if (!assetPath.Contains("/CID_")) continue;
                
            // doing a foreach instead of LINQ because LINQ is fucking stupid and slow
            bool any = false;
            foreach (var x in skins)
            {
                if (x.Id != FileUtil.SubstringFromLast(assetPath, '/').Split('.')[0]) continue;
                any = true;
                break;
            }
            if (any)
                continue;
                
            List<Cosmetic> CosmeticsToInsert = new();
                
            if (_provider.TryLoadObject(assetPath.Split('.')[0], out var asset))
            {
                Cosmetic skin = new();

                skin.Name = asset.TryGetValue(out FText DisplayName, "DisplayName") ? DisplayName.Text : "TBD";
                skin.Description = asset.TryGetValue(out FText Description, "Description") ? Description.Text : "To be determined...";

                skin.Id = FileUtil.SubstringFromLast(assetPath, '/').Split('.')[0];
                    
                if (skin.Name.ToLower() is "null" or "tbd" or "hero" || skin.Id.ToLower().Contains("cid_vip_"))
                    continue;
                    
                skin.Rarity = new Rarity
                {
                    Value = asset.TryGetValue(out EFortRarity Rarity, "Rarity") ? Rarity.ToString().Split("::")[0] : "Uncommon"
                };

                if (skin.Name is "Recruit" or "Random")
                    skin.Rarity.Value = "Common";

                if (skin.Name is "Random")
                    skin.IsRandom = true;

                skin.Series = asset.TryGetValue(out UObject Series, "Series")
                    ? new Series()
                    {
                        BackendValue = FileUtil.SubstringFromLast(Series.GetFullName(), '/').Split('.')[0]
                    } : null;
                    
                skin.Images = new Images();

                if (File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/skins/" + skin.Id + ".png")))
                    skin.Images.SmallIcon = "skins/" + skin.Id + ".png";
                else
                {
                    if (asset.TryGetValue(out UObject HID, "HeroDefinition"))
                    {
                        if (HID.TryGetValue(out UTexture2D smallIcon, "SmallPreviewImage"))
                        {
                            using var ms = new MemoryStream();
                            smallIcon.Decode()?.Encode(ms, SKEncodedImageFormat.Png, 30);

                            Directory.CreateDirectory(Path.Combine(Config.ApplicationPath, "wwwroot/skins/"));
                            if (!File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/skins/" + skin.Id + ".png")))
                                await File.WriteAllBytesAsync(Path.Combine(Config.ApplicationPath, "wwwroot/skins/" + skin.Id + ".png"), ms.ToArray());

                            skin.Images.SmallIcon = "skins/" + skin.Id + ".png";
                        }
                        else
                        {
                            Logger.Log("Cannot parse the small icon for " + skin.Id);
                            continue;
                        }

                    }
                    else
                    {
                        Logger.Log("Cannot parse the HID for " + skin.Id);
                        continue;
                    }
                }

                if (shouldShowStyles)
                {
                    if (asset.TryGetValue(out UObject[] variant, "ItemVariants"))
                    {
                        foreach (var variants in variant)
                        {
                            if (variants.TryGetValue(out FStructFallback FVariantChannelTag,
                                    "VariantChannelTag"))
                            {
                                FVariantChannelTag.TryGetValue(out FName VariantChannelTag, "TagName");
                                if (!VariantChannelTag.Text.Contains("Material") && !VariantChannelTag.Text.Contains("Parts"))
                                    continue;
                                    
                                    
                                if (variants.TryGetValue(out FStructFallback[] MaterialOptions, "MaterialOptions"))
                                    foreach (var MaterialOption in MaterialOptions)
                                    {
                                        if (MaterialOption.TryGetValue(out FStructFallback[] MaterialParams, "VariantMaterialParams") && MaterialParams.Length != 0)
                                            continue;

                                        if (MaterialOption.TryGetValue(out FText VariantName, "VariantName"))
                                        {
                                            FName TagName = new FName();

                                            if (MaterialOption.TryGetValue(out FStructFallback CustomizationVariantTag,
                                                    "CustomizationVariantTag"))
                                                CustomizationVariantTag.TryGetValue(out TagName, "TagName");

                                            if (string.IsNullOrWhiteSpace(VariantName.Text) || VariantName.Text.ToLower() == "default" || VariantName.Text.ToLower() == skin.Name.ToLower())
                                                continue;
                                                
                                            if (!File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/skins/" + skin.Id + "_" + VariantName.Text.Replace(" ","_").Replace("\\","").Replace("/","") + ".png") + ".png"))
                                            {
                                                if (MaterialOption.TryGetValue(out UTexture2D PreviewImage,
                                                        "PreviewImage"))
                                                {
                                                    await using var ms = new MemoryStream();
                                                    PreviewImage.Decode()?.Encode(ms, SKEncodedImageFormat.Png, 30);
                            
                                                    Directory.CreateDirectory(Path.Combine(Config.ApplicationPath, "wwwroot/skins/"));
                                                    if (!File.Exists(Path.Combine(Config.ApplicationPath, "wwwroot/skins/" + skin.Id + "_" + VariantName.Text.Replace(" ","_").Replace("\\","").Replace("/","") + ".png") + ".png"))
                                                        await File.WriteAllBytesAsync(Path.Combine(Config.ApplicationPath, "wwwroot/skins/" + skin.Id + "_" + VariantName.Text.Replace(" ","_").Replace("\\","").Replace("/","") + ".png"), ms.ToArray());
                                                }
                                            }

                                            CosmeticsToInsert.Add(new Cosmetic()
                                            {
                                                Name = VariantName.Text,
                                                Description = skin.Name + " style: " + skin.Description,
                                                Id = skin.Id,
                                                Rarity = skin.Rarity,
                                                Series = skin.Series,
                                                Images = new Images()
                                                {
                                                    SmallIcon = "skins/" + skin.Id  + "_" + VariantName.Text.Replace(" ","_").Replace("\\","").Replace("/","") + ".png"
                                                },
                                                VariantChannel = VariantChannelTag.Text,
                                                VariantTag = TagName.Text
                                            });
                                        }
                                    }
                            }
                        }
                    }
                }
                    
                foreach (var value in CosmeticsToInsert)
                    skins.Add(await new AddSkins().AddSkinOptions(value, _swapperService, _provider));

                skins.Add(await new AddSkins().AddSkinOptions(skin, _swapperService, _provider));
            }
            else
                Logger.Log($"Failed to load {assetPath}");

            // sort skins by alphabetical order
            skins = skins.OrderBy(x => x.Id).ToList();

            // Remove items from the array that are duplicates
            for (var i = 0; i < skins.Count; i++)
            for (var j = i + 1; j < skins.Count; j++)
            {
                if (skins[i].Name != skins[j].Name || 
                    skins[i].Images.SmallIcon != skins[j].Images.SmallIcon ||
                    skins[i].Description != skins[j].Description) continue;
                skins.RemoveAt(j);
                j--;
            }
        }
    }
}
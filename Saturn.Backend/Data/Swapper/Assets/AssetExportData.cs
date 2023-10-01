using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Exports.StaticMesh;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Objects.GameplayTags;
using CUE4Parse.UE4.Objects.UObject;

namespace Saturn.Backend.Data.Swapper.Assets;

public class AssetExportData : ExportDataBase
{
    public List<ExportPart> ExportParts = new();
    public List<ExportMesh> Parts = new();
    public List<ExportPart> StyleExportParts = new();
    public List<ExportMesh> StyleParts = new();
    public List<ExportMaterial> StyleMaterials = new();
    public List<ExportMeshOverride> StyleMeshes = new();
    public List<ExportMaterialParams> StyleMaterialParams = new();
    public string? PrimaryFireAbility = null;
    public string? Mesh = null;

    public static async Task<AssetExportData?> Create(UObject asset, EAssetType assetType, FStructFallback[] styles)
    {
        var data = new AssetExportData();
        assetType = assetType is EAssetType.Gallery ? EAssetType.Prop : assetType;
        data.Name = assetType is EAssetType.Mesh or EAssetType.Wildlife ? asset.Name : asset.GetOrDefault("DisplayName", new FText("Unnamed")).Text;
        data.Type = assetType.ToString();
        var canContinue = await Task.Run(async () =>
        {
            switch (assetType)
            {
                case EAssetType.Outfit:
                {
                    var parts = asset.GetOrDefault("BaseCharacterParts", Array.Empty<UObject>());
                    if (asset.TryGetValue(out UObject heroDefinition, "HeroDefinition"))
                    {
                        if (parts.Length == 0)
                        {
                            var specializations = heroDefinition.Get<UObject[]>("Specializations").FirstOrDefault();
                            parts = specializations?.GetOrDefault("CharacterParts", Array.Empty<UObject>()) ?? Array.Empty<UObject>();
                        }
                    }

                    data.ExportParts = ExportHelpers.CharacterParts(parts, data.Parts);
                    break;
                }
                case EAssetType.Backpack:
                {
                    var parts = asset.GetOrDefault("CharacterParts", Array.Empty<UObject>());
                    data.ExportParts = ExportHelpers.CharacterParts(parts, data.Parts);
                    break;
                }
                case EAssetType.Glider:
                {
                    var mesh = asset.Get<USkeletalMesh>("SkeletalMesh");
                    if (mesh != null)
                    {
                        data.Mesh = mesh.GetPathName();
                    }

                    data.ExportParts.Add(new ExportPart()
                    {
                        Path = asset.GetPathName(),
                        Part = "Gameplay"
                    });
                    break;
                }
                case EAssetType.Pickaxe:
                {
                    var weapon = asset.Get<UObject>("WeaponDefinition");
                    data.PrimaryFireAbility = ExportHelpers.Weapon(weapon, data.Parts);
                    data.ExportParts.Add(new ExportPart()
                    {
                        Path = weapon.GetPathName(),
                        Part = "Gameplay"
                    });
                    break;
                }
                case EAssetType.Weapon:
                {
                    ExportHelpers.Weapon(asset, data.Parts);
                    break;
                }
                case EAssetType.Mesh:
                {
                    if (asset is UStaticMesh staticMesh)
                    {
                        ExportHelpers.Mesh(staticMesh, data.Parts);
                    }
                    else if (asset is USkeletalMesh skeletalMesh)
                    {
                        ExportHelpers.Mesh(skeletalMesh, data.Parts);
                    }

                    break;
                }
                case EAssetType.Wildlife:
                {
                    ExportHelpers.Mesh(asset as USkeletalMesh, data.Parts);
                    break;
                }
                case EAssetType.Dance:
                {
                    data.ExportParts.Add(new ExportPart()
                    {
                        Path = asset.GetPathName(),
                        Part = "Gameplay"
                    });
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException("Unsupported asset type!");
            }
            
            return true;
        });

        if (!canContinue) return null;

        await Task.Run(() => data.ProcessStyles(asset, styles));
        await Task.WhenAll(ExportHelpers.Tasks);
        return data;
    }
}

public static class AssetExportExtensions
{
    public static void ProcessStyles(this AssetExportData data, UObject asset, FStructFallback[] selectedStyles)
    {
        var totalMetaTags = new List<string>();
        var metaTagsToApply = new List<string>();
        var metaTagsToRemove = new List<string>();
        foreach (var style in selectedStyles)
        {
            var tags = style.Get<FStructFallback>("MetaTags");

            var tagsToApply = tags.Get<FGameplayTagContainer>("MetaTagsToApply");
            metaTagsToApply.AddRange(tagsToApply.GameplayTags.Select(x => x.TagName.Text));

            var tagsToRemove = tags.Get<FGameplayTagContainer>("MetaTagsToRemove");
            metaTagsToRemove.AddRange(tagsToRemove.GameplayTags.Select(x => x.TagName.Text));
        }

        totalMetaTags.AddRange(metaTagsToApply);
        metaTagsToRemove.ForEach(tag => totalMetaTags.RemoveAll(x => x.Equals(tag, StringComparison.OrdinalIgnoreCase)));

        var itemStyles = asset.GetOrDefault("ItemVariants", Array.Empty<UObject>());
        var tagDrivenStyles = itemStyles.Where(style => style.ExportType.Equals("FortCosmeticLoadoutTagDrivenVariant"));
        foreach (var tagDrivenStyle in tagDrivenStyles)
        {
            var options = tagDrivenStyle.Get<FStructFallback[]>("Variants");
            foreach (var option in options)
            {
                var requiredConditions = option.Get<FStructFallback[]>("RequiredConditions");
                foreach (var condition in requiredConditions)
                {
                    var metaTagQuery = condition.Get<FStructFallback>("MetaTagQuery");
                    var tagDictionary = metaTagQuery.Get<FStructFallback[]>("TagDictionary");
                    var requiredTags = tagDictionary.Select(x => x.Get<FName>("TagName").Text).ToList();
                    if (requiredTags.All(x => totalMetaTags.Contains(x)))
                    {
                        ExportStyleData(option, data);
                    }
                }
            }
        }

        foreach (var style in selectedStyles)
        {
            ExportStyleData(style, data);
        }
    }

    private static void ExportStyleData(FStructFallback style, AssetExportData data)
    {
        data.StyleExportParts = ExportHelpers.CharacterParts(style.GetOrDefault("VariantParts", Array.Empty<UObject>()), data.StyleParts);
        ExportHelpers.OverrideMaterials(style.GetOrDefault("VariantMaterials", Array.Empty<FStructFallback>()), data.StyleMaterials);
        ExportHelpers.OverrideMeshes(style.GetOrDefault("VariantMeshes", Array.Empty<FStructFallback>()), data.StyleMeshes);
        ExportHelpers.OverrideParameters(style.GetOrDefault("VariantMaterialParams", Array.Empty<FStructFallback>()), data.StyleMaterialParams);
    }
}
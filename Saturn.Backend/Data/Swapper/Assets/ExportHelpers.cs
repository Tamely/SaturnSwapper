using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CUE4Parse_Conversion.Meshes;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Material;
using CUE4Parse.UE4.Assets.Exports.Material.Editor;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Exports.StaticMesh;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Objects.Engine;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.Utils;

namespace Saturn.Backend.Data.Swapper.Assets;

public static class ExportHelpers
{
    public static readonly List<Task> Tasks = new();
    public static List<ExportPart> CharacterParts(IEnumerable<UObject> inputParts, List<ExportMesh> exportMeshes)
    {
        var exportParts = new List<ExportPart>();
        var headMorphType = ECustomHatType.None;
        var headMorphNames = new Dictionary<ECustomHatType, string>();
        FLinearColor? skinColor = null;
        foreach (var part in inputParts)
        {
            var skeletalMesh = part.GetOrDefault<USkeletalMesh?>("SkeletalMesh");
            if (skeletalMesh is null) continue;

            var exportPart = new ExportPart();
            exportPart.Path = part.GetPathName();
            exportPart.MeshPath = skeletalMesh.GetPathName();

            exportPart.NumLods = (skeletalMesh.LODModels ?? Array.Empty<FStaticLODModel>()).Length;

            var characterPartType = part.GetOrDefault<EFortCustomPartType>("CharacterPartType");
            exportPart.Part = characterPartType.ToString();

            var genderPermitted = part.GetOrDefault("GenderPermitted", EFortCustomGender.Male);
            exportPart.GenderPermitted = genderPermitted;

            if (part.TryGetValue<UObject>(out var additionalData, "AdditionalData"))
            {
                var socketName = additionalData.GetOrDefault<FName?>("AttachSocketName");
                var attachToSocket = part.GetOrDefault("bAttachToSocket", true);
                if (attachToSocket)
                {
                    exportPart.SocketName = socketName?.Text;
                }

                if (additionalData.TryGetValue(out FName hatType, "HatType"))
                {
                    Enum.TryParse(hatType.Text.Replace("ECustomHatType::ECustomHatType_", string.Empty), out headMorphType);
                }

                exportPart.MorphName = headMorphType.ToString();

                if (additionalData.ExportType.Equals("CustomCharacterHeadData"))
                {
                    foreach (var type in Enum.GetValues<ECustomHatType>())
                    {
                        if (additionalData.TryGetValue(out FName[] morphNames, type + "MorphTargets"))
                        {
                            headMorphNames[type] = morphNames[0].Text;
                        }
                    }

                    if (additionalData.TryGetValue(out UObject skinColorSwatch, "SkinColorSwatch"))
                    {
                        var colorPairs = skinColorSwatch.GetOrDefault("ColorPairs", Array.Empty<FStructFallback>());
                        var skinColorPair = colorPairs.FirstOrDefault(x => x.Get<FName>("ColorName").Text.Equals("Skin Boost Color and Exponent", StringComparison.OrdinalIgnoreCase));
                        if (skinColorPair is not null) skinColor = skinColorPair.Get<FLinearColor>("ColorValue");
                    }
                }
            }

            foreach (var mat in skeletalMesh.Materials)
            {
                if (mat is null) continue;
                
                if (!mat.TryLoad(out var materialObject)) continue;
                if (materialObject is not UMaterialInterface material) continue;
                
                var exportMaterial = CreateExportMaterial(material);
                exportPart.Materials.Add(exportMaterial);
            }

            if (part.TryGetValue(out FStructFallback[] materialOverrides, "MaterialOverrides"))
            {
                OverrideMaterials(materialOverrides, ref exportPart);
            }
            
            exportParts.Add(exportPart);
        }

        var headPart = exportParts.FirstOrDefault(x => x.Part.Equals("Head"));
        var bodyPart = exportParts.FirstOrDefault(x => x.Part.Equals("Body"));
        var facePart = exportParts.FirstOrDefault(x => x.Part.Equals("Face"));
        if (headMorphType != ECustomHatType.None && headPart is not null)
        {
            // tag this
        }

        exportMeshes.AddRange(exportParts);
        return exportParts;
    }
    
    public static void OverrideMaterials(FStructFallback[] overrides, ref ExportPart exportPart)
    {
        foreach (var materialOverride in overrides)
        {
            var overrideMaterial = materialOverride.Get<FSoftObjectPath>("OverrideMaterial");
            if (!overrideMaterial.TryLoad(out var materialObject)) continue;
            if (materialObject is not UMaterialInterface material) continue;

            var exportMaterial = CreateExportMaterial<ExportMaterialOverride>(material, materialOverride.Get<int>("MaterialOverrideIndex"));
            exportMaterial.MaterialNameToSwap = materialOverride.GetOrDefault<FSoftObjectPath>("MaterialToSwap").AssetPathName.Text.SubstringAfterLast(".");

            if (exportPart.Materials.Count == 0)
            {
                exportPart.OverrideMaterials.Add(exportMaterial);
            }
            
            for (var idx = 0; idx < exportPart.Materials.Count; idx++)
            {
                if (exportMaterial.SlotIndex >= exportPart.Materials.Count) continue;
                if (exportPart.Materials[exportMaterial.SlotIndex].Hash == exportPart.Materials[idx].Hash)
                {
                    exportPart.OverrideMaterials.Add(exportMaterial with { SlotIndex = idx });
                }
            }
        }
    } 
    
    public static void OverrideMaterials(FStructFallback[] overrides, List<ExportMaterial> exportMaterials)
    {
        foreach (var materialOverride in overrides)
        {
            var overrideMaterial = materialOverride.Get<FSoftObjectPath>("OverrideMaterial");
            if (!overrideMaterial.TryLoad(out var materialObject)) continue;
            if (materialObject is not UMaterialInterface material) continue;

            var exportMaterial = CreateExportMaterial<ExportMaterialOverride>(material, materialOverride.Get<int>("MaterialOverrideIndex"));
            exportMaterial.MaterialNameToSwap = materialOverride.GetOrDefault<FSoftObjectPath>("MaterialToSwap").AssetPathName.Text.SubstringAfterLast(".");

            exportMaterials.Add(exportMaterial);
        }
    }
    
    public static (List<TextureParameter>, List<ScalarParameter>, List<VectorParameter>) MaterialParametersOverride(FStructFallback data)
    {
        var textures = new List<TextureParameter>();
        foreach (var parameter in data.GetOrDefault("TextureParams", Array.Empty<FStructFallback>()))
        {
            if (!parameter.TryGetValue(out UTexture2D texture, "Value")) continue;
            textures.Add(new TextureParameter(parameter.Get<FName>("ParamName").Text, texture.GetPathName(), texture.SRGB, texture.CompressionSettings));
        }

        var scalars = new List<ScalarParameter>();
        foreach (var parameter in data.GetOrDefault("FloatParams", Array.Empty<FStructFallback>()))
        {
            var scalar = parameter.Get<float>("Value");
            scalars.Add(new ScalarParameter(parameter.Get<FName>("ParamName").Text, scalar));
        }

        var vectors = new List<VectorParameter>();
        foreach (var parameter in data.GetOrDefault("ColorParams", Array.Empty<FStructFallback>()))
        {
            if (!parameter.TryGetValue(out FLinearColor color, "Value")) continue;
            vectors.Add(new VectorParameter(parameter.Get<FName>("ParamName").Text, color));
        }

        return (textures, scalars, vectors);
    }
    
    public static ExportMesh? Mesh(USkeletalMesh? skeletalMesh)
    {
        return Mesh<ExportMesh>(skeletalMesh);
    }

    public static T? Mesh<T>(USkeletalMesh? skeletalMesh) where T : ExportMesh, new()
    {
        if (skeletalMesh is null) return null;
        if (!skeletalMesh.TryConvert(out var convertedMesh)) return null;
        if (convertedMesh.LODs.Count <= 0) return null;

        var exportMesh = new T();
        exportMesh.MeshPath = skeletalMesh.GetPathName();

        exportMesh.NumLods = convertedMesh.LODs.Count;

        var sections = convertedMesh.LODs[0].Sections.Value;
        for (var idx = 0; idx < sections.Length; idx++)
        {
            var section = sections[idx];
            if (section.Material is null) continue;

            if (!section.Material.TryLoad(out var materialObject)) continue;
            if (materialObject is not UMaterialInterface material) continue;

            var exportMaterial = CreateExportMaterial(material, idx);
            exportMesh.Materials.Add(exportMaterial);
        }

        return exportMesh;
    }
    
    public static void Mesh<T>(USkeletalMesh? skeletalMesh, List<T> exportParts) where T : ExportMesh, new()
    {
        if (skeletalMesh is null) return;
        if (!skeletalMesh.TryConvert(out var convertedMesh)) return;
        if (convertedMesh.LODs.Count <= 0) return;

        var exportPart = new T();
        exportPart.MeshPath = skeletalMesh.GetPathName();

        exportPart.NumLods = convertedMesh.LODs.Count;

        var sections = convertedMesh.LODs[0].Sections.Value;
        for (var idx = 0; idx < sections.Length; idx++)
        {
            var section = sections[idx];
            if (section.Material is null) continue;

            if (!section.Material.TryLoad(out var materialObject)) continue;
            if (materialObject is not UMaterialInterface material) continue;

            var exportMaterial = CreateExportMaterial(material, idx);
            exportPart.Materials.Add(exportMaterial);
        }

        exportParts.Add(exportPart);
    }

    public static void Mesh<T>(UStaticMesh? staticMesh, List<T> exportParts) where T : ExportMesh, new()
    {
        if (staticMesh is null) return;
        if (!staticMesh.TryConvert(out var convertedMesh)) return;
        if (convertedMesh.LODs.Count <= 0) return;

        var exportPart = new T();
        exportPart.MeshPath = staticMesh.GetPathName();

        exportPart.NumLods = convertedMesh.LODs.Count;

        var sections = convertedMesh.LODs[0].Sections.Value;
        for (var idx = 0; idx < sections.Length; idx++)
        {
            var section = sections[idx];
            if (section.Material is null) continue;


            if (!section.Material.TryLoad(out var materialObject)) continue;
            if (materialObject is not UMaterialInterface material) continue;

            var exportMaterial = CreateExportMaterial(material, idx);
            exportPart.Materials.Add(exportMaterial);
        }

        exportParts.Add(exportPart);
    }
    
    public static void OverrideMeshes(FStructFallback[] overrides, List<ExportMeshOverride> exportMeshes)
    {
        foreach (var meshOverride in overrides)
        {
            var meshToSwap = meshOverride.Get<FSoftObjectPath>("MeshToSwap");
            var meshToOverride = meshOverride.Get<USkeletalMesh>("OverrideMesh");

            var overrideMeshExport = Mesh<ExportMeshOverride>(meshToOverride);
            if (overrideMeshExport is null) continue;

            overrideMeshExport.MeshToSwap = meshToSwap.AssetPathName.Text;
            exportMeshes.Add(overrideMeshExport);
        }
    }
    
    public static void OverrideParameters(FStructFallback[] overrides, List<ExportMaterialParams> exportParams)
    {
        foreach (var paramData in overrides)
        {
            var exportMaterialParams = new ExportMaterialParams();
            exportMaterialParams.MaterialToAlter = paramData.Get<FSoftObjectPath>("MaterialToAlter").AssetPathName.Text;
            (exportMaterialParams.Textures, exportMaterialParams.Scalars, exportMaterialParams.Vectors) = MaterialParametersOverride(paramData);
            exportMaterialParams.Hash = exportMaterialParams.GetHashCode();
            exportParams.Add(exportMaterialParams);
        }
    }
    
    public static string Weapon(UObject weaponDefinition, List<ExportMesh> exportParts)
    {
        var weapons = GetWeaponMeshes(weaponDefinition);
        foreach (var weapon in weapons)
        {
            if (weapon is UStaticMesh staticMesh)
            {
                Mesh(staticMesh, exportParts);
            }
            else if (weapon is USkeletalMesh skeletalMesh)
            {
                Mesh(skeletalMesh, exportParts);
            }
        }
        
        return weaponDefinition.GetOrDefault<FSoftObjectPath>("WeaponActorClass").AssetPathName.Text;
    }
    
    public static List<UObject?> GetWeaponMeshes(UObject weaponDefinition)
    {
        var weapons = new List<UObject?>();
        USkeletalMesh? mainSkeletalMesh = null;
        mainSkeletalMesh = weaponDefinition.GetOrDefault("PickupSkeletalMesh", mainSkeletalMesh);
        mainSkeletalMesh = weaponDefinition.GetOrDefault("WeaponMeshOverride", mainSkeletalMesh);
        weapons.Add(mainSkeletalMesh);

        if (mainSkeletalMesh is null)
        {
            weaponDefinition.TryGetValue(out UStaticMesh? mainStaticMesh, "PickupStaticMesh");
            weapons.Add(mainStaticMesh);
        }

        weaponDefinition.TryGetValue(out USkeletalMesh? offHandMesh, "WeaponMeshOffhandOverride");
        weapons.Add(offHandMesh);

        if (weapons.Count > 0) return weapons;

        // TODO MATERIAL STYLES
        if (weaponDefinition.TryGetValue(out UBlueprintGeneratedClass blueprint, "WeaponActorClass"))
        {
            var defaultObject = blueprint.ClassDefaultObject.Load()!;
            if (defaultObject.TryGetValue(out UObject weaponMeshData, "WeaponMesh"))
            {
                weapons.Add(weaponMeshData.GetOrDefault<USkeletalMesh>("SkeletalMesh"));
            }

            if (defaultObject.TryGetValue(out UObject leftWeaponMeshData, "LeftHandWeaponMesh"))
            {
                weapons.Add(leftWeaponMeshData.GetOrDefault<USkeletalMesh>("SkeletalMesh"));
            }
        }

        return weapons;
    }

    public static ExportMaterial CreateExportMaterial(UMaterialInterface material, int materialIndex = 0)
    {
        return CreateExportMaterial<ExportMaterial>(material, materialIndex);
    }

    public static T CreateExportMaterial<T>(UMaterialInterface material, int materialIndex = 0)
        where T : ExportMaterial, new()
    {
        var exportMaterial = new T
        {
            MaterialPath = material.GetPathName(),
            MaterialName = material.Name,
            SlotIndex = materialIndex
        };

        if (material is UMaterialInstanceConstant materialInstance)
        {
            var (textures, scalars, vectors, switches, componentMasks) = MaterialParameters(materialInstance);
            exportMaterial.Textures = textures;
            exportMaterial.Scalars = scalars;
            exportMaterial.Vectors = vectors;
            exportMaterial.Switches = switches;
            exportMaterial.ComponentMasks = componentMasks;
            exportMaterial.IsGlass = IsGlassMaterial(materialInstance);
            exportMaterial.MasterMaterialName = materialInstance.GetLastParent()?.Name;
        }
        else if (material is { } materialInterface)
        {
            var (textures, scalars, vectors) = MaterialParameters(materialInterface);
            exportMaterial.Textures = textures;
            exportMaterial.Scalars = scalars;
            exportMaterial.Vectors = vectors;
            exportMaterial.IsGlass = IsGlassMaterial(materialInterface);
        }

        exportMaterial.Hash = material.GetPathName().GetHashCode();
        return exportMaterial;
    }

    public static bool IsGlassMaterial(UMaterialInstanceConstant? materialInstance)
    {
        if (materialInstance is null) return false;
        
        var lastParent = materialInstance.GetLastParent();
        if (lastParent is null) return false;
        
        var glassMaterialNames = new[]
        {
            "M_MED_Glass_Master",
            "M_MED_Glass_WithDiffuse",
            "M_Valet_Glass_Master",
            "M_MineralPowder_Glass",
            "M_CP_GlassGallery_Master",
            "M_LauchTheBalloon_Microwave_Glass",
            "M_MED_Glass_HighTower",
            "M_OctopusBall",
            "F_MED_SharpFang_Backpack_Glass_Master"
        };

        return glassMaterialNames.Contains(lastParent.Name, StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsGlassMaterial(UMaterialInterface material)
    {
        if (material is UMaterialInstanceConstant materialInstance)
        {
            return IsGlassMaterial(materialInstance);
        }
        
        var glassMaterialNames = new[]
        {
            "M_MED_Glass_Master",
            "M_MED_Glass_WithDiffuse",
            "M_Valet_Glass_Master",
            "M_MineralPowder_Glass",
            "M_CP_GlassGallery_Master",
            "M_LauchTheBalloon_Microwave_Glass",
            "M_MED_Glass_HighTower",
            "M_OctopusBall"
        };

        return glassMaterialNames.Contains(material.Name, StringComparer.OrdinalIgnoreCase);
    }
    
    public static (List<TextureParameter>, List<ScalarParameter>, List<VectorParameter>, List<SwitchParameter>, List<ComponentMaskParameter>) MaterialParameters(UMaterialInstanceConstant materialInstance)
    {
        var textures = new List<TextureParameter>();
        foreach (var parameter in materialInstance.TextureParameterValues)
        {
            if (!parameter.ParameterValue.TryLoad(out UTexture2D texture)) continue;
            textures.Add(new TextureParameter(parameter.Name, texture.GetPathName(), texture.SRGB, texture.CompressionSettings));
        }

        var scalars = new List<ScalarParameter>();
        foreach (var parameter in materialInstance.ScalarParameterValues)
        {
            scalars.Add(new ScalarParameter(parameter.Name, parameter.ParameterValue));
        }

        var vectors = new List<VectorParameter>();
        foreach (var parameter in materialInstance.VectorParameterValues)
        {
            if (parameter.ParameterValue is null) continue;
            vectors.Add(new VectorParameter(parameter.Name, parameter.ParameterValue.Value));
        }
        
        var switches = new List<SwitchParameter>();
        var componentMasks = new List<ComponentMaskParameter>();
        if (materialInstance.StaticParameters is not null)
        {
            foreach (var parameter in materialInstance.StaticParameters.StaticSwitchParameters)
            {
                if (parameter.ParameterInfo is null) continue;
                switches.Add(new SwitchParameter(parameter.Name, parameter.Value));
            }
            
            foreach (var parameter in materialInstance.StaticParameters.StaticComponentMaskParameters)
            {
                if (parameter.ParameterInfo is null) continue;
                componentMasks.Add(new ComponentMaskParameter(parameter.Name, parameter.ToLinearColor()));
            }
        }

        if (materialInstance.TryLoadEditorData<UMaterialInstanceEditorOnlyData>(out var materialInstanceEditorData) && materialInstanceEditorData.StaticParameters is not null)
        {
            foreach (var parameter in materialInstanceEditorData.StaticParameters.StaticSwitchParameters)
            {
                if (parameter.ParameterInfo is null) continue;
                switches.AddUnique(new SwitchParameter(parameter.Name, parameter.Value));
            }
            
            foreach (var parameter in materialInstanceEditorData.StaticParameters.StaticComponentMaskParameters)
            {
                if (parameter.ParameterInfo is null) continue;
                componentMasks.AddUnique(new ComponentMaskParameter(parameter.Name, parameter.ToLinearColor()));
            }
        }

        if (materialInstance.Parent is UMaterialInstanceConstant materialParent)
        {
            var (parentTextures, parentScalars, parentVectors, parentSwitches, parentComponentMasks) = MaterialParameters(materialParent);
            foreach (var parentTexture in parentTextures)
            {
                if (textures.Any(x => x.Name.Equals(parentTexture.Name))) continue;
                textures.Add(parentTexture);
            }

            foreach (var parentScalar in parentScalars)
            {
                if (scalars.Any(x => x.Name.Equals(parentScalar.Name))) continue;
                scalars.Add(parentScalar);
            }

            foreach (var parentVector in parentVectors)
            {
                if (vectors.Any(x => x.Name.Equals(parentVector.Name))) continue;
                vectors.Add(parentVector);
            }
            
            foreach (var parentSwitch in parentSwitches)
            {
                if (switches.Any(x => x.Name.Equals(parentSwitch.Name))) continue;
                switches.Add(parentSwitch);
            }
            
            foreach (var parentComponentMask in parentComponentMasks)
            {
                if (componentMasks.Any(x => x.Name.Equals(parentComponentMask.Name))) continue;
                componentMasks.Add(parentComponentMask);
            }
        }

        var parameters = new CMaterialParams2();
        materialInstance.GetParams(parameters, EMaterialFormat.AllLayers);

        if (parameters.TryGetTexture2d(out var diffuseTexture, CMaterialParams2.Diffuse[0]))
        {
            textures.Add(new TextureParameter("Diffuse", diffuseTexture.GetPathName(), diffuseTexture.SRGB, diffuseTexture.CompressionSettings));
        }

        if (parameters.TryGetTexture2d(out var specularMasksTexture, CMaterialParams2.SpecularMasks[0]))
        {
            textures.Add(new TextureParameter("SpecularMasks", specularMasksTexture.GetPathName(), specularMasksTexture.SRGB, specularMasksTexture.CompressionSettings));
        }

        if (parameters.TryGetTexture2d(out var normalsTexture, CMaterialParams2.Normals[0]))
        {
            textures.Add(new TextureParameter("Normals", normalsTexture.GetPathName(), normalsTexture.SRGB, normalsTexture.CompressionSettings));
        }

        return (textures, scalars, vectors, switches, componentMasks);
    }

    public static (List<TextureParameter>, List<ScalarParameter>, List<VectorParameter>) MaterialParameters(UMaterialInterface materialInterface)
    {
        var parameters = new CMaterialParams2();
        materialInterface.GetParams(parameters, EMaterialFormat.AllLayers);

        var textures = new List<TextureParameter>();
        foreach (var (name, value) in parameters.Textures)
        {
            if (value is UTexture2D texture)
            {
                textures.Add(new TextureParameter(name, texture.GetPathName(), texture.SRGB, texture.CompressionSettings));
                break;
            }
        }
        
        if (materialInterface.TryLoadEditorData<UMaterialEditorOnlyData>(out var materialEditorData) && materialEditorData is not null)
        {
            bool TryAddExpressionTexture(string expressionName, string paramName)
            {
                if (!materialEditorData.TryGetValue(out FExpressionInput input, expressionName)) return false;
                if (!input.Expression.TryLoad(out var expression)) return false;
                if (!expression!.TryGetValue(out UTexture2D texture, "Texture")) return false;
                
                textures.AddUnique(new TextureParameter(paramName, texture.GetPathName(), texture.SRGB, texture.CompressionSettings));
                return true;
            }
            
            TryAddExpressionTexture("BaseColor", "Diffuse");
            TryAddExpressionTexture("Specular", "SpecularMasks");
            TryAddExpressionTexture("Metallic", "SpecularMasks");
            TryAddExpressionTexture("Roughness", "SpecularMasks");
            TryAddExpressionTexture("Normal", "Normals");
            TryAddExpressionTexture("EmissiveColor", "Emissive");
        }

        return (textures, new List<ScalarParameter>(), new List<VectorParameter>());
    }
    
    public static UMaterialInterface? GetLastParent(this UMaterialInstanceConstant obj)
    {
        var hasParent = true;
        var activeParent = obj.Parent;
        while (hasParent)
        {
            if (activeParent is UMaterialInstanceConstant materialInstance)
            {
                activeParent = materialInstance.Parent;
            }
            else
            {
                hasParent = false;
            }
        }

        return activeParent as UMaterialInterface;
    }
}
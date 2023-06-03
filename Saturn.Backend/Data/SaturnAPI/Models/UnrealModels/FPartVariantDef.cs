using System.Collections.Generic;

namespace Saturn.Backend.Data.SaturnAPI.Models.UnrealModels;

public class FPartVariantDef
{
    public Dictionary<string, CharacterPart> VariantParts { get; set; } = new();
    public List<FMaterialVariants> VariantMaterials { get; set; } = new();
    public List<FMaterialParamterDef> VariantMaterialParams { get; set; } = new();
    public List<FVariantParticleSystemInitializerData> InitalParticelSystemData { get; set; } = new();
    public List<FParticleVariant> VariantParticles { get; set; } = new();
    public List<FParticleParamterVariant> VariantParticleParams { get; set; } = new();
    public List<FSoundVariant> VariantSounds { get; set; } = new();
    //public List<FFoleySoundVariant> VariantFoley { get; set; }
    public List<FSocketTransformVariant> SocketTransforms { get; set; } = new();
    //public List<FScriptedActionVariant> VariantActions { get; set; }
    public List<FMeshVariant> VariantMeshes { get; set; } = new();
}
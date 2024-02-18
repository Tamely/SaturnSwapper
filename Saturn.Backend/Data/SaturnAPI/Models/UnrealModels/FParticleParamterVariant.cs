using System.Collections.Generic;

namespace Saturn.Backend.Data.SaturnAPI.Models.UnrealModels;

public class FParticleParamterVariant
{
    public string ParticleSystemToAlter { get; set; }
    public List<FMaterialVectorVariant> ColorParams { get; set; } = new();
    public List<FVectorParamVariant> VectorParams { get; set; } = new();
    public List<FMaterialFloatVariant> FloatParams { get; set; } = new();
}
using System.Collections.Generic;

namespace Saturn.Backend.Data.SaturnAPI.Models.UnrealModels;

public class FMaterialParamterDef
{
    public string MaterialToAlter { get; set; }
    public List<FMaterialVectorVariant> ColorParams { get; set; } = new();
    public List<FMaterialTextureVariant> TextureParams { get; set; } = new();
    public List<FMaterialFloatVariant> FloatParams { get; set; } = new();
}
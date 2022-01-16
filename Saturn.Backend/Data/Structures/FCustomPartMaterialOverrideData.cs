using CUE4Parse.UE4.Assets.Exports.Material;

namespace Saturn.Backend.Data.Structures;

public struct FCustomPartMaterialOverrideData
{
    public int MaterialOverrideIndex { get; set; }
    public UMaterialInterface OverrideMaterial { get; set; }
}
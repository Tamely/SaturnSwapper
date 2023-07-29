using System.Collections.Generic;
using System.Text.Json.Serialization;
using CUE4Parse.UE4.Assets.Exports.Sound;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.Core.Math;

namespace Saturn.Backend.Data.Swapper.Assets;

public class ExportMesh
{
    public string MeshPath;
    public int NumLods;
    public FVector Offset = FVector.ZeroVector;
    public FVector Scale = FVector.OneVector;
    public List<ExportMaterial> Materials = new();
    public List<ExportMaterial> OverrideMaterials = new();
}

public class ExportMeshOverride : ExportMesh
{
    public string MeshToSwap;
}

public class ExportPart : ExportMesh
{
    public string Part;
    public string? MorphName;
    public string? SocketName;

    [JsonIgnore] public EFortCustomGender GenderPermitted;
}

public record ExportMaterial
{
    public string MaterialPath;
    public string MaterialName;
    public string? MasterMaterialName;
    public int SlotIndex;
    public int Hash;
    public bool IsGlass;
    public List<TextureParameter> Textures = new();
    public List<ScalarParameter> Scalars = new();
    public List<VectorParameter> Vectors = new();
    public List<SwitchParameter> Switches = new();
    public List<ComponentMaskParameter> ComponentMasks = new();
}

public record ExportMaterialOverride : ExportMaterial
{
    public string? MaterialNameToSwap;
}

public record ExportMaterialParams
{
    public string MaterialToAlter;
    public int Hash;
    public List<TextureParameter> Textures = new();
    public List<ScalarParameter> Scalars = new();
    public List<VectorParameter> Vectors = new();
    public List<SwitchParameter> Switches = new();
    public List<ComponentMaskParameter> ComponentMasks = new();
}

public record TextureParameter(string Name, string Value, bool sRGB, TextureCompressionSettings CompressionSettings);

public record ScalarParameter(string Name, float Value);

public record VectorParameter(string Name, FLinearColor Value)
{
    public FLinearColor Value { get; set; } = Value;
}

public record SwitchParameter(string Name, bool Value);

public record ComponentMaskParameter(string Name, FLinearColor Value);

public record TransformPArameter(string Name, FTransform Value);

public class AnimationData
{
    public string Skeleton;
    public List<EmoteSection> Sections = new();
    public List<EmoteProp> Props = new();
    public List<ExportSound> Sounds = new();
}

public record EmoteSection(string Path, string Name, float Time, float Length, bool Loop = false)
{
    public string AdditivePath;
    public List<Curve> Curves = new();
}

public class Sound
{
    public USoundWave? SoundWave;
    public float Time;
    public bool Loop;

    public Sound(USoundWave? soundWave, float time, bool loop)
    {
        SoundWave = soundWave;
        Time = time;
        Loop = loop;
    }

    public bool IsValid()
    {
        return SoundWave is not null && Time >= 0;
    }
}

public record ExportSound(string Path, string AudioExtension, float Time, bool Loop);

public class EmoteProp
{
    public string SocketName;
    public FVector LocationOffset;
    public FRotator RotationOFfset;
    public FVector Scale;
    public ExportMesh? Prop;
    public string Animation;
}

public class Curve
{
    public string Name;
    public List<CurveKey> Keys;
}

public record CurveKey(float Time, float Value);
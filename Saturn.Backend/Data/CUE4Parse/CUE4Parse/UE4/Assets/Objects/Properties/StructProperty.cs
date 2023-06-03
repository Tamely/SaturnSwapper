using System;
using System.Collections.Generic;
using System.Linq;
using CUE4Parse.UE4.Assets.Objects.Unversioned;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Objects.GameplayTags;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.Utils;
using Newtonsoft.Json;
using Saturn.Backend.Data;

namespace CUE4Parse.UE4.Assets.Objects
{
    [JsonConverter(typeof(StructPropertyConverter))]
    public class StructProperty : FPropertyTagType<UScriptStruct>
    {
        public StructProperty(FAssetArchive Ar, FPropertyTagData? tagData, ReadType type)
        {
            Value = new UScriptStruct(Ar, tagData?.StructType, tagData?.Struct, type);
        }

        public override void Serialize(List<byte> Ar)
        {
            switch (Value.StructType)
            {
                case FStructFallback fallback:
                    foreach (var fragment in fallback.Header.Fragments)
                        Ar.AddRange(BitConverter.GetBytes(fragment.GetPacked()));
                    
                    FUnversionedHeader.FFragments.Remove(FUnversionedHeader.FFragments.Find(x => x[0].GetPacked() == fallback.Header.Fragments[0].GetPacked()));

                    if (fallback.Header.ZeroMask.Length > 0)
                    {
                        byte[] ret = new byte[(fallback.Header.ZeroMask.Length - 1) / 8 + 1];
                        fallback.Header.ZeroMask.CopyTo(ret, 0);
                        Ar.AddRange(ret);
                    
                        FUnversionedHeader.ZeroMasks.Remove(fallback.Header.ZeroMask);
                    }

                    foreach (var property in fallback.Properties)
                    {
                        if (property.Size == 0) continue;

                        property.Tag.Serialize(Ar);
                    }
                    break;
                case FLinearColor color:
                    Ar.AddRange(BitConverter.GetBytes(color.R));
                    Ar.AddRange(BitConverter.GetBytes(color.G));
                    Ar.AddRange(BitConverter.GetBytes(color.B));
                    Ar.AddRange(BitConverter.GetBytes(color.A));
                    break;
                case FColor color:
                    Ar.Add(color.R);
                    Ar.Add(color.G);
                    Ar.Add(color.B);
                    Ar.Add(color.A);
                    break;
                case FGameplayTagContainer container:
                    Ar.AddRange(BitConverter.GetBytes(container.GameplayTags.Length));
                    foreach (var tag in container.GameplayTags)
                    {
                        Ar.AddRange(BitConverter.GetBytes(tag.Index));
                        Ar.AddRange(BitConverter.GetBytes(tag.Number));
                    }
                    break;
                case FSoftObjectPath path:
                    new SoftObjectProperty(path.AssetPathName.Text, path.SubPathString, path.Owner).Serialize(Ar);
                    break;
                case FVector vector:
                    Ar.AddRange(BitConverter.GetBytes((double)vector.X));
                    Ar.AddRange(BitConverter.GetBytes((double)vector.Y));
                    Ar.AddRange(BitConverter.GetBytes((double)vector.Z));
                    break;
                case FQuat quat:
                    Ar.AddRange(BitConverter.GetBytes((double)quat.X));
                    Ar.AddRange(BitConverter.GetBytes((double)quat.Y));
                    Ar.AddRange(BitConverter.GetBytes((double)quat.Z));
                    Ar.AddRange(BitConverter.GetBytes((double)quat.W));
                    break;
                case FRotator rot:
                    Ar.AddRange(BitConverter.GetBytes((double)rot.Pitch));
                    Ar.AddRange(BitConverter.GetBytes((double)rot.Yaw));
                    Ar.AddRange(BitConverter.GetBytes((double)rot.Roll));
                    break;
                default:
                    throw new NotImplementedException("StructProperty: " + Value.StructType.GetType().Name + " is not implemented!");
            }
        }

        public override string ToString() => Value.ToString().SubstringBeforeLast(')') + ", StructProperty)";
    }

    public class StructPropertyConverter : JsonConverter<StructProperty>
    {
        public override void WriteJson(JsonWriter writer, StructProperty value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Value);
        }

        public override StructProperty ReadJson(JsonReader reader, Type objectType, StructProperty existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
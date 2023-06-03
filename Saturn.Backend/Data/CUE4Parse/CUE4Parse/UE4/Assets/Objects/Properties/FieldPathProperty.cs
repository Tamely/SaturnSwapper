using System;
using System.Collections.Generic;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.UObject;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Assets.Objects
{
    [JsonConverter(typeof(FieldPathPropertyConverter))]
    public class FieldPathProperty : FPropertyTagType<FFieldPath>
    {
        public FieldPathProperty(FAssetArchive Ar, ReadType type)
        {
            Value = type switch
            {
                ReadType.ZERO => new FFieldPath(),
                _ => new FFieldPath(Ar)
            };
        }
        
        public override void Serialize(List<byte> Ar)
        {
            Ar.AddRange(BitConverter.GetBytes(Value.Path.Count));
            foreach (var path in Value.Path)
            {
                Ar.AddRange(BitConverter.GetBytes(path.Index));
                Ar.AddRange(BitConverter.GetBytes(path.Number));
            }

            if (Value.ResolvedOwner != null && Value.ResolvedOwner.Index != 0)
                Ar.AddRange(BitConverter.GetBytes(Value.ResolvedOwner.Index));
        }
    }
    
    public class FieldPathPropertyConverter : JsonConverter<FieldPathProperty>
    {
        public override void WriteJson(JsonWriter writer, FieldPathProperty value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Value);
        }

        public override FieldPathProperty ReadJson(JsonReader reader, Type objectType, FieldPathProperty existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
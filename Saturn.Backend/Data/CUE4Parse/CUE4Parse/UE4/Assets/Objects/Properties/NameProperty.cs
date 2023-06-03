using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.UObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CUE4Parse.UE4.Assets.Objects
{
    [JsonConverter(typeof(NamePropertyConverter))]
    public class NameProperty : FPropertyTagType<FName>
    {
        public NameProperty(FAssetArchive Ar, ReadType type)
        {
            Value = type switch
            {
                ReadType.ZERO => new FName(),
                _ => Ar.ReadFName()
            };
        }

        public override void Serialize(List<byte> Ar)
        {
            Ar.AddRange(BitConverter.GetBytes(Value.Index));
            Ar.AddRange(BitConverter.GetBytes(Value.Number));
        }
    }

    public class NamePropertyConverter : JsonConverter<NameProperty>
    {
        public override void WriteJson(JsonWriter writer, NameProperty value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Value);
        }

        public override NameProperty ReadJson(JsonReader reader, Type objectType, NameProperty existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
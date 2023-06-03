using CUE4Parse.UE4.Readers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CUE4Parse.UE4.Assets.Objects
{
    [JsonConverter(typeof(StrPropertyConverter))]
    public class StrProperty : FPropertyTagType<string>
    {
        public StrProperty(FArchive Ar, ReadType type)
        {
            Value = type switch
            {
                ReadType.ZERO => string.Empty,
                _ => Ar.ReadFString()
            };
        }

        public override void Serialize(List<byte> Ar)
        {
            Ar.AddRange(BitConverter.GetBytes(Value.Length + 1));
            Ar.AddRange(Encoding.UTF8.GetBytes(Value + "\0"));
        }
    }

    public class StrPropertyConverter : JsonConverter<StrProperty>
    {
        public override void WriteJson(JsonWriter writer, StrProperty value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Value);
        }

        public override StrProperty ReadJson(JsonReader reader, Type objectType, StrProperty existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
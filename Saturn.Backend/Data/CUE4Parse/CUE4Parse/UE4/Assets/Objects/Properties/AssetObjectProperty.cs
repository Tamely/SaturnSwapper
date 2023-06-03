using System;
using System.Collections.Generic;
using System.Text;
using CUE4Parse.UE4.Readers;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Assets.Objects
{
    [JsonConverter(typeof(AssetObjectPropertyConverter))]
    public class AssetObjectProperty : FPropertyTagType<string>
    {
        public AssetObjectProperty(FArchive Ar, ReadType type)
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
    
    public class AssetObjectPropertyConverter : JsonConverter<AssetObjectProperty>
    {
        public override void WriteJson(JsonWriter writer, AssetObjectProperty value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Value);
        }

        public override AssetObjectProperty ReadJson(JsonReader reader, Type objectType, AssetObjectProperty existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
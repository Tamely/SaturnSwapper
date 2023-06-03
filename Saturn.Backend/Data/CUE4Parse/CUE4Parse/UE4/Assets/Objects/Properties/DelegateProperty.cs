using System;
using System.Collections.Generic;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.UObject;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Assets.Objects
{
    [JsonConverter(typeof(DelegatePropertyConverter))]
    public class DelegateProperty : FPropertyTagType<FName>
    {
        public readonly int Num;

        public DelegateProperty(FAssetArchive Ar, ReadType type)
        {
            if (type == ReadType.ZERO)
            {
                Num = 0;
                Value = new FName();
            }
            else
            {
                Num = Ar.Read<int>();
                Value = Ar.ReadFName();    
            }
        }
        
        public override void Serialize(List<byte> Ar)
        {
            Ar.AddRange(BitConverter.GetBytes(Num));
            Ar.AddRange(BitConverter.GetBytes(Value.Index));
            Ar.AddRange(BitConverter.GetBytes(Value.Number));
        }
    }
    
    public class DelegatePropertyConverter : JsonConverter<DelegateProperty>
    {
        public override void WriteJson(JsonWriter writer, DelegateProperty value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            
            writer.WritePropertyName("Num");
            writer.WriteValue(value.Num);
            
            writer.WritePropertyName("Name");
            serializer.Serialize(writer, value.Value);
            
            writer.WriteEndObject();
        }

        public override DelegateProperty ReadJson(JsonReader reader, Type objectType, DelegateProperty existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
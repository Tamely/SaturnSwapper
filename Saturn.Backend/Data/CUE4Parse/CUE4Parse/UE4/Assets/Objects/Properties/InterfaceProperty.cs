using System;
using System.Collections.Generic;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.UObject;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Assets.Objects
{
    [JsonConverter(typeof(InterfacePropertyConverter))]
    public class InterfaceProperty : FPropertyTagType<FScriptInterface>
    {
        public InterfaceProperty(FAssetArchive Ar, ReadType type)
        {
            Value = type switch
            {
                ReadType.ZERO => new FScriptInterface(),
                _ => new FScriptInterface(Ar)
            };
        }

        public override void Serialize(List<byte> Ar)
        {
            Ar.AddRange(BitConverter.GetBytes(Value.Object.Index));
        }
    }
    
    public class InterfacePropertyConverter : JsonConverter<InterfaceProperty>
    {
        public override void WriteJson(JsonWriter writer, InterfaceProperty value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Value);
        }

        public override InterfaceProperty ReadJson(JsonReader reader, Type objectType, InterfaceProperty existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
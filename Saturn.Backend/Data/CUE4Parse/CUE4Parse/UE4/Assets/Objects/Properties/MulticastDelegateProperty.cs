using System;
using System.Collections.Generic;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.UObject;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Assets.Objects
{
    [JsonConverter(typeof(MulticastDelegatePropertyConverter))]
    public class MulticastDelegateProperty : FPropertyTagType<FMulticastScriptDelegate>
    {
        public MulticastDelegateProperty(FAssetArchive Ar, ReadType type)
        {
            Value = type switch
            {
                ReadType.ZERO => new FMulticastScriptDelegate(Array.Empty<FScriptDelegate>()),
                _ => new FMulticastScriptDelegate(Ar)
            };
        }

        public override void Serialize(List<byte> Ar)
        {
            Ar.AddRange(BitConverter.GetBytes(Value.InvocationList.Length));
            foreach (var invocation in Value.InvocationList)
            {
                Ar.AddRange(BitConverter.GetBytes(invocation.Object.Index));
                Ar.AddRange(BitConverter.GetBytes(invocation.FunctionName.Index));
                Ar.AddRange(BitConverter.GetBytes(invocation.FunctionName.Number));
            }
        }
    }

    public class MulticastDelegatePropertyConverter : JsonConverter<MulticastDelegateProperty>
    {
        public override void WriteJson(JsonWriter writer, MulticastDelegateProperty value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Value);
        }

        public override MulticastDelegateProperty ReadJson(JsonReader reader, Type objectType, MulticastDelegateProperty existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class MulticastInlineDelegateProperty : MulticastDelegateProperty
    {
        public MulticastInlineDelegateProperty(FAssetArchive Ar, ReadType type) : base(Ar, type) { }
    }

    public class MulticastSparseDelegateProperty : MulticastDelegateProperty
    {
        public MulticastSparseDelegateProperty(FAssetArchive Ar, ReadType type) : base(Ar, type) { }
    }
}
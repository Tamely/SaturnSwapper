using System;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Exceptions;
using System.Collections.Generic;
using System.Text;
using CUE4Parse.UE4.Objects.Niagara;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.Utils;
using Newtonsoft.Json;
using Saturn.Backend.Data;

namespace CUE4Parse.UE4.Assets.Objects
{
    [JsonConverter(typeof(UScriptMapConverter))]
    public class UScriptMap
    {
        public Dictionary<FName, (FTopLevelAssetPath, string)> KVPs;
        public Dictionary<FPropertyTagType?, FPropertyTagType?> Properties;

        public UScriptMap()
        {
            Properties = new Dictionary<FPropertyTagType?, FPropertyTagType?>();
        }

        public UScriptMap(FAssetArchive Ar, FPropertyTagData tagData)
        {
            if (tagData.InnerType == null || tagData.ValueType == null)
                throw new ParserException(Ar, "Can't serialize UScriptMap without key or value type");

            if (!Ar.HasUnversionedProperties && Ar.Versions.MapStructTypes.TryGetValue(tagData.Name, out var mapStructTypes))
            {
                if (!string.IsNullOrEmpty(mapStructTypes.Key)) tagData.InnerTypeData = new FPropertyTagData(mapStructTypes.Key);
                if (!string.IsNullOrEmpty(mapStructTypes.Value)) tagData.ValueTypeData = new FPropertyTagData(mapStructTypes.Value);
            }

            var numKeysToRemove = Ar.Read<int>();
            for (var i = 0; i < numKeysToRemove; i++)
            {
                FPropertyTagType.ReadPropertyTagType(Ar, tagData.InnerType, tagData.InnerTypeData, ReadType.MAP);
            }

            var numEntries = Ar.Read<int>();
            var pos = Ar.Position;

            if (SaturnData.IsPickaxe)
            {
                KVPs = new Dictionary<FName, (FTopLevelAssetPath, string)>(numEntries);
                for (var i = 0; i < numEntries; i++)
                {
                    if (pos + 8 + 20 > Ar.Length)
                        break;
                
                    var key = Ar.ReadFName();
                    var value = (new FTopLevelAssetPath(Ar), Ar.ReadFString());
                    KVPs[key] = value;
                }
            }

            Ar.Position = pos;
            Properties = new Dictionary<FPropertyTagType?, FPropertyTagType?>(numEntries);
            for (var i = 0; i < numEntries; i++)
            {
                var isReadingValue = false;
                try
                {
                    var key = FPropertyTagType.ReadPropertyTagType(Ar, tagData.InnerType, tagData.InnerTypeData, ReadType.MAP);
                    isReadingValue = true;
                    var value = FPropertyTagType.ReadPropertyTagType(Ar, tagData.ValueType, tagData.ValueTypeData, ReadType.MAP);
                    Properties[key] = value;
                }
                catch (ParserException e)
                {
                    throw new ParserException(Ar, $"Failed to read {(isReadingValue ? "value" : "key")} for index {i} in map", e);
                }
            }
        }

        public void Serialize(List<byte> Ar)
        {
            Ar.AddRange(BitConverter.GetBytes(0)); // numKeysToRemove
            Ar.AddRange(BitConverter.GetBytes(Properties.Count));

            if (SaturnData.IsPickaxe)
            {
                foreach (var kvp in KVPs)
                {
                    Ar.AddRange(BitConverter.GetBytes(kvp.Key.Index));
                    Ar.AddRange(BitConverter.GetBytes(kvp.Key.Number));
                
                    Ar.AddRange(BitConverter.GetBytes(kvp.Value.Item1.PackageName.Index));
                    Ar.AddRange(BitConverter.GetBytes(kvp.Value.Item1.PackageName.Number));
                    Ar.AddRange(BitConverter.GetBytes(kvp.Value.Item1.AssetName.Index));
                    Ar.AddRange(BitConverter.GetBytes(kvp.Value.Item1.AssetName.Number));

                    if (kvp.Value.Item2.Length == 0)
                    {
                        Ar.AddRange(BitConverter.GetBytes(0));
                    }
                    else
                    {
                        Ar.AddRange(BitConverter.GetBytes(kvp.Value.Item2.Length + 1));
                        Ar.AddRange(Encoding.UTF8.GetBytes(kvp.Value.Item2));
                    }
                }
            }
            else
            {
                foreach (var kvp in Properties)
                {
                    if (kvp.Key is EnumProperty keyEnumProp)
                    {
                        Ar.AddRange(BitConverter.GetBytes(keyEnumProp.Value.Index));
                        Ar.AddRange(BitConverter.GetBytes(keyEnumProp.Value.Number));
                    }
                    else
                    {
                        kvp.Key?.Serialize(Ar);
                    }
                    
                    if (kvp.Value is EnumProperty valueEnumProp)
                    {
                        Ar.AddRange(BitConverter.GetBytes(valueEnumProp.Value.Index));
                        Ar.AddRange(BitConverter.GetBytes(valueEnumProp.Value.Number));
                    }
                    else
                    {
                        kvp.Value?.Serialize(Ar);
                    }
                }
            }
        }
    }

    public class UScriptMapConverter : JsonConverter<UScriptMap>
    {
        public override void WriteJson(JsonWriter writer, UScriptMap value, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            foreach (var kvp in value.Properties)
            {
                switch (kvp.Key)
                {
                    case StructProperty:
                        writer.WriteStartObject();
                        writer.WritePropertyName("Key");
                        serializer.Serialize(writer, kvp.Key);
                        writer.WritePropertyName("Value");
                        serializer.Serialize(writer, kvp.Value);
                        writer.WriteEndObject();
                        break;
                    default:
                        writer.WriteStartObject();
                        writer.WritePropertyName(kvp.Key?.ToString().SubstringBefore('(').Trim() ?? "no key name???");
                        serializer.Serialize(writer, kvp.Value);
                        writer.WriteEndObject();
                        break;
                }
            }

            writer.WriteEndArray();
        }

        public override UScriptMap ReadJson(JsonReader reader, Type objectType, UScriptMap existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}

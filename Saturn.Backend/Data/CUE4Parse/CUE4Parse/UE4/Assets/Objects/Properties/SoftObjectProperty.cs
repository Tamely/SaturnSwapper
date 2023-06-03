using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.UE4.Versions;
using CUE4Parse.Utils;
using Newtonsoft.Json;
using Saturn.Backend.Data;

namespace CUE4Parse.UE4.Assets.Objects
{
    [JsonConverter(typeof(SoftObjectPropertyConverter))]
    public class SoftObjectProperty : FPropertyTagType<FSoftObjectPath>
    {
        public SoftObjectProperty(FAssetArchive Ar, ReadType type)
        {
            if (type == ReadType.ZERO)
            {
                Value = new FSoftObjectPath();
            }
            else
            {
                //var pos = Ar.Position;
                Value = new FSoftObjectPath(Ar);
                //if (!Ar.HasUnversionedProperties && type == ReadType.MAP && Ar.Game != EGame.GAME_Valorant)
                //{
                //    // skip ahead, putting the total bytes read to 16
                //    Ar.Position += 16 - (Ar.Position - pos);
                //}
            }
        }

        public SoftObjectProperty(string path, string subPath, IPackage owner)
        {
            Value = new FSoftObjectPath(path, subPath, owner);
        }
        
        public static int IndexOf(List<FNameEntrySerialized> nameMap, string name)
        {
            for (int i = 0; i < nameMap.Count; i++)
            {
                if (nameMap[i].Name == name)
                    return i;
            }
            return -1;
        }

        public override void Serialize(List<byte> Ar)
        {
            string searchString = Value.AssetPathName.Text;
            int num = 0;
            
            if (((IoPackage)Value.Owner).PathOverrides.ContainsKey(searchString))
                searchString = ((IoPackage)Value.Owner).PathOverrides[searchString];

            if (int.TryParse(searchString.Split('.')[0].SubstringAfterLast('_'), out int parsedInt))
            {
                string assetName = searchString.SubstringAfterLast('/').Split('.')[0];
                assetName = assetName.Split('.')[0].Replace("_" + assetName.Split('.')[0].SubstringAfterLast('_'), "");
                string tempSearch = searchString.SubstringBeforeLast('/') + "/" + assetName + "." + assetName;

                if (IndexOf(Value.Owner.NameMap, tempSearch.Split('.')[0]) is not -1 
                    && IndexOf(Value.Owner.NameMap, assetName) is not -1)
                {
                    searchString = tempSearch;
                    num = parsedInt + 1;
                }
            }

            if (searchString.Contains('.'))
            {
                int packageIdx = IndexOf(Value.Owner.NameMap, searchString.Split('.')[0]);
                
                if (packageIdx == -1)
                    throw new Exception($"Couldn't find {searchString.Split('.')[0]} in NameMap");

                Ar.AddRange(BitConverter.GetBytes((uint)packageIdx));
                Ar.AddRange(BitConverter.GetBytes(num));
                
                int assetIdx = IndexOf(Value.Owner.NameMap, searchString.Split('.')[1]);
                
                if (assetIdx == -1)
                    throw new Exception($"Couldn't find {searchString.Split('.')[1]} in NameMap");

                Ar.AddRange(BitConverter.GetBytes((uint)assetIdx));
                Ar.AddRange(BitConverter.GetBytes(num));
            }
            else
            {
                Ar.AddRange(BitConverter.GetBytes((uint)IndexOf(Value.Owner.NameMap, searchString)));
                Ar.AddRange(BitConverter.GetBytes(num));
            }
            
            Ar.AddRange(BitConverter.GetBytes(Value.SubPathString.Length == 0 ? 0 : Value.SubPathString.Length + 1));
            if (Value.SubPathString.Length > 0)
            {
                Ar.AddRange(Encoding.UTF8.GetBytes(Value.SubPathString + "\0"));
            }
        }
    }

    public class SoftObjectPropertyConverter : JsonConverter<SoftObjectProperty>
    {
        public override void WriteJson(JsonWriter writer, SoftObjectProperty value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Value);
        }

        public override SoftObjectProperty ReadJson(JsonReader reader, Type objectType, SoftObjectProperty existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}

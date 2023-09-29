using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UAssetAPI.UnrealTypes;
using UAssetAPI.ExportTypes;

namespace UAssetAPI.PropertyTypes.Objects
{
    /// <summary>
    /// Describes an <see cref="FString"/>.
    /// </summary>
    public class StrPropertyData : PropertyData<FString>
    {
        public StrPropertyData(FName name) : base(name)
        {

        }

        public StrPropertyData()
        {

        }

        private static readonly FString CurrentPropertyType = new FString("StrProperty");
        public override FString PropertyType { get { return CurrentPropertyType; } }

        public override void Read(AssetBinaryReader reader, bool includeHeader, long leng1, long leng2 = 0)
        {
            if (includeHeader)
            {
                PropertyGuid = reader.ReadPropertyGuid();
            }

            Value = reader.ReadFString();
        }
        
        public override byte[] Serialize(UnrealPackage Asset)
        {
            List<byte> data = new();
            
            if (string.IsNullOrWhiteSpace(Value.Value))
            {
                data.AddRange(BitConverter.GetBytes(0));
            }
            else
            {
                string nullTerminatedStr = Value.Value + "\0";
                data.AddRange(BitConverter.GetBytes(Value.Encoding is UnicodeEncoding ? -nullTerminatedStr.Length : nullTerminatedStr.Length));
                byte[] actualStrData = Value.Encoding.GetBytes(nullTerminatedStr);
                data.AddRange(actualStrData);
            }

            return data.ToArray();
        }

        public override int Write(AssetBinaryWriter writer, bool includeHeader)
        {
            if (includeHeader)
            {
                writer.WritePropertyGuid(PropertyGuid);
            }

            int here = (int)writer.BaseStream.Position;
            writer.Write(Value);
            return (int)writer.BaseStream.Position - here;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override void FromString(string[] d, UAsset asset)
        {
            var encoding = Encoding.ASCII;
            if (d.Length >= 5) encoding = (d[4].Equals("utf-16") ? Encoding.Unicode : Encoding.ASCII);
            Value = FString.FromString(d[0], encoding);
        }
    }
}
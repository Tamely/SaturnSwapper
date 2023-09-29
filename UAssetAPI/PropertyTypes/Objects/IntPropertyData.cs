using System;
using System.Collections.Generic;
using UAssetAPI.UnrealTypes;

namespace UAssetAPI.PropertyTypes.Objects
{
    /// <summary>
    /// Describes a 32-bit signed integer variable (<see cref="int"/>).
    /// </summary>
    public class IntPropertyData : PropertyData<int>
    {
        public IntPropertyData(FName name) : base(name)
        {

        }

        public IntPropertyData()
        {

        }

        private static readonly FString CurrentPropertyType = new FString("IntProperty");
        public override FString PropertyType { get { return CurrentPropertyType; } }
        public override object DefaultValue { get { return (int)0; } }

        public override void Read(AssetBinaryReader reader, bool includeHeader, long leng1, long leng2 = 0)
        {
            if (includeHeader)
            {
                PropertyGuid = reader.ReadPropertyGuid();
            }

            Value = reader.ReadInt32();
        }
        
        public override byte[] Serialize(UnrealPackage Asset)
        {
            List<byte> data = new();
            
            data.AddRange(BitConverter.GetBytes(Value));

            return data.ToArray();
        }

        public override int Write(AssetBinaryWriter writer, bool includeHeader)
        {
            if (includeHeader)
            {
                writer.WritePropertyGuid(PropertyGuid);
            }

            writer.Write(Value);
            return sizeof(int);
        }

        public override string ToString()
        {
            return Convert.ToString(Value);
        }

        public override void FromString(string[] d, UAsset asset)
        {
            Value = 0;
            if (int.TryParse(d[0], out int res)) Value = res;
        }
    }
}

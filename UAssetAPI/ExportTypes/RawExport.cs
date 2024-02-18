using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UAssetAPI.UnrealTypes;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;

namespace UAssetAPI.ExportTypes
{
    /// <summary>
    /// An export that could not be properly parsed by UAssetAPI, and is instead represented as an array of bytes as a fallback.
    /// </summary>
    public class RawExport : Export
    {
        public byte[] Data;

        public RawExport(Export super)
        {
            Asset = super.Asset;
            Extras = super.Extras;
        }

        public RawExport(byte[] data, UAsset asset, byte[] extras) : base(asset, extras)
        {
            Data = data;
        }

        public RawExport()
        {

        }

        public override void Write(AssetBinaryWriter writer)
        {
            writer.Write(Data);
        }
        
        public static int IndexOfSequence(byte[] buffer, byte[] pattern)
        {
            int i = Array.IndexOf(buffer, pattern[0], 0);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual(pattern))
                    return i;
                i = Array.IndexOf(buffer, pattern[0], i + 1);
            }

            return -1;
        }

        public bool Swap(PropertyData search, PropertyData replace)
        {
            byte[] searchBytes = search.Serialize(Asset);
            byte[] replaceBytes = replace.Serialize(Asset);

            int offset = IndexOfSequence(Data, searchBytes);
            if (offset == -1)
            {
                return false;
            }

            List<byte> dataAsList = new List<byte>(Data);
            dataAsList.RemoveRange(offset, searchBytes.Length);
            dataAsList.InsertRange(offset, replaceBytes);

            Data = dataAsList.ToArray();

            int sizeToAdd = replaceBytes.Length - searchBytes.Length;
            SerialSize += sizeToAdd;

            bool bCanStart = false;
            foreach (var export in Asset.Exports)
            {
                if (export.Equals(this))
                {
                    bCanStart = true;
                    continue;
                }

                if (bCanStart)
                {
                    export.SerialOffset += sizeToAdd;
                }
            }

            return true;
        }
    }
}

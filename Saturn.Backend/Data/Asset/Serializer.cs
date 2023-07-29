using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CUE4Parse.Utils;

namespace Saturn.Backend.Data.Asset;

public class Serializer
{
    private readonly Deserializer _data;

    public Serializer(Deserializer asset)
    {
        _data = asset;
    }

    public byte[] Serialize()
    {
        List<byte> asset = new();
        
        int nameMapSize = _data.ReadNameMap.Sum(x => x.Name.Length);
        int numStringBytes = _data.ModifiedNameMap.Sum(x => x.Name.Length);

        numStringBytes += (_data.ModifiedNameMap.Count - _data.ReadNameMap.Count) * (8 + 2); // For the added entries: 8 bytes for the hash, 2 bytes for the header
        
        // Header
        asset.AddRange(BitConverter.GetBytes(_data.Summary.bHasVersioningInfo));
        asset.AddRange(BitConverter.GetBytes(_data.Summary.HeaderSize - (uint)nameMapSize + (uint)numStringBytes));
        asset.AddRange(BitConverter.GetBytes(_data.Summary.Name.NameIndex));
        asset.AddRange(BitConverter.GetBytes(_data.Summary.Name.ExtraIndex));
        asset.AddRange(BitConverter.GetBytes((uint)_data.Summary.PackageFlags));
        asset.AddRange(BitConverter.GetBytes(_data.Summary.CookedHeaderSize - (uint)nameMapSize + (uint)numStringBytes));
        asset.AddRange(BitConverter.GetBytes((uint)_data.Summary.ImportedPublicExportHashesOffset - (uint)nameMapSize + (uint)numStringBytes));
        asset.AddRange(BitConverter.GetBytes((uint)_data.Summary.ImportMapOffset - (uint)nameMapSize + (uint)numStringBytes));
        asset.AddRange(BitConverter.GetBytes((uint)_data.Summary.ExportMapOffset - (uint)nameMapSize + (uint)numStringBytes));
        asset.AddRange(BitConverter.GetBytes((uint)_data.Summary.ExportBundleEntriesOffset - (uint)nameMapSize + (uint)numStringBytes));
        if (_data.Summary.GraphDataOffset == 0)
        {
            asset.AddRange(BitConverter.GetBytes((uint)_data.Summary.DependencyBundleHeadersOffset - (uint)nameMapSize + (uint)numStringBytes));
            asset.AddRange(BitConverter.GetBytes((uint)_data.Summary.DependencyBundleEntriesOffset - (uint)nameMapSize + (uint)numStringBytes));
            asset.AddRange(BitConverter.GetBytes((uint)_data.Summary.ImportedPackageNamesOffset - (uint)nameMapSize + (uint)numStringBytes));
        }
        else
        {
            asset.AddRange(BitConverter.GetBytes((uint)_data.Summary.GraphDataOffset - (uint)nameMapSize + (uint)numStringBytes));
        }
        
        // Name map meta
        asset.AddRange(BitConverter.GetBytes(_data.ModifiedNameMap.Count));
        asset.AddRange(BitConverter.GetBytes(numStringBytes));

        // NameMap
        foreach (var name in _data.ModifiedNameMap)
            asset.AddRange(BitConverter.GetBytes(CityHash.CityHash64(Encoding.UTF8.GetBytes(name.Name.ToLower()))));

        foreach (var name in _data.ModifiedNameMap)
            asset.AddRange(new byte[] {0 , (byte)name.Name.Length});

        foreach (var name in _data.ModifiedNameMap)
            asset.AddRange(Encoding.UTF8.GetBytes(name.Name));
        
        // Bulk Data
        asset.AddRange(_data.BulkData.Length == 0 ? Array.Empty<byte>() : _data.BulkData);
        
        // Property data
        asset.AddRange(_data.Properties);
        
        // Import map
        asset.RemoveRange(_data.Summary.ImportMapOffset - nameMapSize + numStringBytes, _data.ImportMap.Length * 8);
        for (int i = 0; i < _data.ImportMap.Length; i++)
        {
            asset.InsertRange((_data.Summary.ImportMapOffset - nameMapSize + numStringBytes) + (i * 8), BitConverter.GetBytes(_data.ImportMap[i].TypeAndId));
        }
        
        // ExportMap
        asset.RemoveRange(_data.Summary.ExportMapOffset - nameMapSize + numStringBytes, _data.ExportMap.Length * 72);
        
        Dictionary<int, ulong> ExtraExportSize = new();
        ulong addedLength = 0;
        for (int i = 0; i < _data.ExportMap.Length; i++)
        {
            List<byte> export = new();
            
            export.AddRange(BitConverter.GetBytes(_data.ExportMap[i].CookedSerialOffset + addedLength));
            addedLength += (ExtraExportSize.ContainsKey(i + 1) ? ExtraExportSize[i + 1] : 0);
            export.AddRange(BitConverter.GetBytes(_data.ExportMap[i].CookedSerialSize + addedLength));
            export.AddRange(BitConverter.GetBytes(_data.ExportMap[i].ObjectName.NameIndex));
            export.AddRange(BitConverter.GetBytes(_data.ExportMap[i].ObjectName.ExtraIndex));
            export.AddRange(BitConverter.GetBytes(_data.ExportMap[i].OuterIndex.TypeAndId));
            export.AddRange(BitConverter.GetBytes(_data.ExportMap[i].ClassIndex.TypeAndId));
            export.AddRange(BitConverter.GetBytes(_data.ExportMap[i].SuperIndex.TypeAndId));
            export.AddRange(BitConverter.GetBytes(_data.ExportMap[i].TemplateIndex.TypeAndId));
            export.AddRange(BitConverter.GetBytes(_data.ExportMap[i].PublicExportHash));
            export.AddRange(BitConverter.GetBytes((uint)_data.ExportMap[i].ObjectFlags));
            export.AddRange(new byte[] { _data.ExportMap[i].FilterFlags, 0, 0, 0 });

            asset.InsertRange((_data.Summary.ExportMapOffset - nameMapSize + numStringBytes) + (i * 72), export);
        }

        return asset.ToArray();
    }
}
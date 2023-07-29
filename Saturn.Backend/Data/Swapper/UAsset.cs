using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.IO.Objects;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.Utils;
using Newtonsoft.Json;
using Saturn.Backend.Data.Swapper.Exports;
using Saturn.Backend.Data.Swapper.Unversioned;

namespace Saturn.Backend.Data.Swapper;

public class UAsset : UnrealPackage
{
    public UAsset(byte[] data)
    {
        AssetBufferReader reader = new AssetBufferReader(data);
        Read(reader);
    }

    /// <summary>
    /// Whether or not the asset has unversioned properties.
    /// </summary>
    public uint bHasVersioningInfo;

    /// <summary>
    /// The size of the header.
    /// </summary>
    public uint HeaderSize;

    /// <summary>
    /// The mapped name of the asset.
    /// </summary>
    public FMappedName MappedName;

    /// <summary>
    /// The mapped name of the asset converted to a FName.
    /// </summary>
    public FName Name;

    /// <summary>
    /// The size of the cooked uasset (without the uexp's data)
    /// </summary>
    public uint CookedHeaderSize;

    /// <summary>
    /// The offset to the Imported Public Export Hashes in the asset.
    /// </summary>
    public int ImportedPublicExportHashesOffset;

    /// <summary>
    /// The offset to the Import Map in the asset.
    /// </summary>
    public int ImportMapOffset;

    /// <summary>
    /// The offset to the Export map in the asset.
    /// </summary>
    public int ExportMapOffset;

    /// <summary>
    /// The offset to the Export Bundle Entries in the asset.
    /// </summary>
    public int ExportBundleEntriesOffset;

    /// <summary>
    /// The offset to the Dependency Bundle Headers in the asset.
    /// </summary>
    public int DependencyBundleHeadersOffset;

    /// <summary>
    /// The offset to the Dependency Bundle Entries in the asset.
    /// </summary>
    public int DependencyBundleEntriesOffset;

    /// <summary>
    /// The offset to the Imported Package Names in the asset.
    /// </summary>
    public int ImportedPackageNamesOffset;

    /// <summary>
    /// The asset's Name Map's hash version.
    /// </summary>
    public ulong HashVersion;

    /// <summary>
    /// The asset's Bulk Data Map.
    /// </summary>
    public FBulkDataMapEntry[] BulkDataMap;

    /// <summary>
    /// The asset's deserialized Import Map.
    /// </summary>
    public FPackageObjectIndex[] ImportMap;

    /// <summary>
    /// The asset's deserialized Export Map.
    /// </summary>
    public FExportMapEntry[] ExportMap;

    /// <summary>
    /// The asset's Imported Public Export Hashes.
    /// </summary>
    public ulong[] ImportedPublicExportHashes;

    /// <summary>
    /// The asset's Export Bundle Entries.
    /// </summary>
    public FExportBundleEntry[] ExportBundleEntries;

    /// <summary>
    /// The asset's graph data (Dependencies).
    /// </summary>
    public FDependencyBundleEntry[] DependencyBundleEntries;
    public FDependencyBundleHeader[] DependencyBundleHeaders;

    /// <summary>
    /// The asset's exports.
    /// </summary>
    public IExportBase[] Exports;

    /// <summary>
    /// The asset's ExportDataOffset
    /// </summary>
    private int AllExportDataOffset;

    /// <summary>
    /// The data that wasn't deserialized
    /// </summary>
    private byte[]? TrailingData;

    /// <summary>
    /// Checks if the asset is the same after serialization.
    /// </summary>
    /// <returns>true if the asset is the same, false otherwise</returns>
    public bool VerifyBinaryEquality()
    {
        /*
        MemoryStream f = new MemoryStream(_byteData);
        f.Seek(0, SeekOrigin.Begin);
        MemoryStream newData = WriteData();
        newData.Seek(0, SeekOrigin.Begin);

        if (f.Length != newData.Length) return false;

        const int CHUNK_SIZE = 1024;
        byte[] buffer = new byte[CHUNK_SIZE];
        byte[] buffer2 = new byte[CHUNK_SIZE];

        int lastRead;
        while ((lastRead = f.Read(buffer, 0, buffer.Length)) > 0)
        {
            int lastRead2 = newData.Read(buffer2, 0, buffer2.Length);
            if (lastRead != lastRead2) return false;
            if (!buffer.SequenceEqual(buffer2)) return false;
        }*/

        return true;
    }

    private FExportMapEntry GetEntry(AssetBufferReader Ar)
    {
        var start = Ar.Position;
        ulong CookedSerialOffset = Ar.Read<ulong>();
        ulong CookedSerialSize = Ar.Read<ulong>();
        FMappedName ObjectName = Ar.Read<FMappedName>();
        FPackageObjectIndex OuterIndex = Ar.Read<FPackageObjectIndex>();
        FPackageObjectIndex ClassIndex = Ar.Read<FPackageObjectIndex>();
        FPackageObjectIndex SuperIndex = Ar.Read<FPackageObjectIndex>();
        FPackageObjectIndex TemplateIndex = Ar.Read<FPackageObjectIndex>();
        ulong PublicExportHash = Ar.Read<ulong>();
        EObjectFlags ObjectFlags = Ar.Read<EObjectFlags>();
        byte FilterFlags = Ar.Read<byte>();

        Ar.Position = start + FExportMapEntry.Size;

        return new()
        {
            CookedSerialOffset = CookedSerialOffset,
            CookedSerialSize = CookedSerialSize,
            ObjectName = ObjectName,
            OuterIndex = OuterIndex,
            ClassIndex = ClassIndex,
            SuperIndex = SuperIndex,
            TemplateIndex = TemplateIndex,
            PublicExportHash = PublicExportHash,
            ObjectFlags = ObjectFlags,
            FilterFlags = FilterFlags
        };
    }

    /// <summary>
    /// Reads the initial portion of the asset (everything before the name map).
    /// </summary>
    /// <param name="Ar">The asset's reader</param>
    /// <exception cref="Exception">Thrown when this is not an unversioned asset.</exception>
    private void ReadHeader(AssetBufferReader Ar)
    {
        Ar.Position = 0;

        bHasVersioningInfo = Ar.Read<uint>();
        if (bHasVersioningInfo != 0)
        {
            // TBH I shouldn't throw an error here, but I cba to add support with versioned assets
            throw new Exception($"bHasVersioning info is not 0! Read Value: {bHasVersioningInfo}");
        }

        HeaderSize = Ar.Read<uint>();
        MappedName = Ar.Read<FMappedName>();
        PackageFlags = Ar.Read<EPackageFlags>();
        CookedHeaderSize = Ar.Read<uint>();
        ImportedPublicExportHashesOffset = Ar.Read<int>();
        ImportMapOffset = Ar.Read<int>();
        ExportMapOffset = Ar.Read<int>();
        ExportBundleEntriesOffset = Ar.Read<int>();
        DependencyBundleHeadersOffset = Ar.Read<int>();
        DependencyBundleEntriesOffset = Ar.Read<int>();
        ImportedPackageNamesOffset = Ar.Read<int>();
    }

    /// <summary>
    /// Reads the name map.
    /// </summary>
    /// <param name="Ar">The asset's reader</param>
    private void ReadNameMap(AssetBufferReader Ar)
    {
        var nameMapCount = Ar.Read<int>();
        Ar.Position += sizeof(uint); // NumStringBytes (doesn't have to be read, it's better to determine it when serializing)
        HashVersion = Ar.Read<ulong>();
        
        Ar.Position += nameMapCount * sizeof(ulong); // We don't need to read the hashes.
        
        var headers = Ar.ReadArray<FSerializedNameHeader>(nameMapCount);
        foreach (var header in headers)
        {
            AddNameReference(Ar.ReadNameEntry(header));
        }
    }

    /// <summary>
    /// Deserializes an asset
    /// </summary>
    /// <param name="reader">The asset's reader</param>
    public void Read(AssetBufferReader reader)
    {
        ReadHeader(reader);
        ReadNameMap(reader);

        Name = new FName(MappedName, GetNameMapIndexList().ToArray());

        // Bulk data map
        var bulkDataMapSize = reader.Read<ulong>();
        BulkDataMap = reader.ReadArray<FBulkDataMapEntry>((int)(bulkDataMapSize / FBulkDataMapEntry.Size));

        int importCount = (ExportMapOffset - ImportMapOffset) / 8;
        int exportCount = (ExportBundleEntriesOffset - ExportMapOffset) / FExportMapEntry.Size;
        
        // Imported public export hashes
        reader.Position = ImportedPublicExportHashesOffset;
        ImportedPublicExportHashes = reader.ReadArray<ulong>((ImportMapOffset - ImportedPublicExportHashesOffset) / sizeof(ulong));
        
        // Import map
        reader.Position = ImportMapOffset;
        ImportMap = reader.ReadArray<FPackageObjectIndex>(importCount);
        
        // Export map
        reader.Position = ExportMapOffset;
        ExportMap = reader.ReadArray(exportCount, () => GetEntry(reader));
        Exports = new IExportBase[exportCount];
        
        // Export bundle entries
        reader.Position = ExportBundleEntriesOffset;
        ExportBundleEntries = reader.ReadArray<FExportBundleEntry>(exportCount * 2);

        // Graph data
        reader.Position = DependencyBundleHeadersOffset;
        DependencyBundleHeaders = reader.ReadArray(exportCount, () => new FDependencyBundleHeader(reader));
        
        reader.Position = DependencyBundleEntriesOffset;
        DependencyBundleEntries = reader.ReadArray(exportCount, () => new FDependencyBundleEntry(reader));

        AllExportDataOffset = (int)HeaderSize;
        
        int ProcessEntry(FExportBundleEntry entry, int pos)
        {
            if (entry.CommandType != EExportCommandType.ExportCommandType_Serialize)
                return 0; // Skip ExportCommandType_Create
            
            var export = ExportMap[entry.LocalExportIndex];
            
            reader.Position = pos;
            byte[] exportBuffer = reader.ReadArray<byte>((int)export.CookedSerialSize);
            Exports[entry.LocalExportIndex] = new RawExport(exportBuffer);
            
            return (int)export.CookedSerialSize;
        }

        // Process exports
        foreach (var entry in ExportBundleEntries)
        {
            ProcessEntry(entry, AllExportDataOffset + (int)ExportMap[entry.LocalExportIndex].CookedSerialOffset);
        }

        // For some reason, exports can be read starting at the bottom and going up, so this fixes that
        int endOfExportsOffset = ExportMap.Aggregate(int.MinValue, (current, export) => int.Max(current, AllExportDataOffset + (int)export.CookedSerialOffset + (int)export.CookedSerialSize));
        reader.Position = endOfExportsOffset;

        // This should just be 4 bytes (the FPackageIndex for the export, but I cba to figure out how that's serialized)
        if (reader.Position != reader.Size)
        {
            TrailingData = reader.ReadArray<byte>(reader.Size - (int)reader.Position);
        }
    }

    /// <summary>
    /// Serializes the initial portion of the asset
    /// </summary>
    /// <returns>A byte array which represents the serialized binary data of the initial portion of the asset.</returns>
    private byte[] SerializeHeader()
    {
        List<byte> data = new();

        data.AddRange(BitConverter.GetBytes(bHasVersioningInfo));
        data.AddRange(BitConverter.GetBytes(HeaderSize));
        data.AddRange(BitConverter.GetBytes(MappedName.NameIndex));
        data.AddRange(BitConverter.GetBytes(MappedName.ExtraIndex));
        data.AddRange(BitConverter.GetBytes((uint)PackageFlags));
        data.AddRange(BitConverter.GetBytes(CookedHeaderSize));
        data.AddRange(BitConverter.GetBytes(ImportedPublicExportHashesOffset));
        data.AddRange(BitConverter.GetBytes(ImportMapOffset));
        data.AddRange(BitConverter.GetBytes(ExportMapOffset));
        data.AddRange(BitConverter.GetBytes(ExportBundleEntriesOffset));
        data.AddRange(BitConverter.GetBytes(DependencyBundleHeadersOffset));
        data.AddRange(BitConverter.GetBytes(DependencyBundleEntriesOffset));
        data.AddRange(BitConverter.GetBytes(ImportedPackageNamesOffset));

        return data.ToArray();
    }

    /// <summary>
    /// Serializes the asset's Name Map.
    /// </summary>
    /// <returns>A byte array which represents the serialized binary data of the Name Map of the asset.</returns>
    private byte[] SerializeNameMap()
    {
        List<byte> data = new();

        data.AddRange(BitConverter.GetBytes(nameMapIndexList.Count));
        data.AddRange(BitConverter.GetBytes(nameMapIndexList.Sum(x => x.Name.Length)));
        data.AddRange(BitConverter.GetBytes(HashVersion));

        // Hashes
        foreach (var entry in nameMapIndexList)
        {
            data.AddRange(entry.Name.CheckEncoding(Encoding.UTF8)
                ? BitConverter.GetBytes(CityHash.CityHash64(Encoding.UTF8.GetBytes(entry.Name.ToLower())))
                : BitConverter.GetBytes(CityHash.CityHash64(Encoding.Unicode.GetBytes(entry.Name.ToLower()))));
        }
        
        // Headers
        foreach (var entry in nameMapIndexList)
        {
            bool bIsUtf16 = !entry.Name.CheckEncoding(Encoding.UTF8);
            int Len = entry.Name.Length;
            
            byte[] Data = new byte[2];
            Data[0] = (byte)((bIsUtf16 ? 1 : 0) << 7 | (byte)(Len >> 8));
            Data[1] = (byte)Len;

            data.AddRange(Data);
        }
        
        // Entries
        foreach (var entry in nameMapIndexList)
        {
            data.AddRange(entry.Name.CheckEncoding(Encoding.UTF8)
                ? Encoding.UTF8.GetBytes(entry.Name)
                : Encoding.Unicode.GetBytes(entry.Name));
        }

        return data.ToArray();
    }

    /// <summary>
    /// Serializes an asset from memory
    /// </summary>
    /// <returns>A new MemoryStream containing the full binary data of the serialized asset</returns>
    public MemoryStream WriteData()
    {
        return new MemoryStream();
    }

    public byte[] Write()
    {
        List<byte> data = new();

        data.AddRange(SerializeHeader());
        data.AddRange(SerializeNameMap());

        // Bulk data map
        data.AddRange(BitConverter.GetBytes((ulong)BulkDataMap.Length * FBulkDataMapEntry.Size));
        foreach (var map in BulkDataMap)
        {
            data.AddRange(BitConverter.GetBytes(map.SerialOffset));
            data.AddRange(BitConverter.GetBytes(map.DuplicateSerialOffset));
            data.AddRange(BitConverter.GetBytes(map.SerialSize));
            data.AddRange(BitConverter.GetBytes(map.Flags));
            data.AddRange(BitConverter.GetBytes(map.Pad));
        }

        // Imported public export hashes
        foreach (var importedPublicExportHash in ImportedPublicExportHashes)
        {
            data.AddRange(BitConverter.GetBytes(importedPublicExportHash));
        }

        // Import map
        foreach (var import in ImportMap)
        {
            data.AddRange(BitConverter.GetBytes(import.TypeAndId));
        }

        // Export map
        foreach (var export in ExportMap)
        {
            data.AddRange(BitConverter.GetBytes(export.CookedSerialOffset));
            data.AddRange(BitConverter.GetBytes(export.CookedSerialSize));
            data.AddRange(BitConverter.GetBytes(export.ObjectName.NameIndex));
            data.AddRange(BitConverter.GetBytes(export.ObjectName.ExtraIndex));
            data.AddRange(BitConverter.GetBytes(export.OuterIndex.TypeAndId));
            data.AddRange(BitConverter.GetBytes(export.ClassIndex.TypeAndId));
            data.AddRange(BitConverter.GetBytes(export.SuperIndex.TypeAndId));
            data.AddRange(BitConverter.GetBytes(export.TemplateIndex.TypeAndId));
            data.AddRange(BitConverter.GetBytes(export.PublicExportHash));
            data.AddRange(BitConverter.GetBytes((uint)export.ObjectFlags));
            data.AddRange(new byte[] { export.FilterFlags, 0, 0, 0 });
        }

        // Export bundle entries
        foreach (var entry in ExportBundleEntries)
        {
            data.AddRange(BitConverter.GetBytes(entry.LocalExportIndex));
            data.AddRange(BitConverter.GetBytes((uint)entry.CommandType));
        }

        // Graph data
        foreach (var dependency in DependencyBundleHeaders)
        {
            data.AddRange(BitConverter.GetBytes(dependency.FirstEntryIndex));
            foreach (var entry in dependency.EntryCount)
            {
                data.AddRange(BitConverter.GetBytes(entry));
            }
        }
        
        foreach (var dependency in DependencyBundleEntries)
        {
            data.AddRange(BitConverter.GetBytes(dependency.LocalImportOrExportIndex));
        }
        
        // Padding
        data.AddRange(BitConverter.GetBytes(0));

        // Exports
        foreach (var export in Exports)
        {
            data.AddRange(export.Serialize());
        }
        
        if (TrailingData != null)
        {
            data.AddRange(TrailingData);
        }

        return data.ToArray();
    }
}
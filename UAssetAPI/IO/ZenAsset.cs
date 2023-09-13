using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;

namespace UAssetAPI.IO
{
    public struct FInternalArc
    {
        public int FromExportBundleIndex;
        public int ToExportBundleIndex;

        public static FInternalArc Read(AssetBinaryReader reader)
        {
            var res = new FInternalArc();
            res.FromExportBundleIndex = reader.ReadInt32();
            res.ToExportBundleIndex = reader.ReadInt32();
            return res;
        }

        public static int Write(AssetBinaryWriter writer, int v2, int v3)
        {
            writer.Write(v2);
            writer.Write(v3);
            return sizeof(int) * 2;
        }

        public int Write(AssetBinaryWriter writer)
        {
            return FInternalArc.Write(writer, FromExportBundleIndex, ToExportBundleIndex);
        }
    }

    public struct FExternalArc
    {
        public int FromImportIndex;
        EExportCommandType FromCommandType;
        public int ToExportBundleIndex;

        public static FExternalArc Read(AssetBinaryReader reader)
        {
            var res = new FExternalArc();
            res.FromImportIndex = reader.ReadInt32();
            res.FromCommandType = (EExportCommandType)reader.ReadByte();
            res.ToExportBundleIndex = reader.ReadInt32();
            return res;
        }

        public static int Write(AssetBinaryWriter writer, int v1, EExportCommandType v2, int v3)
        {
            writer.Write(v1);
            writer.Write((byte)v2);
            writer.Write(v3);
            return sizeof(int) + sizeof(uint) + sizeof(int);
        }

        public int Write(AssetBinaryWriter writer)
        {
            return FExternalArc.Write(writer, FromImportIndex, FromCommandType, ToExportBundleIndex);
        }
    }

    public enum EExportCommandType : uint
    {
        ExportCommandType_Create,
        ExportCommandType_Serialize,
        ExportCommandType_Count
    }

    public struct FExportBundleHeader
    {
        public ulong SerialOffset;
        public uint FirstEntryIndex;
        public uint EntryCount;

        public static FExportBundleHeader Read(AssetBinaryReader reader)
        {
            var res = new FExportBundleHeader();
            res.SerialOffset = reader.Asset.ObjectVersionUE5 > ObjectVersionUE5.UNKNOWN ? reader.ReadUInt64() : ulong.MaxValue;
            res.FirstEntryIndex = reader.ReadUInt32();
            res.EntryCount = reader.ReadUInt32();
            return res;
        }

        public static int Write(AssetBinaryWriter writer, ulong v1, uint v2, uint v3)
        {
            if (writer.Asset.ObjectVersionUE5 > ObjectVersionUE5.UNKNOWN) writer.Write(v1);
            writer.Write(v2);
            writer.Write(v3);
            return (writer.Asset.ObjectVersionUE5 > ObjectVersionUE5.UNKNOWN ? sizeof(ulong) : 0) + sizeof(uint) * 2;
        }

        public int Write(AssetBinaryWriter writer)
        {
            return FExportBundleHeader.Write(writer, SerialOffset, FirstEntryIndex, EntryCount);
        }
    }    

    public struct FExportBundleEntry
    {
        public uint LocalExportIndex;
        public EExportCommandType CommandType;

        public static FExportBundleEntry Read(AssetBinaryReader reader)
        {
            var res = new FExportBundleEntry();
            res.LocalExportIndex = reader.ReadUInt32();
            res.CommandType = (EExportCommandType)reader.ReadUInt32();
            return res;
        }

        public static int Write(AssetBinaryWriter writer, uint lei, EExportCommandType typ)
        {
            writer.Write((uint)lei);
            writer.Write((uint)typ);
            return sizeof(uint) * 2;
        }

        public int Write(AssetBinaryWriter writer)
        {
            return FExportBundleEntry.Write(writer, LocalExportIndex, CommandType);
        }
    }

    public enum EZenPackageVersion : uint
    {
        Initial,

        LatestPlusOne,
        Latest = LatestPlusOne - 1
    }

    public class FSerializedNameHeader
    {
        public bool bIsWide;
        public int Len;

        public static FSerializedNameHeader Read(BinaryReader reader)
        {
            var b1 = reader.ReadByte();
            var b2 = reader.ReadByte();

            var res = new FSerializedNameHeader();
            res.bIsWide = (b1 & (byte)0x80) > 0;
            res.Len = ((b1 & (byte)0x7F) << 8) + b2;
            return res;
        }

        public static void Write(BinaryWriter writer, bool bIsWideVal, int lenVal)
        {
            byte b1 = (byte)(((byte)(bIsWideVal ? 1 : 0)) << 7 | (byte)(lenVal >> 8));
            byte b2 = (byte)lenVal;
            writer.Write(b1); writer.Write(b2);
        }

        public void Write(BinaryWriter writer)
        {
            FSerializedNameHeader.Write(writer, bIsWide, Len);
        }
    }

    public class ZenAsset : UnrealPackage
    {
        /// <summary>
        /// The global data of the game that this asset is from.
        /// </summary>
        public IOGlobalData GlobalData;

        public EZenPackageVersion ZenVersion;
        public FName Name;
        public FName SourceName;
        
        /// <summary>
        /// Should serialized hashes be verified on read?
        /// </summary>
        public bool VerifyHashes = false;

        /// <summary>
        /// If the asset is IoStore or not.
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
        /// The asset's Bulk Data Map.
        /// </summary>
        public FBulkDataMapEntry[] BulkDataMap;

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
        /// The asset's ExportDataOffset
        /// </summary>
        private int AllExportDataOffset;

        /// <summary>
        /// The data that wasn't deserialized
        /// </summary>
        private byte[]? TrailingData;

        /// <summary>
        /// The version used to hash.
        /// </summary>
        private ulong HashVersion;

        /// <summary>
        /// The imported package names as a byte[] (TODO: Fix this to FName[]!)
        /// </summary>
        public byte[] ImportedPackageNames;

        /// <summary>
        /// Map of object imports. UAssetAPI used to call these "links."
        /// </summary>
        public List<FPackageObjectIndex> Imports;

        private Dictionary<ulong, string> CityHash64Map = new Dictionary<ulong, string>();
        private void AddCityHash64MapEntryRaw(string val)
        {
            ulong hsh = CRCGenerator.GenerateImportHashFromObjectPath(val);
            if (CityHash64Map.ContainsKey(hsh))
            {
                if (CRCGenerator.ToLower(CityHash64Map[hsh]) == CRCGenerator.ToLower(val)) return;
                throw new FormatException("CityHash64 hash collision between \"" + CityHash64Map[hsh] + "\" and \"" + val + "\"");
            }
            CityHash64Map.Add(hsh, val);
        }
        public string GetStringFromCityHash64(ulong val)
        {
            if (CityHash64Map.ContainsKey(val)) return CityHash64Map[val];
            if (Mappings.CityHash64Map.ContainsKey(val)) return Mappings.CityHash64Map[val];
            return null;
        }

        public bool VerifyBinaryEquality(byte[] uassetData)
        {
            return uassetData.SequenceEqual(WriteData().ToArray());
        }

        /// <summary>
        /// Finds the class path and export name of the SuperStruct of this asset, if it exists.
        /// </summary>
        /// <param name="parentClassPath">The class path of the SuperStruct of this asset, if it exists.</param>
        /// <param name="parentClassExportName">The export name of the SuperStruct of this asset, if it exists.</param>
        public override void GetParentClass(out FName parentClassPath, out FName parentClassExportName)
        {
            throw new NotImplementedException("Unimplemented method ZenAsset.GetParentClass");
        }

        internal override FName GetParentClassExportName()
        {
            throw new NotImplementedException("Unimplemented method ZenAsset.GetParentClassExportName");
        }
        
        public override int AddNameReference(FString name, bool forceAddDuplicates = false)
        {
            if (isReadingTime) return base.AddNameReference(name, forceAddDuplicates);
            
            HeaderSize += (uint)name.Value.Length;
            CookedHeaderSize += (uint)name.Value.Length;
            ImportedPublicExportHashesOffset += name.Value.Length;
            ImportMapOffset += name.Value.Length;
            ExportMapOffset += name.Value.Length;
            ExportBundleEntriesOffset += name.Value.Length;
            DependencyBundleHeadersOffset += name.Value.Length;
            DependencyBundleEntriesOffset += name.Value.Length;
            ImportedPackageNamesOffset += name.Value.Length;

            return base.AddNameReference(name, forceAddDuplicates);

        }

        /// <summary>
        /// Reads the initial portion of the asset (everything before the name map).
        /// </summary>
        /// <param name="Ar">The asset's reader</param>
        /// <exception cref="Exception">Thrown when this is not an unversioned asset.</exception>
        private void ReadHeader(AssetBinaryReader Ar)
        {
            Ar.BaseStream.Position = 0;

            bHasVersioningInfo = Ar.ReadUInt32();
            if (bHasVersioningInfo != 0)
            {
                // TBH I shouldn't throw an error here, but I cba to add support with versioned assets
                throw new Exception($"bHasVersioning info is not 0! Read Value: {bHasVersioningInfo}");
            }

            HeaderSize = Ar.ReadUInt32();
            MappedName = new FMappedName(Ar);
            PackageFlags = (EPackageFlags)Ar.ReadUInt32();
            CookedHeaderSize = Ar.ReadUInt32();
            ImportedPublicExportHashesOffset = Ar.ReadInt32();
            ImportMapOffset = Ar.ReadInt32();
            ExportMapOffset = Ar.ReadInt32();
            ExportBundleEntriesOffset = Ar.ReadInt32();
            DependencyBundleHeadersOffset = Ar.ReadInt32();
            DependencyBundleEntriesOffset = Ar.ReadInt32();
            ImportedPackageNamesOffset = Ar.ReadInt32();
        }
        
        /// <summary>
        /// Reads the name map.
        /// </summary>
        /// <param name="Ar">The asset's reader</param>
        private void ReadNameMap(AssetBinaryReader Ar)
        {
            Ar.ReadNameBatch(VerifyHashes, out HashVersion, out List<FString> tempNameMap);
            ClearNameIndexList();

            foreach (var entry in tempNameMap)
            {
                AddNameReference(entry, true);
            }
        }

        /// <summary>
        /// Reads an asset into memory.
        /// </summary>
        /// <param name="reader">The input reader.</param>
        /// <param name="manualSkips">An array of export indexes to skip parsing. For most applications, this should be left blank.</param>
        /// <param name="forceReads">An array of export indexes that must be read, overriding entries in the manualSkips parameter. For most applications, this should be left blank.</param>
        /// <exception cref="UnknownEngineVersionException">Thrown when <see cref="ObjectVersion"/> is unspecified.</exception>
        /// <exception cref="FormatException">Throw when the asset cannot be parsed correctly.</exception>
        public override void Read(AssetBinaryReader reader, int[] manualSkips = null, int[] forceReads = null)
        {
            if (Mappings == null) throw new InvalidOperationException();
            reader.Asset = this;
            isReadingTime = true;
            
            ReadHeader(reader);
            ReadNameMap(reader);
            
            Name = new FName(this, MappedName);

            // Bulk data map
            var bulkDataMapSize = reader.ReadUInt64();
            BulkDataMap = new FBulkDataMapEntry[bulkDataMapSize / FBulkDataMapEntry.Size];
            for (int i = 0; i < BulkDataMap.Length; i++)
            {
                BulkDataMap[i].SerialOffset = reader.ReadUInt64();
                BulkDataMap[i].DuplicateSerialOffset = reader.ReadUInt64();
                BulkDataMap[i].SerialSize = reader.ReadUInt64();
                BulkDataMap[i].Flags = reader.ReadUInt32();
                BulkDataMap[i].Pad = reader.ReadUInt32();
            }

            int importCount = (ExportMapOffset - ImportMapOffset) / 8;
            int exportCount = (ExportBundleEntriesOffset - ExportMapOffset) / FExportMapEntry.Size;
        
            // Imported public export hashes
            reader.BaseStream.Position = ImportedPublicExportHashesOffset;
            ImportedPublicExportHashes = new ulong[(ImportMapOffset - ImportedPublicExportHashesOffset) / sizeof(ulong)];
            for (int i = 0; i < ImportedPublicExportHashes.Length; i++)
            {
                ImportedPublicExportHashes[i] = reader.ReadUInt64();
            }

            // Import map
            reader.BaseStream.Position = ImportMapOffset;
            Imports = new List<FPackageObjectIndex>(importCount);
            for (int i = 0; i < importCount; i++)
            {
                var import = new FPackageObjectIndex
                {
                    Hash = reader.ReadUInt64()
                };
                Imports.Add(import);
            }

            // Export map
            reader.BaseStream.Position = ExportMapOffset;
            Exports = new List<Export>();
            for (var i = 0; i < exportCount; i++)
            {
                var newExport = new Export(this, Array.Empty<byte>());
                newExport.ReadExportMapEntry(reader);
                Exports.Add(newExport);
            }

            // Export bundle entries
            reader.BaseStream.Position = ExportBundleEntriesOffset;
            ExportBundleEntries = reader.ReadArray<FExportBundleEntry>(exportCount * 2);

            // Graph data
            int graphDataHeaderCount = (DependencyBundleEntriesOffset - DependencyBundleHeadersOffset) / 20;
            int graphDataEntryCount = (ImportedPackageNamesOffset - DependencyBundleEntriesOffset) / 4;

            reader.BaseStream.Position = DependencyBundleHeadersOffset;
            DependencyBundleHeaders = reader.ReadArray(graphDataHeaderCount, () => new FDependencyBundleHeader(reader));
            
            reader.BaseStream.Position = DependencyBundleEntriesOffset;
            DependencyBundleEntries = reader.ReadArray(graphDataEntryCount, () => new FDependencyBundleEntry(reader));

            reader.BaseStream.Position = ImportedPackageNamesOffset;
            ImportedPackageNames = reader.ReadArray<byte>((int)((HeaderSize - 4) - ImportedPackageNamesOffset));

            AllExportDataOffset = (int)HeaderSize;
            
            int ProcessEntry(FExportBundleEntry entry, int pos)
            {
                if (entry.CommandType != EExportCommandType.ExportCommandType_Serialize)
                    return 0; // Skip ExportCommandType_Create
                
                // TODO: Unscuff this
                reader.BaseStream.Position = pos;
                Exports[(int)entry.LocalExportIndex] = (RawExport)Exports[(int)entry.LocalExportIndex].ConvertToChildExport<RawExport>();
                ((RawExport)Exports[(int)entry.LocalExportIndex]).Data = reader.ReadBytes((int)Exports[(int)entry.LocalExportIndex].SerialSize);
                
                // ConvertExportToChildExportAndRead(reader, (int)entry.LocalExportIndex);
            
                return (int)Exports[(int)entry.LocalExportIndex].SerialSize;
            }

            // Process exports
            foreach (var entry in ExportBundleEntries)
            {
                ProcessEntry(entry, AllExportDataOffset + (int)Exports[(int)entry.LocalExportIndex].SerialOffset);
            }

            // For some reason, exports can be read starting at the bottom and going up, so this fixes that
            int endOfExportsOffset = Exports.Aggregate(AllExportDataOffset, (current, export) => int.Max(current, AllExportDataOffset + (int)export.SerialOffset + (int)export.SerialSize));
            reader.BaseStream.Position = endOfExportsOffset;
            
            // This should just be 4 bytes (the package tag) but I cba to figure out how that's serialized
            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                TrailingData = reader.ReadArray<byte>((int)reader.BaseStream.Length - (int)reader.BaseStream.Position);
            }
            isReadingTime = false;
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
            data.AddRange(BitConverter.GetBytes(nameMapIndexList.Sum(x => x.Value.Length)));
            data.AddRange(BitConverter.GetBytes(HashVersion));

            // Hashes
            foreach (var entry in nameMapIndexList)
            {
                data.AddRange(BitConverter.GetBytes(CRCGenerator.CityHash64WithLower(entry)));
            }

            // Headers
            foreach (var entry in nameMapIndexList)
            {
                bool bIsUtf16 = Equals(entry.Encoding, Encoding.Unicode);
                int Len = entry.Value.Length;

                byte[] Data = new byte[2];
                Data[0] = (byte)((bIsUtf16 ? 1 : 0) << 7 | (byte)(Len >> 8));
                Data[1] = (byte)Len;

                data.AddRange(Data);
            }

            // Entries
            foreach (var entry in nameMapIndexList)
            {
                data.AddRange(entry.Encoding.GetBytes(entry.Value));
            }
            
            return data.ToArray();
        }

        /// <summary>
        /// Serializes an asset from memory.
        /// </summary>
        /// <returns>A stream that the asset has been serialized to.</returns>
        public override MemoryStream WriteData()
        {
            isSerializationTime = true;
            
            MemoryStream ret = new MemoryStream();
            AssetBinaryWriter writer = new AssetBinaryWriter(ret, this);
            
            writer.Write(SerializeHeader());
            writer.Write(SerializeNameMap());
            
            // Bulk data map
            writer.Write((ulong)BulkDataMap.Length * FBulkDataMapEntry.Size);
            foreach (var map in BulkDataMap)
            {
                writer.Write(map.SerialOffset);
                writer.Write(map.DuplicateSerialOffset);
                writer.Write(map.SerialSize);
                writer.Write(map.Flags);
                writer.Write(map.Pad);
            }

            // Imported public export hashes
            foreach (var importedPublicExportHash in ImportedPublicExportHashes)
            {
                writer.Write(importedPublicExportHash);
            }

            // Import map
            foreach (var import in Imports)
            {
                writer.Write(import.Hash);
            }

            // Export map
            foreach (var export in Exports)
            {
                export.WriteExportMapEntry(writer);
            }

            // Export bundle entries
            foreach (var entry in ExportBundleEntries)
            {
                entry.Write(writer);
            }

            // Graph data
            foreach (var dependency in DependencyBundleHeaders)
            {
                writer.Write(dependency.FirstEntryIndex);
                foreach (var entry in dependency.EntryCount)
                {
                    writer.Write(entry);
                }
            }
            
            foreach (var dependency in DependencyBundleEntries)
            {
                writer.Write(dependency.LocalImportOrExportIndex);
            }

            // Imported package names
            writer.Write(ImportedPackageNames ?? Array.Empty<byte>());

            // Padding
            writer.Write(0);
            
            // Exports
            foreach (var export in Exports)
            {
                export.Write(writer);
            }
            
            writer.Write(TrailingData ?? Array.Empty<byte>());

            writer.Seek(0, SeekOrigin.Begin);
            isSerializationTime = false;

            return ret;
        }

        /// <summary>
        /// Serializes and writes an asset to disk from memory.
        /// </summary>
        /// <param name="outputPath">The path on disk to write the asset to.</param>
        /// <exception cref="UnknownEngineVersionException">Thrown when <see cref="ObjectVersion"/> is unspecified.</exception>
        public override void Write(string outputPath)
        {
            if (Mappings == null) throw new InvalidOperationException();
            if (ObjectVersion == ObjectVersion.UNKNOWN) throw new UnknownEngineVersionException("Cannot begin serialization before an object version is specified");

            MemoryStream newData = WriteData();
            using (FileStream f = File.Open(outputPath, FileMode.Create, FileAccess.Write))
            {
                newData.CopyTo(f);
            }
        }
        
        public bool Swap(PropertyData search, PropertyData replace)
        {
            foreach (var export in Exports)
            {
                if (((RawExport)export).Swap(search, replace))
                {
                    return true;
                }
            }

            return false;
        }
        
        public bool Swap(string search, string replace)
        {
            if (search.Contains('.'))
            {
                if (!replace.Contains('.'))
                {
                    throw new Exception("Replace needs to contain a '.' if search contains one!");
                }
                
                var searchOne = search.Split('.')[0];
                var searchTwo = search.Split('.')[1];
                
                var idx = nameMapIndexList.FindIndex(x => string.Equals(x.Value, searchOne, StringComparison.CurrentCultureIgnoreCase));
                if (idx == -1)
                {
                    return false;
                }
                nameMapIndexList[idx] = new FString(replace.Split('.')[0]);
                
                idx = nameMapIndexList.FindIndex(x => string.Equals(x.Value, searchTwo, StringComparison.CurrentCultureIgnoreCase));
                if (idx == -1)
                {
                    return false;
                }
                nameMapIndexList[idx] = new FString(replace.Split('.')[1]);
            }
            else
            {
                var idx = nameMapIndexList.FindIndex(x => string.Equals(x.Value, search, StringComparison.CurrentCultureIgnoreCase));
                if (idx == -1)
                {
                    return false;
                }
                nameMapIndexList[idx] = new FString(replace);
            }
            
            FixNameMapLookupIfNeeded();

            return true;
        }

        public ZenAsset Swap(ZenAsset newAsset)
        {
            if (Exports.Count != newAsset.Exports.Count)
                throw new Exception("ExportMap length mismatch");
            
            var nameMap = GetNameMapIndexList();
            int sizeToAdd = nameMap[(int)MappedName.NameIndex].Value.Length - newAsset.nameMapIndexList[(int)newAsset.MappedName.NameIndex].Value.Length;
            sizeToAdd += nameMap[(int)MappedName.NameIndex - 1].Value.Length - newAsset.nameMapIndexList[(int)newAsset.MappedName.NameIndex - 1].Value.Length;
            
            newAsset.nameMapIndexList[(int)newAsset.MappedName.NameIndex] = nameMap[(int)MappedName.NameIndex];
            newAsset.nameMapIndexList[(int)newAsset.MappedName.NameIndex - 1] = nameMap[(int)MappedName.NameIndex - 1];
            newAsset.FixNameMapLookupIfNeeded();

            for (int i = 0; i < newAsset.Exports.Count; i++)
            {
                newAsset.Exports[i].PublicExportHash = Exports[i].PublicExportHash;
            }
            
            newAsset.HeaderSize += (uint)sizeToAdd;
            newAsset.CookedHeaderSize += (uint)sizeToAdd;
            newAsset.ImportedPublicExportHashesOffset += sizeToAdd;
            newAsset.ImportMapOffset += sizeToAdd;
            newAsset.ExportMapOffset += sizeToAdd;
            newAsset.ExportBundleEntriesOffset += sizeToAdd;
            newAsset.DependencyBundleHeadersOffset += sizeToAdd;
            newAsset.DependencyBundleEntriesOffset += sizeToAdd;
            newAsset.ImportedPackageNamesOffset += sizeToAdd;

            return newAsset;
        }

        /// <summary>
        /// Reads an asset from disk and initializes a new instance of the <see cref="UAsset"/> class to store its data in memory.
        /// </summary>
        /// <param name="path">The path of the asset file on disk that this instance will read from.</param>
        /// <param name="engineVersion">The version of the Unreal Engine that will be used to parse this asset. If the asset is versioned, this can be left unspecified.</param>
        /// <param name="mappings">A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.</param>
        /// <exception cref="UnknownEngineVersionException">Thrown when this is an unversioned asset and <see cref="ObjectVersion"/> is unspecified.</exception>
        /// <exception cref="FormatException">Throw when the asset cannot be parsed correctly.</exception>
        public ZenAsset(string path, EngineVersion engineVersion = EngineVersion.UNKNOWN, Usmap mappings = null)
        {
            this.FilePath = path;
            this.Mappings = mappings;
            SetEngineVersion(engineVersion);

            Read(PathToReader(path));
        }

        /// <summary>
        /// Reads an asset from a BinaryReader and initializes a new instance of the <see cref="ZenAsset"/> class to store its data in memory.
        /// </summary>
        /// <param name="reader">The asset's BinaryReader that this instance will read from.</param>
        /// <param name="engineVersion">The version of the Unreal Engine that will be used to parse this asset. If the asset is versioned, this can be left unspecified.</param>
        /// <param name="mappings">A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.</param>
        /// <param name="useSeparateBulkDataFiles">Does this asset uses separate bulk data files (.uexp, .ubulk)?</param>
        /// <exception cref="UnknownEngineVersionException">Thrown when this is an unversioned asset and <see cref="ObjectVersion"/> is unspecified.</exception>
        /// <exception cref="FormatException">Throw when the asset cannot be parsed correctly.</exception>
        public ZenAsset(AssetBinaryReader reader, EngineVersion engineVersion = EngineVersion.UNKNOWN, Usmap mappings = null, bool useSeparateBulkDataFiles = false)
        {
            this.Mappings = mappings;
            UseSeparateBulkDataFiles = useSeparateBulkDataFiles;
            SetEngineVersion(engineVersion);
            Read(reader);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenAsset"/> class. This instance will store no asset data and does not represent any asset in particular until the <see cref="Read"/> method is manually called.
        /// </summary>
        /// <param name="engineVersion">The version of the Unreal Engine that will be used to parse this asset. If the asset is versioned, this can be left unspecified.</param>
        /// <param name="mappings">A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.</param>
        public ZenAsset(EngineVersion engineVersion = EngineVersion.UNKNOWN, Usmap mappings = null)
        {
            this.Mappings = mappings;
            SetEngineVersion(engineVersion);
        }

        /// <summary>
        /// Reads an asset from disk and initializes a new instance of the <see cref="ZenAsset"/> class to store its data in memory.
        /// </summary>
        /// <param name="path">The path of the asset file on disk that this instance will read from.</param>
        /// <param name="objectVersion">The object version of the Unreal Engine that will be used to parse this asset</param>
        /// <param name="customVersionContainer">A list of custom versions to parse this asset with.</param>
        /// <param name="mappings">A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.</param>
        /// <exception cref="UnknownEngineVersionException">Thrown when this is an unversioned asset and <see cref="ObjectVersion"/> is unspecified.</exception>
        /// <exception cref="FormatException">Throw when the asset cannot be parsed correctly.</exception>
        public ZenAsset(string path, ObjectVersion objectVersion, List<CustomVersion> customVersionContainer, Usmap mappings = null)
        {
            this.FilePath = path;
            this.Mappings = mappings;
            ObjectVersion = objectVersion;
            CustomVersionContainer = customVersionContainer;

            Read(PathToReader(path));
        }

        /// <summary>
        /// Reads an asset from a BinaryReader and initializes a new instance of the <see cref="ZenAsset"/> class to store its data in memory.
        /// </summary>
        /// <param name="reader">The asset's BinaryReader that this instance will read from.</param>
        /// <param name="objectVersion">The object version of the Unreal Engine that will be used to parse this asset</param>
        /// <param name="customVersionContainer">A list of custom versions to parse this asset with.</param>
        /// <param name="mappings">A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.</param>
        /// <param name="useSeparateBulkDataFiles">Does this asset uses separate bulk data files (.uexp, .ubulk)?</param>
        /// <exception cref="UnknownEngineVersionException">Thrown when this is an unversioned asset and <see cref="ObjectVersion"/> is unspecified.</exception>
        /// <exception cref="FormatException">Throw when the asset cannot be parsed correctly.</exception>
        public ZenAsset(AssetBinaryReader reader, ObjectVersion objectVersion, List<CustomVersion> customVersionContainer, Usmap mappings = null, bool useSeparateBulkDataFiles = false)
        {
            this.Mappings = mappings;
            UseSeparateBulkDataFiles = useSeparateBulkDataFiles;
            ObjectVersion = objectVersion;
            CustomVersionContainer = customVersionContainer;
            Read(reader);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenAsset"/> class. This instance will store no asset data and does not represent any asset in particular until the <see cref="Read"/> method is manually called.
        /// </summary>
        /// <param name="objectVersion">The object version of the Unreal Engine that will be used to parse this asset</param>
        /// <param name="customVersionContainer">A list of custom versions to parse this asset with.</param>
        /// <param name="mappings">A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.</param>
        public ZenAsset(ObjectVersion objectVersion, List<CustomVersion> customVersionContainer, Usmap mappings = null)
        {
            this.Mappings = mappings;
            ObjectVersion = objectVersion;
            CustomVersionContainer = customVersionContainer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenAsset"/> class. This instance will store no asset data and does not represent any asset in particular until the <see cref="Read"/> method is manually called.
        /// </summary>
        public ZenAsset()
        {

        }
    }
}

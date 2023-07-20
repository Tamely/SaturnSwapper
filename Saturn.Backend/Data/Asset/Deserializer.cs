using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.IO.Objects;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.Utils;
using GenericReader;

namespace Saturn.Backend.Data.Asset;

public class Deserializer
{
    private readonly byte[] _data;

    public Deserializer(byte[] asset)
    {
        _data = asset;
    }

    public int TotalSize => _data.Length;
    
    public byte[] Properties;
    public byte[] BulkData = Array.Empty<byte>();

    public FZenPackageSummary Summary;
    public FPackageObjectIndex[] ImportMap;
    public FExportMapEntry[] ExportMap;
    public List<FNameEntrySerialized> ReadNameMap;
    public List<FNameEntrySerialized> ModifiedNameMap;
    
    public Deserializer Swap(Deserializer replacement)
    {
        replacement.SwapNameMap(replacement.ModifiedNameMap[^1].Name, ModifiedNameMap[^1].Name);
        replacement.SwapNameMap(replacement.ModifiedNameMap[^2].Name, ModifiedNameMap[^2].Name);

        if (replacement.ExportMap.Length != ExportMap.Length)
            throw new Exception("ExportMap length mismatch");
            
        for (int i = 0; i < replacement.ExportMap.Length; i++)
            replacement.ExportMap[i].PublicExportHash = ExportMap[i].PublicExportHash;

        return replacement;
    }
    
    public bool DoesNameMapContainName(string name)
    {
        return ModifiedNameMap.Any(n => n.Name == name);
    }
    
    public void AddName(string name)
    {
        if (name.Contains('.'))
        {
            var path = -1;
            var asset = -1;
            if (DoesNameMapContainName(name.Split('.')[0]))
                path = ModifiedNameMap.FindIndex(n => n.Name == name.Split('.')[0]);
                
            if (DoesNameMapContainName(name.Split('.')[1]))
                asset = ModifiedNameMap.FindIndex(n => n.Name == name.Split('.')[1]);
                
            if (path != -1 && asset != -1) return;

            if (asset == -1)
            {
                ModifiedNameMap.Add(new FNameEntrySerialized(name.Split('.')[1]));
            }

            if (path == -1)
            {
                ModifiedNameMap.Add(new FNameEntrySerialized(name.Split('.')[0]));
            }

            return;
        }

        if (DoesNameMapContainName(name)) return;

        ModifiedNameMap.Add(new FNameEntrySerialized(name));
    }

    public void SwapNameMap(string search, string replace)
    {
        if (search.Contains('.') && replace.Contains('.'))
        {
            bool bFoundFirst = false;
            bool bFoundSecond = false;
                
            for (int i = 0; i < ModifiedNameMap.Count; i++)
            {
                if (ModifiedNameMap[i].Name == search.Split('.')[0] && !bFoundFirst)
                {
                    ModifiedNameMap[i] = new FNameEntrySerialized(replace.Split('.')[0], ModifiedNameMap[i].hashVersion);
                    bFoundFirst = true;
                }

                if (ModifiedNameMap[i].Name == search.Split('.')[1] && !bFoundSecond)
                {
                    ModifiedNameMap[i] = new FNameEntrySerialized(replace.Split('.')[1], ModifiedNameMap[i].hashVersion);
                    bFoundSecond = true;
                }
            }
                
            if (!bFoundFirst)
                throw new Exception("Could not find path name in name map!");
            if (!bFoundSecond)
                throw new Exception("Could not find asset name in name map!");
        }
        else
        {
            bool bFound = false;
            for (int i = 0; i < ModifiedNameMap.Count; i++)
            {
                if (ModifiedNameMap[i].Name == search)
                {
                    ModifiedNameMap[i] = new FNameEntrySerialized(replace, ModifiedNameMap[i].hashVersion);
                    bFound = true;
                }
            }
                
            if (!bFound)
                throw new Exception("Could not find name in name map!");
        }
    }

    public void Deserialize()
    {
        GenericBufferReader Ar = new GenericBufferReader(_data);

        Summary = new FZenPackageSummary()
        {
            bHasVersioningInfo = Ar.Read<uint>(),
            HeaderSize = Ar.Read<uint>(),
            Name = Ar.Read<FMappedName>(),
            PackageFlags = Ar.Read<EPackageFlags>(),
            CookedHeaderSize = Ar.Read<uint>(),
            ImportedPublicExportHashesOffset = Ar.Read<int>(),
            ImportMapOffset = Ar.Read<int>(),
            ExportMapOffset = Ar.Read<int>(),
            ExportBundleEntriesOffset = Ar.Read<int>(),
            DependencyBundleHeadersOffset = Ar.Read<int>(),
            DependencyBundleEntriesOffset = Ar.Read<int>(),
            ImportedPackageNamesOffset = Ar.Read<int>()
        };

        int ImportCount = (Summary.ExportMapOffset - Summary.ImportMapOffset) / 8;
        int ExportCount = (Summary.ExportBundleEntriesOffset - Summary.ExportMapOffset) / 72;
        
        ReadNameMap = LoadNameBatch(Ar).ToList();
        ModifiedNameMap = new List<FNameEntrySerialized>(ReadNameMap);

        if (Summary.ImportedPublicExportHashesOffset - Ar.Position > 0)
        {
            BulkData = Ar.ReadBytes((int)(Summary.ImportedPublicExportHashesOffset - Ar.Position));
        }

        Properties = Ar.ReadBytes(Ar.Size - (int)Ar.Position);

        Ar.Position = Summary.ImportMapOffset;
        ImportMap = Ar.ReadArray<FPackageObjectIndex>(ImportCount);

        Ar.Position = Summary.ExportMapOffset;
        ExportMap = new FExportMapEntry[ExportCount];
        LoadExportMap(Ar);
    }

    public void Invalidate()
    {
        ModifiedNameMap[^2] = new FNameEntrySerialized("Tamely");
    }

    private void LoadExportMap(GenericBufferReader Ar)
    {
        for (int i = 0; i < ExportMap.Length; i++)
        {
            var start = Ar.Position;
            ExportMap[i].CookedSerialOffset = Ar.Read<ulong>();
            ExportMap[i].CookedSerialSize = Ar.Read<ulong>();
            ExportMap[i].ObjectName = Ar.Read<FMappedName>();
            ExportMap[i].OuterIndex = Ar.Read<FPackageObjectIndex>();
            ExportMap[i].ClassIndex = Ar.Read<FPackageObjectIndex>();
            ExportMap[i].SuperIndex = Ar.Read<FPackageObjectIndex>();
            ExportMap[i].TemplateIndex = Ar.Read<FPackageObjectIndex>();
            
            ExportMap[i].GlobalImportIndex = new FPackageObjectIndex(FPackageObjectIndex.Invalid);
            ExportMap[i].PublicExportHash = Ar.Read<ulong>();

            ExportMap[i].ObjectFlags = Ar.Read<EObjectFlags>();
            ExportMap[i].FilterFlags = Ar.Read<byte>();
            Ar.Position = start + FExportMapEntry.Size;
        }
    }

    private FNameEntrySerialized[] LoadNameBatch(GenericBufferReader Ar)
    {
        var num = Ar.Read<int>();
        if (num == 0)
        {
            return Array.Empty<FNameEntrySerialized>();
        }

        Ar.Position += sizeof(uint); // var numStringBytes = Ar.Read<uint>();
        var hashVersion = Ar.Read<ulong>();

        Ar.Position += num * sizeof(ulong); // var hashes = Ar.ReadArray<ulong>(num);
        var headers = Ar.ReadArray<FSerializedNameHeader>(num);
        var entries = new FNameEntrySerialized[num];
        for (var i = 0; i < num; i++)
        {
            var header = headers[i];
            var length = (int) header.Length;
            var s = header.IsUtf16 ? new string(Ar.ReadArray<char>(length)) : Encoding.UTF8.GetString(Ar.ReadBytes(length));
            entries[i] = new FNameEntrySerialized(s)
            {
                hashVersion = hashVersion
            };
        }

        return entries;
    }
}
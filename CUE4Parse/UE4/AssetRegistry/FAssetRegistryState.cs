using System;
using CUE4Parse.UE4.AssetRegistry.Objects;
using CUE4Parse.UE4.AssetRegistry.Readers;
using CUE4Parse.UE4.Readers;
using Newtonsoft.Json;
using Serilog;

namespace CUE4Parse.UE4.AssetRegistry
{
    [JsonConverter(typeof(FAssetRegistryStateConverter))]
    public class FAssetRegistryState
    {
        public FAssetData[] PreallocatedAssetDataBuffers;
        public FDependsNode[] PreallocatedDependsNodeDataBuffers;
        public FAssetPackageData[] PreallocatedPackageDataBuffers;
        
        public FAssetRegistryReader Reader;

        public FAssetRegistryState()
        {
            PreallocatedAssetDataBuffers = Array.Empty<FAssetData>();
            PreallocatedDependsNodeDataBuffers = Array.Empty<FDependsNode>();
            PreallocatedPackageDataBuffers = Array.Empty<FAssetPackageData>();
        }

        public FAssetRegistryState(FArchive Ar) : this()
        {
            var header = new FAssetRegistryHeader(Ar);
            var version = header.Version;
            switch (version)
            {
                case < FAssetRegistryVersionType.AddAssetRegistryState:
                    Log.Warning("Cannot read registry state before {Version}", version);
                    break;
                case < FAssetRegistryVersionType.FixedTags:
                {
                    var nameTableReader = new FNameTableArchiveReader(Ar, header);
                    Load(nameTableReader);
                    break;
                }
                default:
                {
                    Reader = new FAssetRegistryReader(Ar, header);
                    Load(Reader);
                    break;
                }
            }
        }

        private void Load(FAssetRegistryArchive Ar)
        {
            PreallocatedAssetDataBuffers = Ar.ReadArray(() => new FAssetData(Ar));

            if (Ar.Header.Version < FAssetRegistryVersionType.RemovedMD5Hash)
                return; // Just ignore the rest of this for now.

            if (Ar.Header.Version < FAssetRegistryVersionType.AddedDependencyFlags)
            {
                var localNumDependsNodes = Ar.Read<int>();
                PreallocatedDependsNodeDataBuffers = new FDependsNode[localNumDependsNodes];
                for (var i = 0; i < localNumDependsNodes; i++)
                {
                    PreallocatedDependsNodeDataBuffers[i] = new FDependsNode(i);
                }
                if (localNumDependsNodes > 0)
                {
                    LoadDependencies_BeforeFlags(Ar);
                }
            }
            else
            {
                var dependencySectionSize = Ar.Read<long>();
                var dependencySectionEnd = Ar.Position + dependencySectionSize;
                var localNumDependsNodes = Ar.Read<int>();
                PreallocatedDependsNodeDataBuffers = new FDependsNode[localNumDependsNodes];
                for (var i = 0; i < localNumDependsNodes; i++)
                {
                    PreallocatedDependsNodeDataBuffers[i] = new FDependsNode(i);
                }
                if (localNumDependsNodes > 0)
                {
                    LoadDependencies(Ar);
                }
                Ar.Position = dependencySectionEnd;
            }

            PreallocatedPackageDataBuffers = Ar.ReadArray(() => new FAssetPackageData(Ar));
        }
        
        public (int packageSearch, int assetSearch, int packageReplace, int assetReplace) Swap(string searchPath, string replacePath)
        {
            int searchPackageIdx = -1;
            int searchAssetIdx = -1;        
            int replacePackageIdx = -1;
            int replaceAssetIdx = -1;

            for (int i = 0; i < Reader.NameMap.Length; i++)
            {
                // Speed optimization
                if ((Reader.NameMap[i].Name ?? string.Empty).Length != searchPath.Length &&
                    (Reader.NameMap[i].Name ?? string.Empty).Length != replacePath.Length) continue;
                
                if (String.Equals(Reader.NameMap[i].Name, searchPath, StringComparison.CurrentCultureIgnoreCase))
                {
                    searchPackageIdx = i - 1;
                    searchAssetIdx = i;
                }

                if (String.Equals(Reader.NameMap[i].Name, replacePath, StringComparison.CurrentCultureIgnoreCase))
                {
                    replacePackageIdx = i - 1;
                    replaceAssetIdx = i;
                }
                
                if (searchPackageIdx is not -1 && searchAssetIdx is not -1 && replacePackageIdx is not -1 && replaceAssetIdx is not -1) break;
            }

            return (searchPackageIdx, searchAssetIdx, replacePackageIdx, replaceAssetIdx);
        }

        private void LoadDependencies_BeforeFlags(FAssetRegistryArchive Ar)
        {
            foreach (var dependsNode in PreallocatedDependsNodeDataBuffers)
            {
                dependsNode.SerializeLoad_BeforeFlags(Ar, PreallocatedDependsNodeDataBuffers);
            }
        }

        private void LoadDependencies(FAssetRegistryArchive Ar)
        {
            foreach (var dependsNode in PreallocatedDependsNodeDataBuffers)
            {
                dependsNode.SerializeLoad(Ar, PreallocatedDependsNodeDataBuffers);
            }
        }
    }
}

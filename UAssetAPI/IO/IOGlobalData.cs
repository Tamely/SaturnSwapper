using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace UAssetAPI.IO
{
    public struct FScriptObjectEntry
    {
        public FName ObjectName;
        public FPackageObjectIndex GlobalIndex;
        public FPackageObjectIndex OuterIndex;
        public FPackageObjectIndex CDOClassIndex;
    }

    public interface INameMap
    {
        IReadOnlyList<FString> GetNameMapIndexList();
        void ClearNameIndexList();
        void SetNameReference(int index, FString value);
        FString GetNameReference(int index);
        bool ContainsNameReference(FString search);
        int SearchNameReference(FString search);
        int AddNameReference(FString name, bool forceAddDuplicates = false);
    }

    /// <summary>
    /// Global data exported from a game's global IO store container.
    /// </summary>
    public class IOGlobalData : INameMap
    {
        /// <summary>
        /// Internal list of name map entries. Do not directly add values to here under any circumstances; use <see cref="AddNameReference"/> instead
        /// </summary>
        [JsonProperty("NameMap", Order = -2)]
        internal List<FString> nameMapIndexList;

        /// <summary>
        /// Internal lookup for name map entries. Do not directly add values to here under any circumstances; use <see cref="AddNameReference"/> instead
        /// </summary>
        internal Dictionary<string, int> nameMapLookup = new Dictionary<string, int>();
        public FScriptObjectEntry[] ScriptObjectEntries;
        public Dictionary<ulong, FScriptObjectEntry> ScriptObjectEntriesMap;

        public string GetScriptName(ulong index)
        {
            return nameMapIndexList[ScriptObjectEntriesMap[index].ObjectName.Index].Value;
        }

        public IOGlobalData(IOStoreContainer container, EngineVersion engineVersion)
        {
            ClearNameIndexList();
        }

        internal void FixNameMapLookupIfNeeded()
        {
            if (nameMapIndexList.Count > 0 && nameMapLookup.Count == 0)
            {
                for (int i = 0; i < nameMapIndexList.Count; i++)
                {
                    nameMapLookup[nameMapIndexList[i].Value] = i;
                }
            }
        }

        /// <summary>
        /// Returns the name map as a read-only list of FStrings.
        /// </summary>
        /// <returns>The name map as a read-only list of FStrings.</returns>
        public IReadOnlyList<FString> GetNameMapIndexList()
        {
            FixNameMapLookupIfNeeded();
            return nameMapIndexList.AsReadOnly();
        }

        /// <summary>
        /// Clears the name map. This method should be used with extreme caution, as it may break unparsed references to the name map.
        /// </summary>
        public void ClearNameIndexList()
        {
            nameMapIndexList = new List<FString>();
            nameMapLookup = new Dictionary<string, int>();
        }

        /// <summary>
        /// Replaces a value in the name map at a particular index.
        /// </summary>
        /// <param name="index">The index to overwrite in the name map.</param>
        /// <param name="value">The value that will be replaced in the name map.</param>
        public void SetNameReference(int index, FString value)
        {
            FixNameMapLookupIfNeeded();
            nameMapIndexList[index] = value;
            nameMapLookup[value.Value] = index;
        }

        /// <summary>
        /// Gets a value in the name map at a particular index.
        /// </summary>
        /// <param name="index">The index to return the value at.</param>
        /// <returns>The value at the index provided.</returns>
        public FString GetNameReference(int index)
        {
            FixNameMapLookupIfNeeded();
            if (index < 0) return new FString(Convert.ToString(-index));
            if (index >= nameMapIndexList.Count) return new FString(Convert.ToString(index));
            return nameMapIndexList[index];
        }

        /// <summary>
        /// Gets a value in the name map at a particular index, but with the index zero being treated as if it is not valid.
        /// </summary>
        /// <param name="index">The index to return the value at.</param>
        /// <returns>The value at the index provided.</returns>
        public FString GetNameReferenceWithoutZero(int index)
        {
            FixNameMapLookupIfNeeded();
            if (index <= 0) return new FString(Convert.ToString(-index));
            if (index >= nameMapIndexList.Count) return new FString(Convert.ToString(index));
            return nameMapIndexList[index];
        }

        /// <summary>
        /// Checks whether or not the value exists in the name map.
        /// </summary>
        /// <param name="search">The value to search the name map for.</param>
        /// <returns>true if the value appears in the name map, otherwise false.</returns>
        public bool ContainsNameReference(FString search)
        {
            FixNameMapLookupIfNeeded();
            return nameMapLookup.ContainsKey(search.Value);
        }

        /// <summary>
        /// Searches the name map for a particular value.
        /// </summary>
        /// <param name="search">The value to search the name map for.</param>
        /// <returns>The index at which the value appears in the name map.</returns>
        /// <exception cref="UAssetAPI.NameMapOutOfRangeException">Thrown when the value provided does not appear in the name map.</exception>
        public int SearchNameReference(FString search)
        {
            //FixNameMapLookupIfNeeded();
            if (ContainsNameReference(search)) return nameMapLookup[search.Value];
            throw new NameMapOutOfRangeException(search);
        }

        /// <summary>
        /// Adds a new value to the name map.
        /// </summary>
        /// <param name="name">The value to add to the name map.</param>
        /// <param name="forceAddDuplicates">Whether or not to add a new entry if the value provided already exists in the name map.</param>
        /// <returns>The index of the new value in the name map. If the value already existed in the name map beforehand, that index will be returned instead.</returns>
        /// <exception cref="ArgumentException">Thrown when forceAddDuplicates is false and the value provided is null or empty.</exception>
        public int AddNameReference(FString name, bool forceAddDuplicates = false)
        {
            FixNameMapLookupIfNeeded();

            if (!forceAddDuplicates)
            {
                if (name?.Value == null) throw new ArgumentException("Cannot add a null FString to the name map");
                if (name.Value == string.Empty) throw new ArgumentException("Cannot add an empty FString to the name map");
                if (ContainsNameReference(name)) return SearchNameReference(name);
            }

            nameMapIndexList.Add(name);
            nameMapLookup[name.Value] = nameMapIndexList.Count - 1;
            return nameMapIndexList.Count - 1;
        }
    }
}

using System;
using System.Collections.Generic;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Objects.UObject;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Saturn.Backend.Data.Swapper;

public abstract class UnrealPackage : INameMap
{
    /// <summary>
    /// The path of the asset on disk. This is optional.
    /// </summary>
    [JsonIgnore]
    public string FilePath;

    private EPackageFlags _packageFlags;

    /// <summary>
    /// The flags for this package
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public EPackageFlags PackageFlags
    {
        get
        {
            return _packageFlags;
        }
        set
        {
            _packageFlags = value;
            _hasUnversionedPropertiesCacheDirty = true;
        }
    }

    private bool _hasUnversionedPropertiesCacheDirty = true;
    private bool _hasUnversionedPropertiesCache;

    /// <summary>
    /// Whether or not this asset has unversioned properties.
    /// </summary>
    public bool HasUnversionedProperties
    {
        get
        {
            if (_hasUnversionedPropertiesCacheDirty)
            {
                _hasUnversionedPropertiesCache = PackageFlags.HasFlag(EPackageFlags.PKG_UnversionedProperties);
                _hasUnversionedPropertiesCacheDirty = false;
            }

            return _hasUnversionedPropertiesCache;
        }
    }
    
    /// <summary>
    /// The corresponding mapping data for the game that this asset is from.
    /// </summary>
    [JsonIgnore] public TypeMappings Mappings;

    /// <summary>
    /// Internal list of name map entries. Do not directly add values to here under any circumstances; use <see cref="AddNameReference"/> instead
    /// </summary>
    [JsonProperty("NameMap", Order = -99)] 
    internal List<FNameEntrySerialized> nameMapIndexList = new List<FNameEntrySerialized>();

    /// <summary>
    /// Internal lookup for name map entries. Do not directly add values to here under any circumstances; use <see cref="AddNameReference"/> instead
    /// </summary>
    internal Dictionary<string, int> nameMapLookup = new Dictionary<string, int>();

    internal void FixNameMapLookupIfNeeded()
    {
        if (nameMapIndexList.Count > 0 && nameMapLookup.Count == 0)
        {
            for (int i = 0; i < nameMapIndexList.Count; i++)
            {
                nameMapLookup[nameMapIndexList[i].Name] = i;
            }
        }
    }

    /// <summary>
    /// Returns the name map as a list of read-only FNameEntrySerialized
    /// </summary>
    /// <returns>The name map as a list of read-only FNameEntrySerialized.</returns>
    public IReadOnlyList<FNameEntrySerialized> GetNameMapIndexList()
    {
        FixNameMapLookupIfNeeded();
        return nameMapIndexList.AsReadOnly();
    }

    /// <summary>
    /// Clears the name map. This method should be used with extreme caution, as it may break unparsed references to the name map.
    /// </summary>
    public void ClearNameIndexList()
    {
        nameMapIndexList = new List<FNameEntrySerialized>();
        nameMapLookup = new Dictionary<string, int>();
    }

    /// <summary>
    /// Replaces a value in the name map at a particular index.
    /// </summary>
    /// <param name="index">The index to overwrite in the name map.</param>
    /// <param name="value">The value that will be replaced in the name map.</param>
    public void SetNameReference(int index, FNameEntrySerialized value)
    {
        FixNameMapLookupIfNeeded();
        nameMapIndexList[index] = value;
        nameMapLookup[value.Name] = index;
    }

    /// <summary>
    /// Gets a value in the name map at a particular index.
    /// </summary>
    /// <param name="index">The index to return the value at.</param>
    /// <returns>The value at the index provided.</returns>
    /// <exception cref="IndexOutOfRangeException">The index provided was not confined to the bounds of the name map.</exception>
    public FNameEntrySerialized GetNameReference(int index)
    {
        FixNameMapLookupIfNeeded();
        if (index < 0 || index >= nameMapIndexList.Count)
        {
            throw new IndexOutOfRangeException($"Could not get name at index: {index} with name map size: {nameMapIndexList.Count}");
        }
        return nameMapIndexList[index];
    }

    /// <summary>
    /// Gets a value in the name map at a particular index, but with the index zero being treated as if it is not valid.
    /// </summary>
    /// <param name="index">The index to return the value at.</param>
    /// <returns>The value at the index provided.</returns>
    /// <exception cref="IndexOutOfRangeException">The index provided was not confined to the bounds of the name map.</exception>
    public FNameEntrySerialized GetNameReferenceWithoutZero(int index)
    {
        FixNameMapLookupIfNeeded();
        if (index <= 0 || index >= nameMapIndexList.Count)
        {
            throw new IndexOutOfRangeException($"Could not get name at index: {index} with name map size: {nameMapIndexList.Count}");
        }
        return nameMapIndexList[index];
    }

    /// <summary>
    /// Checks whether or not the value exists in the name map.
    /// </summary>
    /// <param name="search">The value to search the name map for.</param>
    /// <returns>true if the value appears in the name map, otherwise false.</returns>
    public bool ContainsNameReference(FNameEntrySerialized search)
    {
        FixNameMapLookupIfNeeded();
        return nameMapLookup.ContainsKey(search.Name);
    }

    /// <summary>
    /// Searches the name map for a particular value.
    /// </summary>
    /// <param name="search">The value to search the name map for.</param>
    /// <returns>The index at which the value appears in the name map.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value provided does not appear in the name map.</exception>
    public int SearchNameReference(FNameEntrySerialized search)
    {
        if (ContainsNameReference(search))
        {
            return nameMapLookup[search.Name];
        }

        throw new ArgumentOutOfRangeException($"Could not find name: {search.Name} in the name map.");
    }

    /// <summary>
    /// Adds a new value to the name map.
    /// </summary>
    /// <param name="name">The value to add to the name map.</param>
    /// <param name="forceAddDuplicates">Whether or not to add a new entry if the value provided already exists in the name map.</param>
    /// <returns>The index of the new value in the name map. If the value already exists in the name map, the existing index will be returned.</returns>
    /// <exception cref="ArgumentException">Thrown when forceAddDuplicates is false and the value provided is null or empty.</exception>
    public int AddNameReference(FNameEntrySerialized name, bool forceAddDuplicates = false)
    {
        FixNameMapLookupIfNeeded();

        if (!forceAddDuplicates)
        {
            if (string.IsNullOrWhiteSpace(name.Name)) throw new ArgumentException("Cannot add a null FNameEntry to the name map");
            if (ContainsNameReference(name)) return SearchNameReference(name);
        }

        nameMapIndexList.Add(name);
        nameMapLookup[name.Name] = nameMapIndexList.Count - 1;
        return nameMapIndexList.Count - 1;
    }
    
    
}
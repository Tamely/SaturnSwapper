using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CUE4Parse.UE4.Objects.UObject;
using GenericReader;
using Newtonsoft.Json;

namespace Saturn.Backend.Data.Swapper.Properties;

public class AncestryInfo : ICloneable
{
    public List<FName> Ancestors = new List<FName>(5);

    public FName Parent
    {
        get => Ancestors[^1];
        set => Ancestors[^1] = value;
    }

    public object Clone()
    {
        var res = new AncestryInfo();
        res.Ancestors.AddRange(Ancestors);
        return res;
    }

    public AncestryInfo CloneWithoutParent()
    {
        var res = (AncestryInfo)Clone();
        res.Ancestors.RemoveAt(res.Ancestors.Count - 1);
        return res;
    }

    public void Initialize(AncestryInfo ancestryInfo, FName parent)
    {
        Ancestors.Clear();
        if (ancestryInfo != null)
        {
            Ancestors.AddRange(ancestryInfo.Ancestors);
        }

        SetAsParent(parent);
    }

    public void SetAsParent(FName parent)
    {
        if (parent != null)
        {
            Ancestors.Add(parent);
        }
    }
}

/// <summary>
/// Generic Unreal Property class.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class PropertyData
{
    /// <summary>
    /// The name of this property.
    /// </summary>
    [JsonProperty] public FName Name = null;

    /// <summary>
    /// The ancestry of this property. Contains information about all the classes/structs that this property is contained within. Not serialized.
    /// </summary>
    [JsonIgnore] public AncestryInfo Ancestry = new AncestryInfo();

    /// <summary>
    /// The duplication index of this property. Used to distinguish properties with the same name in the same struct.
    /// </summary>
    [JsonProperty] public int DuplicationIndex = 0;

    /// <summary>
    /// An optional property GUID. Nearly always null.
    /// </summary>
    public Guid? PropertyGuid = null;

    /// <summary>
    /// The offset of this property on disk. This is only for the the user and doesn't affect the API.
    /// </summary>
    public long Offset = -1;

    /// <summary>
    /// An optional tag which can be set on any property in memory. This is only for the user and doesn't affect the API.
    /// </summary>
    public object Tag;

    protected object _rawValue;
    public virtual object RawValue
    {
        get
        {
            if (_rawValue == null && DefaultValue != null)
            {
                _rawValue = DefaultValue;
            }

            return _rawValue;
        }
        set => _rawValue = value;
    }

    public void SetObject(object value)
    {
        RawValue = value;
    }

    public T GetObject<T>()
    {
        if (RawValue is null) return default;
        return (T)RawValue;
    }

    public PropertyData(FName name)
    {
        Name = name;
    }
    
    public PropertyData() {}

    private static string FallbackPropertyType = string.Empty;
    
    /// <summary>
    /// Determines whether or not this particular property should be registered in the property registry and automatically used when parsing assets.
    /// </summary>
    public virtual bool ShouldBeRegistered => true;

    /// <summary>
    /// Determines whether or not this particular property has custom serialization within a StructProperty.
    /// </summary>
    public virtual bool HasCustomStructSerialization => false;

    /// <summary>
    /// The type of this property as a string.
    /// </summary>
    public virtual string PropertyType => FallbackPropertyType;

    /// <summary>
    /// The default value of this property, used as a fallback when no value is defined. Null by default.
    /// </summary>
    public virtual object DefaultValue => null;
    
    /// <summary>
    /// Reads out a property from a BufferReader.
    /// </summary>
    /// <param name="reader">The BufferReader to read from.</param>
    /// <param name="includeHeader">Whether or not to also read the "header" of the property, which is data considered by UE to be data part of the PropertyData base class rather than any particular child class.</param>
    /// <param name="len1">An estimate for the length of the data being read out.</param>
    /// <param name="len2">A second estimate for the length of the data being read out.</param>
    public virtual void Read(GenericBufferReader reader, bool includeHeader, long len1, long len2 = 0) {}

    /// <summary>
    /// Resolves the ancestry of all child properties of this property.
    /// </summary>
    /// <param name="asset">The asset to get the ancestry for.</param>
    /// <param name="ancestrySoFar">The ancestry we have so far.</param>
    public virtual void ResolveAncestries(UnrealPackage asset, AncestryInfo ancestrySoFar)
    {
        Ancestry = ancestrySoFar;
    }

    /// <summary>
    /// Serializes a property.
    /// </summary>
    /// <param name="includeHeader">Whether or not to also read the "header" of the property, which is data considered by UE to be data part of the PropertyData base class rather than any particular child class.</param>
    /// <returns>The serialized property</returns>
    public virtual byte[] Serialize(bool includeHeader)
    {
        return Array.Empty<byte>();
    }

    /// <summary>
    /// Does the body of this property entirely consist of null bytes? If so, the body will not be serialized in unversioned properties.
    /// </summary>
    /// <returns>Whether or not the property is zero.</returns>
    public virtual bool IsZero()
    {
        var bytes = Serialize(false);
        return bytes.All(entry => entry == 0);
    }

    /// <summary>
    /// Sets certain fields of the property based off of an array of strings.
    /// </summary>
    /// <param name="d">An array of strings to derive certain fields from.</param>
    /// <param name="asset">The asset that the property belongs to.</param>
    public virtual void FromString(string[] d, UAsset asset) {}

    /// <summary>
    /// Performs a deep clone of the current PropertyDat instance.
    /// </summary>
    /// <returns>A deep copy of the current property.</returns>
    public object Clone()
    {
        var res = (PropertyData)MemberwiseClone();
        res.Name = new FName()
        {
            Number = Name.Number,
            Index = Name.Index
        };

        if (res.RawValue is ICloneable cloneableValue) res.RawValue = cloneableValue.Clone();

        HandleCloned(res);
        return res;
    }

    protected virtual void HandleCloned(PropertyData res)
    {
        // Child classes can impl this for custom cloning behavior.
    }
}

public abstract class PropertyData<T> : PropertyData
{
    /// <summary>
    /// The "main value" of this property, if it exists. Properties may contain other values as well, in which case they will be present as other fields in the child class.
    /// </summary>
    [JsonProperty]
    public T Value
    {
        get => GetObject<T>();
        set => SetObject(value);
    }

    public PropertyData(FName name) : base(name){}
    public PropertyData() : base() {}
}
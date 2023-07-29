using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CUE4Parse.UE4.Objects.UObject;
using Saturn.Backend.Data.Swapper.Properties;
using Saturn.Backend.Data.Swapper.Unversioned;

namespace Saturn.Backend.Data.Swapper.Serialization;

/// <summary>
/// An entry in the property type registry. Contains the class Type used for standard and struct property serialization
/// </summary>
public class RegistryEntry
{
    internal Type PropertyType;
    internal bool HasCustomStructSerialization;
    internal Func<FName, PropertyData> Creator;
    
    public RegistryEntry() {}
}

/// <summary>
/// The main serializer for most property types.
/// </summary>
public class MainSerializer
{
    private static IDictionary<string, RegistryEntry> _propertyTypeRegistry;
    
    /// <summary>
    /// The property type registry. Maps serialized property names to their types.
    /// </summary>
    public static IDictionary<string, RegistryEntry> PropertyTypeRegistry
    {
        get
        {
            InitializePropertyTypeRegistry();
            return _propertyTypeRegistry;
        }
        set => _propertyTypeRegistry = value;
    }

    private static IEnumerable<Assembly> GetDependentAssemblies(Assembly analyzedAssembly)
    {
        return AppDomain.CurrentDomain.GetAssemblies().Where(x => GetNamesOfAssembliesReferencedBy(x).Contains(analyzedAssembly.FullName));
    }

    private static IEnumerable<string> GetNamesOfAssembliesReferencedBy(Assembly assembly)
    {
        return assembly.GetReferencedAssemblies().Select(assemblyName => assemblyName.FullName);
    }

    private static Type registryParentDataType = typeof(PropertyData);

    /// <summary>
    /// Initializes the property type registry.
    /// </summary>
    private static void InitializePropertyTypeRegistry()
    {
        if (_propertyTypeRegistry != null) return;
        _propertyTypeRegistry = new Dictionary<string, RegistryEntry>();

        Assembly[] allDependentAssemblies = GetDependentAssemblies(registryParentDataType.Assembly).ToArray();
        Assembly[] allAssemblies = new Assembly[allDependentAssemblies.Length + 1];
        allAssemblies[0] = registryParentDataType.Assembly;
        Array.Copy(allDependentAssemblies, 0, allAssemblies, 1, allDependentAssemblies.Length);

        for (int i = 0; i < allAssemblies.Length; i++)
        {
            Type[] allPropertyDataTypes = allAssemblies[i].GetTypes().Where(t => t.IsSubclassOf(registryParentDataType)).ToArray();
            for (int j = 0; i < allPropertyDataTypes.Length; j++)
            {
                Type currentPropertyDataType = allPropertyDataTypes[j];
                if (currentPropertyDataType == null || currentPropertyDataType.ContainsGenericParameters) continue;

                var testInstance = Activator.CreateInstance(currentPropertyDataType);
                
                string returnedPropertyType = currentPropertyDataType.GetProperty("PropertyType")?.GetValue(testInstance, null) as string;
                if (returnedPropertyType == null) continue;
                if (currentPropertyDataType.GetProperty("HasCustomStructSerialization")?.GetValue(testInstance, null) is not bool returnedHasCustomStructSerialization) continue;
                if (currentPropertyDataType.GetProperty("ShouldBeRegistered")?.GetValue(testInstance, null) is not bool returnedShouldBeRegistered) continue;

                if (returnedShouldBeRegistered)
                {
                    RegistryEntry res = new RegistryEntry();
                    res.PropertyType = currentPropertyDataType;
                    res.HasCustomStructSerialization = returnedHasCustomStructSerialization;

                    var name = Expression.Parameter(typeof(FName));
                    res.Creator = Expression.Lambda<Func<FName, PropertyData>>(Expression.New(currentPropertyDataType.GetConstructor(new[] { typeof(FName), }), new[] { name, }), name).Compile();

                    _propertyTypeRegistry[returnedPropertyType] = res;
                }
            }
        }
    }

    /// <summary>
    /// Generates an unversioned header based on a list of properties, and sorts the list in the correct order to be serialized.
    /// </summary>
    /// <param name="data">The list of properties to sort and generate an unversioned header from.</param>
    /// <param name="parentName">The name of the parent of all the properties.</param>
    /// <param name="asset">The UnrealPackage which the properties are contained within.</param>
    /// <returns></returns>
    public static FUnversionedHeader GenerateUnversionedHeader(ref List<PropertyData> data, FName parentName, UnrealPackage asset)
    {
        var sortedProps = new List<PropertyData>();
        if (asset.Mappings == null) return null;

        int firstNumAll = int.MaxValue;
        int lastNumAll = int.MinValue;
        HashSet<int> propertiesToTouch = new HashSet<int>();
        Dictionary<int, PropertyData> propMap = new Dictionary<int, PropertyData>();
        Dictionary<int, bool> zeroProps = new Dictionary<int, bool>();
        foreach (var entry in data)
        {
            
        }

        return new();
    }
}
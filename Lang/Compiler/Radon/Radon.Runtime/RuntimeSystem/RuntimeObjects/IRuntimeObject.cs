using System;
using System.Collections.Generic;
using System.Linq;
using Radon.CodeAnalysis.Emit;
using System.Reflection;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal interface IRuntimeObject
{
    private static Type[] CachedPrims = Array.Empty<Type>();
    
    public int Size { get; } // Size in bytes; -1 if dynamic
    public RuntimeType Type { get; } // Type of the object
    public object? Value { get; } // Value of the object

    public int ResolveSize();
    
    public byte[] Serialize(); // Serializes the object

    public IRuntimeObject? ComputeOperation(OpCode operation, IRuntimeObject? other); // Computes an operation on the object

    public T GetValue<T>();
    
    public IRuntimeObject? ConvertTo(RuntimeType type); // Converts the object to the specified type

    public static IRuntimeObject CreateDefault(RuntimeType type)
    {
        if (type.TypeInfo.IsPrimitive)
        {
            return CreatePrimitive(type, null);
        }
        
        if (type.TypeInfo.IsArray)
        {
            return new ManagedArray(type, 0);
        }
        
        return new ManagedObject(type);
    }

    public static NullObject Null(RuntimeType type) => new(type);

    public static IRuntimeObject Deserialize(RuntimeType type, byte[] data)
    {
        var stream = new BinaryStream(data);
        return Deserialize(type, stream);
    }

    private static IRuntimeObject Deserialize(RuntimeType type, BinaryStream stream)
    {
        if (type.TypeInfo.IsPrimitive)
        {
            var size = CreateDefault(type).ResolveSize();
            var data = stream.ReadArray<byte>(size);
            return CreatePrimitive(type, data);
        }
        
        if (type.TypeInfo.IsArray)
        {
            var length = stream.Read<int>();
            var array = new ManagedArray(type, length);
            for (var i = 0; i < length; i++)
            {
                var underlyingType = type.TypeInfo.UnderlyingType;
                if (underlyingType is null)
                {
                    throw new InvalidOperationException("Underlying type is null.");
                }
                
                var runtimeType = ManagedRuntime.System.GetType(underlyingType);
                var element = Deserialize(runtimeType, stream);
                if (element is null)
                {
                    throw new InvalidOperationException("Failed to deserialize an element.");
                }
                
                array.SetElement(i, element);
            }
            
            return array;
        }

        if (type.TypeInfo.IsEnum)
        {
            var underlyingType = type.TypeInfo.UnderlyingType;
            if (underlyingType is null)
            {
                throw new InvalidOperationException("Underlying type is null.");
            }
            
            var runtimeType = ManagedRuntime.System.GetType(underlyingType);
            var value = Deserialize(runtimeType, stream);
            if (value is null)
            {
                throw new InvalidOperationException("Failed to deserialize the enum value");
            }

            return value;
        }
        
        var fields = type.TypeInfo.Fields;
        var instance = new ManagedObject(type);
        foreach (var field in fields)
        {
            var fieldType = field.Type;
            var runtimeType = ManagedRuntime.System.GetType(fieldType);
            var value = Deserialize(runtimeType, stream);
            if (value is null)
            {
                throw new InvalidOperationException("Failed to deserialize a field.");
            }
            
            instance.SetField(field, value);
        }
        
        return instance;
    }
    
    private static IRuntimeObject CreatePrimitive(RuntimeType type, byte[]? data)
    {
        // Get all types that derive from ManagedPrimitive<T>
        // Get all types
        if (CachedPrims.Length == 0)
        {
            var types = typeof(ManagedPrimitive<>).Assembly.GetTypes();
            var primitiveTypes = new List<Type>();
            foreach (var t in types)
            {
                var baseType = t.BaseType;
                var primitiveType = typeof(ManagedPrimitive);
                if (baseType is null)
                {
                    continue;
                }
                
                if (baseType.IsSubclassOf(primitiveType) && !t.IsAbstract)
                {
                    primitiveTypes.Add(t);
                }
            }

            CachedPrims = primitiveTypes.ToArray();
        }

        // Get the primitive type where the IRuntimeObject.Type field is equal to the "type" parameter
        foreach (var prim in CachedPrims)
        {
            var defaultPrim = Activator.CreateInstance(prim);
            var property = prim.GetProperty("Type", BindingFlags.Public | BindingFlags.Instance);
            var propertyType = (RuntimeType?)property?.GetValue(defaultPrim);
            if (propertyType is null)
            {
                continue;
            }
            
            if (propertyType.TypeInfo == type.TypeInfo)
            {
                // Create an instance of the primitive type
                var primInstance = data is null ? defaultPrim : Activator.CreateInstance(prim, data);
                if (primInstance is null)
                {
                    throw new InvalidOperationException("Failed to create an instance of the primitive type.");
                }
                
                return (IRuntimeObject)primInstance;
            }
        }
            
        throw new InvalidOperationException("Failed to find a primitive type.");
    }
}

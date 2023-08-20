using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Radon.CodeAnalysis.Emit;

namespace Radon.Common;

public static class ObjectExtensions
{
    private static readonly Dictionary<Type, bool> CachedTypes = new();
    
    public static unsafe T Encrypt<T>(this object value, long key)
    {
        if (typeof(T) != value.GetType())
        {
            throw new ArgumentException("The type of the value must be the same as the type of the generic parameter.");
        }
        
        if (value.IsUnmanaged())
        {
            var size = Marshal.SizeOf<T>();
            var bytes = stackalloc byte[size];
            Marshal.StructureToPtr(value, (nint)bytes, true);
            for (var i = 0; i < size; i++)
            {
                bytes[i] ^= (byte)key;
            }

            return Marshal.PtrToStructure<T>((nint)bytes)!;
        }

        return (T)value;
    }
    
    public static T Decrypt<T>(this object value, long key) => value.Encrypt<T>(key);

    public static bool IsUnmanaged(this object obj)
    {
        var type = obj.GetType();
        return IsUnmanaged(type);
    }

    public static bool IsUnmanaged(this Type type)
    {
        // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
        if (CachedTypes.ContainsKey(type))
        {
            return CachedTypes[type];
        }

        bool result;
        if (type.IsPrimitive || type.IsPointer || type.IsEnum)
        {
            result = true;
        }
        else if (type.IsGenericType || !type.IsValueType)
        {
            result = false;
        }
        else
        {
            result = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .All(f => IsUnmanaged((Type)f.FieldType));
        }
        
        CachedTypes.Add(type, result);
        return result;
    }
    
    public static bool IsNumber(this object value)
    {
        return value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal;
    }

    public static byte[] Serialize(this object obj)
    {
        var emitter = new BinaryEmitter(obj);
        return emitter.Emit();
    }
}
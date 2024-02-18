using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Radon.CodeAnalysis.Emit;
using Radon.CodeAnalysis.Emit.Binary;
using AssemblyFlags = Radon.CodeAnalysis.Emit.Binary.AssemblyFlags;

namespace Radon.Common;

public sealed class BinaryParser
{
    private readonly MemoryStream _stream;
    private readonly long _key;
    private bool _decrypt;
    public BinaryParser(byte[] bytes, bool decrypt = false, long key = 0L)
    {
        _stream = new MemoryStream(bytes);
        _decrypt = decrypt;
        _key = key;
    }

    public object Parse(Type t)
    {
        return ReadObject(t);
    }

    private unsafe object ReadObject(Type type)
    {
        if (_stream.Position == _stream.Length)
        {
            const string msg = "Reached end of stream";
            Logger.Log(msg, LogLevel.Error);
            throw new Exception(msg);
        }

        if (type == typeof(Instruction))
        {
            var label = (int)ReadObject(typeof(int));
            var opCode = (OpCode)ReadObject(typeof(OpCode));
            var operand = opCode.NoOperandRequired() ? -1 : (int)ReadObject(typeof(int));
            return new Instruction(label, opCode, operand);
        }

        if (type == typeof(AssemblyHeader))
        {
            var previous = _decrypt;
            _decrypt = false;
            var magicNumber = (ulong)ReadObject(typeof(ulong));
            var guid = (Guid)ReadObject(typeof(Guid));
            var flags = (AssemblyFlags)ReadObject(typeof(AssemblyFlags));
            var encryptionKey = (long)ReadObject(typeof(long));
            var version = (double)ReadObject(typeof(double));
            _decrypt = previous;
            return new AssemblyHeader(magicNumber, guid, flags, encryptionKey, version);
        }
        
        if (type.IsUnmanaged())
        {
            if (type.IsEnum)
            {
                var enumType = Enum.GetUnderlyingType(type);
                var value = ReadObject(enumType);
                return Enum.ToObject(type, value);
            }

            // Get the size of the primitive type
            var size = Marshal.SizeOf(type);
            var bytes = new byte[size];
            var readLength = _stream.Read(bytes, 0, size);
            if (readLength != size)
            {
                var msg = $"Failed to read {size} bytes from stream";
                Logger.Log(msg, LogLevel.Error);
                throw new Exception(msg);
            }

            // Convert the bytes to the value type
            fixed (byte* ptr = bytes)
            {
                var valueType = Marshal.PtrToStructure(new IntPtr(ptr), type);
                if (valueType is null)
                {
                    var msg = $"Failed to convert bytes to {type.Name}";
                    Logger.Log(msg, LogLevel.Error);
                    throw new Exception(msg);
                }
                if (_decrypt)
                {
                    valueType = valueType.Decrypt<ValueType>(_key);
                }
                
                return valueType;
            }
        }

        if (type == typeof(string))
        {
            var length = (int)ReadObject(typeof(int));
            var bytes = new byte[length];
            var readLength = _stream.Read(bytes, 0, length);
            if (readLength != length)
            {
                var msg = $"Failed to read {length} bytes from stream";
                Logger.Log(msg, LogLevel.Error);
                throw new Exception(msg);
            }
            
            var str = Encoding.UTF8.GetString(bytes);
            if (_decrypt)
            {
                str = str.Decrypt(_key);
            }
            
            return str;
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            if (elementType is null)
            {
                var msg = $"Failed to get element type of array type {type.Name}";
                Logger.Log(msg, LogLevel.Error);
                throw new Exception(msg);
            }
            
            var length = (int)ReadObject(typeof(int));
            var array = Array.CreateInstance(elementType, length);
            for (var i = 0; i < length; i++)
            {
                array.SetValue(ReadObject(elementType), i);
            }
            
            return array;
        }

        // Create an instance of the type
        var instance = Activator.CreateInstance(type);
        if (instance is null)
        {
            var msg = $"Failed to create instance of type {type.Name}";
            Logger.Log(msg, LogLevel.Error);
            throw new Exception(msg);
        }
        
        // Get all non-static fields
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            var fieldType = field.FieldType;
            var fieldValue = ReadObject(fieldType);
            field.SetValue(instance, fieldValue);
        }

        return instance;
    }
}
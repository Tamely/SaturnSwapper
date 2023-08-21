using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Radon.CodeAnalysis.Emit.Binary;
using Radon.Common;

namespace Radon.CodeAnalysis.Emit;

internal sealed class BinaryEmitter
{
    private readonly object _obj;
    private readonly MemoryStream _stream;
    private readonly bool _encrypt;
    private readonly long _key;
    public BinaryEmitter(object obj, bool encrypt = false, long key = 0L)
    {
        _obj = obj;
        _stream = new MemoryStream();
        _encrypt = encrypt;
        _key = key;
    }

    public static byte[] Emit(object obj)
    {
        var emitter = new BinaryEmitter(obj);
        return emitter.Emit();
    }

    public byte[] Emit()
    {
        WriteObject(_obj);
        return _stream.ToArray();
    }

    private unsafe void WriteObject(object value)
    {
        var type = value.GetType();
        if (value is Instruction instruction)
        {
            var label = instruction.Label;
            var opCode = instruction.OpCode;
            WriteObject(label);
            WriteObject(opCode);
            if (!opCode.NoOperandRequired())
            {
                var operand = instruction.Operand;
                WriteObject(operand);
            }
            
            return;
        }
        
        if (value.IsUnmanaged())
        {
            if (type.IsEnum)
            {
                var enumValue = (Enum)value;
                var enumType = Enum.GetUnderlyingType(type);
                var convertedValue = Convert.ChangeType(enumValue, enumType);
                WriteObject(convertedValue);
                return;
            }
            
            var valueType = (ValueType)value;
            if (_encrypt)
            {
                valueType = valueType.Encrypt<ValueType>(_key);
            }
            
            // Get the size of the primitive type
            var size = Marshal.SizeOf(type);
            var bytes = stackalloc byte[size];
            Marshal.StructureToPtr(valueType, (nint)bytes, true);

            // Write the bytes to the stream
            _stream.Write(new ReadOnlySpan<byte>(bytes, size));
            return;
        }

        if (type == typeof(string))
        {
            var str = (string)value;
            if (_encrypt)
            {
                str = str.Encrypt(_key);
            }
            
            var bytes = Encoding.UTF8.GetBytes(str);
            var length = bytes.Length;
            WriteObject(length);
            _stream.Write(bytes, 0, length);
            return;
        }
        
        if (type.IsArray)
        {
            var array = (Array)value;
            var length = array.Length;
            WriteObject(length);
            foreach (var item in array)
            {
                WriteObject(item);
            }
            
            return;
        }
        
        // Get all non-static fields
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            var fieldValue = field.GetValue(value);
            if (fieldValue == null)
            {
                throw new Exception("Field value cannot be null.");
            }
            
            WriteObject(fieldValue);
        }
    }
}

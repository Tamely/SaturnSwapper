using System;
using System.Collections.Generic;
using System.Linq;
using Radon.CodeAnalysis.Emit;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedString : RuntimeObject
{
    public override int Size { get; }
    public override RuntimeType Type { get; }
    public override object Value { get; }
    
    public ManagedString(string value)
    {
        Size = value.Length;
        Type = ManagedRuntime.System.GetType("string");
        Value = value;
    }
    
    public override int ResolveSize()
    {
        return Size;
    }

    public override byte[] Serialize()
    {
        var bytes = new List<byte>();
        foreach (var character in GetValue<string>())
        {
            bytes.Add((byte)character);
        }
        
        bytes.Add(0);
        return bytes.ToArray();
    }

    public override IRuntimeObject? ComputeOperation(OpCode operation, IRuntimeObject? other)
    {
        if (operation == OpCode.Concat && other is ManagedString otherString)
        {
            return new ManagedString(GetValue<string>() + otherString.GetValue<string>());
        }
        
        return null;
    }

    public override unsafe IRuntimeObject? ConvertTo(RuntimeType type)
    {
        if (type.TypeInfo.IsNumeric)
        {
            var value = GetValue<string>();
            if (!Int128.TryParse(value, out var result))
            {
                return null;
            }

            var size = IRuntimeObject.CreateDefault(type).ResolveSize();
            var bytes = new byte[sizeof(Int128)];
            fixed (byte* ptr = bytes)
            {
                *(Int128*)ptr = result;
            }

            var numberBytes = new byte[size];
            for (var i = 0; i < size; i++)
            {
                numberBytes[i] = bytes[i];
            }

            var endOfBytes = new byte[bytes.Length - numberBytes.Length];
            for (var i = 0; i < endOfBytes.Length; i++)
            {
                endOfBytes[i] = bytes[i + numberBytes.Length];
            }
            
            // If any of the bytes are not 0, then the number is too big to fit in the type
            if (endOfBytes.Any(b => b != 0))
            {
                return null;
            }
            
            return IRuntimeObject.Deserialize(type, numberBytes);
        }
        
        return null;
    }
}
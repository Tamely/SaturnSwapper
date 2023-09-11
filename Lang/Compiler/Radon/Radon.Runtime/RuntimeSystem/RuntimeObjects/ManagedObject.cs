using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Disassembly;
using Radon.CodeAnalysis.Emit;
using Radon.Common;
using Radon.Runtime.Memory;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal class ManagedObject : RuntimeObject
{
    private readonly Dictionary<FieldInfo, RuntimeObject> _fields;
    public override RuntimeType Type { get; }
    public override int Size { get; }
    public override nuint Pointer { get; }
    public ImmutableArray<RuntimeObject> Fields => _fields.Values.ToImmutableArray();

    public ManagedObject(RuntimeType type, int size, nuint pointer)
    {
        Type = type;
        Size = size;
        Pointer = pointer;
        _fields = new Dictionary<FieldInfo, RuntimeObject>();
        var fields = type.TypeInfo.Fields;
        foreach (var field in fields)
        {
            var address = pointer + (nuint)field.Offset;
            var fieldType = ManagedRuntime.System.GetType(field.Type);
            _fields.Add(field, fieldType.CreateDefault(address));
        }
    }
    
    public void SetField(FieldInfo field, RuntimeObject value)
    {
        Logger.Log($"Setting field {field.Name}.", LogLevel.Info);
        if (!_fields.ContainsKey(field))
        {
            throw new InvalidOperationException("Field does not exist.");
        }
        
        _fields[field] = value;
        var address = Pointer + (nuint)field.Offset;
        MemoryUtils.Copy(value.Pointer, address, value.Size);
    }
    
    public RuntimeObject GetField(FieldInfo field)
    {
        Logger.Log($"Getting field {field.Name}.", LogLevel.Info);
        if (!_fields.ContainsKey(field))
        {
            throw new InvalidOperationException("Field does not exist.");
        }
        
        return _fields[field];
    }
    
    public override RuntimeObject ComputeOperation(OpCode operation, RuntimeObject? other, StackFrame stackFrame)
    {
        if (this is ManagedString str && other is ManagedString otherStr)
        {
            return ComputeStringOperation(operation, str, otherStr, stackFrame);
        }
        
        if (Type.TypeInfo.IsNumeric && other is { Type.TypeInfo.IsNumeric: true })
        {
            return Type.TypeInfo.IsFloatingPoint 
                ? ComputeFloatingPointOperation(operation, other, stackFrame) 
                : ComputeIntegerOperation(operation, other, stackFrame);
        }
        
        if (Type.TypeInfo.IsNumeric && other is null)
        {
            return Type.TypeInfo.IsFloatingPoint
                ? ComputeFloatingPointUnaryOperation(operation, stackFrame)
                : ComputeIntegerUnaryOperation(operation, stackFrame);
        }
        
        throw new InvalidOperationException($"Operation {operation} is not supported for {Type.TypeInfo.Fullname}");
    }

    public override RuntimeObject CopyTo(nuint address)
    {
        return new ManagedObject(Type, Size, address);
    }

    private RuntimeObject ComputeStringOperation(OpCode operation, ManagedString str, ManagedString other, StackFrame stackFrame)
    {
        string value;
        switch (operation)
        {
            case OpCode.Cnct:
            {
                value = str + other.ToString();
                break;
            }
            default:
            {
                throw new InvalidOperationException($"Operation {operation} is not supported for {Type.TypeInfo.Fullname}");
            }
        }
        
        var allocateString = stackFrame.AllocateString(value);
        return allocateString;
    }
    
    private unsafe RuntimeObject ComputeFloatingPointOperation(OpCode operation, RuntimeObject other, StackFrame stackFrame)
    {
        dynamic leftValue;
        dynamic rightValue;
        if (Type == ManagedRuntime.Float32)
        {
            leftValue = *(float*)Pointer;
        }
        else
        {
            leftValue = *(double*)Pointer;
        }
        
        if (other is ManagedObject otherValue)
        {
            if (otherValue.Type == ManagedRuntime.Int32)
            {
                rightValue = *(int*)otherValue.Pointer;
            }
            else if (otherValue.Type == ManagedRuntime.Float32)
            {
                rightValue = *(float*)otherValue.Pointer;
            }
            else
            {
                rightValue = *(double*)otherValue.Pointer;
            }
        }
        else
        {
            throw new InvalidOperationException($"Operation {operation} is not supported for {Type.TypeInfo.Fullname}");
        }

        dynamic result;
        switch (operation)
        {
            case OpCode.Add:
            {
                result = leftValue + rightValue;
                break;
            }
            case OpCode.Sub:
            {
                result = leftValue - rightValue;
                break;
            }
            case OpCode.Mul:
            {
                result = leftValue * rightValue;
                break;
            }
            case OpCode.Div:
            {
                result = leftValue / rightValue;
                break;
            }
            case OpCode.Mod:
            {
                result = leftValue % rightValue;
                break;
            }
            case OpCode.Or:
            {
                result = leftValue | rightValue;
                break;
            }
            case OpCode.And:
            {
                result = leftValue & rightValue;
                break;
            }
            case OpCode.Xor:
            {
                result = leftValue ^ rightValue;
                break;
            }
            case OpCode.Shl:
            {
                result = leftValue << rightValue;
                break;
            }
            case OpCode.Shr:
            {
                result = leftValue >> rightValue;
                break;
            }
            case OpCode.Ceq:
            {
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, leftValue == rightValue);
            }
            case OpCode.Cne:
            {
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, leftValue != rightValue);
            }
            case OpCode.Cgt:
            {
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, leftValue > rightValue);
            }
            case OpCode.Cge:
            {
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, leftValue >= rightValue);
            }
            case OpCode.Clt:
            {
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, leftValue < rightValue);
            }
            case OpCode.Cle:
            {
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, leftValue <= rightValue);
            }
            default:
            {
                throw new InvalidOperationException($"Operation {operation} is not supported for {Type.TypeInfo.Fullname}");
            }
        }
        
        var bytes = new byte[Size];
        fixed (byte* ptr = bytes)
        {
            if (Type == ManagedRuntime.Float32)
            {
                *(float*)ptr = result;
            }
            else
            {
                *(double*)ptr = result;
            }
        }
                
        var obj = stackFrame.AllocateConstant(Type, bytes);
        return obj;
    }

    private unsafe RuntimeObject ComputeIntegerOperation(OpCode operation, RuntimeObject other, StackFrame stackFrame)
    {
        var leftBytes = new byte[16];
        if (other.Size > Size)
        {
            throw new InvalidOperationException($"Size of {other.Type.TypeInfo.Fullname} is greater than {Type.TypeInfo.Fullname}");
        }
        
        var rightBytes = new byte[16];
        Int128 leftValue;
        Int128 rightValue;
        int shift;
        fixed (byte* leftPtr = leftBytes)
        {
            MemoryUtils.Copy(Pointer, (nuint)leftPtr, Size);
            leftValue = *(Int128*)leftPtr;
        }
        
        fixed (byte* rightPtr = rightBytes)
        {
            MemoryUtils.Copy(other.Pointer, (nuint)rightPtr, other.Size);
            rightValue = *(Int128*)rightPtr;
            shift = *(int*)rightPtr;
        }
        
        Int128 result;
        switch (operation)
        {
            case OpCode.Add:
            {
                result = leftValue + rightValue;
                break;
            }
            case OpCode.Sub:
            {
                result = leftValue - rightValue;
                break;
            }
            case OpCode.Mul:
            {
                result = leftValue * rightValue;
                break;
            }
            case OpCode.Div:
            {
                result = leftValue / rightValue;
                break;
            }
            case OpCode.Mod:
            {
                result = leftValue % rightValue;
                break;
            }
            case OpCode.Or:
            {
                result = leftValue | rightValue;
                break;
            }
            case OpCode.And:
            {
                result = leftValue & rightValue;
                break;
            }
            case OpCode.Xor:
            {
                result = leftValue ^ rightValue;
                break;
            }
            case OpCode.Shl:
            {
                result = leftValue << shift;
                break;
            }
            case OpCode.Shr:
            {
                result = leftValue >> shift;
                break;
            }
            case OpCode.Ceq:
            {
                var compare = leftValue == rightValue;
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, compare);
            }
            case OpCode.Cne:
            {
                var compare = leftValue != rightValue;
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, compare);
            }
            case OpCode.Cgt:
            {
                var compare = leftValue > rightValue;
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, compare);
            }
            case OpCode.Cge:
            {
                var compare = leftValue >= rightValue;
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, compare);
            }
            case OpCode.Clt:
            {
                var compare = leftValue < rightValue;
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, compare);
            }
            case OpCode.Cle:
            {
                var compare = leftValue <= rightValue;
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, compare);
            }
            default:
            {
                throw new InvalidOperationException($"Operation {operation} is not supported for {Type.TypeInfo.Fullname}");
            }
        }
        
        var bytes = new byte[Size];
        fixed (byte* ptr = bytes)
        {
            var resultPtr = &result;
            MemoryUtils.Copy((nuint)resultPtr, (nuint)ptr, Size);
        }
        
        var obj = stackFrame.AllocateConstant(Type, bytes);
        return obj;
    }

    [Obsolete("This method doesn't work")]
    private unsafe RuntimeObject ComputeIntegerOperationBitwise(OpCode operation, RuntimeObject other, StackFrame stackFrame)
    {
        var leftBytes = new byte[Size];
        if (other.Size > Size)
        {
            throw new InvalidOperationException($"Size of {other.Type.TypeInfo.Fullname} is greater than {Type.TypeInfo.Fullname}");
        }
        
        var rightBytes = new byte[Size];
        fixed (byte* leftPtr = leftBytes)
        {
            MemoryUtils.Copy(Pointer, (nuint)leftPtr, Size);
        }
        
        fixed (byte* rightPtr = rightBytes)
        {
            MemoryUtils.Copy(other.Pointer, (nuint)rightPtr, other.Size);
        }

        byte[] result;
        switch (operation)
        {
            case OpCode.Add:
            {
                result = BitwiseMath.BitwiseAdd(leftBytes, rightBytes);
                break;
            }
            case OpCode.Sub:
            {
                result = BitwiseMath.BitwiseSubtract(leftBytes, rightBytes);
                break;
            }
            case OpCode.Mul:
            {
                result = BitwiseMath.BitwiseMultiply(leftBytes, rightBytes);
                break;
            }
            case OpCode.Div:
            {
                result = BitwiseMath.BitwiseDivide(leftBytes, rightBytes);
                break;
            }
            case OpCode.Mod:
            {
                result = BitwiseMath.BitwiseModulo(leftBytes, rightBytes);
                break;
            }
            case OpCode.Or:
            {
                result = BitwiseMath.BitwiseOr(leftBytes, rightBytes);
                break;
            }
            case OpCode.And:
            {
                result = BitwiseMath.BitwiseAnd(leftBytes, rightBytes);
                break;
            }
            case OpCode.Xor:
            {
                result = BitwiseMath.BitwiseXor(leftBytes, rightBytes);
                break;
            }
            case OpCode.Shl:
            {
                result = BitwiseMath.BitwiseShiftLeft(leftBytes, rightBytes);
                break;
            }
            case OpCode.Shr:
            {
                result = BitwiseMath.BitwiseShiftRight(leftBytes, rightBytes);
                break;
            }
            case OpCode.Ceq:
            {
                var compare = BitwiseMath.Compare(leftBytes, rightBytes);
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, compare == 0);
            }
            case OpCode.Cne:
            {
                var compare = BitwiseMath.Compare(leftBytes, rightBytes);
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, compare != 0);
            }
            case OpCode.Cgt:
            {
                var compare = BitwiseMath.Compare(leftBytes, rightBytes);
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, compare > 0);
            }
            case OpCode.Cge:
            {
                var compare = BitwiseMath.Compare(leftBytes, rightBytes);
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, compare >= 0);
            }
            case OpCode.Clt:
            {
                var compare = BitwiseMath.Compare(leftBytes, rightBytes);
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, compare < 0);
            }
            case OpCode.Cle:
            {
                var compare = BitwiseMath.Compare(leftBytes, rightBytes);
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, compare <= 0);
            }
            default:
            {
                throw new InvalidOperationException($"Operation {operation} is not supported for {Type.TypeInfo.Fullname}");
            }
        }
        
        var obj = stackFrame.AllocateConstant(Type, result);
        return obj;
    }

    private unsafe RuntimeObject ComputeFloatingPointUnaryOperation(OpCode operation, StackFrame stackFrame)
    {
        dynamic value;
        if (Type == ManagedRuntime.Float32)
        {
            value = *(float*)Pointer;
        }
        else
        {
            value = *(double*)Pointer;
        }

        dynamic result;
        switch (operation)
        {
            case OpCode.Neg:
            {
                result = -value;
                break;
            }
            default:
            {
                throw new InvalidOperationException($"Operation {operation} is not supported for {Type.TypeInfo.Fullname}");
            }
        }
        
        var bytes = new byte[Size];
        fixed (byte* ptr = bytes)
        {
            if (Type == ManagedRuntime.Float32)
            {
                *(float*)ptr = result;
            }
            else
            {
                *(double*)ptr = result;
            }
        }
        
        var obj = stackFrame.AllocateConstant(Type, bytes);
        return obj;
    }

    private RuntimeObject ComputeIntegerUnaryOperation(OpCode operation, StackFrame stackFrame)
    {
        var bytes = new byte[Size];
        byte[] result;
        switch (operation)
        {
            case OpCode.Neg:
            {
                result = BitwiseMath.BitwiseNegate(bytes);
                break;
            }
            default:
            {
                throw new InvalidOperationException(
                    $"Operation {operation} is not supported for {Type.TypeInfo.Fullname}");
            }
        }

        var obj = stackFrame.AllocateConstant(Type, result);
        return obj;
    }

    public override unsafe string ToString()
    {
        var bytes = new byte[Size];
        fixed (byte* ptr = bytes)
        {
            var bytesPointer = new nuint(ptr);
            MemoryUtils.Copy(Pointer, bytesPointer, Size);
        }
        
        var value = BitConverter.ToString(bytes);
        return $"Type: {Type.TypeInfo.Fullname} Value: {value}";
    }
}
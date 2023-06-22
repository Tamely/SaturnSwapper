using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Radon.CodeAnalysis.Emit;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;
using Radon.Runtime.RuntimeInfo;
using Radon.Runtime.Utilities;

namespace Radon.Runtime.RuntimeSystem.Objects;

internal interface IObject
{
    public static IObject Null = new NullObject();
    public RuntimeType Type { get; }
}

internal readonly record struct NullObject(RuntimeType Type) : IObject;

internal sealed class ManagedObject : IObject
{
    private readonly object _object;
    private readonly byte[] _value;
    private readonly Dictionary<FieldInfo, ManagedObject> _fields; 
    public RuntimeType Type { get; }

    public ManagedObject(RuntimeType type, object value)
    {
        Type = type;
        _object = value;
        if (type.TypeInfo.Size == -1)
        {
            throw new Exception("Cannot create a managed object of a type with an unknown size.");
        }
        
        var emitter = new BinaryEmitter(value);
        var bytes = emitter.Emit();
        if (bytes.Length != type.TypeInfo.Size)
        {
            throw new Exception("The size of the value does not match the size of the type.");
        }
        
        _fields = new Dictionary<FieldInfo, ManagedObject>();
        foreach (var field in Type.TypeInfo.Fields)
        {
            if (field.Name == "list")
            {
                throw new Exception("Cannot create a managed object of a type with a list field.");
            }
            
            var fieldOffset = field.Offset;
            var fieldSize = field.Type.Size;
            var fieldBytes = bytes[fieldOffset..(fieldOffset + fieldSize)];
            var fieldRuntimeType = ManagedRuntime.System.GetType(field.Type);
            var fieldObject = new ManagedObject(fieldRuntimeType, fieldBytes);
            _fields.Add(field, fieldObject);
        }
        
        _value = bytes;
    }

    public ManagedObject(RuntimeType type)
        : this(type, new byte[type.TypeInfo.Size])
    {
    }

    public T GetValue<T>()
    {
        var binaryParser = new BinaryParser(_value);
        try
        {
            return (T) binaryParser.Parse(typeof(T));
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to parse value of type {Type.TypeInfo.Name}", e);
        }
    }

    public IObject InvokeMethod(AssemblyInfo assembly, MethodInfo method, MemberReference reference, ImmutableArray<IObject> arguments)
    {
        var methodRuntime = new MethodRuntime(assembly, this, method, arguments);
        return methodRuntime.Invoke();
    }

    public IObject Convert(RuntimeType type)
    {
        if (Type == type)
        {
            return this;
        }
        
        if (Type.TypeInfo.Size > type.TypeInfo.Size)
        {
            throw new Exception("Cannot convert a type to a smaller type.");
        }

        // I am implementing IEEE 754, so until it's fully implemented, we can't convert between floating point types.
        if (Type.TypeInfo.IsFloatingPoint &&
            !type.TypeInfo.IsFloatingPoint)
        {
            throw new Exception("Cannot convert a floating point type to a non-floating point type.");
        }
        
        if (!Type.TypeInfo.IsFloatingPoint &&
            type.TypeInfo.IsFloatingPoint)
        {
            throw new Exception("Cannot convert a non-floating point type to a floating point type.");
        }

        var bytes = new byte[type.TypeInfo.Size];
        Array.Copy(_value, bytes, _value.Length);
        return new ManagedObject(type, bytes);
    }
    
    public IObject GetField(MemberReference reference, AssemblyInfo assembly)
    {
        if (!assembly.MemberReferences.TryGetValue(reference, out var member))
        {
            throw new Exception("The member reference does not exist in the assembly.");
        }
        
        if (member.MemberDefinition is not FieldInfo field)
        {
            throw new Exception("The member reference is not a field.");
        }
        
        if (!_fields.TryGetValue(field, out var value))
        {
            throw new Exception("The field does not exist in the object.");
        }
        
        return value;
    }
    
    public unsafe void SetField(MemberReference reference, IObject value, AssemblyInfo assembly)
    {
        if (!assembly.MemberReferences.TryGetValue(reference, out var member))
        {
            throw new Exception("The member reference does not exist in the assembly.");
        }
        
        if (member.MemberDefinition is not FieldInfo field)
        {
            throw new Exception("The member reference is not a field.");
        }
        
        if (field.Type != value.Type.TypeInfo)
        {
            throw new Exception("The type of the value does not match the type of the field.");
        }
        
        if (!_fields.ContainsKey(field))
        {
            throw new Exception("The field does not exist in the object.");
        }

        if (value.Type.TypeInfo.Size != field.Type.Size)
        {
            throw new Exception("The size of the value does not match the size of the field.");
        }
        
        if (value is not ManagedObject managedObject)
        {
            throw new Exception("The value must be a managed object.");
        }
        
        _fields[field] = (ManagedObject)value;
        fixed (byte* ptr = _value)
        {
            var fieldPtr = ptr + field.Offset;
            var managedValue = managedObject._value;
            for (var i = 0; i < managedValue.Length; i++)
            {
                fieldPtr[i] = managedValue[i];
            }
        }
    }

    public IObject ComputeBinaryOperation(OperationType operation, IObject other)
    {
        if (other is not ManagedObject otherObject)
        {
            throw new ArgumentException("The other object must be a managed object.", nameof(other));
        }
        
        var thisTypeInfo = Type.TypeInfo;
        var otherTypeInfo = other.Type.TypeInfo;
        if (!thisTypeInfo.IsNumeric || !otherTypeInfo.IsNumeric)
        {
            throw new ArgumentException("The other object must be a numeric type.", nameof(other));
        }
        
        if (thisTypeInfo.Size != otherTypeInfo.Size)
        {
            throw new ArgumentException("The other object must be the same size as this object.", nameof(other));
        }
        
        if (thisTypeInfo.IsSigned != otherTypeInfo.IsSigned)
        {
            throw new ArgumentException("The other object must have the same sign as this object.", nameof(other));
        }

        if (thisTypeInfo.IsFloatingPoint && otherTypeInfo.IsFloatingPoint)
        {
            // I would do the same bitwise operations as the normal non-floating point types, however, that requires more effort than it's worth.
            // I might implement it in the future, but for now, I'm just going to use the built-in operators.
            dynamic thisValue;
            dynamic otherValue;
            if (thisTypeInfo.Name == "float")
            {
                thisValue = GetValue<float>();
                otherValue = otherObject.GetValue<float>();
            }
            else
            {
                thisValue = GetValue<double>();
                otherValue = otherObject.GetValue<double>();
            }

            object result = operation switch
            {
                OperationType.Add => thisValue + otherValue,
                OperationType.Subtract => thisValue - otherValue,
                OperationType.Multiply => thisValue * otherValue,
                OperationType.Divide => thisValue / otherValue,
                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
            };

            return new ManagedObject(Type, result);
        }

        // You might be wondering why I'm not using the built-in operators for the non-floating point types.
        // The reason is because I think it's more fun to implement the operations myself.
        switch (operation)
        {
            case OperationType.Add:
                return BitwiseAdd(otherObject);
            case OperationType.Subtract:
                return BitwiseSubtract(otherObject);
            case OperationType.Multiply:
                return BitwiseMultiply(otherObject);
            case OperationType.Divide:
                return BitwiseDivide(otherObject);
            case OperationType.Concatenate:
                return Concatenate(otherObject);
            default:
                throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
        }
    }

    private IObject BitwiseAdd(ManagedObject other)
    {
        var result = new byte[_value.Length];
        var carry = 0;
        for (var i = 0; i < _value.Length; i++)
        {
            var sum = _value[i] + other._value[i] + carry;
            result[i] = (byte)(sum % 256);
            carry = sum / 256;
        }
        
        return new ManagedObject(Type, result);
    }
    
    private IObject BitwiseSubtract(ManagedObject other)
    {
        var result = BitwiseSubtract(_value, other._value);
        return new ManagedObject(Type, result);
    }

    private byte[] BitwiseSubtract(byte[] value, byte[] other)
    {
        var result = new byte[value.Length];
        var borrow = 0;
        for (var i = 0; i < value.Length; i++)
        {
            var diff = value[i] - other[i] - borrow;
            if (diff < 0)
            {
                diff += 256;
                borrow = 1;
            }
            else
            {
                borrow = 0;
            }

            result[i] = (byte)diff;
        }
        
        return result;
    }
    
    private IObject BitwiseMultiply(ManagedObject other)
    {
        var result = new byte[_value.Length];
        for (var i = 0; i < _value.Length; i++)
        {
            var carry = 0;
            for (var j = 0; j < _value.Length; j++)
            {
                var product = _value[i] * other._value[j] + result[i + j] + carry;
                result[i + j] = (byte)(product % 256);
                carry = product / 256;
            }
            
            result[i] += (byte)carry;
        }
        
        return new ManagedObject(Type, result);
    }

    private IObject BitwiseDivide(ManagedObject other)
    {
        var (quotient, _) = BitwiseDivide(_value, other._value);
        return new ManagedObject(Type, quotient);
    }
    
    // ReSharper disable once UnusedTupleComponentInReturnValue
    private (byte[] quotient, byte[] remainder) BitwiseDivide(byte[] value, byte[] other) // Remainder will be used for modulus
    {
        var quotient = new byte[value.Length];
        var remainder = new byte[value.Length];
        Array.Copy(value, remainder, value.Length);
        
        var divisor = new byte[value.Length];
        Array.Copy(other, divisor, other.Length);
        
        var divisorShift = MostSignificantBit(value) - MostSignificantBit(other);
        if (divisorShift < 0)
        {
            return (quotient, remainder);
        }
        
        divisor = ShiftLeft(divisor, divisorShift);
        for (var i = divisorShift; i >= 0; i--)
        {
            if (Compare(remainder, divisor) >= 0)
            {
                quotient[i / 8] |= (byte)(1 << (i % 8));
                remainder = BitwiseSubtract(remainder, divisor);
            }
            
            divisor = ShiftRight(divisor, 1);
        }
        
        return (quotient, remainder);
    }
    
    private static int MostSignificantBit(byte[] bytes)
    {
        for (var i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] == 0)
            {
                continue;
            }
            
            for (var j = 7; j >= 0; j--)
            {
                if ((bytes[i] & (1 << j)) != 0)
                {
                    return (bytes.Length - i - 1) * 8 + j;
                }
            }
        }
        
        return -1;
    }
    
    private static int Compare(byte[] a, byte[] b)
    {
        for (var i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
            {
                return a[i] < b[i] ? -1 : 1;
            }
        }
        
        return 0;
    }
    
    private static byte[] ShiftLeft(byte[] a, int shift)
    {
        var result = new byte[a.Length];
        var byteShift = shift / 8;
        var bitShift = shift % 8;
        for (var i = 0; i < a.Length - byteShift - 1; i++)
        {
            result[i] = (byte)((a[i + byteShift] << bitShift) | (a[i + byteShift + 1] >> (8 - bitShift)));
        }
        
        result[a.Length - byteShift - 1] = (byte)(a[^1] << bitShift);
        return result;
    }
    
    private static byte[] ShiftRight(byte[] a, int shift)
    {
        var result = new byte[a.Length];
        var byteShift = shift / 8;
        var bitShift = shift % 8;
        for (var i = a.Length - 1; i > byteShift; i--)
        {
            result[i] = (byte)((a[i - byteShift] >> bitShift) | (a[i - byteShift - 1] << (8 - bitShift)));
        }
        
        result[byteShift] = (byte)(a[0] >> bitShift);
        return result;
    }
    
    private IObject Concatenate(ManagedObject other)
    {
        var str = string.Concat(_object, other._object);
        return new ManagedObject(Type, str);
    }
    
    public override string ToString()
    {
        // 0x12345678
        var sb = new StringBuilder();
        sb.Append("0x");
        for (var i = _value.Length - 1; i >= 0; i--)
        {
            sb.Append(_value[i].ToString("X2"));
        }
        
        return sb.ToString();
    }
}

internal enum OperationType
{
    Add = OpCode.Add,
    Subtract = OpCode.Sub,
    Multiply = OpCode.Mul,
    Divide = OpCode.Div,
    Concatenate = OpCode.Concat
}

internal sealed record RuntimeType
{
    private readonly Dictionary<FieldInfo, IObject> _staticFields;
    private readonly Dictionary<EnumMemberInfo, IObject> _enumValues;
    private bool _initialized;
    
    public TypeInfo TypeInfo { get; }

    public RuntimeType(TypeInfo typeInfo)
    {
        TypeInfo = typeInfo;
        _staticFields = new Dictionary<FieldInfo, IObject>();
        _initialized = false;
        var staticFields = typeInfo.Fields.Where(x => x.IsStatic);
        foreach (var field in staticFields)
        {
            _staticFields.Add(field, IObject.Null);
        }
        
        _enumValues = new Dictionary<EnumMemberInfo, IObject>();
        if (typeInfo.IsEnum)
        {
            var enumValues = typeInfo.EnumMembers;
            foreach (var enumValue in enumValues)
            {
                _enumValues.Add(enumValue, IObject.Null);
            }
        }
    }

    public void Initialize()
    {
        foreach (var enumMember in _enumValues.Keys)
        {
            _enumValues[enumMember] = new ManagedObject(ManagedRuntime.System.GetType(enumMember.Type), enumMember.Value);
        }
    }

    private void StaticInitialization()
    {
        _initialized = true;
        var staticConstructor = TypeInfo.StaticConstructor;
        if (staticConstructor is null)
        {
            Logger.Log("No static constructor found.", LogLevel.Warning);
            return;
        }

        var runtime = new MethodRuntime(ManagedRuntime.System.AssemblyInfo, IObject.Null, staticConstructor, ImmutableArray<IObject>.Empty);
        runtime.Invoke();
    }

    public IObject GetStaticField(MemberReference reference)
    {
        if (reference.MemberType != MemberType.Field)
        {
            throw new ArgumentException("Member reference must be a field.", nameof(reference));
        }
        
        if (!_initialized)
        {
            StaticInitialization();
        }
        
        var field = TypeInfo.GetByRef<FieldInfo>(MemberType.Field, reference);
        return _staticFields[field];
    }
    
    public void SetStaticField(MemberReference reference, IObject value)
    {
        if (reference.MemberType != MemberType.Field)
        {
            throw new ArgumentException("Member reference must be a field.", nameof(reference));
        }
        
        if (!_initialized)
        {
            StaticInitialization();
        }
        
        var field = TypeInfo.GetByRef<FieldInfo>(MemberType.Field, reference);
        _staticFields[field] = value;
    }
    
    public IObject GetEnumValue(MemberReference reference)
    {
        if (reference.MemberType != MemberType.EnumMember)
        {
            throw new ArgumentException("Member reference must be an enum member.", nameof(reference));
        }
        
        if (!_initialized)
        {
            StaticInitialization();
        }
        
        var enumMember = TypeInfo.GetByRef<EnumMemberInfo>(MemberType.EnumMember, reference);
        return _enumValues[enumMember];
    }

    public ManagedObject CreateInstance(AssemblyInfo assembly, MemberReferenceInfo constructorReference, ImmutableArray<IObject> arguments)
    {
        if (constructorReference.MemberDefinition is not MethodInfo constructor)
        {
            throw new ArgumentException("Member reference must be a constructor.", nameof(constructorReference));
        }
        
        if (constructorReference.MemberType != MemberType.Method)
        {
            throw new ArgumentException("Member reference must be a method.", nameof(constructorReference));
        }
        
        if (!_initialized)
        {
            StaticInitialization();
        }
        
        var managedObject = new ManagedObject(this);
        var runtime = new MethodRuntime(assembly, managedObject, constructor, arguments);
        runtime.Invoke();
        return managedObject;
    }

    public IObject InvokeStaticMethod(AssemblyInfo assembly, MethodInfo method, ImmutableArray<IObject> arguments)
    {
        if (!method.IsStatic)
        {
            throw new ArgumentException("Method must be static.", nameof(method));
        }
        
        if (!_initialized)
        {
            StaticInitialization();
        }
        
        var methodRuntime = new MethodRuntime(assembly, IObject.Null, method, arguments);
        return methodRuntime.Invoke();
    }
}

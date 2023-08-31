using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.CodeAnalysis.Disassembly;

internal sealed record TypeInfo
{
    private readonly Dictionary<EnumMember, EnumMemberInfo> _enumMembers;
    private readonly Dictionary<Field, FieldInfo> _fields;
    private readonly Dictionary<Method, MethodInfo> _methods;
    private readonly Dictionary<Method, MethodInfo> _constructors;
    private readonly Metadata _metadata;
    public bool IsRuntimeType { get; }
    public bool IsPublic { get; }
    public bool IsPrivate { get; }
    public bool IsEntry { get; }
    public bool IsReferenceType { get; }
    public bool IsValueType { get; }
    public bool IsPointer { get; }
    public bool IsStruct { get; }
    public bool IsEnum { get; }
    public bool IsArray { get; }
    public bool IsPrimitive { get; }
    public bool IsNumeric { get; }
    public bool IsSigned { get; }
    public bool IsFloatingPoint { get; }
    public int Size { get; }

    public int Dimension
    {
        get
        {
            if (!IsArray)
            {
                throw new InvalidOperationException("Cannot get dimension of non-array type.");
            }
            
            var dimension = 0;
            var type = this;
            while (type.IsArray)
            {
                dimension++;
                type = type.UnderlyingType!;
            }
            
            return dimension;
        }
    }
    
    public string Name { get; }
    public string Fullname => ToString();
    public TypeInfo? UnderlyingType { get; }
    public ImmutableArray<EnumMemberInfo> EnumMembers { get; }
    public ImmutableArray<FieldInfo> Fields { get; }
    public ImmutableArray<MethodInfo> Methods { get; }
    public ImmutableArray<MethodInfo> Constructors { get; }
    public MethodInfo? StaticConstructor { get; }
    public TypeDefinition Definition { get; }
    public TypeInfo(TypeDefinition type, Metadata metadata)
    {
        IsRuntimeType = type.Flags.HasFlag(BindingFlags.RuntimeInternal);
        IsPublic = type.Flags.HasFlag(BindingFlags.Public);
        IsPrivate = type.Flags.HasFlag(BindingFlags.NonPublic);
        IsEntry = type.Flags.HasFlag(BindingFlags.Entry);
        IsReferenceType = type.Flags.HasFlag(BindingFlags.Ref);
        IsValueType = type.Kind.HasFlag(TypeKind.ValueType);
        IsPointer = type.Kind.HasFlag(TypeKind.Pointer);
        IsStruct = type.Kind.HasFlag(TypeKind.Struct);
        IsEnum = type.Kind.HasFlag(TypeKind.Enum);
        IsArray = type.Kind.HasFlag(TypeKind.Array);
        IsPrimitive = type.Kind.HasFlag(TypeKind.Primitive);
        IsNumeric = type.Kind.HasFlag(TypeKind.Numeric);
        IsSigned = type.Kind.HasFlag(TypeKind.Signed);
        IsFloatingPoint = type.Kind.HasFlag(TypeKind.FloatingPoint);
        Size = type.Size;
        Name = metadata.Strings.Strings[type.Name];
        if (type.UnderlyingType != -1)
        {
            var underlyingType = metadata.Types.Types[type.UnderlyingType];
            UnderlyingType = TypeTracker.Add(underlyingType, metadata, this);
        }
        
        Definition = type;
        _enumMembers = new Dictionary<EnumMember, EnumMemberInfo>();
        _fields = new Dictionary<Field, FieldInfo>();
        _methods = new Dictionary<Method, MethodInfo>();
        _constructors = new Dictionary<Method, MethodInfo>();
        _metadata = metadata;
        var enumMembers = ImmutableArray.CreateBuilder<EnumMemberInfo>();
        var enumMemberCount = type.EnumMemberCount;
        var firstEnumMember = type.EnumMemberStartOffset;
        for (var i = 0; i < enumMemberCount; i++)
        {
            var enumMember = metadata.EnumMembers.Members[firstEnumMember + i];
            var info = new EnumMemberInfo(enumMember, metadata, this);
            enumMembers.Add(info);
            _enumMembers.Add(enumMember, info);
        }
        
        var fields = ImmutableArray.CreateBuilder<FieldInfo>();
        var fieldCount = type.FieldCount;
        var firstField = type.FieldStartOffset;
        for (var i = 0; i < fieldCount; i++)
        {
            var field = metadata.Fields.Fields[firstField + i];
            var info = new FieldInfo(field, metadata, this);
            fields.Add(info);
            _fields.Add(field, info);
        }
        
        var methods = ImmutableArray.CreateBuilder<MethodInfo>();
        var methodCount = type.MethodCount;
        var firstMethod = type.MethodStartOffset;
        for (var i = 0; i < methodCount; i++)
        {
            var method = metadata.Methods.Methods[firstMethod + i];
            var info = new MethodInfo(method, metadata, this);
            methods.Add(info);
            _methods.Add(method, info);
        }

        var constructors = ImmutableArray.CreateBuilder<MethodInfo>();
        var constructorCount = type.ConstructorCount;
        var firstConstructor = type.ConstructorStartOffset;
        for (var i = 0; i < constructorCount; i++)
        {
            var constructor = metadata.Methods.Methods[firstConstructor + i];
            var info = new MethodInfo(constructor, metadata, this);
            constructors.Add(info);
            _constructors.Add(constructor, info);
        }
        
        var staticConstructor = type.StaticConstructor;
        if (staticConstructor != -1)
        {
            var staticConstructorDefinition = metadata.Methods.Methods[staticConstructor];
            var staticConstructorInfo = new MethodInfo(staticConstructorDefinition, metadata, this);
            StaticConstructor = staticConstructorInfo;
        }
        
        EnumMembers = enumMembers.ToImmutable();
        Fields = fields.ToImmutable();
        Methods = methods.ToImmutable();
        Constructors = constructors.ToImmutable();
    }
    
    public T GetByRef<T>(MemberType type, MemberReference reference, bool checkType = true)
        where T : IMemberInfo
    {
        try
        {
            if (type != reference.MemberType)
            {
                throw new ArgumentException("Member reference type does not match the type of the member.", nameof(reference));
            }
            
            switch (type)
            {
                case MemberType.Field:
                    if (typeof(T) != typeof(FieldInfo) && checkType)
                    {
                        throw new ArgumentException("Type parameter must be FieldInfo.", nameof(T));
                    }
                    
                    var field = _metadata.Fields.Fields[reference.MemberDefinition];
                    if (!_fields.TryGetValue(field, out var fieldInfo))
                    {
                        throw new InvalidOperationException("Field not found.");
                    }
                    
                    return (T)(object)fieldInfo;
                case MemberType.Method:
                    if (typeof(T) != typeof(MethodInfo) && checkType)
                    {
                        throw new ArgumentException("Type parameter must be MethodInfo.", nameof(T));
                    }
                    
                    var method = _metadata.Methods.Methods[reference.MemberDefinition];
                    if (!_methods.TryGetValue(method, out var methodInfo))
                    {
                        throw new InvalidOperationException("Method not found.");
                    }
                    
                    return (T)(object)methodInfo;
                case MemberType.Constructor:
                    if (typeof(T) != typeof(MethodInfo) && checkType)
                    {
                        throw new ArgumentException("Type parameter must be MethodInfo.", nameof(T));
                    }
                    
                    var constructor = _metadata.Methods.Methods[reference.MemberDefinition];
                    if (!_constructors.TryGetValue(constructor, out var constructorInfo))
                    {
                        throw new InvalidOperationException("Constructor not found.");
                    }
                    
                    return (T)(object)constructorInfo;
                case MemberType.EnumMember:
                    if (typeof(T) != typeof(EnumMemberInfo) && checkType)
                    {
                        throw new ArgumentException("Type parameter must be EnumMemberInfo.", nameof(T));
                    }
                    
                    var enumMember = _metadata.EnumMembers.Members[reference.MemberDefinition];
                    if (!_enumMembers.TryGetValue(enumMember, out var enumMemberInfo))
                    {
                        throw new InvalidOperationException("Enum member not found.");
                    }
                    
                    return (T)(object)enumMemberInfo;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to get member '{type}' by reference", e);
        }
    }
    
    public override string ToString()
    {
        return Name;
    }
}
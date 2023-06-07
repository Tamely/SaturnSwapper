using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Conversions;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

public abstract class TypeSymbol : Symbol
{
    private static readonly ImmutableArray<MemberSymbol> EmptyMembers = ImmutableArray<MemberSymbol>.Empty;
    private static readonly ImmutableArray<TypeParameterSymbol> EmptyTypeParameters = ImmutableArray<TypeParameterSymbol>.Empty;
    private static readonly ImmutableArray<SyntaxKind> Mods = ImmutableArray.Create(SyntaxKind.RuntimeInternalKeyword);

    public static readonly TypeSymbol Error = new StructSymbol("?", -1, EmptyMembers, null, Mods);
    public static readonly TypeSymbol Void = new StructSymbol("void", 0, EmptyMembers, null, Mods);
    public static readonly TypeSymbol Bool = new StructSymbol("bool", 1, EmptyMembers, null, Mods);
    public static readonly TypeSymbol Byte = new StructSymbol("byte", 1, EmptyMembers, null, Mods);
    public static readonly TypeSymbol SByte = new StructSymbol("sbyte", 1, EmptyMembers, null, Mods);
    public static readonly TypeSymbol Short = new StructSymbol("short", 2, EmptyMembers, null, Mods);
    public static readonly TypeSymbol UShort = new StructSymbol("ushort", 2, EmptyMembers, null, Mods);
    public static readonly TypeSymbol Int = new StructSymbol("int", 4, EmptyMembers, null, Mods);
    public static readonly TypeSymbol UInt = new StructSymbol("uint", 4, EmptyMembers, null, Mods);
    public static readonly TypeSymbol Long = new StructSymbol("long", 8, EmptyMembers, null, Mods);
    public static readonly TypeSymbol ULong = new StructSymbol("ulong", 8, EmptyMembers, null, Mods);
    public static readonly TypeSymbol Float = new StructSymbol("float", 4, EmptyMembers, null, Mods);
    public static readonly TypeSymbol Double = new StructSymbol("double", 8, EmptyMembers, null, Mods);
    public static readonly TypeSymbol Char = new StructSymbol("char", 1, EmptyMembers, null, Mods);
    public static readonly TypeSymbol String = new StructSymbol("string", -1, EmptyMembers, null, Mods);
    public static readonly TypeSymbol Archive = new StructSymbol("archive", -1, EmptyMembers, null, Mods);
    public static readonly EnumSymbol SeekOrigin = new("seekorigin", EmptyMembers, null, Mods);
    public static readonly TypeSymbol List = new StructSymbol("list", EmptyMembers, null, Mods);
    public static readonly TypeSymbol System = new StructSymbol("system", EmptyMembers, null, Mods);

    static TypeSymbol()
    {
        SeekOrigin.AddEnumMember("Begin", 0);
        SeekOrigin.AddEnumMember("Current", 1);
        SeekOrigin.AddEnumMember("End", 2);
        
        var typeParameterBuilder = new TypeParameterBuilder();
        {
            MethodSymbol writeMethod;
            {
                var t = typeParameterBuilder.AddTypeParameter("T");
                var parameters = ImmutableArray.Create(new ParameterSymbol("value", t, 0));
                writeMethod = new MethodSymbol(Archive, "Write", Void, 
                    typeParameterBuilder.Build(), parameters, Mods);
            }

            MethodSymbol readMethod;
            {
                var t = typeParameterBuilder.AddTypeParameter("T");
                var parameters = ImmutableArray<ParameterSymbol>.Empty;
                readMethod = new MethodSymbol(Archive, "Read", t, 
                    typeParameterBuilder.Build(), parameters, Mods);
            }

            MethodSymbol seekMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("offset", Int, 0),
                    new ParameterSymbol("origin", SeekOrigin, 1));
                seekMethod = new MethodSymbol(Archive, "Seek", Void, EmptyTypeParameters, parameters, Mods);
            }

            MethodSymbol seekStrMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("str", String, 0));
                seekStrMethod = new MethodSymbol(Archive, "Seek", Void, EmptyTypeParameters, parameters, Mods);
            }

            MethodSymbol swapMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("other", Archive, 0));
                swapMethod = new MethodSymbol(Archive, "Swap", Void, EmptyTypeParameters, parameters, Mods);
            }

            MethodSymbol importMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("path", String, 0));
                importMethod = new MethodSymbol(Archive, "Import", Void, EmptyTypeParameters, parameters, Mods);
            }
            
            Archive.AddMember(writeMethod);
            Archive.AddMember(readMethod);
            Archive.AddMember(seekMethod);
            Archive.AddMember(seekStrMethod);
            Archive.AddMember(swapMethod);
            Archive.AddMember(importMethod);
        }
        {
            typeParameterBuilder.Build(); // Clear type parameters
            var typeParameter = typeParameterBuilder.AddTypeParameter("T");
            List = List.WithTypeParameters(typeParameterBuilder.Build());
            MethodSymbol addMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("item", typeParameter, 0));
                addMethod = new MethodSymbol(List, "Add", Void, EmptyTypeParameters, parameters, Mods);
            }
            
            MethodSymbol addRangeMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("items", List.WithTypeParameters(typeParameter), 0));
                addRangeMethod = new MethodSymbol(List, "AddRange", Void, EmptyTypeParameters, parameters, Mods);
            }
            
            MethodSymbol removeMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("item", typeParameter, 0));
                removeMethod = new MethodSymbol(List, "Remove", Void, EmptyTypeParameters, parameters, Mods);
            }
            
            MethodSymbol removeAtMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("index", Int, 0));
                removeAtMethod = new MethodSymbol(List, "RemoveAt", Void, EmptyTypeParameters, parameters, Mods);
            }
            
            MethodSymbol clearMethod;
            {
                var parameters = ImmutableArray<ParameterSymbol>.Empty;
                clearMethod = new MethodSymbol(List, "Clear", Void, EmptyTypeParameters, parameters, Mods);
            }
            
            MethodSymbol containsMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("item", typeParameter, 0));
                containsMethod = new MethodSymbol(List, "Contains", Bool, EmptyTypeParameters, parameters, Mods);
            }
            
            MethodSymbol lengthMethod;
            {
                var parameters = ImmutableArray<ParameterSymbol>.Empty;
                lengthMethod = new MethodSymbol(List, "Length", Int, EmptyTypeParameters, parameters, Mods);
            }
            
            ConstructorSymbol constructor;
            {
                var parameters = ImmutableArray<ParameterSymbol>.Empty;
                constructor = new ConstructorSymbol(List, parameters, Mods);
            }
            
            List.AddMember(addMethod);
            List.AddMember(addRangeMethod);
            List.AddMember(removeMethod);
            List.AddMember(removeAtMethod);
            List.AddMember(clearMethod);
            List.AddMember(containsMethod);
            List.AddMember(lengthMethod);
            List.AddMember(constructor);
        }
        {
            MethodSymbol printMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("value", String, 0));
                printMethod = new MethodSymbol(System, "Print", Void, EmptyTypeParameters, parameters, Mods);
            }
            
            MethodSymbol downloadMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("url", String, 0),
                    new ParameterSymbol("path", String, 1));
                downloadMethod = new MethodSymbol(System, "Download", Void, EmptyTypeParameters, parameters, Mods);
            }
            
            System.AddMember(printMethod);
            System.AddMember(downloadMethod);
            System.WithModifer(SyntaxKind.StaticKeyword);
        }
    }
    
    public static bool operator ==(TypeSymbol? left, TypeSymbol? right)
    {
        if (left is null)
        {
            return right is null;
        }
        
        return left.Equals(right);
    }
    
    public static bool operator !=(TypeSymbol? left, TypeSymbol? right)
    {
        return !(left == right);
    }

    internal void AddMember(MemberSymbol member)
    {
        if (member.ParentType != this)
        {
            member = member.WithParentType(this);
        }
        
        Members = Members.Add(member);
    }

    public MemberSymbol? GetMember(string name)
    {
        foreach (var member in Members)
        {
            if (member.Name == name)
            {
                return member;
            }
        }

        return null;
    }
    
    internal bool TryLookupMethod<TMethodSymbol>(string name, ImmutableArray<BoundExpression> arguments, 
        out bool methodNotFound, out bool cannotConvertType, out bool ambiguousCall, out TypeSymbol? from, 
        out TypeSymbol? to, out ImmutableArray<TMethodSymbol> ambiguousCalls, out TMethodSymbol methodSymbol) 
        where TMethodSymbol : AbstractMethodSymbol
    {
        methodNotFound = false;
        cannotConvertType = false;
        ambiguousCall = false;
        from = null;
        to = null;
        ambiguousCalls = ImmutableArray<TMethodSymbol>.Empty;
        methodSymbol = null!;
        var possibleCandidates = new List<(TMethodSymbol, TypeSymbol From, TypeSymbol To)>();
        var ambiguousCandidates = new List<TMethodSymbol>();
        foreach (var member in Members)
        {
            if (member is TMethodSymbol method &&
                method.Name == name)
            {
                if (method.Parameters.Length != arguments.Length)
                {
                    goto failed;
                }
                
                for (var i = 0; i < method.Parameters.Length; i++)
                {
                    var parameterType = method.Parameters[i].Type;
                    var argument = arguments[i];
                    var conversion = Conversion.Classify(argument, parameterType);
                    if (!(conversion.Exists && 
                        (conversion.IsIdentity ||
                         conversion.IsImplicit)))
                    {
                        possibleCandidates.Add((method, argument.Type, parameterType));
                        goto failed;
                    }
                }
                
                ambiguousCandidates.Add(method); // If this is called twice, there are at least two methods that fit
                                                 // the call, making it ambiguous.
                failed:;
            }
        }

        if (possibleCandidates.Count == 0 &&
            ambiguousCandidates.Count == 0)
        {
            methodNotFound = true;
        }
        else
        {
            if (ambiguousCandidates.Count == 1 &&
                possibleCandidates.Count == 0)
            {
                methodSymbol = ambiguousCandidates[0];
            }
            else if (ambiguousCandidates.Count > 1 ||
                     possibleCandidates.Count > 1)
            {
                ambiguousCall = true;
                foreach (var (method, _, _) in possibleCandidates)
                {
                    ambiguousCandidates.Add(method);
                }
                
                ambiguousCalls = ambiguousCandidates.ToImmutableArray();
            }
            else if (possibleCandidates.Count == 1)
            {
                var (_, fromType, toType) = possibleCandidates[0];
                from = fromType;
                to = toType;
                cannotConvertType = true;
            }
        }
        
        return !methodNotFound && !ambiguousCall && !cannotConvertType;
    }
    
    public TypeSymbol WithMembers(ImmutableArray<MemberSymbol> members)
    {
        return this switch
        {
            StructSymbol s => new StructSymbol(s.Name, members, s.ParentAssembly, s.Modifiers),
            EnumSymbol e => new EnumSymbol(e.Name, members, e.ParentAssembly, e.Modifiers),
            _ => throw new InvalidOperationException("Cannot add members to this type.")
        };
    }

    public TypeSymbol WithTypeParameters(ImmutableArray<TypeParameterSymbol> typeParameters)
    {
        if (this is StructSymbol s)
        {
            return new StructSymbol(s.Name, typeParameters, s.Members, s.ParentAssembly, s.Modifiers);
        }
        
        throw new InvalidOperationException("Cannot add type parameters to this type.");
    }
    
    public TypeSymbol WithTypeParameters(params TypeParameterSymbol[] typeParameters)
    { 
        return WithTypeParameters(typeParameters.ToImmutableArray());
    }
    
    public TypeSymbol WithModifer(SyntaxKind modifier)
    {
        return this switch
        {
            StructSymbol s => new StructSymbol(s.Name, s.TypeParameters, s.Members, s.ParentAssembly, s.Modifiers.Add(modifier)),
            EnumSymbol e => new EnumSymbol(e.Name, e.Members, e.ParentAssembly, e.Modifiers.Add(modifier)),
            _ => throw new InvalidOperationException("Cannot add modifiers to this type.")
        };
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not TypeSymbol other)
        {
            return false;
        }

        return Name == other.Name; // We only need to do this for now,
        // because we don't have generics for types, and we don't have away to include external assemblies.
    }
    
    public override int GetHashCode()
    {
        try
        {
            return HashCode.Combine(Name, ParentAssembly);
        }
        catch (Exception)
        {
            return Name.GetHashCode();
        }
    }

    public static ImmutableArray<TypeSymbol> GetPrimitiveTypes()
    {
        return ImmutableArray.Create(
            Void, Bool, Byte, SByte, Short, UShort, Int, UInt, Long, ULong, Float, Double, Char, String, Archive, SeekOrigin, List);
    }
    
    public static ImmutableArray<TypeSymbol> GetNumericTypes()
    {
        return ImmutableArray.Create(
            Byte, SByte, Short, UShort, Int, UInt, Long, ULong, Float, Double);
    }
    
    public static ImmutableArray<TypeSymbol> GetSignedNumericTypes()
    {
        return ImmutableArray.Create(SByte, Short, Int, Long);
    }
    
    public static ImmutableArray<TypeSymbol> GetFloatingPointTypes()
    {
        return ImmutableArray.Create(Float, Double);
    }

    public abstract ImmutableArray<MemberSymbol> Members { get; private protected set; }
    public abstract AssemblySymbol? ParentAssembly { get; }
    public abstract int Size { get; }
    public sealed override bool HasType => true;
    public sealed override TypeSymbol Type => this;
}
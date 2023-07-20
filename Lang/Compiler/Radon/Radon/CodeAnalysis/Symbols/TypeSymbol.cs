using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Binding;
using Radon.CodeAnalysis.Binding.Analyzers;
using Radon.CodeAnalysis.Binding.Semantics.Conversions;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Syntax;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Symbols;

public abstract class TypeSymbol : Symbol
{
    private static readonly ImmutableArray<MemberSymbol> EmptyMembers = ImmutableArray<MemberSymbol>.Empty;
    private static readonly ImmutableArray<SyntaxKind> Mods = ImmutableArray.Create(SyntaxKind.PublicKeyword, SyntaxKind.RuntimeInternalKeyword);

    public static readonly StructSymbol Error = new("?", -1, EmptyMembers, null, Mods);
    public static readonly StructSymbol Void = new("void", 0, EmptyMembers, null, Mods);
    public static readonly StructSymbol Bool = new("bool", 1, EmptyMembers, null, Mods);
    public static readonly StructSymbol SByte = new("sbyte", 1, EmptyMembers, null, Mods);
    public static readonly StructSymbol Byte = new("byte", 1, EmptyMembers, null, Mods);
    public static readonly StructSymbol Short = new("short", 2, EmptyMembers, null, Mods);
    public static readonly StructSymbol UShort = new("ushort", 2, EmptyMembers, null, Mods);
    public static readonly StructSymbol Int = new("int", 4, EmptyMembers, null, Mods);
    public static readonly StructSymbol UInt = new("uint", 4, EmptyMembers, null, Mods);
    public static readonly StructSymbol Long = new("long", 8, EmptyMembers, null, Mods);
    public static readonly StructSymbol ULong = new("ulong", 8, EmptyMembers, null, Mods);
    public static readonly StructSymbol Float = new("float", 4, EmptyMembers, null, Mods);
    public static readonly StructSymbol Double = new("double", 8, EmptyMembers, null, Mods);
    public static readonly StructSymbol Char = new("char", 1, EmptyMembers, null, Mods);
    public static readonly StructSymbol String = new("string", -1, EmptyMembers, null, Mods);
    public static readonly StructSymbol Archive = new("archive", -1, EmptyMembers, null, Mods);
    public static readonly EnumSymbol SeekOrigin = new("seekorigin", EmptyMembers, null, Mods);
    public static readonly TemplateSymbol List;
    public static readonly StructSymbol System = new("system", 0, EmptyMembers, null, Mods);

    static TypeSymbol()
    {
        SeekOrigin.AddEnumMember("Begin", 0);
        SeekOrigin.AddEnumMember("Current", 1);
        SeekOrigin.AddEnumMember("End", 2);
        
        var typeParameterBuilder = new TypeParameterBuilder();
        {
            TemplateMethodSymbol writeMethod;
            {
                var t = typeParameterBuilder.AddTypeParameter("T");
                var parameters = ImmutableArray.Create(new ParameterSymbol("value", t, 0));
                writeMethod = new(Archive, "Write", Void, parameters, Mods, typeParameterBuilder.Build());
            }

            TemplateMethodSymbol readMethod;
            {
                var t = typeParameterBuilder.AddTypeParameter("T");
                var parameters = ImmutableArray<ParameterSymbol>.Empty;
                readMethod = new(Archive, "Read", t, parameters, Mods, typeParameterBuilder.Build());
            }

            MethodSymbol seekMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("offset", Int, 0),
                    new ParameterSymbol("origin", SeekOrigin, 1));
                seekMethod = new MethodSymbol(Archive, "Seek", Void, parameters, Mods);
            }

            MethodSymbol seekStrMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("str", String, 0));
                seekStrMethod = new MethodSymbol(Archive, "Seek", Void, parameters, Mods);
            }

            MethodSymbol swapMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("other", Archive, 0));
                swapMethod = new MethodSymbol(Archive, "Swap", Void, parameters, Mods);
            }

            MethodSymbol importMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("path", String, 0));
                importMethod = new MethodSymbol(Archive, "Import", Void, parameters, Mods);
            }

            Archive.AddMember(writeMethod);
            Archive.AddMember(readMethod);
            Archive.AddMember(seekMethod);
            Archive.AddMember(seekStrMethod);
            Archive.AddMember(swapMethod);
            Archive.AddMember(importMethod);
        }
        {
            var t = typeParameterBuilder.AddTypeParameter("T");
            List = new TemplateSymbol("list", EmptyMembers, null, Mods, typeParameterBuilder.Build());

            MethodSymbol addMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("value", t, 0));
                addMethod = new MethodSymbol(List, "Add", Void, parameters, Mods);
            }

            MethodSymbol addRangeMethod;
            {
                var listT = new PrimitiveTemplateSymbol(List, ImmutableArray.Create<TypeSymbol>(t));
                var parameters = ImmutableArray.Create(new ParameterSymbol("items", listT, 0));
                addRangeMethod = new MethodSymbol(List, "AddRange", Void, parameters, Mods);
            }

            MethodSymbol removeMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("item", t, 0));
                removeMethod = new MethodSymbol(List, "Remove", Void, parameters, Mods);
            }

            MethodSymbol removeAtMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("index", Int, 0));
                removeAtMethod = new MethodSymbol(List, "RemoveAt", Void, parameters, Mods);
            }

            MethodSymbol clearMethod;
            {
                var parameters = ImmutableArray<ParameterSymbol>.Empty;
                clearMethod = new MethodSymbol(List, "Clear", Void, parameters, Mods);
            }

            MethodSymbol containsMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("item", t, 0));
                containsMethod = new MethodSymbol(List, "Contains", Bool, parameters, Mods);
            }

            MethodSymbol lengthMethod;
            {
                var parameters = ImmutableArray<ParameterSymbol>.Empty;
                lengthMethod = new MethodSymbol(List, "Length", Int, parameters, Mods);
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
                printMethod = new MethodSymbol(System, "Print", Void, parameters, Mods);
            }

            MethodSymbol downloadMethod;
            {
                var parameters = ImmutableArray.Create(new ParameterSymbol("url", String, 0),
                    new ParameterSymbol("path", String, 1));
                downloadMethod = new MethodSymbol(System, "Download", Void, parameters, Mods);
            }

            System.AddMember(printMethod);
            System.AddMember(downloadMethod);
            System = (StructSymbol)System.WithModifer(SyntaxKind.StaticKeyword);
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
        
        if (Members.Contains(member))
        {
            return;
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
    
    public bool TryGetMember(string name, out MemberSymbol? member)
    {
        member = GetMember(name);
        return member is not null;
    }
    
    internal bool TryLookupMethod<TMethodSymbol>(Binder binder, string name, ImmutableArray<TypeSymbol> typeArguments,
        ImmutableArray<BoundExpression> arguments, SyntaxNode callSite, out bool methodNotFound, 
        out ImmutableArray<TMethodSymbol> ambiguousCalls, out TMethodSymbol methodSymbol) 
        where TMethodSymbol : AbstractMethodSymbol
    {
        methodNotFound = false;
        ambiguousCalls = ImmutableArray<TMethodSymbol>.Empty;
        methodSymbol = null!;
        var possibleCandidates = new List<(TMethodSymbol Method, TypeSymbol From, TypeSymbol To)>();
        var ambiguousCandidates = new List<TMethodSymbol>();
        foreach (var member in Members)
        {
            if (member is TMethodSymbol method &&
                method.Name == name)
            {
                var parameterTypes = new List<TypeSymbol>();
                for (var i = 0; i < method.Parameters.Length; i++)
                {
                    var parameter = method.Parameters[i];
                    parameterTypes.Add(parameter.Type);
                }

                if (method is TemplateMethodSymbol templateMethod)
                {
                    if (templateMethod.TypeParameters.Length != typeArguments.Length)
                    {
                        binder.Diagnostics.ReportIncorrectNumberOfTypeArguments(callSite.Location, name, templateMethod.TypeParameters.Length, typeArguments.Length);
                        goto failed;
                    }

                    if (typeof(TMethodSymbol).IsAssignableTo(typeof(MethodSymbol)) ||
                        typeof(TMethodSymbol).IsAssignableTo(typeof(AbstractMethodSymbol)))
                    {
                        var typeBinder = templateMethod.ParentType.TypeBinder;
                        if (typeBinder is null)
                        {
                            goto failed;
                        }
                        
                        method = (TMethodSymbol)(object)typeBinder.BuildTemplateMethod(templateMethod, typeArguments, callSite);
                        for (var i = 0; i < method.Parameters.Length; i++)
                        {
                            var parameter = method.Parameters[i];
                            parameterTypes[i] = parameter.Type;
                        }
                    }
                }
                
                if (method.Parameters.Length != arguments.Length)
                {
                    goto failed;
                }
                
                for (var i = 0; i < method.Parameters.Length; i++)
                {
                    var parameterType = parameterTypes[i];
                    var argument = arguments[i];
                    var conversion = Conversion.Classify(binder, argument, parameterType);
                    if (!(conversion.Exists && 
                        (conversion.IsIdentity ||
                         conversion.IsImplicit)))
                    {
                        binder.Diagnostics.ReportCannotConvert(argument.Syntax.Location, argument.Type, parameterType);
                        possibleCandidates.Add((method, argument.Type, parameterType));
                        ambiguousCandidates.Add(method);
                        goto failed;
                    }
                }
                
                ambiguousCandidates.Add(method); // If this is called twice, there are at least two methods that fit
                                                 // the call, making it ambiguous.
                failed:;
            }
        }

        var ambiguousCall = false;
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
        }
        
        return !methodNotFound && !ambiguousCall;
    }
    
    public TypeSymbol WithMembers(ImmutableArray<MemberSymbol> members)
    {
        return this switch
        {
            StructSymbol s => new StructSymbol(s.Name, members, s.ParentAssembly, s.Modifiers),
            EnumSymbol e => new EnumSymbol(e.Name, members, e.ParentAssembly, e.Modifiers),
            TemplateSymbol t => new TemplateSymbol(t.Name, members, t.ParentAssembly, t.Modifiers, t.TypeParameters),
            _ => throw new InvalidOperationException("Cannot add members to this type.")
        };
    }
    
    public TypeSymbol WithModifer(SyntaxKind modifier)
    {
        return this switch
        {
            StructSymbol s => new StructSymbol(s.Name, s.Members, s.ParentAssembly, s.Modifiers.Add(modifier)),
            EnumSymbol e => new EnumSymbol(e.Name, e.Members, e.ParentAssembly, e.Modifiers.Add(modifier)),
            TemplateSymbol t => new TemplateSymbol(t.Name, t.Members, t.ParentAssembly, t.Modifiers.Add(modifier), t.TypeParameters),
            _ => throw new InvalidOperationException("Cannot add modifiers to this type.")
        };
    }
    
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(obj, this))
        {
            return true;
        }
        
        if (obj is not TypeSymbol other)
        {
            return false;
        }

        if (other is TemplateSymbol template &&
            this is TemplateSymbol template1)
        {
            return Name == other.Name &&
                   template.TypeParameters.Length == template1.TypeParameters.Length;
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
        return ImmutableArray.Create<TypeSymbol>(
            Void, Bool, SByte, Byte, Short, UShort, Int, UInt, Long, ULong, Float, Double, Char, String, Archive, SeekOrigin, List, System);
    }
    
    public static ImmutableArray<TypeSymbol> GetNumericTypes()
    {
        return ImmutableArray.Create<TypeSymbol>(
            SByte, Byte, Short, UShort, Int, UInt, Long, ULong, Float, Double);
    }
    
    public static ImmutableArray<TypeSymbol> GetSignedNumericTypes()
    {
        return ImmutableArray.Create<TypeSymbol>(SByte, Short, Int, Long);
    }
    
    public static ImmutableArray<TypeSymbol> GetFloatingPointTypes()
    {
        return ImmutableArray.Create<TypeSymbol>(Float, Double);
    }

    public abstract ImmutableArray<MemberSymbol> Members { get; private protected set; }
    public abstract AssemblySymbol? ParentAssembly { get; }
    public abstract int Size { get; internal set; }
    internal abstract TypeBinder? TypeBinder { get; set; }
    internal TemplateBinder? TemplateBinder { get; set; } // This is only used for templates.
    public sealed override bool HasType => true;
    public sealed override TypeSymbol Type => this;
}
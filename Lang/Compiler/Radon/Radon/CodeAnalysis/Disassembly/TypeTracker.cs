using System.Collections.Generic;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.CodeAnalysis.Disassembly;

internal static class TypeTracker
{
    private static readonly Dictionary<TypeDefinition, TypeInfo> Types = new();
    
    public static TypeInfo Add(TypeDefinition typeDefinition, Metadata metadata, TypeInfo? parent)
    {
        if (parent != null &&
            parent.Definition == typeDefinition)
        {
            return parent;
        }
        
        if (Types.TryGetValue(typeDefinition, out var type))
        {
            return type;
        }
        
        type = new TypeInfo(typeDefinition, metadata);
        Types.Add(typeDefinition, type);
        return type;
    }
    
    public static TypeInfo GetTypeInfo(TypeDefinition typeDefinition)
    {
        // It has to exist, otherwise we wouldn't be able to get here.
        return Types[typeDefinition];
    }
}
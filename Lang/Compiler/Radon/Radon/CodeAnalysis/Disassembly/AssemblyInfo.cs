using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Emit.Binary;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.CodeAnalysis.Disassembly;

internal sealed record AssemblyInfo
{
    public Guid Guid { get; }
    public long EncryptionKey { get; }
    public double Version { get; }
    public Instruction[] Instructions { get; }
    public ImmutableArray<TypeInfo> Types { get; }
    public Metadata Metadata { get; }
    public ImmutableDictionary<TypeReference, TypeReferenceInfo> TypeReferences { get; }
    public ImmutableDictionary<MemberReference, MemberReferenceInfo> MemberReferences { get; }
    public AssemblyInfo(Assembly assembly)
    {
        var metadata = assembly.Metadata;
        Guid = assembly.Guid;
        EncryptionKey = assembly.EncryptionKey;
        Version = assembly.Version;
        Instructions = assembly.Instructions.Instructions;
        var types = ImmutableArray.CreateBuilder<TypeInfo>();
        foreach (var type in metadata.Types.Types)
        {
            types.Add(TypeTracker.Add(type, metadata, null));
        }
        
        var typeReferences = ImmutableDictionary.CreateBuilder<TypeReference, TypeReferenceInfo>();
        foreach (var typeReference in metadata.TypeReferences.TypeReferences)
        {
            typeReferences.Add(typeReference, new TypeReferenceInfo(typeReference, metadata));
        }
        
        var memberReferences = ImmutableDictionary.CreateBuilder<MemberReference, MemberReferenceInfo>();
        foreach (var memberReference in metadata.MemberReferences.MemberReferences)
        {
            memberReferences.Add(memberReference, new MemberReferenceInfo(memberReference, metadata));
        }
        
        Types = types.ToImmutable();
        TypeReferences = typeReferences.ToImmutable();
        MemberReferences = memberReferences.ToImmutable();
        Metadata = metadata;
    }

    public override string ToString()
    {
        return $"AssemblyInfo: {Guid}";
    }
}
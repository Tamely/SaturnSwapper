using System;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.CodeAnalysis.Disassembly;

internal sealed record FieldInfo : IMemberInfo
{
    public bool IsStatic { get; }
    public string Name { get; }
    public string Fullname => ToString();
    public TypeInfo Type { get; }
    public TypeInfo Parent { get; }
    public int Offset { get; }
    public Field Definition { get; }
    public FieldInfo(Field field, Metadata metadata, TypeInfo parent)
    {
        IsStatic = field.BindingFlags.HasFlag(BindingFlags.Static);
        Name = metadata.Strings.Strings[field.Name];
        Type = TypeTracker.Add(metadata.Types.Types[field.Type], metadata, parent);
        Parent = TypeTracker.Add(metadata.Types.Types[field.Parent], metadata, parent);
        Offset = field.Offset;
        if (Parent != parent)
        {
            throw new InvalidOperationException("Parent type does not match the parent type of the field.");
        }
        
        Definition = field;
    }
    
    public override string ToString()
    {
        return $"{Parent}.{Name}";
    }
}
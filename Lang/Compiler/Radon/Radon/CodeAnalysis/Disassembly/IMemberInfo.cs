namespace Radon.CodeAnalysis.Disassembly;

public interface IMemberInfo
{
    public string Name { get; }
    public string Fullname { get; }
    public TypeInfo Parent { get; }
    public TypeInfo Type { get; }
}
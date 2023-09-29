using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct Method(BindingFlags Flags, int Name, int ReturnType, int Parent, int ParameterCount, int FirstParameter, 
    int LocalCount, int FirstLocal, int InstructionCount, int FirstInstruction)
{
    public readonly BindingFlags Flags = Flags;
    public readonly int Name = Name;
    public readonly int ReturnType = ReturnType;
    public readonly int Parent = Parent;
    public readonly int ParameterCount = ParameterCount;
    public readonly int FirstParameter = FirstParameter;
    public readonly int LocalCount = LocalCount;
    public readonly int FirstLocal = FirstLocal;
    public readonly int InstructionCount = InstructionCount;
    public readonly int FirstInstruction = FirstInstruction;
}
using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct Method
{
    public readonly BindingFlags Flags;
    public readonly int Name;
    public readonly int ReturnType;
    public readonly int Parent;
    public readonly int ParameterCount;
    public readonly int FirstParameter;
    public readonly int LocalCount;
    public readonly int FirstLocal;
    public readonly int InstructionCount;
    public readonly int FirstInstruction;
    
    public Method(BindingFlags flags, int name, int returnType, int parent, int parameterCount, int firstParameter, 
        int localCount, int firstLocal, int instructionCount, int firstInstruction)
    {
        Flags = flags;
        Name = name;
        ReturnType = returnType;
        Parent = parent;
        ParameterCount = parameterCount;
        FirstParameter = firstParameter;
        LocalCount = localCount;
        FirstLocal = firstLocal;
        InstructionCount = instructionCount;
        FirstInstruction = firstInstruction;
    }
}
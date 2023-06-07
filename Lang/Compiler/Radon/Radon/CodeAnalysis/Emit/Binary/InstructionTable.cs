using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct InstructionTable
{
    public readonly Instruction[] Instructions;
    
    public InstructionTable(Instruction[] instructions)
    {
        Instructions = instructions;
    }
}
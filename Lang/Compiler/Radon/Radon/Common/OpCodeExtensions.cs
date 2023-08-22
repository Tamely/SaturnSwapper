using Radon.CodeAnalysis.Emit;

namespace Radon.Common;

public static class OpCodeExtensions
{
    public static bool NoOperandRequired(this OpCode opCode)
    {
        // I understand that this method is unoptimized, but I'm doing it like this to preserve the order of the opcodes
        if (opCode <= OpCode.Shr)
        {
            return true;
        }
        
        if (opCode is >= OpCode.Ldthis and <= OpCode.Stelem)
        {
            return true;
        }
        
        if (opCode is OpCode.Ret)
        {
            return true;
        }
        
        if (opCode is >= OpCode.Ceq and <= OpCode.Cle)
        {
            return true;
        }

        if (opCode is OpCode.Ldlen)
        {
            return true;
        }
        
        return false;
    }
}
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Radon.CodeAnalysis.Emit.Binary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct Instruction
{
    public readonly int Label;
    public readonly OpCode OpCode;
    public readonly int Operand;
    
    public Instruction(int label, OpCode opCode, int operand)
    {
        Label = label;
        OpCode = opCode;
        Operand = operand;
    }

    public override string ToString()
    {
        // Opcode Operand: OpCodeBytes OperandBytes
        var sb = new StringBuilder();
        sb.Append(OpCode);
        sb.Append(' ');
        sb.Append(Operand);
        
        sb.Append(" : ");
        var opcodeByte = (ushort)OpCode;
        var bytes = BitConverter.GetBytes(opcodeByte);
        sb.Append(bytes[0].ToString("X2"));
        sb.Append(' ');
        sb.Append(bytes[1].ToString("X2"));
        sb.Append(' ');
        if (Operand != -1)
        {
            var operandBytes = BitConverter.GetBytes(Operand);
            foreach (var b in operandBytes)
            {
                sb.Append(b.ToString("X2"));
                sb.Append(' ');
            }
        }
        
        return sb.ToString();
    }
}
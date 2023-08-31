using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Radon.CodeAnalysis.Disassembly;
using Radon.CodeAnalysis.Emit;
using Radon.CodeAnalysis.Emit.Binary;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;
using Radon.Common;

namespace ILViewer;

// ReSharper disable once InconsistentNaming
internal sealed class ILWriter
{
    private const string Indentation = "    ";
    private readonly StringBuilder _sb;
    private readonly List<ILToken> _tokens;
    private readonly AssemblyInfo _assembly;
    private readonly Metadata _metadata;
    private int _indentLevel;
    private bool _isFirstTokenOfLine = true;
    private MethodInfo? _currentMethod;
    
    public ImmutableArray<ILToken> Tokens => _tokens.ToImmutableArray();

    public ILWriter(AssemblyInfo assembly)
    {
        _sb = new StringBuilder();
        _tokens = new List<ILToken>();
        _assembly = assembly;
        _metadata = assembly.Metadata;
        BuildAssembly();
    }

    private void BuildAssembly()
    {
        foreach (var type in _assembly.Types)
        {
            WriteType(type);
        }
    }

    private void WriteType(TypeInfo type)
    {
        if (type.IsArray || type.IsPointer)
        {
            return;
        }
        
        WriteBindingFlags(type.Definition.Flags);
        if (type.IsPrimitive)
        {
            WriteToken("primitive", ILTokenKind.Keyword, true);
        }
        if (type.IsSigned)
        {
            WriteToken("signed", ILTokenKind.Keyword, true);
        }
        if (type.IsNumeric)
        {
            WriteToken("numeric", ILTokenKind.Keyword, true);
        }
        if (type.IsFloatingPoint)
        {
            WriteToken("floating_point", ILTokenKind.Keyword, true);
        }
        if (type.IsReferenceType)
        {
            WriteToken("ref", ILTokenKind.Keyword, true);
        }
        if (type.IsStruct)
        {
            WriteToken("struct", ILTokenKind.Keyword, true);
        }
        if (type.IsEnum)
        {
            WriteToken("enum", ILTokenKind.Keyword, true);
        }

        WriteToken(type.Fullname, ILTokenKind.TypeIdentifier, false);
        WriteNewLine();
        if (type.UnderlyingType != null)
        {
            WriteToken(Indentation, ILTokenKind.Trivia, false);
            WriteToken("underlying_type", ILTokenKind.Keyword, true);
            WriteToken(type.UnderlyingType.Fullname, ILTokenKind.TypeIdentifier, false);
            WriteNewLine();
        }
        
        WriteToken("{", ILTokenKind.Punctuation, false);
        WriteNewLine();
        Indent();
        foreach (var enumMember in type.EnumMembers)
        {
            WriteEnumMember(enumMember, type.UnderlyingType!);
            WriteNewLine();
        }
        
        foreach (var field in type.Fields)
        {
            WriteField(field);
            WriteNewLine();
        }
        
        if (type.StaticConstructor != null)
        {
            WriteMethod(type.StaticConstructor);
            WriteNewLine();
        }

        foreach (var constructor in type.Constructors)
        {
            WriteMethod(constructor);
            WriteNewLine();
        }

        for (var i = 0; i < type.Methods.Length; i++)
        {
            var method = type.Methods[i];
            WriteMethod(method);
            if (i != type.Methods.Length - 1)
            {
                WriteNewLine();
            }
        }

        Unindent();
        WriteToken("}", ILTokenKind.Punctuation, false);
        WriteNewLine();
        WriteNewLine();
    }
    
    private void WriteEnumMember(EnumMemberInfo enumMember, TypeInfo underlyingType)
    {
        WriteToken(".field", ILTokenKind.DirectiveKeyword, true);
        WriteBindingFlags(enumMember.Definition.MemberBindingFlags);
        WriteToken(underlyingType.Fullname, ILTokenKind.TypeIdentifier, true);
        WriteToken(enumMember.Name, ILTokenKind.EnumMemberIdentifier, true);
        WriteToken("=", ILTokenKind.Punctuation, true);
        WriteToken("{", ILTokenKind.Punctuation, true);
        foreach (var b in enumMember.Value)
        {
            WriteToken(b.ToString(), ILTokenKind.Number, true);
        }
        
        WriteToken("}", ILTokenKind.Punctuation, false);
    }
    
    private void WriteField(FieldInfo field)
    {
        WriteToken(".field", ILTokenKind.DirectiveKeyword, true);
        WriteBindingFlags(field.Definition.BindingFlags);
        WriteToken(field.Type.Fullname, ILTokenKind.TypeIdentifier, true);
        WriteToken(field.Name, ILTokenKind.FieldIdentifier, true);
    }

    private void WriteMethod(MethodInfo method)
    {
        WriteToken(".method", ILTokenKind.DirectiveKeyword, true);
        WriteBindingFlags(method.Definition.Flags);
        WriteToken(method.Type.Fullname, ILTokenKind.TypeIdentifier, true);
        WriteToken(method.Name, ILTokenKind.MethodIdentifier, false);
        WriteToken("(", ILTokenKind.Punctuation, false);
        for (var i = 0; i < method.Parameters.Count; i++)
        {
            var parameter = method.Parameters.ValueAt(i);
            WriteToken(parameter.Type.Fullname, ILTokenKind.TypeIdentifier, true);
            WriteToken(parameter.Name, ILTokenKind.VariableIdentifier, false);
            if (i != method.Parameters.Count - 1)
            {
                WriteToken(",", ILTokenKind.Punctuation, true);
            }
        }
        
        WriteToken(")", ILTokenKind.Punctuation, false);
        WriteNewLine();
        WriteToken("{", ILTokenKind.Punctuation, false);
        WriteNewLine();
        Indent();
        WriteToken(".locals", ILTokenKind.DirectiveKeyword, true);
        WriteToken("init", ILTokenKind.Keyword, true);
        WriteToken("(", ILTokenKind.Punctuation, false);
        if (method.Locals.Count > 0)
        {
            WriteNewLine();
            Indent();
            for (var i = 0; i < method.Locals.Count; i++)
            {
                var local = method.Locals.ValueAt(i);
                WriteLocal(local, i);
                if (i != method.Locals.Count - 1)
                {
                    WriteToken(",", ILTokenKind.Punctuation, true);
                }
                
                WriteNewLine();
            }
            
            Unindent();
        }

        WriteToken(")", ILTokenKind.Punctuation, false);
        if (method.InstructionCount != 0)
        {
            WriteNewLine();    
        }
        
        _currentMethod = method;
        var instructions = _assembly.Instructions;
        var loopStack = new Stack<Tuple<int, int>>();
        var skip = 0;
        for (var i = method.FirstInstruction; i < method.FirstInstruction + method.InstructionCount; i++)
        {
            var instruction = instructions[i];
            Tuple<int, int, int>? loop = null;
            if (skip == 0)
            {
                loop = ResolveLoop(instruction, instructions, i);
            }
            
            if (loop != null)
            {
                skip = loop.Item3;
                loopStack.Push(new Tuple<int, int>(loop.Item1, loop.Item2));
            }
            
            if (loopStack.Count > 0 &&
                i == loopStack.Peek().Item1)
            {
                _indentLevel++;
            }
            
            if (loopStack.Count > 0 &&
                i == loopStack.Peek().Item2)
            {
                _indentLevel--;
                loopStack.Pop();
            }
            
            WriteInstruction(instruction);
            if (i != method.FirstInstruction + method.InstructionCount - 1)
            {
                WriteNewLine();
            }
            
            if (skip > 0)
            {
                skip--;
            }
        }
        
        Unindent();
        WriteNewLine();
        WriteToken("}", ILTokenKind.Punctuation, false);
        WriteNewLine();
    }

    private Tuple<int, int, int>? ResolveLoop(Instruction instruction, Instruction[] instructions, int index)
    {
        try
        {
            var opCode = instruction.OpCode;
            // Check if it's the start of a for loop

            // A for loop will look something like:
            // brtrue <continue label>
            // nop                      // This is the break label
            // br <end label>
            // nop                      // This is the body label
            // <body>
            // nop                      // This is the continue label
            // <increment/action>
            // brtrue <body label>
            // nop                      // This is the end label
            var loopStart = index + 1;
            if (opCode == OpCode.Brtrue)
            {
                var nextInstruction = instructions[index + 1];
                if (nextInstruction.OpCode == OpCode.Nop)
                {
                    var endLabel = instructions[index + 2].Operand;
                    var bodyLabel = instructions[index + 3].Label;
                    for (var i = index + 3; i < instructions.Length; i++)
                    {
                        var currentInstruction = instructions[i];
                        var peek = instructions[i + 1];
                        if (currentInstruction.OpCode == OpCode.Brtrue &&
                            currentInstruction.Operand == bodyLabel &&
                            peek.OpCode == OpCode.Nop &&
                            peek.Label == endLabel)
                        {
                            return new Tuple<int, int, int>(loopStart, i, 3);
                        }
                    }
                }
            }
            
            // Check if it's the start of a while loop

            // A while loop will look something like:
            // br <continue label>
            // nop                      // This is the body label
            // <body>
            // nop                      // This is the continue label
            // <condition>
            // brtrue <body label>
            // nop                      // This is the break label
            if (opCode == OpCode.Br)
            {
                var nextInstruction = instructions[index + 1];
                if (nextInstruction.OpCode == OpCode.Nop)
                {
                    var bodyLabel = nextInstruction.Label;
                    for (var i = index + 2; i < instructions.Length; i++)
                    {
                        var currentInstruction = instructions[i];
                        if (currentInstruction.OpCode == OpCode.Brtrue &&
                            currentInstruction.Operand == bodyLabel &&
                            instructions[i + 1].OpCode == OpCode.Nop)
                        {
                            return new Tuple<int, int, int>(loopStart, i, 0);
                        }
                    }
                }
            }

            return null;
        }
        // What likely happened is we reached the end of a for loop, but the loop checker thought it was the start of a loop.
        catch (IndexOutOfRangeException)
        {
            return null;
        }
    }
    
    private void WriteLocal(LocalInfo local, int index)
    {
        WriteToken("[", ILTokenKind.Punctuation, false);
        WriteToken(index.ToString(), ILTokenKind.Number, false);
        WriteToken("]", ILTokenKind.Punctuation, true);
        WriteToken(local.Type.Fullname, ILTokenKind.TypeIdentifier, true);
        WriteToken(local.Name, ILTokenKind.VariableIdentifier, false);
    }
    
    private void WriteInstruction(Instruction instruction)
    {
        if (_currentMethod is null)
        {
            throw new InvalidOperationException("Cannot write an instruction without a current method.");
        }
        
        // Get the hex form of the instruction label.
        // For instance: IL_00CA
        var label = instruction.Label.ToString("X4");
        // We don't want the "0x" prefix, so we remove the first two characters.
        if (label.StartsWith("0x"))
        {
            label = label[2..];
        }
        
        var opCode = instruction.OpCode;
        var operand = instruction.Operand;
        WriteToken($"IL_{label}", ILTokenKind.Label, false);
        WriteToken(":", ILTokenKind.Punctuation, true);
        if (instruction.OpCode.NoOperandRequired())
        {
            WriteToken(opCode.ToString().ToLower(), ILTokenKind.OpCode, false);
            return;
        }
        
        // The longest operand name is 7 characters long, so we pad the operand name with spaces to make it 7 characters long.
        var operandName = opCode.ToString().PadRight(10);
        WriteToken(operandName.ToLower(), ILTokenKind.OpCode, true);
        switch (opCode)
        {
            case OpCode.Ldc:
            {
                var constant = _metadata.Constants.Constants[operand];
                var convertedObject = ConvertConstant(constant);
                if (convertedObject.IsNumber())
                {
                    WriteToken(convertedObject.ToString()!, ILTokenKind.Number, false);
                    break;
                }

                if (convertedObject is string s)
                {
                    WriteToken($"\"{s}\"", ILTokenKind.String, false);
                    break;
                }
                
                if (convertedObject is char c)
                {
                    WriteToken($"'{c}'", ILTokenKind.String, false);
                    break;
                }

                if (convertedObject is bool b)
                {
                    WriteToken(b ? "true" : "false", ILTokenKind.Keyword, false);
                    break;
                }
                
                WriteToken(convertedObject.ToString()!, ILTokenKind.UnknownValue, false);
                break;
            }
            case OpCode.Ldstr:
            {
                var constant = _metadata.Constants.Constants[operand];
                var str = _metadata.Strings.Strings[constant.ValueOffset];
                WriteToken($"\"{str}\"", ILTokenKind.String, false);
                break;
            }
            case OpCode.Lddft:
            case OpCode.Conv:
            case OpCode.Newarr:
            {
                var typeDef = _metadata.Types.Types[operand];
                var type = TypeTracker.GetTypeInfo(typeDef);
                WriteToken(type.Fullname, ILTokenKind.TypeIdentifier, false);
                break;
            }
            case OpCode.Ldloc:
            case OpCode.Stloc:
            case OpCode.Ldloca:
            {
                var local = _metadata.Locals.Locals[operand];
                var localInfo = _currentMethod.Locals[local];
                WriteToken(localInfo.Name, ILTokenKind.VariableIdentifier, false);
                break;
            }
            case OpCode.Ldarg:
            case OpCode.Starg:
            case OpCode.Ldarga:
            {
                var parameter = _metadata.Parameters.Parameters[operand];
                var parameterInfo = _currentMethod.Parameters[parameter];
                WriteToken(parameterInfo.Name, ILTokenKind.VariableIdentifier, false);
                break;
            }
            case OpCode.Ldfld:
            case OpCode.Stfld:
            case OpCode.Ldsfld:
            case OpCode.Stsfld:
            case OpCode.Ldflda:
            case OpCode.Ldsflda:
            {
                var memberReference = _metadata.MemberReferences.MemberReferences[operand];
                var memberRef = _assembly.MemberReferences[memberReference];
                if (memberRef.MemberInfo is not FieldInfo)
                {
                    throw new InvalidOperationException("Invalid member reference for instruction");
                }
                
                WriteToken(memberRef.MemberInfo.Type.Fullname, ILTokenKind.TypeIdentifier, true);
                WriteToken(memberRef.ParentType.Fullname, ILTokenKind.TypeIdentifier, false);
                WriteToken(".", ILTokenKind.Punctuation, false);
                WriteToken(memberRef.MemberInfo.Name, ILTokenKind.FieldIdentifier, false);
                break;
            }
            case OpCode.Ldind:
            case OpCode.Stind:
            {
                var typeDef = _metadata.Types.Types[operand];
                var type = TypeTracker.GetTypeInfo(typeDef);
                WriteToken(type.Fullname, ILTokenKind.TypeIdentifier, false);
                break;
            }
            case OpCode.Ldtype:
            {
                var typeDef = _metadata.Types.Types[operand];
                var type = TypeTracker.GetTypeInfo(typeDef);
                WriteToken(type.Fullname, ILTokenKind.TypeIdentifier, false);
                break;
            }
            case OpCode.Call:
            {
                var memberReference = _metadata.MemberReferences.MemberReferences[operand];
                var memberRef = _assembly.MemberReferences[memberReference];
                if (memberRef.MemberInfo is not MethodInfo method)
                {
                    throw new InvalidOperationException();
                }

                if (method.IsStatic)
                {
                    WriteToken("static", ILTokenKind.Keyword, true);
                }
                else
                {
                    WriteToken("instance", ILTokenKind.Keyword, true);
                }
                
                WriteToken(method.Type.Fullname, ILTokenKind.TypeIdentifier, true);
                WriteToken(method.Parent.Fullname, ILTokenKind.TypeIdentifier, false);
                WriteToken(".", ILTokenKind.Punctuation, false);
                WriteToken(method.Name, ILTokenKind.MethodIdentifier, false);
                WriteToken("(", ILTokenKind.Punctuation, false);
                for (var i = 0; i < method.Parameters.Count; i++)
                {
                    var parameter = method.Parameters.ValueAt(i);
                    WriteToken(parameter.Type.Fullname, ILTokenKind.TypeIdentifier, false);
                    if (i != method.Parameters.Count - 1)
                    {
                        WriteToken(",", ILTokenKind.Punctuation, true);
                    }
                }
                
                WriteToken(")", ILTokenKind.Punctuation, false);
                break;
            }
            case OpCode.Brtrue:
            case OpCode.Brfalse:
            case OpCode.Br:
            {
                var labelNumber = operand.ToString("X4");
                if (labelNumber.StartsWith("0x"))
                {
                    labelNumber = labelNumber[2..];
                }
                
                WriteToken($"IL_{labelNumber}", ILTokenKind.Label, false);
                break;
            }
            case OpCode.Newobj:
            {
                var typeReference = _metadata.TypeReferences.TypeReferences[operand];
                var typeRef = _assembly.TypeReferences[typeReference];
                var ctor = typeRef.ConstructorReference;
                if (ctor.MemberInfo is not MethodInfo constructor)
                {
                    throw new InvalidOperationException();
                }
                
                WriteToken("instance", ILTokenKind.Keyword, true);
                WriteToken(constructor.Type.Fullname, ILTokenKind.TypeIdentifier, true);
                WriteToken(constructor.Parent.Fullname, ILTokenKind.TypeIdentifier, false);
                WriteToken(".", ILTokenKind.Punctuation, false);
                WriteToken(constructor.Name, ILTokenKind.MethodIdentifier, false);
                WriteToken("(", ILTokenKind.Punctuation, false);
                for (var i = 0; i < constructor.Parameters.Count; i++)
                {
                    var parameter = constructor.Parameters.ValueAt(i);
                    WriteToken(parameter.Type.Fullname, ILTokenKind.TypeIdentifier, false);
                    if (i != constructor.Parameters.Count - 1)
                    {
                        WriteToken(",", ILTokenKind.Punctuation, true);
                    }
                }
                
                WriteToken(")", ILTokenKind.Punctuation, false);
                break;
            }
        }
    }

    private void WriteBindingFlags(BindingFlags flags)
    {
        if (flags == BindingFlags.None)
        {
            return;
        }
        
        if (flags.HasFlag(BindingFlags.Public))
        {
            WriteToken("public", ILTokenKind.Keyword, true);
        }
        
        if (flags.HasFlag(BindingFlags.NonPublic))
        {
            WriteToken("private", ILTokenKind.Keyword, true);
        }

        if (flags.HasFlag(BindingFlags.Instance))
        {
            WriteToken("instance", ILTokenKind.Keyword, true);
        }
        
        if (flags.HasFlag(BindingFlags.Static))
        {
            WriteToken("static", ILTokenKind.Keyword, true);
        }
        
        if (flags.HasFlag(BindingFlags.RuntimeInternal))
        {
            WriteToken("runtime_internal", ILTokenKind.Keyword, true);
        }

        if (flags.HasFlag(BindingFlags.Entry))
        {
            WriteToken("entry", ILTokenKind.Keyword, true);
        }
    }

    private void WriteToken(string text, ILTokenKind kind, bool hasSpaceAfter)
    {
        if (_isFirstTokenOfLine)
        {
            for (var i = 0; i < _indentLevel; i++)
            {
                var indentToken = new ILToken(ILTokenKind.Trivia, Indentation);
                _tokens.Add(indentToken);
                Write(Indentation);
            }
            
            _isFirstTokenOfLine = false;
        }
        
        var token = new ILToken(kind, text);
        _tokens.Add(token);
        Write(text);
        if (text == "\n")
        {
            _isFirstTokenOfLine = true;
            return;
        }
        
        if (hasSpaceAfter && kind != ILTokenKind.EOF && text[^1] != ' ')
        {
            WriteToken(" ", ILTokenKind.Trivia, false);
        }
    }

    private void WriteNewLine()
    {
        WriteToken("\n", ILTokenKind.Trivia, false);
    }
    
    private void Write(string text)
    {
        _sb.Append(text);
    }

    private void Indent()
    {
        _indentLevel++;
    }
    
    private void Unindent()
    {
        _indentLevel--;
    }
    
    private unsafe object ConvertConstant(Constant constant)
    {
        var pool = _metadata.ConstantsPool.Values;
        byte* ptr;
        fixed (byte* p = pool)
        {
            ptr = p + constant.ValueOffset;
        }

        object value;
        switch (constant.Type)
        {
            case ConstantType.Int8:
            {
                value = *(sbyte*)ptr;
                break;
            }
            case ConstantType.UInt8:
            {
                value = *ptr;
                break;
            }
            case ConstantType.Int16:
            {
                value = *(short*)ptr;
                break;
            }
            case ConstantType.UInt16:
            {
                value = *(ushort*)ptr;
                break;
            }
            case ConstantType.Int32:
            {
                value = *(int*)ptr;
                break;
            }
            case ConstantType.UInt32:
            {
                value = *(uint*)ptr;
                break;
            }
            case ConstantType.Int64:
            {
                value = *(long*)ptr;
                break;
            }
            case ConstantType.UInt64:
            {
                value = *(ulong*)ptr;
                break;
            }
            case ConstantType.Float32:
            {
                value = *(float*)ptr;
                break;
            }
            case ConstantType.Float64:
            {
                value = *(double*)ptr;
                break;
            }
            case ConstantType.String:
            {
                var stringIndex = *(int*)ptr;
                value = _metadata.Strings.Strings[stringIndex];
                break;
            }
            case ConstantType.Char:
            {
                value = *(char*)ptr;
                break;
            }
            case ConstantType.Boolean:
            {
                value = *(bool*)ptr;
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }
        
        return value;
    }
    
    public override string ToString()
    {
        return _sb.ToString();
    }
}
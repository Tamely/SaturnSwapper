using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Emit;
using Radon.CodeAnalysis.Emit.Binary;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;
using Radon.Runtime.RuntimeInfo;
using Radon.Runtime.RuntimeSystem.RuntimeObjects;
using Radon.Runtime.Utilities;

namespace Radon.Runtime.RuntimeSystem;

internal readonly struct MethodRuntime
{
    private readonly AssemblyInfo _assembly;
    private readonly Metadata _metadata;
    private readonly IRuntimeObject _instance;
    private readonly MethodInfo _method;
    private readonly Stack<MethodInfo> _callStack;
    private readonly ImmutableArray<Instruction> _instructions;
    private readonly StackFrame _stackFrame;
    
    public MethodRuntime(AssemblyInfo assembly, IRuntimeObject instance, MethodInfo method, ImmutableArray<IRuntimeObject> arguments)
    {
        _assembly = assembly;
        _metadata = assembly.Metadata;
        _instance = instance;
        _method = method;
        _callStack = new Stack<MethodInfo>();
        var instructionCount = _method.InstructionCount;
        var instructionStart = _method.FirstInstruction;
        var instructions = new Instruction[instructionCount];
        for (var i = 0; i < instructionCount; i++)
        {
            instructions[i] = assembly.Instructions[instructionStart + i];
        }
        
        _instructions = instructions.ToImmutableArray();
        var locals = _method.Locals;
        _stackFrame = new StackFrame(locals, arguments);
    }

    public IRuntimeObject Invoke()
    {
        switch (_method.IsStatic)
        {
            case true when _instance is not NullObject:
                throw new InvalidOperationException("Cannot invoke a static method on an instance.");
            case false when _instance is NullObject:
                throw new InvalidOperationException("Cannot invoke an instance method on a null instance.");
        }
        
        _callStack.Push(_method);
        if (_method.IsRuntimeMethod)
        {
            
        }

        RunInstructions();
        if (_stackFrame.StackCount != 1)
        {
            if (_stackFrame.StackCount == 0)
            {
                Logger.Log("Stack frame is empty. This could be an issue if the method is not void.", LogLevel.Warning);
            }
            else
            {
                Logger.Log("Stack frame is not empty.", LogLevel.Warning);
                // clear the stack frame
                _stackFrame.Clear();
            }
            
            return IRuntimeObject.Null(ManagedRuntime.System.GetType(_method.ReturnType));
        }
        
        var result = _stackFrame.Pop();
        _stackFrame.Clear();
        return result;
    }

    private void RunInstructions()
    {
        foreach (var instruction in _instructions)
        {
            try
            {
                switch (instruction.OpCode)
                {
                    case OpCode.Nop:
                    {
                        break;
                    }
                    case OpCode.Add:
                    case OpCode.Sub:
                    case OpCode.Mul:
                    case OpCode.Div:
                    case OpCode.Concat:
                    {
                        var right = _stackFrame.Pop();
                        var left = _stackFrame.Pop();
                        var result = right.ComputeOperation(instruction.OpCode, left);
                        if (result is null)
                        {
                            throw new InvalidOperationException($"Operation {instruction.OpCode} returned null.");
                        }
                        
                        _stackFrame.Push(result);
                        break;
                    }
                    case OpCode.Ldc:
                    {
                        var constant = _metadata.Constants.Constants[instruction.Operand];
                        var convertedObject = ConvertConstant(constant);
                        _stackFrame.Push(convertedObject);
                        break;
                    }
                    case OpCode.Ldstr:
                    {
                        var constant = _metadata.Constants.Constants[instruction.Operand];
                        var str = _metadata.Strings.Strings[constant.ValueOffset];
                        var strObject = new ManagedString(str);
                        _stackFrame.Push(strObject);
                        break;
                    }
                    case OpCode.Lddft: // Load default value
                    {
                        var typeDef = _metadata.Types.Types[instruction.Operand];
                        var type = ManagedRuntime.System.GetType(typeDef);
                        var defaultValue = IRuntimeObject.CreateDefault(type);
                        _stackFrame.Push(defaultValue);
                        break;
                    }
                    case OpCode.Ldloc:
                    {
                        var local = _stackFrame.GetLocal(instruction.Operand);
                        _stackFrame.Push(local);
                        break;
                    }
                    case OpCode.Stloc:
                    {
                        var value = _stackFrame.Pop();
                        _stackFrame.SetLocal(instruction.Operand, value);
                        break;
                    }
                    case OpCode.Ldarg:
                    {
                        var argument = _stackFrame.GetArgument(instruction.Operand);
                        _stackFrame.Push(argument);
                        break;
                    }
                    case OpCode.Ldfld:
                    {
                        var memberReference = _metadata.MemberReferences.MemberReferences[instruction.Operand];
                        var memberRef = _assembly.MemberReferences[memberReference];
                        var instance = _stackFrame.Pop();
                        if (instance is not ManagedObject managedObject)
                        {
                            throw new InvalidOperationException("Cannot load a field from a non-managed object.");
                        }
                        
                        var value = managedObject.GetField(memberRef);
                        _stackFrame.Push(value);
                        break;
                    }
                    case OpCode.Stfld:
                    {
                        var memberReference = _metadata.MemberReferences.MemberReferences[instruction.Operand];
                        var memberRef = _assembly.MemberReferences[memberReference];
                        var value = _stackFrame.Pop();
                        var instance = _stackFrame.Pop();
                        if (instance is not ManagedObject managedObject)
                        {
                            throw new InvalidOperationException("Cannot store a field on a non-managed object.");
                        }
                        
                        managedObject.SetField(memberRef, value);
                        break;
                    }
                    case OpCode.Ldsfld:
                    {
                        var memberReference = _metadata.MemberReferences.MemberReferences[instruction.Operand];
                        var memberRef = _assembly.MemberReferences[memberReference];
                        var type = memberRef.ParentType;
                        var runtimeType = ManagedRuntime.System.GetType(type);
                        var value = runtimeType.GetStaticField(memberRef);
                        _stackFrame.Push(value);
                        break;
                    }
                    case OpCode.Stsfld:
                    {
                        var memberReference = _metadata.MemberReferences.MemberReferences[instruction.Operand];
                        var value = _stackFrame.Pop();
                        var memberRef = _assembly.MemberReferences[memberReference];
                        var type = memberRef.ParentType;
                        var runtimeType = ManagedRuntime.System.GetType(type);
                        runtimeType.SetStaticField(memberRef, value);
                        break;
                    }
                    case OpCode.Ldenum:
                    {
                        var memberReference = _metadata.MemberReferences.MemberReferences[instruction.Operand];
                        var memberRef = _assembly.MemberReferences[memberReference];
                        var type = memberRef.ParentType;
                        var runtimeType = ManagedRuntime.System.GetType(type);
                        var value = runtimeType.GetEnumValue(memberRef);
                        _stackFrame.Push(value);
                        break;
                    }
                    case OpCode.Ldthis:
                    {
                        _stackFrame.Push(_instance);
                        break;
                    }
                    case OpCode.Ldelem:
                    {
                        var index = _stackFrame.Pop();
                        var array = _stackFrame.Pop();
                        if (index is not ManagedObject managedIndex)
                        {
                            throw new InvalidOperationException("The index must be a managed object.");
                        }
                        
                        if (array is not ManagedArray managedArray)
                        {
                            throw new InvalidOperationException("Cannot load an element from a non-array object.");
                        }
                        
                        var value = managedArray.GetElement(managedIndex.GetValue<int>());
                        _stackFrame.Push(value);
                        break;
                    }
                    case OpCode.Stelem:
                    {
                        var value = _stackFrame.Pop();
                        var index = _stackFrame.Pop();
                        var array = _stackFrame.Pop();
                        if (index is not ManagedObject managedIndex)
                        {
                            throw new InvalidOperationException("The index must be a managed object.");
                        }
                        
                        if (array is not ManagedArray managedArray)
                        {
                            throw new InvalidOperationException("Cannot load an element from a non-array object.");
                        }
                        
                        managedArray.SetElement(managedIndex.GetValue<int>(), value);
                        break;
                    }
                    case OpCode.Call:
                    {
                        var memberReference = _metadata.MemberReferences.MemberReferences[instruction.Operand];
                        var memberRef = _assembly.MemberReferences[memberReference];
                        var type = memberRef.ParentType;
                        var runtimeType = ManagedRuntime.System.GetType(type);
                        var instance = _method.IsStatic ? IRuntimeObject.Null(runtimeType) : _stackFrame.Pop();
                        if (memberRef.MemberInfo is not MethodInfo methodInfo)
                        {
                            throw new InvalidOperationException("Cannot call a non-method member.");
                        }
                        
                        var arguments = new IRuntimeObject[methodInfo.Parameters.Length];
                        var method = runtimeType.TypeInfo.GetByRef<MethodInfo>(MemberType.Method, memberReference);
                        for (var i = 0; i < arguments.Length; i++)
                        {
                            arguments[i] = _stackFrame.Pop();
                        }

                        var result = instance switch
                        {
                            NullObject => runtimeType.InvokeStaticMethod(_assembly, method, arguments.ToImmutableArray()),
                            ManagedObject managedObject => managedObject.InvokeMethod(_assembly, method, arguments.ToImmutableArray()),
                            _ => throw new InvalidOperationException("Unknown instance type. Cannot invoke method.")
                        };

                        if (ManagedRuntime.System.GetType(method.ReturnType) != ManagedRuntime.System.GetType("void"))
                        {
                            _stackFrame.Push(result);
                        }

                        break;
                    }
                    case OpCode.Import:
                    {
                        //var path = _metadata.Strings.Strings[instruction.Operand];
                        // TODO: Tamely implements the import instruction.
                        break;
                    }
                    case OpCode.Conv:
                    {
                        var expression = _stackFrame.Pop();
                        var typeDef = _metadata.Types.Types[instruction.Operand];
                        var type = ManagedRuntime.System.GetType(typeDef);
                        var convertedObject = expression.ConvertTo(type);
                        if (convertedObject is null)
                        {
                            throw new InvalidOperationException($"Cannot convert {expression} to {type}.");
                        }
                        
                        _stackFrame.Push(convertedObject);
                        break;
                    }
                    case OpCode.Newarr:
                    {
                        var size = _stackFrame.Pop();
                        if (size is not ManagedObject managedSize)
                        {
                            throw new InvalidOperationException("Cannot create an array with a non-managed object.");
                        }

                        var type = _assembly.Types[instruction.Operand];
                        var runtimeType = ManagedRuntime.System.GetType(type);
                        var array = runtimeType.CreateArray(managedSize.GetValue<int>());
                        _stackFrame.Push(array);
                        break;
                    }
                    case OpCode.Newobj:
                    {
                        var typeReference = _metadata.TypeReferences.TypeReferences[instruction.Operand];
                        var typeRef = _assembly.TypeReferences[typeReference];
                        var type = typeRef.TypeDefinition;
                        var runtimeType = ManagedRuntime.System.GetType(type);
                        var constructorReference = typeRef.ConstructorReference;
                        if (constructorReference.MemberInfo is not MethodInfo constructor)
                        {
                            throw new InvalidOperationException("Cannot create an object with a non-constructor.");
                        }
                        
                        var arguments = new IRuntimeObject[constructor.Parameters.Length];
                        for (var i = 0; i < arguments.Length; i++)
                        {
                            arguments[i] = _stackFrame.Pop();
                        }

                        var instance = runtimeType.CreateInstance(_assembly, constructorReference, arguments.ToImmutableArray());
                        _stackFrame.Push(instance);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, LogLevel.Error);
            }
        }
    }

    private unsafe IRuntimeObject ConvertConstant(Constant constant)
    {
        var pool = _metadata.ConstantsPool.Values;
        byte* ptr;
        fixed (byte* p = pool)
        {
            ptr = p + constant.ValueOffset;
        }

        switch (constant.Type)
        {
            case ConstantType.Int8:
            {
                var value = *(sbyte*)ptr;
                return new ManagedSByte(value);
            }
            case ConstantType.UInt8:
            {
                var value = *ptr;
                return new ManagedByte(value);
            }
            case ConstantType.Int16:
            {
                var value = *(short*)ptr;
                return new ManagedShort(value);
            }
            case ConstantType.UInt16:
            {
                var value = *(ushort*)ptr;
                return new ManagedUShort(value);
            }
            case ConstantType.Int32:
            {
                var value = *(int*)ptr;
                return new ManagedInt(value);
            }
            case ConstantType.UInt32:
            {
                var value = *(uint*)ptr;
                return new ManagedUInt(value);
            }
            case ConstantType.Int64:
            {
                var value = *(long*)ptr;
                return new ManagedLong(value);
            }
            case ConstantType.UInt64:
            {
                var value = *(ulong*)ptr;
                return new ManagedULong(value);
            }
            case ConstantType.Float32:
            {
                var value = *(float*)ptr;
                return new ManagedFloat(value);
            }
            case ConstantType.Float64:
            {
                var value = *(double*)ptr;
                return new ManagedDouble(value);
            }
            case ConstantType.String:
            {
                var stringIndex = *(int*)ptr;
                var stringConstant = _metadata.Strings.Strings[stringIndex];
                var stringObject = new ManagedString(stringConstant);
                return stringObject;
            }
            case ConstantType.Char:
            {
                var value = *(char*)ptr;
                return new ManagedChar(value);
            }
            case ConstantType.Boolean:
            {
                var value = *(bool*)ptr;
                return new ManagedBoolean(value);
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }
}
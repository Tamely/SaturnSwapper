using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Radon.CodeAnalysis.Emit;
using Radon.CodeAnalysis.Emit.Binary;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;
using Radon.Runtime.RuntimeInfo;
using Radon.Runtime.RuntimeSystem.Objects;
using Radon.Runtime.Utilities;
using Radon.Utilities;

namespace Radon.Runtime.RuntimeSystem;

internal sealed class ManagedRuntime
{
    public static ManagedRuntime System { get; private set; }
    public AssemblyInfo AssemblyInfo { get; }
    public Dictionary<TypeInfo, RuntimeType> Types { get; }
    public ImmutableDictionary<TypeInfo, RuntimeType> PrimitiveTypes { get; }
    public ManagedRuntime(AssemblyInfo assemblyInfo)
    {
        System = this;
        AssemblyInfo = assemblyInfo;
        Types = new Dictionary<TypeInfo, RuntimeType>();
        var builder = ImmutableDictionary.CreateBuilder<TypeInfo, RuntimeType>();
        foreach (var type in assemblyInfo.Types)
        {
            if (Types.ContainsKey(type))
            {
                continue;
            }
            
            var runtimeType = new RuntimeType(type);
            Types.Add(type, runtimeType);
            if (type.IsPrimitive)
            {
                builder.Add(type, runtimeType);
            }
        }
        
        foreach (var type in Types.Values)
        {
            type.Initialize();
        }
        
        PrimitiveTypes = builder.ToImmutable();
    }
    
    public RuntimeType GetType(TypeInfo typeInfo)
    {
        if (Types.TryGetValue(typeInfo, out var type))
        {
            return type;
        }
        
        throw new InvalidOperationException($"Type {typeInfo.Name} does not exist.");
    }

    public RuntimeType GetType(string name)
    {
        var typeInfo = Types.Keys.FirstOrDefault(x => x.Name == name);
        if (typeInfo is null)
        {
            throw new InvalidOperationException($"Type {name} does not exist.");
        }
        
        return GetType(typeInfo);
    }

    public RuntimeType GetType(TypeDefinition definition)
    {
        var typeInfo = Types.Keys.FirstOrDefault(x => x.Definition == definition);
        if (typeInfo is null)
        {
            throw new InvalidOperationException($"Type {definition.Name} does not exist.");
        }
        
        return GetType(typeInfo);
    }
}

internal readonly struct MethodRuntime
{
    private readonly AssemblyInfo _assembly;
    private readonly Metadata _metadata;
    private readonly IObject _instance;
    private readonly MethodInfo _method;
    private readonly Stack<MethodInfo> _callStack;
    private readonly ImmutableArray<Instruction> _instructions;
    private readonly StackFrame _stackFrame;
    
    public MethodRuntime(AssemblyInfo assembly, IObject? instance, MethodInfo method, ImmutableArray<IObject> arguments)
    {
        _assembly = assembly;
        _metadata = assembly.Metadata;
        _instance = instance ?? IObject.Null;
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

    public IObject Invoke()
    {
        switch (_method.IsStatic)
        {
            case true when
                _instance is not null:
                throw new InvalidOperationException("Cannot invoke a static method on an instance.");
            case false when
                _instance is null:
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
            
            return IObject.Null;
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
                        if (left is not ManagedObject leftObject ||
                            right is not ManagedObject rightObject)
                        {
                            throw new InvalidOperationException("Cannot perform arithmetic on non-number objects.");
                        }

                        var result = leftObject.ComputeBinaryOperation((OperationType)instruction.OpCode, rightObject);
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
                        var strObject = new ManagedObject(ManagedRuntime.System.GetType("string"), str);
                        _stackFrame.Push(strObject);
                        break;
                    }
                    case OpCode.Lddft: // Load default value
                    {
                        var typeDef = _metadata.Types.Types[instruction.Operand];
                        var type = ManagedRuntime.System.GetType(typeDef);
                        var defaultValue = GetDefaultValue(type);
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
                        var instance = _stackFrame.Pop();
                        if (instance is not ManagedObject managedObject)
                        {
                            throw new InvalidOperationException("Cannot load a field from a non-managed object.");
                        }
                        
                        var value = managedObject.GetField(memberReference, _assembly);
                        _stackFrame.Push(value);
                        break;
                    }
                    case OpCode.Ldsfld:
                    {
                        var memberReference = _metadata.MemberReferences.MemberReferences[instruction.Operand];
                        var memberRef = _assembly.MemberReferences[memberReference];
                        var type = memberRef.ParentType;
                        var runtimeType = ManagedRuntime.System.GetType(type);
                        var value = runtimeType.GetStaticField(memberReference);
                        _stackFrame.Push(value);
                        break;
                    }
                    case OpCode.Ldenum:
                    {
                        var memberReference = _metadata.MemberReferences.MemberReferences[instruction.Operand];
                        var memberRef = _assembly.MemberReferences[memberReference];
                        var type = memberRef.ParentType;
                        var runtimeType = ManagedRuntime.System.GetType(type);
                        var value = runtimeType.GetEnumValue(memberReference);
                        _stackFrame.Push(value);
                        break;
                    }
                    case OpCode.Ldthis:
                    {
                        _stackFrame.Push(_instance);
                        break;
                    }
                    case OpCode.Stfld:
                    {
                        var memberReference = _metadata.MemberReferences.MemberReferences[instruction.Operand];
                        var value = _stackFrame.Pop();
                        var instance = _stackFrame.Pop();
                        if (instance is not ManagedObject managedObject)
                        {
                            throw new InvalidOperationException("Cannot store a field on a non-managed object.");
                        }
                        
                        managedObject.SetField(memberReference, value, _assembly);
                        break;
                    }
                    case OpCode.Stsfld:
                    {
                        var memberReference = _metadata.MemberReferences.MemberReferences[instruction.Operand];
                        var value = _stackFrame.Pop();
                        var memberRef = _assembly.MemberReferences[memberReference];
                        var type = memberRef.ParentType;
                        var runtimeType = ManagedRuntime.System.GetType(type);
                        runtimeType.SetStaticField(memberReference, value);
                        break;
                    }
                    case OpCode.Call:
                    {
                        var memberReference = _metadata.MemberReferences.MemberReferences[instruction.Operand];
                        var memberRef = _assembly.MemberReferences[memberReference];
                        var type = memberRef.ParentType;
                        var runtimeType = ManagedRuntime.System.GetType(type);
                        var instance = _method.IsStatic ? IObject.Null : _stackFrame.Pop();
                        var arguments = new IObject[memberReference.ParameterTypes.Length];
                        var method = runtimeType.TypeInfo.GetByRef<MethodInfo>(MemberType.Method, memberReference);
                        for (var i = 0; i < arguments.Length; i++)
                        {
                            arguments[i] = _stackFrame.Pop();
                        }

                        var result = instance switch
                        {
                            NullObject => runtimeType.InvokeStaticMethod(_assembly, method, arguments.ToImmutableArray()),
                            ManagedObject managedObject => managedObject.InvokeMethod(_assembly, method,
                                memberReference, arguments.ToImmutableArray()),
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
                        var path = _metadata.Strings.Strings[instruction.Operand];
                        // TODO: Tamely implements the import instruction.
                        break;
                    }
                    case OpCode.Conv:
                    {
                        var expression = _stackFrame.Pop();
                        if (expression is not ManagedObject managedObject)
                        {
                            throw new InvalidOperationException("Cannot convert a non-managed object.");
                        }
                        
                        var typeDef = _metadata.Types.Types[instruction.Operand];
                        var type = ManagedRuntime.System.GetType(typeDef);
                        var convertedObject = managedObject.Convert(type);
                        _stackFrame.Push(convertedObject);
                        break;
                    }
                    case OpCode.Newobj:
                    {
                        var typeReference = _metadata.TypeReferences.TypeReferences[instruction.Operand];
                        var typeRef = _assembly.TypeReferences[typeReference];
                        var type = typeRef.TypeDefinition;
                        var runtimeType = ManagedRuntime.System.GetType(type);
                        var constructorReference = typeRef.ConstructorReference;
                        var arguments = new IObject[constructorReference.ParameterTypes.Length];
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

    private unsafe IObject ConvertConstant(Constant constant)
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
                var type = ManagedRuntime.System.GetType("sbyte");
                var value = *(sbyte*) ptr;
                return new ManagedObject(type, value);
            }
            case ConstantType.UInt8:
            {
                var type = ManagedRuntime.System.GetType("byte");
                var value = *ptr;
                return new ManagedObject(type, value);
            }
            case ConstantType.Int16:
            {
                var type = ManagedRuntime.System.GetType("short");
                var value = *(short*) ptr;
                return new ManagedObject(type, value);
            }
            case ConstantType.UInt16:
            {
                var type = ManagedRuntime.System.GetType("ushort");
                var value = *(ushort*) ptr;
                return new ManagedObject(type, value);
            }
            case ConstantType.Int32:
            {
                var type = ManagedRuntime.System.GetType("int");
                var value = *(int*) ptr;
                return new ManagedObject(type, value);
            }
            case ConstantType.UInt32:
            {
                var type = ManagedRuntime.System.GetType("uint");
                var value = *(uint*) ptr;
                return new ManagedObject(type, value);
            }
            case ConstantType.Int64:
            {
                var type = ManagedRuntime.System.GetType("long");
                var value = *(long*) ptr;
                return new ManagedObject(type, value);
            }
            case ConstantType.UInt64:
            {
                var type = ManagedRuntime.System.GetType("ulong");
                var value = *(ulong*) ptr;
                return new ManagedObject(type, value);
            }
            case ConstantType.Float32:
            {
                var type = ManagedRuntime.System.GetType("float");
                var value = *(float*) ptr;
                return new ManagedObject(type, value);
            }
            case ConstantType.Float64:
            {
                var type = ManagedRuntime.System.GetType("double");
                var value = *(double*) ptr;
                return new ManagedObject(type, value);
            }
            case ConstantType.String:
            {
                // get an int from the pool
                var type = ManagedRuntime.System.GetType("string");
                var stringIndex = *(int*)ptr;
                var stringConstant = _metadata.Strings.Strings[stringIndex];
                var stringObject = new ManagedObject(type, stringConstant);
                return stringObject;
            }
            case ConstantType.Char:
            {
                var type = ManagedRuntime.System.GetType("char");
                var value = *ptr;
                return new ManagedObject(type, value);
            }
            case ConstantType.Boolean:
            {
                var type = ManagedRuntime.System.GetType("bool");
                var value = *ptr;
                return new ManagedObject(type, value);
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private IObject GetDefaultValue(RuntimeType type)
    {
        var typeInfo = type.TypeInfo;
        if (typeInfo.IsEnum)
        {
            var underlyingType = typeInfo.UnderlyingType;
            var runtimeType = ManagedRuntime.System.GetType(underlyingType!);
            return GetDefaultValue(runtimeType);
        }

        return new ManagedObject(type);
    }
}

internal readonly struct StackFrame
{
    private readonly Stack<IObject> _stack;
    private readonly List<IObject> _popped;
    private readonly Dictionary<LocalInfo, IObject> _locals;
    private readonly ImmutableArray<IObject> _arguments;

    public StackFrame(ImmutableArray<LocalInfo> locals, ImmutableArray<IObject> arguments)
    {
        _stack = new Stack<IObject>();
        _popped = new List<IObject>();
        _locals = new Dictionary<LocalInfo, IObject>();
        _arguments = arguments;
        foreach (var local in locals)
        {
            _locals.Add(local, IObject.Null);
        }
    }
    
    public ImmutableArray<IObject> Popped => _popped.ToImmutableArray();
    public int StackCount => _stack.Count;
    
    public void Clear()
    {
        _stack.Clear();
        _popped.Clear();
        _locals.Clear();
    }
    
    public void Push(IObject value)
    {
        _stack.Push(value);
    }
    
    public IObject Pop()
    {
        var value = _stack.Pop();
        _popped.Add(value);
        return value;
    }
    
    public void SetLocal(int index, IObject value)
    {
        var local = _locals.KeyAt(index);
        _locals[local] = value;
    }
    
    public IObject GetLocal(int index)
    {
        var local = _locals.KeyAt(index);
        return _locals[local];
    }
    
    public IObject GetArgument(int index)
    {
        return _arguments[index];
    }
}

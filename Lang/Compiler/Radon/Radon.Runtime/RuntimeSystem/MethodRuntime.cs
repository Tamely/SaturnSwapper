using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using CUE4Parse;
using Radon.CodeAnalysis.Disassembly;
using Radon.CodeAnalysis.Emit;
using Radon.CodeAnalysis.Emit.Binary;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;
using Radon.Common;
using Radon.Runtime.Memory;
using Radon.Runtime.RuntimeSystem.RuntimeObjects;
using Radon.Runtime.RuntimeSystem.RuntimeObjects.Properties;
using UAssetAPI;
using UAssetAPI.IO;
using UAssetAPI.PropertyFactories;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;

namespace Radon.Runtime.RuntimeSystem;

internal sealed class MethodRuntime
{
    private readonly AssemblyInfo _assembly;
    private readonly Metadata _metadata;
    private readonly RuntimeObject? _instance;
    private readonly MethodInfo _method;
    private readonly ImmutableArray<Instruction> _instructions;
    private readonly StackFrame _stackFrame;
    private readonly ReadOnlyDictionary<ParameterInfo, RuntimeObject> _arguments;

    public MethodRuntime(AssemblyInfo assembly, RuntimeObject? instance, MethodInfo method,
        ReadOnlyDictionary<ParameterInfo, RuntimeObject> arguments)
    {
        _assembly = assembly;
        _metadata = assembly.Metadata;
        _instance = instance;
        _method = method;
        _arguments = arguments;
        var instructionCount = _method.InstructionCount;
        var instructionStart = _method.FirstInstruction;
        var instructions = new Instruction[instructionCount];
        for (var i = 0; i < instructionCount; i++)
        {
            var instruction = assembly.Instructions[instructionStart + i];
            instructions[i] = instruction;
        }

        // Sort the instructions from lowest to highest label.
        _instructions = instructions.OrderBy(i => i.Label).ToImmutableArray();
        var locals = _method.Locals;
        var size = 0;
        foreach (var (_, local) in locals)
        {
            size += local.Type.Size;
        }

        var stackSize = ResolveStackSize();
        size += stackSize.MaxStackSize;
        _stackFrame = ManagedRuntime.StackManager.AllocateStackFrame(size, stackSize.MaxStack, instance,
            locals.Values.ToImmutableArray(), arguments);
    }

    private (int MaxStackSize, int MaxStack) ResolveStackSize()
    {
        var maxStack = 0;
        var stack = new Stack<TypeInfo>(); // The type of the item on the stack.
        var totalStack = new Stack<TypeInfo>();
        foreach (var instruction in _instructions)
        {
            // We need to get the max amount of items that will be on the stack at any given time.
            // This is used to determine the size of the stack frame.
            var opCode = instruction.OpCode;
            var operand = instruction.Operand;
            switch (opCode)
            {
                case OpCode.Add:
                case OpCode.Sub:
                case OpCode.Mul:
                case OpCode.Div:
                case OpCode.Cnct:
                case OpCode.Mod:
                case OpCode.Or:
                case OpCode.And:
                case OpCode.Xor:
                case OpCode.Shl:
                case OpCode.Shr:
                {
                    stack.Pop(); // Type of left
                    var left = stack.Pop(); // Pop right
                    stack.Push(left); // Push the result
                    totalStack.Push(left);
                    break;
                }
                case OpCode.Neg:
                {
                    var type = stack.Pop();
                    stack.Push(type);
                    totalStack.Push(type);
                    break;
                }
                case OpCode.Ldc:
                {
                    var constant = _metadata.Constants.Constants[operand];
                    var type = GetRuntimeType(constant.Type);
                    stack.Push(type.TypeInfo);
                    totalStack.Push(type.TypeInfo);
                    break;
                }
                case OpCode.Ldlen:
                {
                    stack.Push(ManagedRuntime.Int32.TypeInfo);
                    totalStack.Push(ManagedRuntime.Int32.TypeInfo);
                    break;
                }
                case OpCode.Ldstr:
                {
                    stack.Push(ManagedRuntime.String.TypeInfo);
                    totalStack.Push(ManagedRuntime.String.TypeInfo);
                    break;
                }
                case OpCode.Lddft:
                {
                    var typeDef = _metadata.Types.Types[instruction.Operand];
                    var type = ManagedRuntime.System.GetType(typeDef);
                    stack.Push(type.TypeInfo);
                    totalStack.Push(type.TypeInfo);
                    break;
                }
                case OpCode.Ldloc:
                {
                    var local = _metadata.Locals.Locals[operand];
                    var localInfo = _method.Locals[local];
                    stack.Push(localInfo.Type);
                    totalStack.Push(localInfo.Type);
                    break;
                }
                case OpCode.Stloc:
                {
                    stack.Pop();
                    break;
                }
                case OpCode.Ldarg:
                {
                    var parameter = _metadata.Parameters.Parameters[operand];
                    var param = _method.Parameters[parameter];
                    stack.Push(param.Type);
                    totalStack.Push(param.Type);
                    break;
                }
                case OpCode.Starg:
                {
                    stack.Pop();
                    break;
                }
                case OpCode.Ldfld:
                {
                    var memberReference = _metadata.MemberReferences.MemberReferences[operand];
                    var memberRef = _assembly.MemberReferences[memberReference];
                    stack.Push(memberRef.Type);
                    totalStack.Push(memberRef.Type);
                    break;
                }
                case OpCode.Stfld:
                {
                    stack.Pop();
                    break;
                }
                case OpCode.Ldsfld:
                {
                    var memberReference = _metadata.MemberReferences.MemberReferences[operand];
                    var memberRef = _assembly.MemberReferences[memberReference];
                    stack.Push(memberRef.Type);
                    totalStack.Push(memberRef.Type);
                    break;
                }
                case OpCode.Stsfld:
                {
                    stack.Pop();
                    break;
                }
                case OpCode.Ldthis:
                {
                    if (_method.IsStatic)
                    {
                        throw new InvalidOperationException("Cannot load 'this' in a static method.");
                    }

                    if (_instance is null)
                    {
                        throw new InvalidOperationException("Instance is null in a non-static method.");
                    }

                    stack.Push(_instance.Type.TypeInfo);
                    totalStack.Push(_instance.Type.TypeInfo);
                    break;
                }
                case OpCode.Ldelem:
                {
                    var array = stack.Pop();
                    stack.Pop();
                    if (array.UnderlyingType is null)
                    {
                        throw new InvalidOperationException("Cannot load element from non-array type.");
                    }

                    stack.Push(array.UnderlyingType);
                    totalStack.Push(array.UnderlyingType);
                    break;
                }
                case OpCode.Stelem:
                {
                    stack.Pop();
                    stack.Pop();
                    stack.Pop();
                    break;
                }
                case OpCode.Conv:
                {
                    stack.Pop();
                    var typeDef = _metadata.Types.Types[operand];
                    var type = ManagedRuntime.System.GetType(typeDef);
                    stack.Push(type.TypeInfo);
                    totalStack.Push(type.TypeInfo);
                    break;
                }
                case OpCode.Newarr:
                {
                    stack.Pop();
                    var typeDef = _metadata.Types.Types[operand];
                    var type = ManagedRuntime.System.GetType(typeDef);
                    stack.Push(type.TypeInfo);
                    totalStack.Push(type.TypeInfo);
                    break;
                }
                case OpCode.Newobj:
                {
                    var typeReference = _metadata.TypeReferences.TypeReferences[operand];
                    var typeRef = _assembly.TypeReferences[typeReference];
                    var type = ManagedRuntime.System.GetType(typeRef.TypeDefinition);
                    if (typeRef.ConstructorReference.MemberInfo is not MethodInfo constructor)
                    {
                        throw new InvalidOperationException("Cannot find constructor.");
                    }
                    
                    for (var i = 0; i < constructor.Parameters.Count; i++)
                    {
                        stack.Pop();
                    }
                    
                    stack.Push(type.TypeInfo);
                    totalStack.Push(type.TypeInfo);
                    break;
                }
                case OpCode.Call:
                {
                    var memberReference = _metadata.MemberReferences.MemberReferences[operand];
                    var memberRef = _assembly.MemberReferences[memberReference];
                    var type = ManagedRuntime.System.GetType(memberRef.ParentType);
                    if (memberRef.MemberInfo is not MethodInfo method)
                    {
                        throw new InvalidOperationException("Cannot find method.");
                    }
                    
                    for (var i = 0; i < method.Parameters.Count; i++)
                    {
                        stack.Pop();
                    }
                    
                    stack.Push(type.TypeInfo);
                    totalStack.Push(type.TypeInfo);
                    break;
                }
                case OpCode.Brtrue:
                {
                    stack.Pop();
                    break;
                }
                case OpCode.Brfalse:
                {
                    stack.Pop();
                    break;
                }
                case OpCode.Br:
                {
                    break;
                }
                case OpCode.Ceq:
                case OpCode.Cne:
                case OpCode.Cgt:
                case OpCode.Cge:
                case OpCode.Clt:
                case OpCode.Cle:
                {
                    stack.Pop();
                    stack.Pop();
                    stack.Push(ManagedRuntime.Boolean.TypeInfo);
                    totalStack.Push(ManagedRuntime.Boolean.TypeInfo);
                    break;
                }
            }
            
            if (stack.Count > maxStack)
            {
                maxStack = stack.Count;
            }
        }

        var maxStackSize = totalStack.Sum(type => type.Size);
        return (maxStackSize, maxStack);
    }

    public unsafe StackFrame Invoke()
    {
        switch (_method.IsStatic)
        {
            case true when _instance is not null:
                throw new InvalidOperationException("Cannot execute a static method on an instance.");
            case false when _instance is null:
                throw new InvalidOperationException("Cannot execute an instance method without an instance.");
        }

        if (_method.IsRuntimeMethod)
        {
            // Determine the runtime method to execute.
            if (_method.Parent.Name == "archive")
            {
                // Some methods are templates
                // Example: archive::Read`int
                // We need to get the name of the method, and it's template arguments.
                var methodName = _method.Name;
                var nameBuilder = new StringBuilder();
                var templateArguments = new List<RuntimeType>();
                var templateStart = 0;
                for (var i = 0; i < methodName.Length; i++)
                {
                    var character = methodName[i];
                    if (character == '`')
                    {
                        templateStart = i + 1; // Skip the `
                        break;
                    }

                    nameBuilder.Append(character);
                }
                
                var typeArgBuilder = new StringBuilder();
                for (var i = templateStart; i < methodName.Length; i++)
                {
                    var character = methodName[i];
                    if (character == '`')
                    {
                        var typeArg = ManagedRuntime.System.GetType(typeArgBuilder.ToString());
                        templateArguments.Add(typeArg);
                        typeArgBuilder.Clear();
                        continue;
                    }

                    typeArgBuilder.Append(character);
                }

                var name = nameBuilder.ToString();
                switch (name)
                {
                    case "SwapArrayProperty":
                    {
                        var searchObject = _arguments.ValueAt(0);
                        var replaceObject = _arguments.ValueAt(1);

                        if (_instance is not ManagedArchive archive)
                        {
                            ThrowUnexpectedValue();
                            return _stackFrame;
                        }

                        FactoryUtils.ASSET = archive.Archive;
                        
                        if (searchObject is ManagedArrayObject searchArray && replaceObject is ManagedArrayObject replaceArray)
                        {
                            archive.Archive.Swap(searchArray.ArrayPropertyData, replaceArray.ArrayPropertyData);
                        }
                        else
                        {
                            ThrowUnexpectedValue();
                            return _stackFrame;
                        }

                        break;
                    }
                    case "SwapSoftObjectProperty":
                    {
                        var searchObject = _arguments.ValueAt(0);
                        var replaceObject = _arguments.ValueAt(1);

                        if (_instance is not ManagedArchive archive)
                        {
                            ThrowUnexpectedValue();
                            return _stackFrame;
                        }

                        FactoryUtils.ASSET = archive.Archive;
                        
                        if (searchObject is ManagedSoftObject searchSoftObject && replaceObject is ManagedSoftObject replaceSoftObject)
                        {
                            archive.Archive.Swap(searchSoftObject.SoftObjectPropertyData, replaceSoftObject.SoftObjectPropertyData);
                        }
                        else
                        {
                            ThrowUnexpectedValue();
                            return _stackFrame;
                        }

                        break;
                    }
                    case "SwapLinearColorProperty":
                    {
                        var searchObject = _arguments.ValueAt(0);
                        var replaceObject = _arguments.ValueAt(1);

                        if (_instance is not ManagedArchive archive)
                        {
                            ThrowUnexpectedValue();
                            return _stackFrame;
                        }

                        FactoryUtils.ASSET = archive.Archive;
                        
                        if (searchObject is ManagedLinearColorObject searchColor && replaceObject is ManagedLinearColorObject replaceColor)
                        {
                            archive.Archive.Swap(searchColor.LinearColorPropertyData, replaceColor.LinearColorPropertyData);
                        }
                        else
                        {
                            ThrowUnexpectedValue();
                            return _stackFrame;
                        }

                        break;
                    }
                    case "CreateArrayProperty":
                    {
                        var arrayObject = _arguments.ValueAt(0);

                        if (arrayObject is not ManagedArray managedArray)
                        {
                            ThrowUnexpectedValue();
                            return _stackFrame;
                        }
                        
                        if (_instance is not ManagedArchive archive)
                        {
                            ThrowUnexpectedValue();
                            return _stackFrame;
                        }

                        var managedSoftObjectList = managedArray.Elements.Cast<ManagedSoftObject>();
                        var softObjectList = managedSoftObjectList.Select(obj => obj.SoftObjectPropertyData).ToList();

                        FactoryUtils.ASSET = archive.Archive;

                        var stackPtr = _stackFrame.Allocate(8);
                        var data = ArrayFactory.Create(softObjectList);
                        var managedArrayObject = new ManagedArrayObject(data, stackPtr);
                        _stackFrame.Push(managedArrayObject);

                        break;
                    }
                    case "CreateLinearColorProperty":
                    {
                        var redObject = _arguments.ValueAt(0);
                        var greenObject = _arguments.ValueAt(1);
                        var blueObject = _arguments.ValueAt(2);
                        var alphaObject = _arguments.ValueAt(3);

                        if (redObject is not ManagedObject redManagedObject 
                            || greenObject is not ManagedObject greenManagedObject 
                            || blueObject is not ManagedObject blueManagedObject 
                            || alphaObject is not ManagedObject alphaManagedObject)
                        {
                            ThrowUnexpectedValue();
                            return _stackFrame;
                        }
                        
                        if (_instance is not ManagedArchive archive)
                        {
                            ThrowUnexpectedValue();
                            return _stackFrame;
                        }

                        float red = MemoryUtils.GetValue<float>(redManagedObject.Pointer);
                        float green = MemoryUtils.GetValue<float>(greenManagedObject.Pointer);
                        float blue = MemoryUtils.GetValue<float>(blueManagedObject.Pointer);
                        float alpha = MemoryUtils.GetValue<float>(alphaManagedObject.Pointer);

                        FactoryUtils.ASSET = archive.Archive;

                        var stackPtr = _stackFrame.Allocate(8);
                        var data = ColorFactory.Create(red, green, blue, alpha);
                        var managedLinearColorObject = new ManagedLinearColorObject(data, stackPtr);
                        _stackFrame.Push(managedLinearColorObject);

                        break;
                    }
                    case "CreateSoftObjectProperty":
                    {
                        var other = _arguments.First().Value;
                        if (other is not ManagedString softObjectStr)
                        {
                            ThrowUnexpectedValue();
                            return _stackFrame;
                        }

                        if (_instance is not ManagedArchive archive)
                        {
                            ThrowUnexpectedValue();
                            return _stackFrame;
                        }
                        
                        string substring = "";
                        if (_arguments.Count > 1)
                        {
                            if (_arguments.ValueAt(1) is not ManagedString subString)
                            {
                                ThrowUnexpectedValue();
                                return _stackFrame;
                            }

                            substring = subString.ToString();
                        }

                        FactoryUtils.ASSET = archive.Archive;

                        var stackPtr = _stackFrame.Allocate(8);
                        var data = SoftObjectFactory.Create(softObjectStr.ToString(), substring);
                        var managedSoftObject = new ManagedSoftObject(data, stackPtr);
                        _stackFrame.Push(managedSoftObject);

                        break;
                    }
                    case "Swap":
                    {
                        var other = _arguments.First().Value;
                        if (other is not ManagedArchive newArchive)
                        {
                            ThrowUnexpectedValue();
                            return _stackFrame;
                        }

                        if (_instance is not ManagedArchive archive)
                        {
                            ThrowUnexpectedValue();
                            return _stackFrame;
                        }
                        
                        archive.SetArchive(archive.Archive.Swap(newArchive.Archive));
                        break;
                    }
                    case "Import":
                    {
                        var value = _arguments.First().Value;
                        if (value is not ManagedString managedString)
                        {
                            ThrowUnexpectedValue();
                            return _stackFrame;
                        }
                        
                        var str = managedString.ToString();

                        var stackPtr = _stackFrame.Allocate(8);
                        
                        byte[] byteData = GlobalFileProvider.Provider.SaveAsset(str);
                        var archive = new ZenAsset(new AssetBinaryReader(byteData), EngineVersion.VER_LATEST, Usmap.CachedMappings);
                        var managedArchive = new ManagedArchive(archive, stackPtr);
                        _stackFrame.Push(managedArchive);

                        break;
                    }
                }
            }
            
            return _stackFrame;
        }

        RunInstructions();
        if (_method.Type == ManagedRuntime.Void.TypeInfo)
        {
            if (_stackFrame.EvaluationStackSize == 0)
            {
                return _stackFrame;
            }

            const string message = "Evaluation stack is not empty on a void method.";
            Logger.Log(message, LogLevel.Error);
            throw new InvalidOperationException(message);
        }

        switch (_stackFrame.EvaluationStackSize)
        {
            case 0:
            {
                const string message = "Evaluation stack is empty on a non-void method.";
                Logger.Log(message, LogLevel.Error);
                throw new InvalidOperationException(message);
            }
            case > 1:
            {
                const string message = "Evaluation stack has more than one item on a non-void method.";
                Logger.Log(message, LogLevel.Error);
                throw new InvalidOperationException(message);
            }
        }

        _stackFrame.ReturnObject = _stackFrame.Pop();
        return _stackFrame;
    }

    private unsafe void RunInstructions()
    {
        for (var label = 0; label < _instructions.Length; label++)
        {
            var instruction = _instructions[label];
            var opCode = instruction.OpCode;
            var operand = instruction.Operand;
            try
            {
                switch (opCode)
                {
                    case OpCode.Nop:
                    {
                        break;
                    }
                    case OpCode.Add:
                    case OpCode.Sub:
                    case OpCode.Mul:
                    case OpCode.Div:
                    case OpCode.Cnct:
                    case OpCode.Mod:
                    case OpCode.Or:
                    case OpCode.And:
                    case OpCode.Xor:
                    case OpCode.Shl:
                    case OpCode.Shr:
                    {
                        var right = _stackFrame.Pop();
                        var left = _stackFrame.Pop();
                        var result = left.ComputeOperation(opCode, right, _stackFrame);
                        _stackFrame.Push(result);
                        break;
                    }
                    case OpCode.Neg:
                    {
                        var value = _stackFrame.Pop();
                        var result = value.ComputeOperation(opCode, null, _stackFrame);
                        _stackFrame.Push(result);
                        break;
                    }
                    case OpCode.Ldc:
                    {
                        var constant = _metadata.Constants.Constants[operand];
                        var value = ConvertConstant(constant);
                        _stackFrame.Push(value);
                        break;
                    }
                    case OpCode.Ldlen:
                    {
                        var array = _stackFrame.Pop();
                        if (array is not ManagedReference reference)
                        {
                            ThrowUnexpectedValue();
                            return;
                        }

                        var managedArray = (ManagedArray)ManagedRuntime.HeapManager.GetObject(reference.Target);
                        var length = managedArray.Length;
                        var value = _stackFrame.AllocatePrimitive(ManagedRuntime.Int32, length);
                        _stackFrame.Push(value);
                        break;
                    }
                    case OpCode.Ldstr:
                    {
                        var constant = _metadata.Constants.Constants[operand];
                        var str = _metadata.Strings.Strings[constant.ValueOffset];
                        var value = _stackFrame.AllocateString(str);
                        _stackFrame.Push(value);
                        break;
                    }
                    case OpCode.Lddft:
                    {
                        var typeDef = _metadata.Types.Types[instruction.Operand];
                        var type = ManagedRuntime.System.GetType(typeDef);
                        var value = type.TypeInfo.IsArray 
                            ? _stackFrame.AllocateArray(type, 0) 
                            : _stackFrame.AllocateObject(type);
                        _stackFrame.Push(value);
                        break;
                    }
                    case OpCode.Ldloc:
                    case OpCode.Stloc:
                    {
                        var local = _metadata.Locals.Locals[operand];
                        var localInfo = _method.Locals[local];
                        if (instruction.OpCode == OpCode.Ldloc)
                        {
                            var value = _stackFrame.GetLocal(localInfo);
                            _stackFrame.Push(value);
                        }
                        else
                        {
                            var value = _stackFrame.Pop();
                            _stackFrame.SetLocal(localInfo, value);
                        }
                        
                        break;
                    }
                    case OpCode.Ldarg:
                    case OpCode.Starg:
                    {
                        var argument = _metadata.Parameters.Parameters[operand];
                        var parameter = _method.Parameters[argument];
                        if (instruction.OpCode == OpCode.Ldarg)
                        {
                            var value = _stackFrame.GetArgument(parameter);
                            _stackFrame.Push(value);
                        }
                        else
                        {
                            var value = _stackFrame.Pop();
                            _stackFrame.SetArgument(parameter, value);
                        }
                        
                        break;
                    }
                    case OpCode.Ldfld:
                    case OpCode.Stfld:
                    {
                        var memberReference = _metadata.MemberReferences.MemberReferences[operand];
                        var memberRef = _assembly.MemberReferences[memberReference];
                        var instance = _stackFrame.Pop();
                        if (memberRef.MemberInfo is not FieldInfo field)
                        {
                            throw new InvalidOperationException($"Cannot load member of type '{memberRef.MemberType}' with this instruction.");
                        }

                        ManagedObject obj;
                        switch (instance)
                        {
                            case ManagedReference managedReference:
                            {
                                var reference = ManagedRuntime.HeapManager.GetObject(managedReference.Target);
                                if (reference is not ManagedObject managedObject)
                                {
                                    throw new InvalidOperationException("Cannot load a field from an instance that is not an object.");
                                }
                            
                                obj = managedObject;
                                break;
                            }
                            case ManagedObject managedObject:
                                obj = managedObject;
                                break;
                            default:
                                throw new InvalidOperationException("Cannot load a field from an instance that is not an object.");
                        }
                        
                        if (instruction.OpCode == OpCode.Ldfld)
                        {
                            var value = obj.GetField(field);
                            _stackFrame.Push(value);
                        }
                        else
                        {
                            var value = _stackFrame.Pop();
                            obj.SetField(field, value);
                        }
                        
                        break;
                    }
                    case OpCode.Ldsfld:
                    case OpCode.Stsfld:
                    {
                        var memberReference = _metadata.MemberReferences.MemberReferences[operand];
                        var memberRef = _assembly.MemberReferences[memberReference];
                        if (memberRef.MemberInfo is not FieldInfo field)
                        {
                            throw new InvalidOperationException($"Cannot load member of type '{memberRef.MemberType}' with this instruction.");
                        }
                        
                        var parent = ManagedRuntime.System.GetType(field.Parent);
                        if (instruction.OpCode == OpCode.Ldsfld)
                        {
                            var value = parent.GetStaticField(field);
                            _stackFrame.Push(value);
                        }
                        else
                        {
                            var value = _stackFrame.Pop();
                            parent.SetStaticField(field, value);
                        }
                        
                        break;
                    }
                    case OpCode.Ldthis:
                    {
                        if (_method.IsStatic)
                        {
                            throw new InvalidOperationException("Cannot load 'this' in a static method.");
                        }
                        
                        if (_instance == null)
                        {
                            throw new InvalidOperationException("Instance is null in a non-static method.");
                        }
                        
                        _stackFrame.Push(_instance);
                        break;
                    }
                    case OpCode.Ldelem:
                    case OpCode.Stelem:
                    {
                        var array = _stackFrame.Pop();
                        var index = _stackFrame.Pop();
                        if (index is not ManagedObject)
                        {
                            ThrowUnexpectedValue();
                            return;
                        }
                        
                        if (array is not ManagedReference reference)
                        {
                            ThrowUnexpectedValue();
                            return;
                        }
                        
                        var managedArray = (ManagedArray)ManagedRuntime.HeapManager.GetObject(reference.Target);
                        var indexValue = *(int*)index.Pointer;
                        if (instruction.OpCode == OpCode.Ldelem)
                        {
                            var value = managedArray.GetElement(indexValue);
                            _stackFrame.Push(value);
                        }
                        else
                        {
                            var value = _stackFrame.Pop();
                            managedArray.SetElement(indexValue, value);
                        }
                        
                        break;
                    }
                    case OpCode.Conv:
                    {
                        var value = _stackFrame.Pop();
                        var typeDef = _metadata.Types.Types[operand];
                        var type = ManagedRuntime.System.GetType(typeDef);
                        var converted = _stackFrame.AllocateObject(type);
                        MemoryUtils.Copy(value.Pointer, converted.Pointer, type.Size);
                        _stackFrame.Push(converted);
                        break;
                    }
                    case OpCode.Newarr:
                    {
                        var size = _stackFrame.Pop();
                        if (size is not ManagedObject)
                        {
                            ThrowUnexpectedValue();
                            return;
                        }
                        
                        var intSize = *(int*)size.Pointer;
                        var typeDef = _metadata.Types.Types[operand];
                        var type = ManagedRuntime.System.GetType(typeDef);
                        var array = _stackFrame.AllocateArray(type, intSize);
                        _stackFrame.Push(array);
                        break;
                    }
                    case OpCode.Newobj:
                    {
                        var typeReference = _metadata.TypeReferences.TypeReferences[operand];
                        var typeRef = _assembly.TypeReferences[typeReference];
                        var type = ManagedRuntime.System.GetType(typeRef.TypeDefinition);
                        var constructorRef = typeRef.ConstructorReference;
                        if (constructorRef.MemberInfo is not MethodInfo constructor)
                        {
                            throw new InvalidOperationException($"Cannot construct an object from member of type '{constructorRef.MemberType}'.");
                        }

                        var arguments = new Dictionary<ParameterInfo, RuntimeObject>();
                        for (var i = constructor.Parameters.Count - 1; i >= 0; i--)
                        {
                            var parameter = constructor.Parameters.ValueAt(i);
                            var value = _stackFrame.Pop();
                            arguments.Add(parameter, value);
                        }
                        
                        var readonlyArguments = new ReadOnlyDictionary<ParameterInfo, RuntimeObject>(arguments);
                        var stackFrame = type.Construct(_assembly, _stackFrame, readonlyArguments, constructor);
                        if (stackFrame.ReturnObject == null)
                        {
                            throw new InvalidOperationException("Constructor did not return an object.");
                        }
                        
                        _stackFrame.Push(stackFrame.ReturnObject);
                        ManagedRuntime.StackManager.DeallocateStackFrame();
                        break;
                    }
                    case OpCode.Call:
                    {
                        var memberReference = _metadata.MemberReferences.MemberReferences[operand];
                        var memberRef = _assembly.MemberReferences[memberReference];
                        var type = ManagedRuntime.System.GetType(memberRef.ParentType);
                        if (memberRef.MemberInfo is not MethodInfo method)
                        {
                            throw new InvalidOperationException($"Cannot call member of type '{memberRef.MemberType}'.");
                        }
                        
                        var instance = method.IsStatic ? null : _stackFrame.Pop();
                        var arguments = new Dictionary<ParameterInfo, RuntimeObject>();
                        for (var i = method.Parameters.Count - 1; i >= 0; i--)
                        {
                            var parameter = method.Parameters.ValueAt(i);
                            var value = _stackFrame.Pop();
                            arguments.Add(parameter, value);
                        }

                        var readonlyArguments = new ReadOnlyDictionary<ParameterInfo, RuntimeObject>(arguments);
                        if (method.IsStatic)
                        {
                            var stackFrame = type.InvokeStatic(_assembly, method, readonlyArguments);
                            var result = stackFrame.ReturnObject;
                            if (result != null)
                            {
                                _stackFrame.Push(result);
                            }
                        }
                        else
                        {
                            var methodRuntime = new MethodRuntime(_assembly, instance, method, readonlyArguments);
                            var stackFrame = methodRuntime.Invoke();
                            var result = stackFrame.ReturnObject;
                            if (result != null)
                            {
                                _stackFrame.Push(result);
                            }
                        }
                        
                        ManagedRuntime.StackManager.DeallocateStackFrame();
                        break;
                    }
                    case OpCode.Ret:
                    {
                        return;
                    }
                    case OpCode.Brtrue:
                    {
                        var value = _stackFrame.Pop();
                        if (value is not ManagedObject)
                        {
                            ThrowUnexpectedValue();
                            return;
                        }
                        
                        var branch = *(bool*)value.Pointer;
                        if (branch)
                        {
                            label = operand - 1;
                        }

                        break;
                    }
                    case OpCode.Brfalse:
                    {
                        var value = _stackFrame.Pop();
                        if (value is not ManagedObject)
                        {
                            ThrowUnexpectedValue();
                            return;
                        }
                        
                        var branch = *(bool*)value.Pointer;
                        if (!branch)
                        {
                            label = operand - 1;
                        }

                        break;
                    }
                    case OpCode.Br:
                    {
                        label = operand - 1;
                        break;
                    }
                    case OpCode.Ceq:
                    case OpCode.Cne:
                    case OpCode.Cgt:
                    case OpCode.Cge:
                    case OpCode.Clt:
                    case OpCode.Cle:
                    {
                        goto case OpCode.Add;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString(), LogLevel.Error);
                Logger.Log($"Exception occurred while executing instruction {label} ({opCode} {operand}).", LogLevel.Error);
                Logger.Log("Exiting...", LogLevel.Error);
                return;
            }
        }
    }

    private static RuntimeType GetRuntimeType(ConstantType constantType)
    {
        return constantType switch
        {
            ConstantType.Int8 => ManagedRuntime.Int8,
            ConstantType.UInt8 => ManagedRuntime.UInt8,
            ConstantType.Int16 => ManagedRuntime.Int16,
            ConstantType.UInt16 => ManagedRuntime.UInt16,
            ConstantType.Int32 => ManagedRuntime.Int32,
            ConstantType.UInt32 => ManagedRuntime.UInt32,
            ConstantType.Int64 => ManagedRuntime.Int64,
            ConstantType.UInt64 => ManagedRuntime.UInt64,
            ConstantType.Float32 => ManagedRuntime.Float32,
            ConstantType.Float64 => ManagedRuntime.Float64,
            ConstantType.String => ManagedRuntime.String,
            ConstantType.Char => ManagedRuntime.Char,
            ConstantType.Boolean => ManagedRuntime.Boolean,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private unsafe RuntimeObject ConvertConstant(Constant constant)
    {
        var pool = _metadata.ConstantsPool.Values;
        byte* ptr;
        fixed (byte* p = pool)
        {
            ptr = p + constant.ValueOffset;
        }
        
        var type = GetRuntimeType(constant.Type);
        if (type == ManagedRuntime.String)
        {
            throw new InvalidOperationException("String constants should be handled in an 'ldstr' instruction.");
        }
        
        var bytes = new byte[type.Size];
        fixed (byte* p = bytes)
        {
            for (var i = 0; i < type.Size; i++)
            {
                *(p + i) = *(ptr + i);
            }
        }
        
        var obj = _stackFrame.AllocateConstant(type, bytes);
        return obj;
    }

    private static void ThrowUnexpectedValue()
    {
        throw new InvalidOperationException("Unexpected value on the stack.");
    }
}
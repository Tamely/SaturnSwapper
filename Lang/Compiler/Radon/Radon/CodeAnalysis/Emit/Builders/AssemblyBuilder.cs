using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Binding.Semantics.Members;
using Radon.CodeAnalysis.Binding.Semantics.Operators;
using Radon.CodeAnalysis.Binding.Semantics.Statements;
using Radon.CodeAnalysis.Binding.Semantics.Types;
using Radon.CodeAnalysis.Emit.Binary;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;
using Radon.CodeAnalysis.Emit.Comparers;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax;
using Radon.Common;

namespace Radon.CodeAnalysis.Emit.Builders;

internal sealed class AssemblyBuilder
{
    private readonly TypeDefinitionComparer _typeDefinitionComparer;
    private readonly MethodComparer _methodComparer;
    private readonly FieldComparer _fieldComparer;
    private readonly LocalComparer _localComparer;
    private readonly ParameterComparer _parameterComparer;
    private AssemblyFlags _flags;
    private readonly BoundAssembly _assembly;
    private readonly List<string> _stringTable;
    private readonly List<Constant> _constantTable;
    private readonly long _encryptionKey;
    private readonly List<Sign> _signTable;
    private readonly List<TypeDefinition> _typeDefinitionTable;
    private readonly List<Field> _fieldTable;
    private readonly List<EnumMember> _enumMemberTable;
    private readonly List<Method> _methodTable;
    private readonly List<Parameter> _parameterTable;
    private readonly List<Local> _localTable;
    private readonly List<MemberReference> _memberReferenceTable;
    private readonly List<TypeReference> _typeReferenceTable;
    private readonly List<byte> _constantPool;
    private readonly List<Instruction> _instructionTable;
    private readonly Dictionary<Method, BoundMethod> _unfinishedMethodMap;
    private readonly Dictionary<Method, BoundConstructor> _unfinishedConstructorMap;
    private readonly Dictionary<TypeSymbol, TypeDefinition> _typeSymbolMap;
    private readonly Dictionary<AbstractMethodSymbol, Method> _allMethodSymbolMap;
    private readonly Dictionary<ConstructorSymbol, Method> _constructorSymbolMap;
    private readonly Dictionary<FieldSymbol, Field> _fieldSymbolMap;
    private readonly Dictionary<LocalVariableSymbol, Local> _localSymbolMap;
    private readonly Dictionary<ParameterSymbol, Parameter> _parameterSymbolMap;
    private readonly Dictionary<TypeSymbol, Dictionary<MethodSymbol, BoundMethod>> _methodMap;
    private readonly Dictionary<TypeSymbol, Dictionary<ConstructorSymbol, BoundConstructor>> _constructorMap;
    
    public AssemblyBuilder(BoundAssembly assembly)
    {
        var methodSymbolComparer = new MethodSymbolComparer();
        var random = new Random();

        _typeDefinitionComparer = new TypeDefinitionComparer();
        _methodComparer = new MethodComparer();
        _fieldComparer = new FieldComparer();
        _localComparer = new LocalComparer();
        _parameterComparer = new ParameterComparer();
        _flags = AssemblyFlags.None;
        _assembly = assembly;
        _stringTable = new List<string>();
        _constantTable = new List<Constant>();
        _encryptionKey = random.NextInt64();
        _signTable = new List<Sign>();
        _typeDefinitionTable = new List<TypeDefinition>();
        _fieldTable = new List<Field>();
        _enumMemberTable = new List<EnumMember>();
        _methodTable = new List<Method>();
        _parameterTable = new List<Parameter>();
        _localTable = new List<Local>();
        _memberReferenceTable = new List<MemberReference>();
        _typeReferenceTable = new List<TypeReference>();
        _constantPool = new List<byte>();
        _instructionTable = new List<Instruction>();
        _unfinishedMethodMap = new Dictionary<Method, BoundMethod>();
        _unfinishedConstructorMap = new Dictionary<Method, BoundConstructor>();
        _typeSymbolMap = new Dictionary<TypeSymbol, TypeDefinition>();
        _allMethodSymbolMap = new Dictionary<AbstractMethodSymbol, Method>(methodSymbolComparer);
        _constructorSymbolMap = new Dictionary<ConstructorSymbol, Method>(methodSymbolComparer);
        _fieldSymbolMap = new Dictionary<FieldSymbol, Field>();
        _localSymbolMap = new Dictionary<LocalVariableSymbol, Local>();
        _parameterSymbolMap = new Dictionary<ParameterSymbol, Parameter>();
        _methodMap = new Dictionary<TypeSymbol, Dictionary<MethodSymbol, BoundMethod>>();
        _constructorMap = new Dictionary<TypeSymbol, Dictionary<ConstructorSymbol, BoundConstructor>>();
        foreach (var type in assembly.Types)
        {
            if (type is BoundStruct or BoundArray)
            {
                ImmutableArray<BoundMember> members;
                if (type is BoundStruct boundStruct)
                {
                    members = boundStruct.Members;
                }
                else
                {
                    members = ((BoundArray)type).Members;
                }
                
                var methodMap = new Dictionary<MethodSymbol, BoundMethod>();
                var constructorMap = new Dictionary<ConstructorSymbol, BoundConstructor>();
                foreach (var method in members)
                {
                    if (method is BoundMethod boundMethod)
                    {
                        methodMap.Add(boundMethod.Symbol, boundMethod);
                    }
                    else if (method is BoundConstructor boundConstructor)
                    {
                        constructorMap.Add(boundConstructor.Symbol, boundConstructor);
                    }
                }
                
                _methodMap.Add(type.TypeSymbol, methodMap);
                _constructorMap.Add(type.TypeSymbol, constructorMap);
            }
        }
    }
    
    private static int AddToList<T>(ICollection<T> list, T value)
    {
        var index = list.IndexOf(value, null);
        if (index != -1)
        {
            return index;
        }
        
        index = list.Count;
        list.Add(value);
        return index;
    }
    
    private static int AddRangeToList<T>(List<T> list, IEnumerable<T> values)
    {
        var index = list.Count;
        list.AddRange(values);
        return index;
    }
    
    private int AddString(string value)
    {
        return AddToList(_stringTable, value);
    }

    private int EmitConstant(object value, TypeSymbol type)
    {
        ConstantType constantType;
        if (type == TypeSymbol.SByte)
        {
            constantType = ConstantType.Int8;
        }
        else if (type == TypeSymbol.Byte)
        {
            constantType = ConstantType.UInt8;
        }
        else if (type == TypeSymbol.Short)
        {
            constantType = ConstantType.Int16;
        }
        else if (type == TypeSymbol.UShort)
        {
            constantType = ConstantType.UInt16;
        }
        else if (type == TypeSymbol.Int)
        {
            constantType = ConstantType.Int32;
        }
        else if (type == TypeSymbol.UInt)
        {
            constantType = ConstantType.UInt32;
        }
        else if (type == TypeSymbol.Long)
        {
            constantType = ConstantType.Int64;
        }
        else if (type == TypeSymbol.ULong)
        {
            constantType = ConstantType.UInt64;
        }
        else if (type == TypeSymbol.Float)
        {
            constantType = ConstantType.Float32;
        }
        else if (type == TypeSymbol.Double)
        {
            constantType = ConstantType.Float64;
        }
        else if (type == TypeSymbol.String)
        {
            constantType = ConstantType.String;
            var stringIndex = AddString((string)value);
            var stringConstant = new Constant(constantType, stringIndex);
            var stringConstantIndex = AddToList(_constantTable, stringConstant);
            return stringConstantIndex;
        }
        else if (type == TypeSymbol.Char)
        {
            constantType = ConstantType.Char;
        }
        else if (type == TypeSymbol.Bool)
        {
            constantType = ConstantType.Boolean;
        }
        else
        {
            throw new NotImplementedException();
        }

        var emitter = new BinaryEmitter(value);
        var bytes = emitter.Emit();
        if (bytes.Length < type.Size)
        {
            // Resize the bytes to fit the size of the type.
            var newBytes = new byte[type.Size];
            Array.Copy(bytes, newBytes, bytes.Length);
        }
        
        var constantPoolOffset = AddRangeToList(_constantPool, bytes);
        var constant = new Constant(constantType, constantPoolOffset);
        var index = AddToList(_constantTable, constant);
        return index;
    }

    public Assembly Build()
    {
        if (_assembly.Assembly is null)
        {
            return new Assembly();
        }

        var guid = _assembly.Assembly.AssemblyId;
        CollectMetadata();
        var unfinishedMethods = _unfinishedMethodMap.Keys.Concat(_unfinishedConstructorMap.Keys);
        foreach (var method in unfinishedMethods)
        {
            BuildMethod(method);
        }
        
        var instructionTable = new InstructionTable(_instructionTable.ToArray());
        var metadata = BuildMetadata(); // Rebuild metadata to include new methods
        return new Assembly(guid, _flags, _encryptionKey, instructionTable, metadata);
    }

    private Metadata BuildMetadata()
    {
        CollectMetadata();
        var stringTable = new StringTable(_stringTable.ToArray());
        var constantTable = new ConstantTable(_constantTable.ToArray());
        var signTable = new SignTable(_signTable.ToArray());
        var typeDefinitionTable = new TypeDefinitionTable(_typeDefinitionTable.ToArray());
        var fieldTable = new FieldTable(_fieldTable.ToArray());
        var enumMemberTable = new EnumMemberTable(_enumMemberTable.ToArray());
        var methodTable = new MethodTable(_methodTable.ToArray());
        var parameterTable = new ParameterTable(_parameterTable.ToArray());
        var localTable = new LocalTable(_localTable.ToArray());
        var typeReferenceTable = new TypeReferenceTable(_typeReferenceTable.ToArray());
        var memberReferenceTable = new MemberReferenceTable(_memberReferenceTable.ToArray());
        var constantPool = new ConstantPool(_constantPool.ToArray());
        return new Metadata(stringTable, constantTable, signTable, typeDefinitionTable, fieldTable, enumMemberTable, 
            methodTable, parameterTable, localTable, typeReferenceTable, memberReferenceTable, constantPool);
    }

    private void CollectMetadata()
    {
        var types = _assembly.Types;
        foreach (var type in types)
        {
            BuildType(type.TypeSymbol);
        }
    }

    private int BuildType(TypeSymbol type)
    {
        // Check if type has already been built
        if (_typeSymbolMap.TryGetValue(type, out var typeDef))
        {
            return _typeDefinitionTable.IndexOf(typeDef, _typeDefinitionComparer);
        }
        
        var flags = BuildFlags(type.Modifiers);
        var underlyingType = -1;
        var isArray = false;
        TypeKind kind;
        switch (type)
        {
            case StructSymbol:
                kind = TypeKind.Struct;
                break;
            case EnumSymbol e:
                kind = TypeKind.Enum;
                underlyingType = BuildType(e.UnderlyingType);
                break;
            case ArrayTypeSymbol a:
                kind = TypeKind.Array;
                underlyingType = BuildType(a.ElementType);
                isArray = true;
                break;
            case BoundTypeParameterSymbol b:
                return BuildType(b.BoundType);
            default:
                throw new Exception($"Unknown type {type}");
        }

        if (!flags.HasFlag(BindingFlags.Ref))
        {
            kind |= TypeKind.ValueType;
        }
        
        if (!isArray)
        {
            // Because array are their own thing, and cannot be primitive or numeric. They are simply arrays.
            if (TypeSymbol.GetPrimitiveTypes().Contains(type))
            {
                kind |= TypeKind.Primitive;
            }

            if (TypeSymbol.GetNumericTypes().Contains(type))
            {
                kind |= TypeKind.Numeric;
                if (TypeSymbol.GetSignedNumericTypes().Contains(type))
                {
                    kind |= TypeKind.Signed;
                }
                
                if (TypeSymbol.GetFloatingPointTypes().Contains(type))
                {
                    kind |= TypeKind.FloatingPoint;
                }
            }
        }

        var index = _typeDefinitionTable.Count;
        // reserve the index for the type definition
        _typeDefinitionTable.Add(default);
        var name = AddString(type.Name);
        var fieldStartOffset = _fieldTable.Count;
        var fields = type.Members.OfType<FieldSymbol>().ToList();
        foreach (var field in fields)
        {
            _fieldTable.Add(default);
            _fieldSymbolMap.Add(field, default);
        }

        for (var i = 0; i < fields.Count; i++)
        {
            var field = fields[i];
            BuildField(field, index, fieldStartOffset + i);
        }

        var enumMemberStartOffset = _enumMemberTable.Count;
        var enumMembers = type.Members.OfType<EnumMemberSymbol>().ToList();
        for (var i = 0; i < enumMembers.Count; i++)
        {
            _enumMemberTable.Add(default);
        }

        for (var i = 0; i < enumMembers.Count; i++)
        {
            var enumMember = enumMembers[i];
            BuildEnumMember(enumMember, index, enumMemberStartOffset + i);
        }

        var methodStartOffset = _methodTable.Count;
        var methods = type.Members.OfType<MethodSymbol>().ToList();
        foreach (var method in methods)
        {
            _methodTable.Add(default);
            _allMethodSymbolMap.Add(method, default);
        }

        for (var i = 0; i < methods.Count; i++)
        {
            var method = methods[i];
            BuildMethod(method, type, index, methodStartOffset + i);
        }

        var constructorCount = type.Members.Count(m => m is ConstructorSymbol);
        var constructorStartOffset = _methodTable.Count;
        var constructors = type.Members.OfType<ConstructorSymbol>().ToList();
        ConstructorSymbol? staticConstructor = null;
        foreach (var constructor in constructors)
        {
            if (constructor.Modifiers.Contains(SyntaxKind.StaticKeyword))
            {
                staticConstructor = constructor;
                continue;
            }
            
            BuildConstructor(constructor, type, index);
        }

        var staticConstructorOffset = -1;
        if (staticConstructor != null)
        {
            staticConstructorOffset = _methodTable.Count;
            BuildConstructor(staticConstructor, type, index);
        }
        
        var size = type.Size;
        var typeDefinition = new TypeDefinition(flags, kind, name, size, underlyingType, fields.Count,
            fieldStartOffset, enumMembers.Count, enumMemberStartOffset, methods.Count, methodStartOffset, 
            constructorCount, constructorStartOffset, staticConstructorOffset);
        
        _typeSymbolMap.Add(type, typeDefinition);
        _typeDefinitionTable[index] = typeDefinition;
        return index;
    }

    private void BuildField(FieldSymbol field, int parentTypeIndex, int index)
    {
        var flags = BuildFlags(field.Modifiers);
        var name = AddString(field.Name);
        var type = BuildType(field.Type); /* We don't have to worry about the parent type because
                                                if the parent type is a struct, then the field type cannot be the same, otherwise
                                                that would create a cycle in the struct layout.
                                                We don't have to worry about the parent type being an enum because
                                                enum fields are always integer type such as int, long, short, etc...
                                             */
        var fieldDefinition = new Field(flags, name, type, parentTypeIndex, field.Offset);
        _fieldTable[index] = fieldDefinition;
        // Replace the default value with the actual field definition
        _fieldSymbolMap[field] = fieldDefinition;
    }
    
    private void BuildEnumMember(EnumMemberSymbol enumMember, int parentTypeIndex, int index)
    {
        var flags = BuildFlags(enumMember.Modifiers);
        var name = AddString(enumMember.Name);
        var type = BuildType(enumMember.UnderlyingType);
        var valueIndex = EmitConstant(enumMember.Value, enumMember.UnderlyingType);
        var enumMemberDefinition = new EnumMember(flags, name, type, valueIndex, parentTypeIndex);
        _enumMemberTable[index] = enumMemberDefinition;
    }
    
    private void BuildMethod(MethodSymbol method, TypeSymbol parentType, int parentTypeIndex, int index)
    {
        var flags = BuildFlags(method.Modifiers);
        var name = AddString(method.Name);
        var returnType = method.Type == parentType ? parentTypeIndex : BuildType(method.Type);
        // parentType = parentTypeIndex
        var parameterCount = method.Parameters.Length;
        var parameterStartOffset = -1;
        if (parameterCount > 0)
        {
            parameterStartOffset = _parameterTable.Count;
            foreach (var t in method.Parameters)
            {
                _parameterTable.Add(default);
                _parameterSymbolMap.Add(t, default);
            }

            for (var i = 0; i < method.Parameters.Length; i++)
            {
                var parameter = method.Parameters[i];
                BuildParameter(parameter, parentType, parentTypeIndex, parameterStartOffset + i);
            }
        }
        else
        {
            parameterCount = -1;
        }

        // The local, and instruction count cannot be determined until all symbols have been built.
        var boundMethod = _methodMap[parentType][method];
        var unfinishedMethod = new Method(flags, name, returnType, parentTypeIndex, parameterCount, 
            parameterStartOffset, -1, -1, -1, -1);
        _unfinishedMethodMap.Add(unfinishedMethod, boundMethod);
        _methodTable[index] = unfinishedMethod;
        _allMethodSymbolMap[method] = unfinishedMethod;
    }
    
    private void BuildConstructor(ConstructorSymbol constructor, TypeSymbol parentType, int parentTypeIndex)
    {
        var flags = BuildFlags(constructor.Modifiers);
        var name = AddString(constructor.Name);
        var returnType = BuildType(constructor.Type);
        // parentType = parentTypeIndex
        var parameterCount = constructor.Parameters.Length;
        var parameterStartOffset = -1;
        if (parameterCount > 0)
        {
            parameterStartOffset = _parameterTable.Count;
            foreach (var t in constructor.Parameters)
            {
                _parameterTable.Add(default);
                _parameterSymbolMap.Add(t, default);
            }

            for (var i = 0; i < constructor.Parameters.Length; i++)
            {
                var parameter = constructor.Parameters[i];
                BuildParameter(parameter, parentType, parentTypeIndex, parameterStartOffset + i);
            }
        }
        else
        {
            parameterCount = -1;
        }

        // The local, and instruction count cannot be determined until all symbols have been built.
        var boundConstructor = _constructorMap[parentType][constructor];
        var unfinishedConstructor = new Method(flags, name, returnType, parentTypeIndex, parameterCount, 
            parameterStartOffset, -1, -1, -1, -1);
        _unfinishedConstructorMap.Add(unfinishedConstructor, boundConstructor);
        _methodTable.Add(unfinishedConstructor);
        _constructorSymbolMap.Add(constructor, unfinishedConstructor);
        _allMethodSymbolMap.Add(constructor, unfinishedConstructor);
    }
    
    private void BuildParameter(ParameterSymbol parameter, TypeSymbol parentType, int parentTypeIndex, int index)
    {
        var name = AddString(parameter.Name);
        var type = parameter.Type == parentType ? parentTypeIndex : BuildType(parameter.Type);
        var parameterDefinition = new Parameter(name, type, parameter.Ordinal);
        _parameterTable[index] = parameterDefinition;
        _parameterSymbolMap[parameter] = parameterDefinition;
    }

    private static BindingFlags BuildFlags(ImmutableArray<SyntaxKind> modifiers)
    {
        var flags = BindingFlags.None;
        foreach (var modifier in modifiers)
        {
            if (modifier == SyntaxKind.StaticKeyword)
            {
                flags |= BindingFlags.Static;
            }

            if (modifier == SyntaxKind.RuntimeInternalKeyword)
            {
                flags |= BindingFlags.RuntimeInternal;
            }
            
            if (modifier == SyntaxKind.PublicKeyword)
            {
                flags |= BindingFlags.Public;
            }
            
            if (modifier == SyntaxKind.PrivateKeyword)
            {
                flags |= BindingFlags.NonPublic;
            }

            if (modifier == SyntaxKind.EntryKeyword)
            {
                flags |= BindingFlags.Entry;
            }

            if (modifier == SyntaxKind.RefKeyword)
            {
                flags |= BindingFlags.Ref;
            }
        }

        if (!flags.HasFlag(BindingFlags.Static))
        {
            flags |= BindingFlags.Instance;
        }

        if (!flags.HasFlag(BindingFlags.Public))
        {
            flags |= BindingFlags.NonPublic;
        }
        
        return flags;
    }

    private void BuildMethod(Method method)
    {
        MethodEmitter methodEmitter;
        if (_unfinishedMethodMap.TryGetValue(method, out var boundMethod))
        {
            methodEmitter = new MethodEmitter(this, boundMethod, method);
        }
        else if (_unfinishedConstructorMap.TryGetValue(method, out var boundConstructor))
        {
            methodEmitter = new MethodEmitter(this, boundConstructor, method);
        }
        else
        {
            throw new Exception("Method not found.");
        }

        var index = _methodTable.IndexOf(method, _methodComparer);
        var finishedMethod = methodEmitter.EmitMethod();
        _methodTable[index] = finishedMethod;
        _allMethodSymbolMap[methodEmitter.Symbol] = finishedMethod;
    }

    private sealed class MethodEmitter
    {
        private readonly AssemblyBuilder _builder;
        private readonly ImmutableArray<BoundStatement> _statements;
        private readonly Method _method;
        private readonly int _localCount;
        private readonly int _firstLocal;
        private readonly List<Instruction> _instructionTable;
        private readonly Dictionary<BoundLabel, List<int>> _unresolvedLabelMap;
        private readonly Dictionary<BoundLabel, int> _labelMap;
        private bool _isAssigning;
        private int _labelCount;

        public AbstractMethodSymbol Symbol { get; }
        
        public MethodEmitter(AssemblyBuilder builder, BoundMethod boundMethod, Method method)
        {
            _builder = builder;
            _statements = boundMethod.Statements;
            _method = method;
            _instructionTable = new List<Instruction>();
            _unresolvedLabelMap = new Dictionary<BoundLabel, List<int>>();
            _labelMap = new Dictionary<BoundLabel, int>();
            _firstLocal = _builder._localTable.Count;
            for (_localCount = 0; _localCount < boundMethod.Locals.Length; _localCount++)
            {
                var local = boundMethod.Locals[_localCount];
                var emittedLocal = new Local(_localCount, _builder.AddString(local.Name),
                    _builder.BuildType(local.Type));
                _builder._localSymbolMap.Add(local, emittedLocal);
                _builder._localTable.Add(emittedLocal);
            }

            _isAssigning = false;
            Symbol = boundMethod.Symbol;
        }
        
        public MethodEmitter(AssemblyBuilder builder, BoundConstructor boundConstructor, Method method)
        {
            _builder = builder;
            _statements = boundConstructor.Statements;
            _method = method;
            _instructionTable = new List<Instruction>();
            _unresolvedLabelMap = new Dictionary<BoundLabel, List<int>>();
            _labelMap = new Dictionary<BoundLabel, int>();
            _firstLocal = _builder._localTable.Count;
            for (_localCount = 0; _localCount < boundConstructor.Locals.Length; _localCount++)
            {
                var local = boundConstructor.Locals[_localCount];
                var emittedLocal = new Local(_localCount, _builder.AddString(local.Name),
                    _builder.BuildType(local.Type));
                _builder._localSymbolMap.Add(local, emittedLocal);
                _builder._localTable.Add(emittedLocal);
            }
            
            _isAssigning = false;
            Symbol = boundConstructor.Symbol;
        }

        public Method EmitMethod()
        {
            EmitInstructions(); // This will also emit the locals.
            foreach (var instruction in _instructionTable)
            {
                if (!instruction.OpCode.NoOperandRequired() && instruction.Operand == -1)
                {
                    throw new Exception($"Operand required on '{instruction.OpCode}' instruction.");
                }

                if (instruction.OpCode.NoOperandRequired() && instruction.Operand != -1)
                {
                    throw new Exception($"Operand not required on '{instruction.OpCode}' instruction.");
                }
            }
            
            var instructionCount = _instructionTable.Count;
            var firstInstruction = _builder._instructionTable.Count;
            _builder._instructionTable.AddRange(_instructionTable);
            return new Method(_method.Flags, _method.Name, _method.ReturnType, _method.Parent, _method.ParameterCount, 
                _method.FirstParameter, _localCount, _firstLocal, instructionCount, firstInstruction);
        }
        
        private int GetPosition()
        {
            return _instructionTable.Count - 1;
        }
        
        private void EmitInstructions()
        {
            var index = 0;
            while (index < _statements.Length)
            {
                var statement = _statements[index];
                EmitStatement(statement);
                index++;
            }
        }
        
        private void EmitInstruction(OpCode opCode, int operand = -1)
        {
            var instruction = new Instruction(_labelCount++, opCode, operand);
            _instructionTable.Add(instruction);
        }

        private void EmitStatement(BoundStatement statement)
        {
            switch (statement)
            {
                case BoundBlockStatement blockStatement:
                    EmitBlockStatement(blockStatement);
                    break;
                case BoundExpressionStatement expressionStatement:
                    EmitExpressionStatement(expressionStatement);
                    break;
                case BoundSignStatement signStatement:
                    EmitSignStatement(signStatement);
                    break;
                case BoundVariableDeclarationStatement variableDeclarationStatement:
                    EmitVariableDeclarationStatement(variableDeclarationStatement);
                    break;
                case BoundReturnStatement returnStatement:
                    EmitReturnStatement(returnStatement);
                    break;
                case BoundConditionalGotoStatement conditionalGotoStatement:
                    EmitConditionalGotoStatement(conditionalGotoStatement);
                    break;
                case BoundGotoStatement gotoStatement:
                    EmitGotoStatement(gotoStatement);
                    break;
                case BoundLabelStatement labelStatement:
                    EmitLabelStatement(labelStatement);
                    break;
            }
        }
        
        private void EmitBlockStatement(BoundBlockStatement blockStatement)
        {
            var statements = blockStatement.Statements;
            foreach (var statement in statements)
            {
                EmitStatement(statement);
            }
        }
        
        private void EmitExpressionStatement(BoundExpressionStatement expressionStatement)
        {
            EmitExpression(expressionStatement.Expression);
        }
        
        private void EmitSignStatement(BoundSignStatement signStatement)
        {
            var key = signStatement.Key;
            var value = signStatement.Value;
            if (key.ToLower() == "encryption" &&
                value.ToLower() == "true")
            {
                _builder._flags |= AssemblyFlags.Encryption;
            }

            key = key.Encrypt(_builder._encryptionKey);
            value = value.Encrypt(_builder._encryptionKey);
            _builder._signTable.Add(new Sign(key, value));
        }
        
        private void EmitVariableDeclarationStatement(BoundVariableDeclarationStatement variableDeclarationStatement)
        {
            if (variableDeclarationStatement.Initializer is null)
            {
                throw new Exception("Variable declaration statement must have an initializer.");
            }
            
            EmitExpression(variableDeclarationStatement.Initializer);
            var local = _builder._localSymbolMap[variableDeclarationStatement.Variable];
            var localIndex = _builder._localTable.IndexOf(local);
            EmitInstruction(OpCode.Stloc, localIndex);
        }
        
        private void EmitReturnStatement(BoundReturnStatement returnStatement)
        {
            if (returnStatement.Expression is not null)
            {
                EmitExpression(returnStatement.Expression);
            }
            
            EmitInstruction(OpCode.Ret);
        }
        
        private void EmitConditionalGotoStatement(BoundConditionalGotoStatement conditionalGotoStatement)
        {
            /*
             * IL:
             *
             * // if <condition>
             *      <then>
             * else
             *      <else>
             *
             * ---->
             *
             * gotoFalse <condition> else
             * <then>
             * goto end
             * else:
             * <else>
             * end:
             *
             * ---->
             *
             * <condition>  // Condition statement
             * brfalse else // Conditional goto statement
             * <then>       // Then statement
             * br end       // Goto statement
             * else:        // Else label
             * <else>       // Else statement
             * end:         // End label
             */
            
            /*
             * The concept is that we're told that label "else" exists, but we don't know the position yet
             * So we will first emit a br<cond> instruction, with -1 as the operand
             * We will save the position of the opcode to a dictionary and the bound label
             * So when come across the bound label statement, we can resolve the opcode
             * So when come across the bound label statement, we can resolve the opcode
             */
            
            var opcode = conditionalGotoStatement.JumpIfTrue ? OpCode.Brtrue : OpCode.Brfalse;
            EmitExpression(conditionalGotoStatement.Condition);
            if (_labelMap.TryGetValue(conditionalGotoStatement.Label, out var label))
            {
                EmitInstruction(opcode, label);
                return;
            }
            
            EmitInstruction(opcode);
            if (!_unresolvedLabelMap.ContainsKey(conditionalGotoStatement.Label))
            {
                _unresolvedLabelMap.Add(conditionalGotoStatement.Label, new List<int> { GetPosition() });
            }
            else
            {
                var list = _unresolvedLabelMap[conditionalGotoStatement.Label];
                list.Add(GetPosition());
            }
        }

        private void EmitGotoStatement(BoundGotoStatement gotoStatement)
        {
            if (_labelMap.TryGetValue(gotoStatement.Label, out var label))
            {
                EmitInstruction(OpCode.Br, label);
                return;
            }
            
            EmitInstruction(OpCode.Br);
            if (!_unresolvedLabelMap.ContainsKey(gotoStatement.Label))
            {
                _unresolvedLabelMap.Add(gotoStatement.Label, new List<int> { GetPosition() });
            }
            else
            {
                var list = _unresolvedLabelMap[gotoStatement.Label];
                list.Add(GetPosition());
            }
        }

        private void EmitLabelStatement(BoundLabelStatement labelStatement)
        {
            var position = _labelCount;
            EmitInstruction(OpCode.Nop);
            _labelMap.TryAdd(labelStatement.Label, position);
            if (!_unresolvedLabelMap.TryGetValue(labelStatement.Label, out var unresolvedPosition))
            {
                return;
            }
            
            foreach (var unresolvedPos in unresolvedPosition)
            {
                var instruction = _instructionTable[unresolvedPos];
                _instructionTable[unresolvedPos] = new Instruction(instruction.Label, instruction.OpCode, position);
            }
        }

        private void EmitExpression(BoundExpression expression)
        {
            switch (expression)
            {
                case BoundAssignmentExpression assignmentExpression:
                    EmitAssignmentExpression(assignmentExpression);
                    break;
                case BoundBinaryExpression binaryExpression:
                    EmitBinaryExpression(binaryExpression);
                    break;
                case BoundConversionExpression conversionExpression:
                    EmitConversionExpression(conversionExpression);
                    break;
                case BoundDefaultExpression defaultExpression:
                    EmitDefaultExpression(defaultExpression);
                    break;
                case BoundInvocationExpression invocationExpression:
                    EmitInvocationExpression(invocationExpression);
                    break;
                case BoundLiteralExpression literalExpression:
                    EmitLiteralExpression(literalExpression);
                    break;
                case BoundMemberAccessExpression memberAccessExpression:
                    EmitMemberAccessExpression(memberAccessExpression);
                    break;
                case BoundNameExpression nameExpression:
                    EmitNameExpression(nameExpression);
                    break;
                case BoundNewExpression newExpression:
                    EmitNewExpression(newExpression);
                    break;
                case BoundNewArrayExpression newArrayExpression:
                    EmitNewArrayExpression(newArrayExpression);
                    break;
                case BoundElementAccessExpression elementAccessExpression:
                    EmitElementAccessExpression(elementAccessExpression);
                    break;
                case BoundThisExpression:
                    EmitThisExpression();
                    break;
                case BoundUnaryExpression unaryExpression:
                    EmitUnaryExpression(unaryExpression);
                    break;
            }
        }

        private void EmitAssignmentExpression(BoundAssignmentExpression expression)
        {
            EmitExpression(expression.Right);
            _isAssigning = true;
            EmitExpression(expression.Left);
            _isAssigning = false;
        }

        private void EmitBinaryExpression(BoundBinaryExpression expression)
        {
            EmitExpression(expression.Left);
            EmitExpression(expression.Right);
            switch (expression.Op.Kind)
            {
                case BoundBinaryOperatorKind.BitwiseOr:
                    EmitInstruction(OpCode.Or);
                    break;
                case BoundBinaryOperatorKind.BitwiseXor:
                    EmitInstruction(OpCode.Xor);
                    break;
                case BoundBinaryOperatorKind.BitwiseAnd:
                    EmitInstruction(OpCode.And);
                    break;
                case BoundBinaryOperatorKind.Equality:
                    EmitInstruction(OpCode.Ceq);
                    break;
                case BoundBinaryOperatorKind.Inequality:
                    EmitInstruction(OpCode.Cne);
                    break;
                case BoundBinaryOperatorKind.LessThan:
                    EmitInstruction(OpCode.Clt);
                    break;
                case BoundBinaryOperatorKind.LessThanOrEqual:
                    EmitInstruction(OpCode.Cle);
                    break;
                case BoundBinaryOperatorKind.GreaterThan:
                    EmitInstruction(OpCode.Cgt);
                    break;
                case BoundBinaryOperatorKind.GreaterThanOrEqual:
                    EmitInstruction(OpCode.Cge);
                    break;
                case BoundBinaryOperatorKind.LeftShift:
                    EmitInstruction(OpCode.Shl);
                    break;
                case BoundBinaryOperatorKind.RightShift:
                    EmitInstruction(OpCode.Shr);
                    break;
                case BoundBinaryOperatorKind.Addition:
                    EmitInstruction(OpCode.Add);
                    break;
                case BoundBinaryOperatorKind.Concatenation:
                    EmitInstruction(OpCode.Cnct);
                    break;
                case BoundBinaryOperatorKind.Subtraction:
                    EmitInstruction(OpCode.Sub);
                    break;
                case BoundBinaryOperatorKind.Multiplication:
                    EmitInstruction(OpCode.Mul);
                    break;
                case BoundBinaryOperatorKind.Division:
                    EmitInstruction(OpCode.Div);
                    break;
                case BoundBinaryOperatorKind.Modulus:
                    EmitInstruction(OpCode.Mod);
                    break;
                default:
                    throw new Exception($"Binary operator kind {expression.Op.Kind} is not implemented.");
            }
        }

        private void EmitConversionExpression(BoundConversionExpression expression)
        {
            EmitExpression(expression.Expression);
            var type = _builder.BuildType(expression.Type);
            EmitInstruction(OpCode.Conv, type);
        }
        
        private void EmitDefaultExpression(BoundDefaultExpression expression)
        {
            var type = _builder.BuildType(expression.Type);
            EmitInstruction(OpCode.Lddft, type);
        }

        private void EmitInvocationExpression(BoundInvocationExpression expression)
        {
            var method = expression.Method;
            if (method is { ParentType: ArrayTypeSymbol, Name: "Length" })
            {
                EmitExpression(expression.Expression);
                EmitInstruction(OpCode.Ldlen);
                return;
            }
            
            var emittedMethod = _builder._allMethodSymbolMap[method];
            var methodIndex = _builder._methodTable.IndexOf(emittedMethod, _builder._methodComparer);
            var arguments = expression.Arguments;
            foreach (var argument in arguments)
            {
                EmitExpression(argument);
            }
            
            EmitExpression(expression.Expression);
            var returnType = _builder.BuildType(expression.Type);
            var parentType = _builder.BuildType(method.ParentType);
            var memberReference = new MemberReference(MemberType.Method, parentType, returnType, 
                methodIndex);
            var memberReferenceIndex = AddToList(_builder._memberReferenceTable, memberReference);
            EmitInstruction(OpCode.Call, memberReferenceIndex);
        }

        private void EmitLiteralExpression(BoundLiteralExpression expression)
        {
            var constantIndex = _builder.EmitConstant(expression.Value, expression.Type);
            EmitInstruction(expression.Type == TypeSymbol.String ? OpCode.Ldstr : OpCode.Ldc, constantIndex);
        }
        
        private void EmitMemberAccessExpression(BoundMemberAccessExpression expression)
        {
            var isStatic = expression.Member.Modifiers.Contains(SyntaxKind.StaticKeyword);
            switch (expression.Member.Kind)
            {
                case SymbolKind.Field:
                    EmitFieldAccessExpression(expression, isStatic);
                    break;
                case SymbolKind.EnumMember:
                    EmitEnumMemberAccessExpression(expression);
                    break;
                case SymbolKind.Method:
                    if (!isStatic)
                    {
                        EmitExpression(expression.Expression);
                    }
                    
                    break;
                default:
                    throw new NotImplementedException($"Member access expression of kind {expression.Member.Kind} is not implemented.");
            }
        }
        
        private void EmitFieldAccessExpression(BoundMemberAccessExpression expression, bool isStatic)
        {
            var instruction = _isAssigning ? isStatic ? OpCode.Stsfld : OpCode.Stfld : isStatic ? OpCode.Ldsfld : OpCode.Ldfld;
            _isAssigning = false;
            var field = (FieldSymbol)expression.Member;
            var emittedField = _builder._fieldSymbolMap[field];
            var fieldIndex = _builder._fieldTable.IndexOf(emittedField, _builder._fieldComparer);
            var fieldType = _builder.BuildType(field.Type);
            var parentType = _builder.BuildType(field.ParentType);
            var memberReference = new MemberReference(MemberType.Field, parentType, fieldType, 
                fieldIndex);
            var memberReferenceIndex = AddToList(_builder._memberReferenceTable, memberReference);
            if (!isStatic)
            {
                EmitExpression(expression.Expression); // Instance
            }
            
            EmitInstruction(instruction, memberReferenceIndex);
        }
        
        private void EmitEnumMemberAccessExpression(BoundMemberAccessExpression expression)
        {
            var enumMember = (EnumMemberSymbol)expression.Member;
            var value = enumMember.Value;
            var constantIndex = _builder.EmitConstant(value, expression.Type);
            EmitInstruction(OpCode.Ldc, constantIndex);
        }
        
        private void EmitNameExpression(BoundNameExpression expression)
        {
            var symbol = expression.Symbol;
            switch (symbol.Kind)
            {
                case SymbolKind.LocalVariable:
                    EmitLocalExpression(expression);
                    break;
                case SymbolKind.Parameter:
                    EmitParameterExpression(expression);
                    break;
                default:
                    throw new NotImplementedException($"Name expression of kind {symbol.Kind} is not implemented.");
            }
        }
        
        private void EmitNewExpression(BoundNewExpression expression)
        {
            var constructor = expression.Constructor;
            var emittedConstructor = _builder._constructorSymbolMap[constructor];
            var constructorIndex = _builder._methodTable.IndexOf(emittedConstructor, _builder._methodComparer);
            var arguments = expression.Arguments;
            foreach (var argument in arguments)
            {
                EmitExpression(argument);
            }

            var returnType = _builder.BuildType(expression.Type);
            var memberReference = new MemberReference(MemberType.Constructor, returnType, 
                returnType, constructorIndex);
            var memberReferenceIndex = AddToList(_builder._memberReferenceTable, memberReference);
            var typeReference = new TypeReference(returnType, memberReferenceIndex);
            var typeReferenceIndex = AddToList(_builder._typeReferenceTable, typeReference);
            EmitInstruction(OpCode.Newobj, typeReferenceIndex);
        }
        
        private void EmitNewArrayExpression(BoundNewArrayExpression expression)
        {
            var arrayType = _builder.BuildType(expression.Type);
            EmitExpression(expression.SizeExpression);
            EmitInstruction(OpCode.Newarr, arrayType);
        }
        
        private void EmitElementAccessExpression(BoundElementAccessExpression expression)
        {
            var instruction = _isAssigning ? OpCode.Stelem : OpCode.Ldelem;
            _isAssigning = false;
            EmitExpression(expression.IndexExpression);
            EmitExpression(expression.Expression); // Array instance
            EmitInstruction(instruction);
        }
        
        private void EmitLocalExpression(BoundNameExpression expression)
        {
            var local = (LocalVariableSymbol)expression.Symbol;
            var emittedLocal = _builder._localSymbolMap[local];
            var localIndex = _builder._localTable.IndexOf(emittedLocal, _builder._localComparer);
            EmitInstruction(_isAssigning ? OpCode.Stloc : OpCode.Ldloc, localIndex);
        }
        
        private void EmitParameterExpression(BoundNameExpression expression)
        {
            var parameter = (ParameterSymbol)expression.Symbol;
            var emittedParameter = _builder._parameterSymbolMap[parameter];
            var parameterIndex = _builder._parameterTable.IndexOf(emittedParameter, _builder._parameterComparer);
            EmitInstruction(_isAssigning ? OpCode.Starg : OpCode.Ldarg, parameterIndex);
        }
        
        private void EmitThisExpression()
        {
            EmitInstruction(OpCode.Ldthis);
        }
        
        private void EmitUnaryExpression(BoundUnaryExpression expression)
        {
            EmitExpression(expression.Operand);
            switch (expression.Op.Kind)
            {
                case BoundUnaryOperatorKind.Identity:
                    EmitInstruction(OpCode.Nop);
                    break;
                case BoundUnaryOperatorKind.Negation:
                    EmitInstruction(OpCode.Neg);
                    break;
                default:
                    throw new NotImplementedException($"Unary expression of kind {expression.Op.Kind} is not implemented.");
            }
        }
    }
}

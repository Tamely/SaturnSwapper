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
    // Comparers
    private readonly TypeDefinitionComparer _typeDefinitionComparer;
    private readonly MethodComparer _methodComparer;
    private readonly FieldComparer _fieldComparer;
    private readonly LocalComparer _localComparer;
    private readonly ParameterComparer _parameterComparer;
    private readonly TypeSymbolComparer _typeSymbolComparer;
    
    // Assembly
    private readonly BoundAssembly _assembly;
    private readonly Guid _guid;
    private AssemblyFlags _flags;
    private readonly long _encryptionKey;
    private readonly List<Instruction> _instructions;
    private readonly List<string> _strings;
    private readonly List<Constant> _constants;
    private readonly List<Sign> _signs;
    private readonly List<TypeDefinition> _types;
    private readonly List<Field> _fields;
    private readonly List<EnumMember> _enumMembers;
    private readonly List<Method> _methods;
    private readonly List<Parameter> _parameters;
    private readonly List<Local> _locals;
    private readonly List<TypeReference> _typeReferences;
    private readonly List<MemberReference> _memberReferences;
    private readonly List<byte> _constantsPool;
    
    // Symbol Maps
    private readonly SymbolMap<TypeSymbol, TypeDefinition> _typeSymbolMap;
    private readonly SymbolMap<FieldSymbol, Field> _fieldSymbolMap;
    // ReSharper disable once CollectionNeverQueried.Local
    private readonly SymbolMap<EnumMemberSymbol, EnumMember> _enumMemberSymbolMap;
    private readonly SymbolMap<MethodSymbol, Method> _methodSymbolMap;
    private readonly SymbolMap<ConstructorSymbol, Method> _constructorSymbolMap;
    private readonly SymbolMap<LocalVariableSymbol, Local> _localSymbolMap;
    private readonly SymbolMap<ParameterSymbol, Parameter> _parameterSymbolMap;
    
    // Method Maps
    private readonly Dictionary<Method, BoundMethod> _unfinishedMethodMap;
    private readonly Dictionary<Method, BoundConstructor> _unfinishedConstructorMap;
    private readonly Dictionary<TypeSymbol, Dictionary<MethodSymbol, BoundMethod>> _methodMap;
    private readonly Dictionary<TypeSymbol, Dictionary<ConstructorSymbol, BoundConstructor>> _constructorMap;
    
    // Others
    private readonly Dictionary<TypeSymbol, int> _typeStack;
    
    
    
    // Constructor
    public AssemblyBuilder(BoundAssembly assembly)
    {
        // Comparers
        _typeDefinitionComparer = new TypeDefinitionComparer();
        _methodComparer = new MethodComparer();
        _fieldComparer = new FieldComparer();
        _localComparer = new LocalComparer();
        _parameterComparer = new ParameterComparer();
        _typeSymbolComparer = new TypeSymbolComparer();
        
        // Assembly
        _assembly = assembly;
        _guid = assembly.Assembly?.AssemblyId ?? throw new Exception("Assembly is null.");
        _flags = AssemblyFlags.Encryption;
        var random = new Random();
        _encryptionKey = random.NextInt64();
        _instructions = new List<Instruction>();
        _strings = new List<string>();
        _constants = new List<Constant>();
        _signs = new List<Sign>();
        _types = new List<TypeDefinition>();
        _fields = new List<Field>();
        _enumMembers = new List<EnumMember>();
        _methods = new List<Method>();
        _parameters = new List<Parameter>();
        _locals = new List<Local>();
        _typeReferences = new List<TypeReference>();
        _memberReferences = new List<MemberReference>();
        _constantsPool = new List<byte>();
        
        // Symbol Maps
        _typeSymbolMap = new SymbolMap<TypeSymbol, TypeDefinition>();
        _fieldSymbolMap = new SymbolMap<FieldSymbol, Field>();
        _enumMemberSymbolMap = new SymbolMap<EnumMemberSymbol, EnumMember>();
        _methodSymbolMap = new SymbolMap<MethodSymbol, Method>();
        _constructorSymbolMap = new SymbolMap<ConstructorSymbol, Method>();
        _localSymbolMap = new SymbolMap<LocalVariableSymbol, Local>();
        _parameterSymbolMap = new SymbolMap<ParameterSymbol, Parameter>();
        
        // Method Maps
        _unfinishedMethodMap = new Dictionary<Method, BoundMethod>();
        _unfinishedConstructorMap = new Dictionary<Method, BoundConstructor>();
        _methodMap = new Dictionary<TypeSymbol, Dictionary<MethodSymbol, BoundMethod>>();
        _constructorMap = new Dictionary<TypeSymbol, Dictionary<ConstructorSymbol, BoundConstructor>>();
        
        // Others
        _typeStack = new Dictionary<TypeSymbol, int>();
        
        // Initialize Method Maps
        foreach (var type in assembly.Types)
        {
            ImmutableArray<BoundMember> members;
            if (type is BoundStruct boundStruct)
            {
                members = boundStruct.Members;
            }
            else if (type is BoundArray boundArray)
            {
                members = boundArray.Members;
            }
            
            var methodMap = new Dictionary<MethodSymbol, BoundMethod>();
            var constructorMap = new Dictionary<ConstructorSymbol, BoundConstructor>();
            foreach (var member in members)
            {
                if (member is BoundMethod boundMethod)
                {
                    methodMap.Add(boundMethod.Symbol, boundMethod);
                }
                else if (member is BoundConstructor boundConstructor)
                {
                    constructorMap.Add(boundConstructor.Symbol, boundConstructor);
                }
            }
            
            _methodMap.Add(type.TypeSymbol, methodMap);
            _constructorMap.Add(type.TypeSymbol, constructorMap);
        }
    }
    
    
    
    // Methods
    public Assembly Build()
    {
        CollectMetadata();
        var methods = _unfinishedMethodMap.Keys.Concat(_unfinishedConstructorMap.Keys).ToArray();
        for (var i = 0; i < methods.Length; i++)
        {
            var method = methods[i];
            if (_unfinishedMethodMap.TryGetValue(method, out var boundMethod))
            {
                if (!_methodSymbolMap.TryGetValue(boundMethod.Symbol, out var m))
                {
                    throw new Exception("Method not found.");
                }

                FinishMethod(boundMethod, m);
            }
            else if (_unfinishedConstructorMap.TryGetValue(method, out var boundConstructor))
            {
                if (!_constructorSymbolMap.TryGetValue(boundConstructor.Symbol, out var c))
                {
                    throw new Exception("Constructor not found.");
                }

                FinishConstructor(boundConstructor, c);
            }
            else
            {
                throw new Exception("Method not found.");
            }
        }

        CheckForDefaults();
        var instructions = new InstructionTable(_instructions.ToArray());
        var metadata = BuildMetadata();
        return new Assembly(_guid, _flags, _encryptionKey, instructions, metadata);
    }

    private void CheckForDefaults()
    {
        for (var i = 0; i < _types.Count; i++)
        {
            var type = _types[i];
            if (type == default)
            {
                throw new Exception($"Type at index {i} is default.");
            }
        }
        
        for (var i = 0; i < _fields.Count; i++)
        {
            var field = _fields[i];
            if (field == default)
            {
                throw new Exception($"Field at index {i} is default.");
            }
        }
        
        for (var i = 0; i < _enumMembers.Count; i++)
        {
            var enumMember = _enumMembers[i];
            if (enumMember == default)
            {
                throw new Exception($"Enum member at index {i} is default.");
            }
        }
        
        for (var i = 0; i < _methods.Count; i++)
        {
            var method = _methods[i];
            if (method == default)
            {
                throw new Exception($"Method at index {i} is default.");
            }
        }
        
        for (var i = 0; i < _parameters.Count; i++)
        {
            var parameter = _parameters[i];
            if (parameter == default)
            {
                throw new Exception($"Parameter at index {i} is default.");
            }
        }
        
        for (var i = 0; i < _locals.Count; i++)
        {
            var local = _locals[i];
            if (local == default)
            {
                throw new Exception($"Local at index {i} is default.");
            }
        }
    }

    private Metadata BuildMetadata()
    {
        CollectMetadata();
        var strings = new StringTable(_strings.ToArray());
        var constants = new ConstantTable(_constants.ToArray());
        var signs = new SignTable(_signs.ToArray());
        var types = new TypeDefinitionTable(_types.ToArray());
        var fields = new FieldTable(_fields.ToArray());
        var enumMembers = new EnumMemberTable(_enumMembers.ToArray());
        var methods = new MethodTable(_methods.ToArray());
        var parameters = new ParameterTable(_parameters.ToArray());
        var locals = new LocalTable(_locals.ToArray());
        var typeReferences = new TypeReferenceTable(_typeReferences.ToArray());
        var memberReferences = new MemberReferenceTable(_memberReferences.ToArray());
        var constantsPool = new ConstantPool(_constantsPool.ToArray());
        return new Metadata(strings, constants, signs, types, fields, enumMembers, methods, parameters, locals, 
            typeReferences, memberReferences, constantsPool);
    }

    private void CollectMetadata()
    {
        var types = _assembly.Types;
        foreach (var type in types)
        {
            BuildType(type.TypeSymbol);
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
            var stringIndex = BuildString((string)value);
            var stringConstant = new Constant(constantType, stringIndex);
            var stringConstantIndex = AddToList(_constants, stringConstant);
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
        
        var constantPoolOffset = AddRangeToList(_constantsPool, bytes);
        var constant = new Constant(constantType, constantPoolOffset);
        var index = AddToList(_constants, constant);
        return index;
    }
    
    private int BuildString(string str)
    {
        return AddToList(_strings, str);
    }

    public int BuildType(TypeSymbol type)
    {
        // Check if type has already been built
        if (_typeSymbolMap.TryGetValue(type, _typeSymbolComparer, out var typeDef))
        {
            return typeDef.Index;
        }
        
        // Check if the type is being built
        if (_typeStack.TryGetValue(type, out var index))
        {
            return index;
        }
        
        // Build SymbolValue
        var typeIndex = _types.Count;
        var typeSymbolValue = new SymbolValue<TypeSymbol, TypeDefinition>(type, default, typeIndex, _types);
        _typeStack.Add(type, typeIndex);
        
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
            case PointerTypeSymbol p:
                kind = TypeKind.Pointer;
                underlyingType = BuildType(p.PointedType);
                break;
            case BoundTypeParameterSymbol b:
                _typeStack.Remove(type);
                _types.RemoveAt(typeIndex);
                return BuildType(b.BoundType);
            default:
                throw new Exception($"Unknown type {type}");
        }
        
        var flags = BuildFlags(type.Modifiers);
        if (!flags.HasFlag(BindingFlags.Ref))
        {
            kind |= TypeKind.ValueType;
        }
        
        if (!isArray)
        {
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
        
        // Name
        var name = BuildString(type.Name);
        
        // Fields
        var fieldStartOffset = _fields.Count;
        var fields = type.Members.OfType<FieldSymbol>().ToArray();
        foreach (var field in fields)
        {
            var fieldSymbolValue = new SymbolValue<FieldSymbol, Field>(field, default, _fields.Count, _fields);
            _fieldSymbolMap.Add(fieldSymbolValue);
        }
        
        foreach (var field in fields)
        {
            BuildField(field);
        }
        
        // Enum Members
        var enumMemberStartOffset = _enumMembers.Count;
        var enumMembers = type.Members.OfType<EnumMemberSymbol>().ToArray();
        foreach (var enumMember in enumMembers)
        {
            var enumMemberSymbolValue = new SymbolValue<EnumMemberSymbol, EnumMember>(enumMember, default, _enumMembers.Count, _enumMembers);
            _enumMemberSymbolMap.Add(enumMemberSymbolValue);
        }
        
        foreach (var enumMember in enumMembers)
        {
            BuildEnumMember(enumMember);
        }
        
        // Methods
        var methodStartOffset = _methods.Count;
        var methods = type.Members.OfType<MethodSymbol>().ToArray();
        foreach (var method in methods)
        {
            var methodSymbolValue = new SymbolValue<MethodSymbol, Method>(method, default, _methods.Count, _methods);
            _methodSymbolMap.Add(methodSymbolValue);
        }
        
        foreach (var method in methods)
        {
            BuildMethod(method, typeSymbolValue);
        }
        
        // Constructors
        var constructorStartOffset = _methods.Count;
        var constructors = type.Members.OfType<ConstructorSymbol>().ToArray();
        var staticConstructorOffset = -1;
        foreach (var constructor in constructors)
        {
            if (constructor.IsStatic)
            {
                staticConstructorOffset = _methods.Count;
            }
            
            var constructorSymbolValue = new SymbolValue<ConstructorSymbol, Method>(constructor, default, _methods.Count, _methods);
            _constructorSymbolMap.Add(constructorSymbolValue);
        }
        
        foreach (var constructor in constructors)
        {
            BuildConstructor(constructor, typeSymbolValue);
        }
        
        var size = type.Size;
        var typeDefinition = new TypeDefinition(flags, kind, name, size, underlyingType, fields.Length, fieldStartOffset, 
            enumMembers.Length, enumMemberStartOffset, methods.Length, methodStartOffset, constructors.Length, 
            constructorStartOffset, staticConstructorOffset);
        typeSymbolValue.Assign(typeDefinition);
        _typeSymbolMap.Add(typeSymbolValue);
        _typeStack.Remove(type);
        return typeIndex;
    }
    
    private void BuildField(FieldSymbol field)
    {
        var flags = BuildFlags(field.Modifiers);
        var name = BuildString(field.Name);
        var type = BuildType(field.Type);
        var parentIndex = BuildType(field.ParentType);
        var fieldDefinition = new Field(flags, name, type, parentIndex, field.Offset);
        _fieldSymbolMap[field] = fieldDefinition;
    }
    
    private void BuildEnumMember(EnumMemberSymbol enumMember)
    {
        var flags = BuildFlags(enumMember.Modifiers);
        var name = BuildString(enumMember.Name);
        var type = BuildType(enumMember.UnderlyingType);
        var valueIndex = EmitConstant(enumMember.Value, enumMember.UnderlyingType);
        var parentIndex = BuildType(enumMember.ParentType);
        var enumMemberDefinition = new EnumMember(flags, name, type, valueIndex, parentIndex);
        _enumMemberSymbolMap[enumMember] = enumMemberDefinition;
    }

    private void BuildMethod(MethodSymbol method, SymbolValue<TypeSymbol, TypeDefinition> parent)
    {
        var flags = BuildFlags(method.Modifiers);
        var name = BuildString(method.Name);
        var returnType = BuildType(method.Type);
        var parentIndex = BuildType(method.ParentType);
        var parameterStartOffset = _parameters.Count;
        foreach (var parameter in method.Parameters)
        {
            var parameterSymbolValue = new SymbolValue<ParameterSymbol, Parameter>(parameter, default, _parameters.Count, _parameters);
            _parameterSymbolMap.Add(parameterSymbolValue);
        }
        
        foreach (var parameter in method.Parameters)
        {
            BuildParameter(parameter);
        }
        
        var boundMethod = _methodMap[parent.Symbol][method];
        var unfinishedMethod = new Method(flags, name, returnType, parentIndex, method.Parameters.Length, 
            parameterStartOffset, -1, -1, -1, -1);
        _unfinishedMethodMap.Add(unfinishedMethod, boundMethod);
        _methodSymbolMap[method] = unfinishedMethod;
    }
    
    private void BuildConstructor(ConstructorSymbol constructor, SymbolValue<TypeSymbol, TypeDefinition> parent)
    {
        var flags = BuildFlags(constructor.Modifiers);
        var name = BuildString(constructor.Name);
        var returnType = BuildType(constructor.Type);
        var parentIndex = BuildType(constructor.ParentType);
        var parameterStartOffset = _parameters.Count;
        foreach (var parameter in constructor.Parameters)
        {
            var parameterSymbolValue = new SymbolValue<ParameterSymbol, Parameter>(parameter, default, _parameters.Count, _parameters);
            _parameterSymbolMap.Add(parameterSymbolValue);
        }
        
        foreach (var parameter in constructor.Parameters)
        {
            BuildParameter(parameter);
        }
        
        var boundConstructor = _constructorMap[parent.Symbol][constructor];
        var unfinishedConstructor = new Method(flags, name, returnType, parentIndex, constructor.Parameters.Length, 
            parameterStartOffset, -1, -1, -1, -1);
        _unfinishedConstructorMap.Add(unfinishedConstructor, boundConstructor);
        _constructorSymbolMap[constructor] = unfinishedConstructor;
    }
    
    private void BuildParameter(ParameterSymbol parameter)
    {
        var name = BuildString(parameter.Name);
        var type = BuildType(parameter.Type);
        var parameterDefinition = new Parameter(name, type, parameter.Ordinal);
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

    private void FinishMethod(BoundMethod boundMethod, SymbolValue<MethodSymbol, Method> method)
    {
        var emitter = new MethodEmitter(this, boundMethod.Statements, boundMethod.Locals, method.Cast<AbstractMethodSymbol>());
        var emittedMethod = emitter.EmitMethod();
        method.Assign(emittedMethod);
    }
    
    private void FinishConstructor(BoundConstructor boundConstructor, SymbolValue<ConstructorSymbol, Method> constructor)
    {
        var emitter = new MethodEmitter(this, boundConstructor.Statements, boundConstructor.Locals, constructor.Cast<AbstractMethodSymbol>());
        var emittedConstructor = emitter.EmitMethod();
        constructor.Assign(emittedConstructor);
    }

    private sealed class MethodEmitter
    {
        private readonly AssemblyBuilder _builder;
        private readonly ImmutableArray<BoundStatement> _statements;
        private readonly List<Instruction> _instructionTable;
        private readonly SymbolValue<AbstractMethodSymbol, Method> _method;
        private readonly int _firstLocal;
        private readonly int _localCount;
        private int _labelCount;
        private readonly Dictionary<BoundLabel, List<int>> _unresolvedLabelMap;
        private readonly Dictionary<BoundLabel, int> _labelMap;
        private bool _isAssigning;

        public MethodEmitter(AssemblyBuilder builder, ImmutableArray<BoundStatement> statements, 
            ImmutableArray<LocalVariableSymbol> locals, SymbolValue<AbstractMethodSymbol, Method> method)
        {
            _builder = builder;
            _statements = statements;
            _instructionTable = new List<Instruction>();
            _method = method;
            _firstLocal = _builder._locals.Count;
            for (_localCount = 0; _localCount < locals.Length; _localCount++)
            {
                var local = locals[_localCount];
                var methodIndex = method.Index;
                var emittedLocal = new Local(methodIndex, _localCount, _builder.BuildString(local.Name),
                    _builder.BuildType(local.Type));
                var localSymbolValue = new SymbolValue<LocalVariableSymbol, Local>(local, emittedLocal, _builder._locals.Count, _builder._locals);
                _builder._localSymbolMap.Add(localSymbolValue);
            }
            
            _labelCount = 0;
            _unresolvedLabelMap = new Dictionary<BoundLabel, List<int>>();
            _labelMap = new Dictionary<BoundLabel, int>();
            _isAssigning = false;
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
            var firstInstruction = _builder._instructions.Count;
            _builder._instructions.AddRange(_instructionTable);
            return new Method(_method.Value.Flags, _method.Value.Name, _method.Value.ReturnType, _method.Value.Parent, 
                _method.Value.ParameterCount, _method.Value.FirstParameter, _localCount, _firstLocal,
                instructionCount, firstInstruction);
        }
        
        private int GetPosition()
        {
            return _instructionTable.Count - 1;
        }
        
        private void EmitInstruction(OpCode opCode, int operand = -1)
        {
            var instruction = new Instruction(_labelCount++, opCode, operand);
            _instructionTable.Add(instruction);
        }
        
        private void EmitInstructions()
        {
            foreach (var statement in _statements)
            {
                EmitStatement(statement);
            }
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
            if (key.ToLower() == "encrypted")
            {
                if (Convert.ToBoolean(value.ToLower()))
                {
                    // Set the encryption flag
                    _builder._flags |= AssemblyFlags.Encryption;
                }
                else
                {
                    // Unset the encryption flag
                    _builder._flags &= ~AssemblyFlags.Encryption;
                }
            }
            
            var sign = new Sign(key, value);
            _builder._signs.Add(sign);
        }
        
        private void EmitVariableDeclarationStatement(BoundVariableDeclarationStatement variableDeclarationStatement)
        {
            if (variableDeclarationStatement.Initializer is null)
            {
                throw new Exception("Variable declaration statement must have an initializer.");
            }
            
            EmitExpression(variableDeclarationStatement.Initializer);
            if (!_builder._localSymbolMap.TryGetValue(variableDeclarationStatement.Variable, out var local))
            {
                throw new Exception("Local variable not found.");
            }
            
            EmitInstruction(OpCode.Stloc, local.Index);
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
                case BoundAddressOfExpression addressOfExpression:
                    EmitAddressOfExpression(addressOfExpression);
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
                case BoundDereferenceExpression dereferenceExpression:
                    EmitDereferenceExpression(dereferenceExpression);
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
        
        private void EmitAddressOfExpression(BoundAddressOfExpression expression)
        {
            var type = _builder.BuildType(expression.Type);
            EmitInstruction(OpCode.Ldtype, type);
            if (expression.Operand is BoundMemberAccessExpression memberAccess)
            {
                if (memberAccess.Member is FieldSymbol field)
                {
                    var emittedField = _builder._fieldSymbolMap[field];
                    if (!_builder._fieldSymbolMap.TryGetValue(field, out var fld))
                    {
                        throw new Exception("Field not found.");
                    }
                    
                    var fieldType = _builder.BuildType(field.Type);
                    var parentType = _builder.BuildType(field.ParentType);
                    var memberReference = new MemberReference(MemberType.Field, parentType, fieldType, 
                        fld.Index);
                    var memberReferenceIndex = AddToList(_builder._memberReferences, memberReference);
                    if (emittedField.BindingFlags.HasFlag(BindingFlags.Static))
                    {
                        EmitInstruction(OpCode.Ldsflda, memberReferenceIndex);
                        return;
                    }
                    
                    EmitInstruction(OpCode.Ldflda, memberReferenceIndex);
                    return;
                }
                
                throw new NotImplementedException($"Address of expression of member kind {memberAccess.Member.Kind} is not implemented.");
            }

            if (expression.Operand is BoundNameExpression nameExpression)
            {
                if (nameExpression.Symbol is ParameterSymbol parameter)
                {
                    if (!_builder._parameterSymbolMap.TryGetValue(parameter, out var param))
                    {
                        throw new Exception("Parameter not found.");
                    }
                    
                    EmitInstruction(OpCode.Ldarga, param.Index);
                    return;
                }
                
                if (nameExpression.Symbol is LocalVariableSymbol local)
                {
                    if (!_builder._localSymbolMap.TryGetValue(local, out var loc))
                    {
                        throw new Exception("Local variable not found.");
                    }
                    
                    EmitInstruction(OpCode.Ldloca, loc.Index);
                    return;
                }
                
                throw new NotImplementedException($"Address of expression of name kind {nameExpression.Symbol.Kind} is not implemented.");
            }

            if (expression.Operand is BoundElementAccessExpression elementAccessExpression)
            {
                EmitExpression(elementAccessExpression.IndexExpression);
                EmitExpression(elementAccessExpression.Expression); // Array instance
                EmitInstruction(OpCode.Ldelema);
                return;
            }
            
            throw new NotImplementedException($"Address of expression of kind {expression.Operand.Kind} is not implemented.");
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
        
        private void EmitDereferenceExpression(BoundDereferenceExpression expression)
        {
            var isAssigning = _isAssigning;
            _isAssigning = false;
            EmitExpression(expression.Operand);
            _isAssigning = isAssigning;
            var type = _builder.BuildType(expression.Type);
            EmitInstruction(_isAssigning ? OpCode.Stind : OpCode.Ldind, type);
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

            if (method is not MethodSymbol methodSymbol)
            {
                throw new Exception("Abstract method symbol is not a method symbol.");
            }
            
            if (!_builder._methodSymbolMap.TryGetValue(methodSymbol, out var m))
            {
                throw new Exception("Method not found.");
            }
            
            var arguments = expression.Arguments;
            foreach (var argument in arguments)
            {
                EmitExpression(argument);
            }
            
            EmitExpression(expression.Expression);
            var returnType = _builder.BuildType(expression.Type);
            var parentType = _builder.BuildType(method.ParentType);
            var memberReference = new MemberReference(MemberType.Method, parentType, returnType, 
                m.Index);
            var memberReferenceIndex = AddToList(_builder._memberReferences, memberReference);
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
            if (!_builder._fieldSymbolMap.TryGetValue(field, out var fld))
            {
                throw new Exception("Field not found.");
            }
            
            var fieldType = _builder.BuildType(field.Type);
            var parentType = _builder.BuildType(field.ParentType);
            var memberReference = new MemberReference(MemberType.Field, parentType, fieldType, 
                fld.Index);
            var memberReferenceIndex = AddToList(_builder._memberReferences, memberReference);
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
            var constantIndex = _builder.EmitConstant(value, enumMember.UnderlyingType);
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
        
        private void EmitLocalExpression(BoundNameExpression expression)
        {
            var local = (LocalVariableSymbol)expression.Symbol;
            if (!_builder._localSymbolMap.TryGetValue(local, out var loc))
            {
                throw new Exception("Local variable not found.");
            }
            
            EmitInstruction(_isAssigning ? OpCode.Stloc : OpCode.Ldloc, loc.Index);
        }
        
        private void EmitParameterExpression(BoundNameExpression expression)
        {
            var parameter = (ParameterSymbol)expression.Symbol;
            if (!_builder._parameterSymbolMap.TryGetValue(parameter, out var param))
            {
                throw new Exception("Parameter not found.");
            }
            
            EmitInstruction(_isAssigning ? OpCode.Starg : OpCode.Ldarg, param.Index);
        }
        
        private void EmitNewExpression(BoundNewExpression expression)
        {
            var constructor = expression.Constructor;
            if (!_builder._constructorSymbolMap.TryGetValue(constructor, out var ctor))
            {
                throw new Exception("Constructor not found.");
            }
            
            var arguments = expression.Arguments;
            foreach (var argument in arguments)
            {
                EmitExpression(argument);
            }

            var returnType = _builder.BuildType(expression.Type);
            var memberReference = new MemberReference(MemberType.Constructor, returnType, 
                returnType, ctor.Index);
            var memberReferenceIndex = AddToList(_builder._memberReferences, memberReference);
            var typeReference = new TypeReference(returnType, memberReferenceIndex);
            var typeReferenceIndex = AddToList(_builder._typeReferences, typeReference);
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
            var isAssigning = _isAssigning;
            _isAssigning = false;
            EmitExpression(expression.IndexExpression);
            EmitExpression(expression.Expression); // Array instance
            EmitInstruction(instruction);
            _isAssigning = isAssigning;
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

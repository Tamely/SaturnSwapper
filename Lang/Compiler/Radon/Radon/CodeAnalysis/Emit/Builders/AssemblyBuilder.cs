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
using Radon.Utilities;

namespace Radon.CodeAnalysis.Emit.Builders;

internal sealed class AssemblyBuilder
{
    private readonly TypeDefinitionComparer _typeDefinitionComparer;
    private readonly MethodComparer _methodComparer;
    private readonly FieldComparer _fieldComparer;
    private readonly EnumMemberComparer _enumMemberComparer;
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
    private readonly Dictionary<EnumMemberSymbol, EnumMember> _enumMemberSymbolMap;
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
        _enumMemberComparer = new EnumMemberComparer();
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
        _enumMemberSymbolMap = new Dictionary<EnumMemberSymbol, EnumMember>();
        _localSymbolMap = new Dictionary<LocalVariableSymbol, Local>();
        _parameterSymbolMap = new Dictionary<ParameterSymbol, Parameter>();
        _methodMap = new Dictionary<TypeSymbol, Dictionary<MethodSymbol, BoundMethod>>();
        _constructorMap = new Dictionary<TypeSymbol, Dictionary<ConstructorSymbol, BoundConstructor>>();
        foreach (var type in assembly.Types)
        {
            if (type is BoundStruct boundStruct)
            {
                var methodMap = new Dictionary<MethodSymbol, BoundMethod>();
                var constructorMap = new Dictionary<ConstructorSymbol, BoundConstructor>();
                foreach (var method in boundStruct.Members)
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
        foreach (var method in _unfinishedMethodMap.Keys)
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
        
        var index = _typeDefinitionTable.Count;
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

        var name = AddString(type.Name);
        var fieldStartOffset = _fieldTable.Count;
        var fields = type.Members.OfType<FieldSymbol>().ToList();
        foreach (var field in fields)
        {
            BuildField(field, index);
        }
        
        var enumMemberStartOffset = _enumMemberTable.Count;
        var enumMembers = type.Members.OfType<EnumMemberSymbol>().ToList();
        foreach (var enumMember in enumMembers)
        {
            BuildEnumMember(enumMember, index);
        }
        
        var methodStartOffset = _methodTable.Count;
        var methods = type.Members.OfType<MethodSymbol>().ToList();
        foreach (var method in methods)
        {
            BuildMethod(method, type, index);
        }

        var constructorCount = type.Members.Count(m => m is ConstructorSymbol);
        var constructorStartOffset = _methodTable.Count;
        var staticConstructorOffset = -1;
        var constructors = type.Members.OfType<ConstructorSymbol>().ToList();
        foreach (var constructor in constructors)
        {
            if (constructor.Modifiers.Contains(SyntaxKind.StaticKeyword))
            {
                staticConstructorOffset = _methodTable.Count;
            }
            
            BuildConstructor(constructor, type, index);
        }

        var size = type.Size;
        var typeDefinition = new TypeDefinition(flags, kind, name, size, underlyingType, fields.Count,
            fieldStartOffset, enumMembers.Count, enumMemberStartOffset, methods.Count, methodStartOffset, 
            constructorCount, constructorStartOffset, staticConstructorOffset);
        
        _typeSymbolMap.Add(type, typeDefinition);
        return AddToList(_typeDefinitionTable, typeDefinition);
    }

    private void BuildField(FieldSymbol field, int parentTypeIndex)
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
        _fieldTable.Add(fieldDefinition);
        _fieldSymbolMap.Add(field, fieldDefinition);
    }
    
    private void BuildEnumMember(EnumMemberSymbol enumMember, int parentTypeIndex)
    {
        var flags = BuildFlags(enumMember.Modifiers);
        var name = AddString(enumMember.Name);
        var type = BuildType(enumMember.UnderlyingType);
        var valueIndex = EmitConstant(enumMember.Value, enumMember.UnderlyingType);
        var enumMemberDefinition = new EnumMember(flags, name, type, valueIndex, parentTypeIndex);
        _enumMemberTable.Add(enumMemberDefinition);
        _enumMemberSymbolMap.Add(enumMember, enumMemberDefinition);
    }
    
    private void BuildMethod(MethodSymbol method, TypeSymbol parentType, int parentTypeIndex)
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
            foreach (var parameter in method.Parameters)
            {
                BuildParameter(parameter, parentType, parentTypeIndex);
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
        _methodTable.Add(unfinishedMethod);
        _allMethodSymbolMap.Add(method, unfinishedMethod);
    }
    
    private void BuildConstructor(ConstructorSymbol constructor, TypeSymbol parentType, int parentTypeIndex)
    {
        var flags = BuildFlags(constructor.Modifiers);
        var name = AddString(constructor.Name);
        // parentType = parentTypeIndex
        var parameterCount = constructor.Parameters.Length;
        var parameterStartOffset = -1;
        if (parameterCount > 0)
        {
            parameterStartOffset = _parameterTable.Count;
            foreach (var parameter in constructor.Parameters)
            {
                BuildParameter(parameter, parentType, parentTypeIndex);
            }
        }
        else
        {
            parameterCount = -1;
        }

        // The local, and instruction count cannot be determined until all symbols have been built.
        var boundConstructor = _constructorMap[parentType][constructor];
        var unfinishedConstructor = new Method(flags, name, parentTypeIndex, parentTypeIndex, parameterCount, 
            parameterStartOffset, -1, -1, -1, -1);
        _unfinishedConstructorMap.Add(unfinishedConstructor, boundConstructor);
        _methodTable.Add(unfinishedConstructor);
        _constructorSymbolMap.Add(constructor, unfinishedConstructor);
        _allMethodSymbolMap.Add(constructor, unfinishedConstructor);
    }
    
    private void BuildParameter(ParameterSymbol parameter, TypeSymbol parentType, int parentTypeIndex)
    {
        var flags = BuildFlags(parameter.Modifiers);
        var name = AddString(parameter.Name);
        var type = parameter.Type == parentType ? parentTypeIndex : BuildType(parameter.Type);
        var parameterDefinition = new Parameter(flags, name, type, parentTypeIndex, parameter.Ordinal);
        _parameterTable.Add(parameterDefinition);
        _parameterSymbolMap.Add(parameter, parameterDefinition);
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
                flags |= BindingFlags.Private;
            }
        }

        if (!flags.HasFlag(BindingFlags.Static))
        {
            flags |= BindingFlags.Instance;
        }

        if (!flags.HasFlag(BindingFlags.Public))
        {
            flags |= BindingFlags.Private;
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
        private readonly List<Local> _localTable;
        private readonly List<Instruction> _instructionTable;
        private bool _isAssigning;

        public AbstractMethodSymbol Symbol { get; }
        
        public MethodEmitter(AssemblyBuilder builder, BoundMethod boundMethod, Method method)
        {
            _builder = builder;
            _statements = boundMethod.Statements;
            _method = method;
            _localTable = new List<Local>();
            _instructionTable = new List<Instruction>();
            foreach (var local in boundMethod.Locals)
            {
                var emittedLocal = new Local(BindingFlags.None, _builder.AddString(local.Name), 
                    _builder.BuildType(local.Type));
                _localTable.Add(emittedLocal);
                _builder._localSymbolMap.Add(local, emittedLocal);
            }
            
            _isAssigning = false;
            Symbol = boundMethod.Symbol;
        }
        
        public MethodEmitter(AssemblyBuilder builder, BoundConstructor boundConstructor, Method method)
        {
            _builder = builder;
            _statements = boundConstructor.Statements;
            _method = method;
            _localTable = new List<Local>();
            _instructionTable = new List<Instruction>();
            foreach (var local in boundConstructor.Locals)
            {
                var emittedLocal = new Local(BindingFlags.None, _builder.AddString(local.Name), 
                    _builder.BuildType(local.Type));
                _localTable.Add(emittedLocal);
                _builder._localSymbolMap.Add(local, emittedLocal);
            }
            
            _isAssigning = false;
            Symbol = boundConstructor.Symbol;
        }

        public Method EmitMethod()
        {
            EmitInstructions(); // This will also emit the locals.
            var localCount = _localTable.Count;
            var firstLocal = _builder._localTable.Count;
            _builder._localTable.AddRange(_localTable);
            var instructionCount = _instructionTable.Count;
            var firstInstruction = _builder._instructionTable.Count;
            _builder._instructionTable.AddRange(_instructionTable);
            return new Method(_method.Flags, _method.Name, _method.ReturnType, _method.Parent, _method.ParameterCount, 
                _method.FirstParameter, localCount, firstLocal, instructionCount, firstInstruction);
        }
        

        private void EmitInstructions()
        {
            foreach (var statement in _statements)
            {
                EmitStatement(statement);
            }
        }
        
        private void EmitInstruction(OpCode opCode, int operand = -1)
        {
            var instruction = new Instruction(opCode, operand);
            if (opCode.HasFlag(OpCode.NoOperandMask) && operand != -1)
            {
                throw new ArgumentException($"OpCode '{opCode}' does not take an operand.");
            }
            
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
            /*
             * IL:
             * 1. Emit the expression.
             * 2. Store the result in a local.
             *
             * e.g
             *
             * var x = 1;
             * > becomes
             * ldc 1 // load constant at constant pool index 1
             * stloc 0 // store the result in local 0
            */

            if (variableDeclarationStatement.Initializer is null)
            {
                throw new Exception("Variable declaration statement must have an initializer.");
            }
            
            EmitExpression(variableDeclarationStatement.Initializer);
            var name = _builder.AddString(variableDeclarationStatement.Variable.Name);
            var type = _builder.BuildType(variableDeclarationStatement.Variable.Type);
            var local = new Local(BindingFlags.None, name, type);
            _localTable.Add(local);
            var localIndex = _localTable.Count - 1;
            EmitInstruction(OpCode.Stloc, localIndex);
        }
        
        private void EmitReturnStatement(BoundReturnStatement returnStatement)
        {
            /*
             * IL:
             * 1. Emit the expression.
             * 2. Return the result.
             *
             * e.g
             *
             * return 1;
             * > becomes
             * ldc 1 // load constant at constant pool index 1
             * ret // return the result
            */
            
            if (returnStatement.Expression is not null)
            {
                EmitExpression(returnStatement.Expression);
            }
            
            EmitInstruction(OpCode.Ret);
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
                case BoundImportExpression importExpression:
                    EmitImportExpression(importExpression);
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
                case BoundBinaryOperatorKind.Addition:
                    EmitInstruction(OpCode.Add);
                    break;
                case BoundBinaryOperatorKind.Concatenation:
                    EmitInstruction(OpCode.Concat);
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
                default:
                    throw new NotImplementedException();
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
        
        private void EmitImportExpression(BoundImportExpression expression)
        {
            EmitExpression(expression.Path);
            EmitInstruction(OpCode.Import);
        }

        private void EmitInvocationExpression(BoundInvocationExpression expression)
        {
            var method = expression.Method;
            var emittedMethod = _builder._allMethodSymbolMap[method];
            var methodIndex = _builder._methodTable.IndexOf(emittedMethod, _builder._methodComparer);
            var arguments = expression.Arguments;

            // Arguments are pushed onto the stack in reverse order.
            for (var i = arguments.Length - 1; i >= 0; i--)
            {
                var argument = arguments[i];
                EmitExpression(argument);
            }

            var returnType = _builder.BuildType(expression.Type);
            var parentType = _builder.BuildType(method.ParentType);
            var memberReference = new MemberReference(BindingFlags.None, MemberType.Method, parentType, returnType, 
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
            var field = (FieldSymbol)expression.Member;
            var emittedField = _builder._fieldSymbolMap[field];
            var fieldIndex = _builder._fieldTable.IndexOf(emittedField, _builder._fieldComparer);
            var fieldType = _builder.BuildType(field.Type);
            var parentType = _builder.BuildType(field.ParentType);
            var memberReference = new MemberReference(BindingFlags.None, MemberType.Field, parentType, fieldType, 
                fieldIndex);
            var memberReferenceIndex = AddToList(_builder._memberReferenceTable, memberReference);
            if (isStatic)
            {
                EmitInstruction(_isAssigning ? OpCode.Stsfld : OpCode.Ldsfld, memberReferenceIndex);
            }
            else
            {
                EmitExpression(expression.Expression); // Instance
                EmitInstruction(_isAssigning ? OpCode.Stfld : OpCode.Ldfld, memberReferenceIndex);
            }
        }
        
        private void EmitEnumMemberAccessExpression(BoundMemberAccessExpression expression)
        {
            var enumMember = (EnumMemberSymbol)expression.Member;
            var emittedEnumMember = _builder._enumMemberSymbolMap[enumMember];
            var enumMemberIndex = _builder._enumMemberTable.IndexOf(emittedEnumMember, _builder._enumMemberComparer);
            var enumMemberType = _builder.BuildType(enumMember.Type);
            var enumParentType = _builder.BuildType(enumMember.ParentType);
            var memberReference = new MemberReference(BindingFlags.None, MemberType.EnumMember, enumParentType, enumMemberType, 
                enumMemberIndex);
            var memberReferenceIndex = AddToList(_builder._memberReferenceTable, memberReference);
            EmitInstruction(OpCode.Ldenum, memberReferenceIndex);
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

            // Arguments are pushed onto the stack in reverse order.
            for (var i = arguments.Length - 1; i >= 0; i--)
            {
                var argument = arguments[i];
                EmitExpression(argument);
            }

            var returnType = _builder.BuildType(expression.Type);
            var memberReference = new MemberReference(BindingFlags.None, MemberType.Constructor, returnType, 
                returnType, constructorIndex);
            var memberReferenceIndex = AddToList(_builder._memberReferenceTable, memberReference);
            var typeReference = new TypeReference(BindingFlags.None, returnType, memberReferenceIndex);
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
            EmitExpression(expression.Expression); // Array instance
            EmitExpression(expression.IndexExpression);
            EmitInstruction(_isAssigning ? OpCode.Stelem : OpCode.Ldelem);
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
            EmitInstruction(OpCode.Ldarg, parameterIndex);
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
                default:
                    throw new NotImplementedException($"Unary expression of kind {expression.Op.Kind} is not implemented.");
            }
        }
    }
}

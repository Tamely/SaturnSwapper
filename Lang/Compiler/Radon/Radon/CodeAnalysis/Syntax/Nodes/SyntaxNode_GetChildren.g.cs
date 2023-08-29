using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Syntax.Nodes.Clauses;
using Radon.CodeAnalysis.Syntax.Nodes.Expressions;
using Radon.CodeAnalysis.Syntax.Nodes.Statements;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Syntax.Nodes.Members;
using Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations.Bodies;
using Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations;
using Radon.CodeAnalysis.Syntax.Nodes.Directives;

namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses
{
    public partial class ArgumentListSyntax
    {
        public override string SyntaxName => "Argument List";
        public override SyntaxKind Kind => SyntaxKind.ArgumentList;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenParenthesisToken;
            foreach (var child in Arguments.GetWithSeparators())
            {
                yield return child;
            }
            
            yield return CloseParenthesisToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses
{
    public partial class ArrayTypeSyntax
    {
        public override string SyntaxName => "Array Type";
        public override SyntaxKind Kind => SyntaxKind.ArrayType;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return TypeSyntax;
            yield return OpenBracketToken;
            if (SizeExpression != null)
            {
                yield return SizeExpression;
            }
            
            yield return CloseBracketToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class AssignmentExpressionSyntax
    {
        public override string SyntaxName => "Assignment Expression";
        public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
        public AssignmentExpressionSyntax(SyntaxTree syntaxTree)
        	: base(syntaxTree)
        {
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class BinaryExpressionSyntax
    {
        public override string SyntaxName => "Binary Expression";
        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
        public BinaryExpressionSyntax(SyntaxTree syntaxTree)
        	: base(syntaxTree)
        {
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements
{
    public partial class BlockStatementSyntax
    {
        public override string SyntaxName => "Block Statement";
        public override SyntaxKind Kind => SyntaxKind.BlockStatement;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenBraceToken;
            foreach (var child in Statements)
            {
                yield return child;
            }
            
            yield return CloseBraceToken;
            if (SemicolonToken != null)
            {
                yield return SemicolonToken;
            }
            
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements
{
    public partial class BreakStatementSyntax
    {
        public override string SyntaxName => "Break Statement";
        public override SyntaxKind Kind => SyntaxKind.BreakStatement;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return BreakKeyword;
            yield return SemicolonToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class CastExpressionSyntax
    {
        public override string SyntaxName => "Cast Expression";
        public override SyntaxKind Kind => SyntaxKind.CastExpression;
        public CastExpressionSyntax(SyntaxTree syntaxTree)
        	: base(syntaxTree)
        {
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Type;
            yield return ColonToken;
            yield return Expression;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes
{
    public partial class CodeCompilationUnitSyntax
    {
        public override string SyntaxName => "Code Compilation Unit";
        public override SyntaxKind Kind => SyntaxKind.CodeCompilationUnit;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var child in DeclaredTypes)
            {
                yield return child;
            }
            
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Members
{
    public partial class ConstructorDeclarationSyntax
    {
        public override string SyntaxName => "Constructor Declaration";
        public override SyntaxKind Kind => SyntaxKind.ConstructorDeclaration;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var child in Modifiers)
            {
                yield return child;
            }
            
            yield return Type;
            yield return ParameterList;
            yield return Body;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements
{
    public partial class ContinueStatementSyntax
    {
        public override string SyntaxName => "Continue Statement";
        public override SyntaxKind Kind => SyntaxKind.ContinueStatement;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ContinueKeyword;
            yield return SemicolonToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class DefaultExpressionSyntax
    {
        public override string SyntaxName => "Default Expression";
        public override SyntaxKind Kind => SyntaxKind.DefaultExpression;
        public DefaultExpressionSyntax(SyntaxTree syntaxTree)
        	: base(syntaxTree)
        {
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return DefaultKeyword;
            yield return ColonToken;
            yield return Type;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class ElementAccessExpressionSyntax
    {
        public override string SyntaxName => "Element Access Expression";
        public override SyntaxKind Kind => SyntaxKind.ElementAccessExpression;
        public ElementAccessExpressionSyntax(SyntaxTree syntaxTree)
        	: base(syntaxTree)
        {
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Expression;
            yield return OpenBracketToken;
            yield return IndexExpression;
            yield return CloseBracketToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses
{
    public partial class ElseClauseSyntax
    {
        public override string SyntaxName => "Else Clause";
        public override SyntaxKind Kind => SyntaxKind.ElseClause;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ElseKeyword;
            yield return ElseStatement;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations.Bodies
{
    public partial class EnumBodySyntax
    {
        public override string SyntaxName => "Enum Body";
        public override SyntaxKind Kind => SyntaxKind.EnumBody;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenBraceToken;
            foreach (var child in Members.GetWithSeparators())
            {
                yield return child;
            }
            
            yield return CloseBraceToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations
{
    public partial class EnumDeclarationSyntax
    {
        public override string SyntaxName => "Enum Declaration";
        public override SyntaxKind Kind => SyntaxKind.EnumDeclaration;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var child in Modifiers)
            {
                yield return child;
            }
            
            yield return EnumKeyword;
            yield return Identifier;
            yield return Body;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Members
{
    public partial class EnumMemberDeclarationSyntax
    {
        public override string SyntaxName => "Enum Member Declaration";
        public override SyntaxKind Kind => SyntaxKind.EnumMemberDeclaration;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            if (EqualsToken != null)
            {
                yield return EqualsToken;
            }
            
            if (Initializer != null)
            {
                yield return Initializer;
            }
            
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements
{
    public partial class ExpressionStatementSyntax
    {
        public override string SyntaxName => "Expression Statement";
        public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Expression;
            yield return SemicolonToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Members
{
    public partial class FieldDeclarationSyntax
    {
        public override string SyntaxName => "Field Declaration";
        public override SyntaxKind Kind => SyntaxKind.FieldDeclaration;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var child in Modifiers)
            {
                yield return child;
            }
            
            yield return Type;
            yield return VariableDeclarator;
            yield return SemicolonToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements
{
    public partial class ForStatementSyntax
    {
        public override string SyntaxName => "For Statement";
        public override SyntaxKind Kind => SyntaxKind.ForStatement;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ForKeyword;
            yield return OpenParenthesisToken;
            yield return Initializer;
            yield return Condition;
            yield return ConditionSemicolonToken;
            yield return Incrementor;
            yield return CloseParenthesisToken;
            yield return Body;
            if (SemicolonToken != null)
            {
                yield return SemicolonToken;
            }
            
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements
{
    public partial class IfStatementSyntax
    {
        public override string SyntaxName => "If Statement";
        public override SyntaxKind Kind => SyntaxKind.IfStatement;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return IfKeyword;
            yield return OpenParenthesisToken;
            yield return Condition;
            yield return CloseParenthesisToken;
            yield return ThenStatement;
            if (ElseClause != null)
            {
                yield return ElseClause;
            }
            
            if (SemicolonToken != null)
            {
                yield return SemicolonToken;
            }
            
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class ImportExpressionSyntax
    {
        public override string SyntaxName => "Import Expression";
        public override SyntaxKind Kind => SyntaxKind.ImportExpression;
        public ImportExpressionSyntax(SyntaxTree syntaxTree)
        	: base(syntaxTree)
        {
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ImportKeyword;
            yield return Path;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Directives
{
    public partial class IncludeDirectiveSyntax
    {
        public override string SyntaxName => "Include Directive";
        public override SyntaxKind Kind => SyntaxKind.IncludeDirective;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return HashToken;
            yield return IncludeKeyword;
            yield return Path;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Directives
{
    public partial class InvalidDirectiveSyntax
    {
        public override string SyntaxName => "Invalid Directive";
        public override SyntaxKind Kind => SyntaxKind.InvalidDirective;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return HashToken;
            yield return Directive;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class InvalidExpressionSyntax
    {
        public override string SyntaxName => "Invalid Expression";
        public override SyntaxKind Kind => SyntaxKind.InvalidExpression;
        public InvalidExpressionSyntax(SyntaxTree syntaxTree)
        	: base(syntaxTree)
        {
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Token;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Members
{
    public partial class InvalidMemberDeclarationSyntax
    {
        public override string SyntaxName => "Invalid Member Declaration";
        public override SyntaxKind Kind => SyntaxKind.InvalidMemberDeclaration;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var child in Modifiers)
            {
                yield return child;
            }
            
            yield return Keyword;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements
{
    public partial class InvalidStatementSyntax
    {
        public override string SyntaxName => "Invalid Statement";
        public override SyntaxKind Kind => SyntaxKind.InvalidStatement;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Token;
            yield return SemicolonToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations
{
    public partial class InvalidTypeDeclarationSyntax
    {
        public override string SyntaxName => "Invalid Type Declaration";
        public override SyntaxKind Kind => SyntaxKind.InvalidTypeDeclaration;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var child in Modifiers)
            {
                yield return child;
            }
            
            yield return Keyword;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class InvocationExpressionSyntax
    {
        public override string SyntaxName => "Invocation Expression";
        public override SyntaxKind Kind => SyntaxKind.InvocationExpression;
        public InvocationExpressionSyntax(SyntaxTree syntaxTree)
        	: base(syntaxTree)
        {
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Expression;
            if (TypeArgumentList != null)
            {
                yield return TypeArgumentList;
            }
            
            yield return ArgumentList;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class LiteralExpressionSyntax
    {
        public override string SyntaxName => "Literal Expression";
        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
        public LiteralExpressionSyntax(SyntaxTree syntaxTree)
        	: base(syntaxTree)
        {
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return LiteralToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class MemberAccessExpressionSyntax
    {
        public override string SyntaxName => "Member Access Expression";
        public override SyntaxKind Kind => SyntaxKind.MemberAccessExpression;
        public MemberAccessExpressionSyntax(SyntaxTree syntaxTree)
        	: base(syntaxTree)
        {
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Expression;
            yield return AccessToken;
            yield return Name;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Members
{
    public partial class MethodDeclarationSyntax
    {
        public override string SyntaxName => "Method Declaration";
        public override SyntaxKind Kind => SyntaxKind.MethodDeclaration;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var child in Modifiers)
            {
                yield return child;
            }
            
            yield return ReturnType;
            yield return Identifier;
            yield return ParameterList;
            yield return Body;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class NameExpressionSyntax
    {
        public override string SyntaxName => "Name Expression";
        public override SyntaxKind Kind => SyntaxKind.NameExpression;
        public NameExpressionSyntax(SyntaxTree syntaxTree)
        	: base(syntaxTree)
        {
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return IdentifierToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class NewArrayExpressionSyntax
    {
        public override string SyntaxName => "New Array Expression";
        public override SyntaxKind Kind => SyntaxKind.NewArrayExpression;
        public NewArrayExpressionSyntax(SyntaxTree syntaxTree)
        	: base(syntaxTree)
        {
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return NewKeyword;
            yield return Type;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class NewExpressionSyntax
    {
        public override string SyntaxName => "New Expression";
        public override SyntaxKind Kind => SyntaxKind.NewExpression;
        public NewExpressionSyntax(SyntaxTree syntaxTree)
        	: base(syntaxTree)
        {
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return NewKeyword;
            yield return Type;
            if (TypeArgumentList != null)
            {
                yield return TypeArgumentList;
            }
            
            yield return ArgumentList;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses
{
    public partial class ParameterListSyntax
    {
        public override string SyntaxName => "Parameter List";
        public override SyntaxKind Kind => SyntaxKind.ParameterList;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenParenthesisToken;
            foreach (var child in Parameters)
            {
                yield return child;
            }
            
            yield return CloseParenthesisToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses
{
    public partial class ParameterSyntax
    {
        public override string SyntaxName => "Parameter";
        public override SyntaxKind Kind => SyntaxKind.Parameter;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var child in Modifiers)
            {
                yield return child;
            }
            
            yield return Type;
            yield return Identifier;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class ParenthesizedExpressionSyntax
    {
        public override string SyntaxName => "Parenthesized Expression";
        public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
        public ParenthesizedExpressionSyntax(SyntaxTree syntaxTree)
        	: base(syntaxTree)
        {
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenParenthesisToken;
            yield return Expression;
            yield return CloseParenthesisToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes
{
    public partial class PluginCompilationUnitSyntax
    {
        public override string SyntaxName => "Plugin Compilation Unit";
        public override SyntaxKind Kind => SyntaxKind.PluginCompilationUnit;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var child in Statements)
            {
                yield return child;
            }
            
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses
{
    public partial class PointerTypeSyntax
    {
        public override string SyntaxName => "Pointer Type";
        public override SyntaxKind Kind => SyntaxKind.PointerType;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Type;
            yield return StarToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements
{
    public partial class ReturnStatementSyntax
    {
        public override string SyntaxName => "Return Statement";
        public override SyntaxKind Kind => SyntaxKind.ReturnStatement;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ReturnKeyword;
            if (Expression != null)
            {
                yield return Expression;
            }
            
            yield return SemicolonToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements
{
    public partial class SignStatementSyntax
    {
        public override string SyntaxName => "Sign Statement";
        public override SyntaxKind Kind => SyntaxKind.SignStatement;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return SignKeyword;
            yield return ColonToken;
            yield return KeyExpression;
            yield return CommaToken;
            yield return ValueExpression;
            if (SemicolonToken != null)
            {
                yield return SemicolonToken;
            }
            
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations.Bodies
{
    public partial class StructBodySyntax
    {
        public override string SyntaxName => "Struct Body";
        public override SyntaxKind Kind => SyntaxKind.StructBody;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenBraceToken;
            foreach (var child in Members)
            {
                yield return child;
            }
            
            yield return CloseBraceToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations
{
    public partial class StructDeclarationSyntax
    {
        public override string SyntaxName => "Struct Declaration";
        public override SyntaxKind Kind => SyntaxKind.StructDeclaration;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var child in Modifiers)
            {
                yield return child;
            }
            
            yield return StructKeyword;
            yield return Identifier;
            yield return Body;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations
{
    public partial class TemplateDeclarationSyntax
    {
        public override string SyntaxName => "Template Declaration";
        public override SyntaxKind Kind => SyntaxKind.TemplateDeclaration;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var child in Modifiers)
            {
                yield return child;
            }
            
            yield return TemplateKeyword;
            yield return Identifier;
            yield return TypeParameterList;
            yield return Body;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Members
{
    public partial class TemplateMethodDeclarationSyntax
    {
        public override string SyntaxName => "Template Method Declaration";
        public override SyntaxKind Kind => SyntaxKind.TemplateMethodDeclaration;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var child in Modifiers)
            {
                yield return child;
            }
            
            yield return TemplateKeyword;
            yield return ReturnType;
            yield return Identifier;
            yield return TypeParameterList;
            yield return ParameterList;
            yield return Body;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class ThisExpressionSyntax
    {
        public override string SyntaxName => "This Expression";
        public override SyntaxKind Kind => SyntaxKind.ThisExpression;
        public ThisExpressionSyntax(SyntaxTree syntaxTree)
        	: base(syntaxTree)
        {
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ThisKeyword;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses
{
    public partial class TypeArgumentListSyntax
    {
        public override string SyntaxName => "Type Argument List";
        public override SyntaxKind Kind => SyntaxKind.TypeArgumentList;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return LessThanToken;
            foreach (var child in Arguments.GetWithSeparators())
            {
                yield return child;
            }
            
            yield return GreaterThanToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses
{
    public partial class TypeParameterListSyntax
    {
        public override string SyntaxName => "Type Parameter List";
        public override SyntaxKind Kind => SyntaxKind.TypeParameterList;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return LessThanToken;
            foreach (var child in Parameters.GetWithSeparators())
            {
                yield return child;
            }
            
            yield return GreaterThanToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses
{
    public partial class TypeParameterSyntax
    {
        public override string SyntaxName => "Type Parameter";
        public override SyntaxKind Kind => SyntaxKind.TypeParameter;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses
{
    public partial class TypeSyntax
    {
        public override string SyntaxName => "Type";
        public override SyntaxKind Kind => SyntaxKind.Type;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            if (TypeArgumentList != null)
            {
                yield return TypeArgumentList;
            }
            
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class UnaryExpressionSyntax
    {
        public override string SyntaxName => "Unary Expression";
        public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
        public UnaryExpressionSyntax(SyntaxTree syntaxTree)
        	: base(syntaxTree)
        {
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            if (PrefixOperator != null)
            {
                yield return PrefixOperator;
            }
            
            yield return Operand;
            if (PostfixOperator != null)
            {
                yield return PostfixOperator;
            }
            
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements
{
    public partial class VariableDeclarationSyntax
    {
        public override string SyntaxName => "Variable Declaration";
        public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Type;
            yield return Declarator;
            yield return SemicolonToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses
{
    public partial class VariableDeclaratorSyntax
    {
        public override string SyntaxName => "Variable Declarator";
        public override SyntaxKind Kind => SyntaxKind.VariableDeclarator;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            if (EqualsToken != null)
            {
                yield return EqualsToken;
            }
            
            if (Initializer != null)
            {
                yield return Initializer;
            }
            
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements
{
    public partial class WhileStatementSyntax
    {
        public override string SyntaxName => "While Statement";
        public override SyntaxKind Kind => SyntaxKind.WhileStatement;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return WhileKeyword;
            yield return OpenParenthesisToken;
            yield return Condition;
            yield return CloseParenthesisToken;
            yield return Body;
            if (SemicolonToken != null)
            {
                yield return SemicolonToken;
            }
            
        }
    }
}


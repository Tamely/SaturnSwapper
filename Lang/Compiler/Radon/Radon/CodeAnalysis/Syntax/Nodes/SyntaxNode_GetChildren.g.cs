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
        public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
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
        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
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

namespace Radon.CodeAnalysis.Syntax.Nodes
{
    public partial class CodeCompilationUnitSyntax
    {
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

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class DefaultExpressionSyntax
    {
        public override SyntaxKind Kind => SyntaxKind.DefaultExpression;
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
        public override SyntaxKind Kind => SyntaxKind.ElementAccessExpression;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Expression;
            yield return OpenBracketToken;
            yield return IndexExpression;
            yield return CloseBracketToken;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations.Bodies
{
    public partial class EnumBodySyntax
    {
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

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions
{
    public partial class ImportExpressionSyntax
    {
        public override SyntaxKind Kind => SyntaxKind.ImportExpression;
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
        public override SyntaxKind Kind => SyntaxKind.InvalidExpression;
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
        public override SyntaxKind Kind => SyntaxKind.InvocationExpression;
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
        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
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
        public override SyntaxKind Kind => SyntaxKind.MemberAccessExpression;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Expression;
            yield return DotToken;
            yield return Name;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Members
{
    public partial class MethodDeclarationSyntax
    {
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
        public override SyntaxKind Kind => SyntaxKind.NameExpression;
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
        public override SyntaxKind Kind => SyntaxKind.NewArrayExpression;
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
        public override SyntaxKind Kind => SyntaxKind.NewExpression;
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
        public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
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

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements
{
    public partial class ReturnStatementSyntax
    {
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
        public override SyntaxKind Kind => SyntaxKind.ThisExpression;
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
        public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OperatorToken;
            yield return Operand;
        }
    }
}

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements
{
    public partial class VariableDeclarationSyntax
    {
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


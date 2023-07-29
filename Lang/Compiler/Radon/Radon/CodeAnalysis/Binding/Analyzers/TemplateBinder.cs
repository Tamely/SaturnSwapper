using System;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.Utilities;

namespace Radon.CodeAnalysis.Binding.Analyzers;

internal sealed class TemplateBinder : TypeBinder
{
    private readonly StructSymbol _type;
    private readonly TemplateSymbol _template;
    private readonly SyntaxNode _node;
    private readonly ImmutableArray<TypeSymbol> _typeArguments;
    private readonly TypeBinder? _typeBinder;
    private Binder? _binder;
    public TemplateBinder(Binder binder, SyntaxNode node, TemplateSymbol template, ImmutableArray<TypeSymbol> typeArguments, string name) 
        : base(binder)
    {
        var context = new SemanticContext(this, node, Diagnostics);
        for (var i = 0; i < template.TypeParameters.Length; i++)
        {
            var typeParameter = template.TypeParameters[i];
            /*if (template.TypeParameters[i].Ordinal != i)
            {
                Console.WriteLine($"Ordinal mismatch: {template.TypeParameters[i].Name} has ordinal {template.TypeParameters[i].Ordinal}, not {i}");
            }*/
            
            if (typeParameter is null)
            {
                throw new InvalidOperationException();
            }
            
            var typeArgument = typeArguments[i];
            Register(context, new BoundTypeParameterSymbol(typeParameter, typeArgument));
        }

        foreach (var member in template.Members)
        {
            Register(context, member);
        }

        _typeBinder = template.TypeBinder;
        _template = template;
        _node = node;
        _typeArguments = typeArguments;
        _type = new StructSymbol(name, ImmutableArray<MemberSymbol>.Empty, template.ParentAssembly, 
            template.Modifiers, this)
        {
            TemplateBinder = this
        };
    }
    
    public ImmutableArray<TypeSymbol> GetTypeArguments()
    {
        return _typeArguments;
    }
    
    public TemplateSymbol GetTemplateSymbol()
    {
        return _template;
    }
    
    public TypeSymbol GetTypeSymbol()
    {
        return _type;
    }
    
    public Binder? GetBinder()
    {
        return _binder;
    }

    public void SetBinder()
    {
        _binder = _typeBinder switch
        {
            NamedTypeBinder utb => new NamedTypeBinder(this, _type, utb.Syntax),
            PrimitiveTypeBinder => new PrimitiveTypeBinder(this, _template, _type),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        _type.TypeBinder = (TypeBinder)_binder;
    }

    public override BoundNode Bind(SyntaxNode? node, params object[] args)
    {
        node ??= _node;
        if (_binder is null)
        {
            throw new InvalidOperationException("Template type has no binder");
        }
        
        return _binder.Bind(node, args);
    }

    public override MethodSymbol BuildTemplateMethod(TemplateMethodSymbol templateMethod, ImmutableArray<TypeSymbol> typeArguments, SyntaxNode callSite)
    {
        if (templateMethod.ParentType.TypeBinder is not { } tb)
        {
            throw new InvalidOperationException("Template method parent type has no binder");
        }
        
        return tb.BuildTemplateMethod(templateMethod, typeArguments, callSite);
    }
}
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Text;

namespace Radon.CodeAnalysis.Binding;

public sealed class Scope
{
    private readonly List<Symbol> _symbols;
    private readonly Dictionary<Symbol, TextLocation> _symbolDeclarations;
    private readonly Dictionary<Symbol, List<TextLocation>> _symbolReferences;
    private readonly List<Scope> _children;
    
    public Scope? Parent { get; }
    public ImmutableArray<Symbol> Symbols => _symbols.ToImmutableArray();
    public ImmutableArray<TextLocation> SymbolDeclarations => _symbolDeclarations.Values.ToImmutableArray();
    public ImmutableArray<Scope> Children => _children.ToImmutableArray();

    public Scope(Scope? parent)
    {
        Parent = parent;
        _symbols = new List<Symbol>();
        _symbolDeclarations = new Dictionary<Symbol, TextLocation>();
        _symbolReferences = new Dictionary<Symbol, List<TextLocation>>();
        _children = new List<Scope>();
    }

    public Scope? GotoParent()
    {
        return Parent;
    }
    
    public Scope CreateChild()
    {
        var child = new Scope(this);
        _children.Add(child);
        return child;
    }

    public void AddSymbolReference(Symbol symbol, TextLocation location)
    {
        // Check if this symbol belongs to this scope.
        // If it doesn't, we go to the scope that it belongs to, and we add the reference there.
        // If it does, we add the reference here.
        if (!_symbols.Contains(symbol))
        {
            Parent?.AddSymbolReference(symbol, location);
            return;
        }
        
        if (!_symbolReferences.ContainsKey(symbol))
        {
            _symbolReferences.Add(symbol, new List<TextLocation> { location });
            return;
        }
        
        _symbolReferences[symbol].Add(location);
    }

    public bool TryDeclareSymbol(Symbol symbol, TextLocation location)
    {
        if (_symbolDeclarations.ContainsKey(symbol))
        {
            return false;
        }
        
        _symbols.Add(symbol);
        _symbolDeclarations.Add(symbol, location);
        _symbolReferences.Add(symbol, new List<TextLocation> { location });
        return true;
    }

    public bool TryLookupSymbol<T>(string name, out T? symbol)
        where T : Symbol
    {
        foreach (var sym in _symbols)
        {
            if (sym.Name == name)
            {
                symbol = (T)sym;
                return true;
            }
        }
        
        // If we can't find the symbol in this scope, we'll try to find it in the parent scope.
        if (Parent is not null)
        {
            return Parent.TryLookupSymbol(name, out symbol);
        }
        
        symbol = default;
        return false;
    }

    public void RemoveSymbol(Symbol symbol)
    {
        _symbols.Remove(symbol);
        _symbolDeclarations.Remove(symbol);
        _symbolReferences.Remove(symbol);
    }
    
    public TextLocation GetSymbolDeclaration(Symbol symbol)
    {
        return _symbolDeclarations[symbol];
    }
    
    public ImmutableArray<TextLocation> GetSymbolReferences(Symbol symbol)
    {
        return _symbolReferences[symbol].ToImmutableArray();
    }
    
    public ImmutableArray<TSymbol> GetSymbols<TSymbol>()
        where TSymbol : Symbol
    {
        var symbols = new List<TSymbol>();
        foreach (var sym in _symbols)
        {
            if (sym is TSymbol symbol)
            {
                symbols.Add(symbol);
            }
        }
        
        if (Parent is not null)
        {
            symbols.AddRange(Parent.GetSymbols<TSymbol>());
        }
        
        return symbols.ToImmutableArray();
    }
}
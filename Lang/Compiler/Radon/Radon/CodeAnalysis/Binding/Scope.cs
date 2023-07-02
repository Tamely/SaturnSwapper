using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Text;

namespace Radon.CodeAnalysis.Binding;

public sealed class Scope
{
    private readonly Dictionary<Symbol, SymbolHash> _symbols;
    private readonly Dictionary<SymbolHash, TextLocation> _symbolDeclarations;
    private readonly Dictionary<SymbolHash, List<TextLocation>> _symbolReferences;
    private readonly List<Scope> _children;
    
    public Scope? Parent { get; }
    public ImmutableArray<Symbol> Symbols => _symbols.Keys.ToImmutableArray();
    public ImmutableArray<TextLocation> SymbolDeclarations => _symbolDeclarations.Values.ToImmutableArray();
    public ImmutableArray<Scope> Children => _children.ToImmutableArray();

    public Scope(Scope? parent)
    {
        Parent = parent;
        _symbols = new Dictionary<Symbol, SymbolHash>();
        _symbolDeclarations = new Dictionary<SymbolHash, TextLocation>();
        _symbolReferences = new Dictionary<SymbolHash, List<TextLocation>>();
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
        if (!_symbols.ContainsKey(symbol))
        {
            Parent?.AddSymbolReference(symbol, location);
            return;
        }
        
        var hash = _symbols[symbol];
        if (!_symbolReferences.ContainsKey(hash))
        {
            _symbolReferences.Add(hash, new List<TextLocation> { location });
            return;
        }
        
        _symbolReferences[hash].Add(location);
    }

    public bool TryDeclareSymbol(Symbol symbol, TextLocation location)
    {
        var hash = SymbolHash.Create(symbol.Name);
        if (symbol is AbstractMethodSymbol method)
        {
            hash = SymbolHash.CreateMethod(method);
        }
        
        // If the symbol hasn't been declared in a previous scope, we'll check if it's been declared in this scope.
        if (_symbolDeclarations.ContainsKey(hash))
        {
            return false;
        }
        
        _symbols.Add(symbol, hash);
        _symbolDeclarations.Add(hash, location);
        _symbolReferences.Add(hash, new List<TextLocation> { location });
        return true;
    }

    public bool TryLookupSymbol<T>(string name, out T? symbol)
        where T : Symbol
    {
        foreach (var (key, value) in _symbols)
        {
            if (value.Name == name)
            {
                symbol = (T)key;
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
        var hash = _symbols[symbol];
        _symbols.Remove(symbol);
        _symbolDeclarations.Remove(hash);
        _symbolReferences.Remove(hash);
    }
    
    public TextLocation GetSymbolDeclaration(Symbol symbol)
    {
        var hash = _symbols[symbol];
        return _symbolDeclarations[hash];
    }
    
    public ImmutableArray<TextLocation> GetSymbolReferences(Symbol symbol)
    {
        var hash = _symbols[symbol];
        return _symbolReferences[hash].ToImmutableArray();
    }
    
    public ImmutableArray<TSymbol> GetSymbols<TSymbol>()
        where TSymbol : Symbol
    {
        var symbols = new List<TSymbol>();
        foreach (var (key, _) in _symbols)
        {
            if (key is TSymbol symbol)
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
using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Text;

namespace Radon.CodeAnalysis.Binding;

public sealed class Scope
{
    private readonly List<Symbol> _symbols;
    private readonly Dictionary<Symbol, TextLocation> _symbolDeclarations;
    private readonly Dictionary<Symbol, List<TextLocation>> _symbolReferences;
    private readonly List<Scope> _children;
    
    public Scope? Parent { get; }
    public TextLocation Location { get; }
    public ImmutableArray<Symbol> Symbols => _symbols.ToImmutableArray();
    public ImmutableArray<TextLocation> SymbolDeclarations => _symbolDeclarations.Values.ToImmutableArray();
    public ImmutableArray<Scope> Children => _children.ToImmutableArray();

    public Scope(Scope? parent, TextLocation location)
    {
        Parent = parent;
        Location = location;
        _symbols = new List<Symbol>();
        _symbolDeclarations = new Dictionary<Symbol, TextLocation>();
        _symbolReferences = new Dictionary<Symbol, List<TextLocation>>();
        _children = new List<Scope>();
    }

    public Scope CreateChild(TextLocation location)
    {
        var child = new Scope(this, location);
        _children.Add(child);
        return child;
    }

    public Scope? GetSmallestScopeAtLocation(TextLocation location)
    {
        if (!Location.Contains(location))
        {
            return null;
        }
        
        foreach (var child in _children)
        {
            var result = child.GetSmallestScopeAtLocation(location);
            if (result is not null)
            {
                return result;
            }
        }
        
        return this;
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
        if (HasConflicts(symbol))
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

    public ImmutableArray<TSymbol> GetAllSymbols<TSymbol>()
        where TSymbol : Symbol
    {
        // Start from the top
        var topScope = this;
        while (topScope.Parent is not null)
        {
            topScope = topScope.Parent;
        }
        
        var symbols = new List<TSymbol>();
        var childSymbols = GetAllChildSymbols<TSymbol>(topScope);
        symbols.AddRange(childSymbols);
        return symbols.ToImmutableArray();
    }

    private IEnumerable<TSymbol> GetAllChildSymbols<TSymbol>(Scope scope)
    {
        var symbols = new List<TSymbol>();
        foreach (var sym in scope.Symbols)
        {
            if (sym is TSymbol symbol)
            {
                symbols.Add(symbol);
            }
        }
        
        foreach (var child in scope.Children)
        {
            symbols.AddRange(GetAllChildSymbols<TSymbol>(child));
        }
        
        return symbols;
    }

    private bool HasConflicts(Symbol symbol)
    {
        if (symbol is VariableSymbol)
        {
            foreach (var sym in _symbols)
            {
                if (sym is VariableSymbol variableSymbol)
                {
                    if (variableSymbol.Name == symbol.Name)
                    {
                        return true;
                    }
                }
            }
            
            return Parent?.HasConflicts(symbol) ?? false;
        }

        if (symbol is MemberSymbol)
        {
            foreach (var sym in _symbols)
            {
                if (sym is AbstractMethodSymbol left && symbol is AbstractMethodSymbol right)
                {
                    if (left.Name != right.Name)
                    {
                        continue;
                    }
                    
                    var leftParameters = left.Parameters;
                    var rightParameters = right.Parameters;
                    if (leftParameters.Length != rightParameters.Length)
                    {
                        continue;
                    }
                    
                    for (var i = 0; i < leftParameters.Length; i++)
                    {
                        if (leftParameters[i].Type != rightParameters[i].Type)
                        {
                            goto Continue;
                        }
                    }
                    
                    return true;
                }

                if (sym is MemberSymbol or TypeSymbol)
                {
                    if (sym.Name == symbol.Name)
                    {
                        return true;
                    }
                }
                
                Continue:;
            }

            return false;
        }

        if (symbol is TypeParameterSymbol)
        {
            foreach (var sym in _symbols)
            {
                if (sym.Name == symbol.Name)
                {
                    return true;
                }
            }

            return false;
        }
        
        return false;
    }
}
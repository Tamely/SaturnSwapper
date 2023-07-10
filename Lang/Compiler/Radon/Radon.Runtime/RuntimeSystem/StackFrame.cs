using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.Runtime.RuntimeInfo;
using Radon.Runtime.RuntimeSystem.RuntimeObjects;
using Radon.Utilities;

namespace Radon.Runtime.RuntimeSystem;

internal readonly struct StackFrame
{
    private readonly Stack<IRuntimeObject> _stack;
    private readonly List<IRuntimeObject> _popped;
    private readonly Dictionary<LocalInfo, IRuntimeObject> _locals;
    private readonly ImmutableArray<IRuntimeObject> _arguments;

    public StackFrame(ImmutableArray<LocalInfo> locals, ImmutableArray<IRuntimeObject> arguments)
    {
        _stack = new Stack<IRuntimeObject>();
        _popped = new List<IRuntimeObject>();
        _locals = new Dictionary<LocalInfo, IRuntimeObject>();
        _arguments = arguments;
        foreach (var local in locals)
        {
            _locals.Add(local, IRuntimeObject.Null(ManagedRuntime.System.GetType(local.Type)));
        }
    }
    
    public ImmutableArray<IRuntimeObject> Popped => _popped.ToImmutableArray();
    public int StackCount => _stack.Count;
    
    public void Clear()
    {
        _stack.Clear();
        _popped.Clear();
        _locals.Clear();
    }
    
    public void Push(IRuntimeObject value)
    {
        _stack.Push(value);
    }
    
    public IRuntimeObject Pop()
    {
        var value = _stack.Pop();
        _popped.Add(value);
        return value;
    }
    
    public void SetLocal(int index, IRuntimeObject value)
    {
        var local = _locals.KeyAt(index);
        _locals[local] = value;
    }
    
    public IRuntimeObject GetLocal(int index)
    {
        var local = _locals.KeyAt(index);
        return _locals[local];
    }
    
    public IRuntimeObject GetArgument(int index)
    {
        return _arguments[index];
    }
}
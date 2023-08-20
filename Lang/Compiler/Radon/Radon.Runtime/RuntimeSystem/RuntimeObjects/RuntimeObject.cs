using Radon.CodeAnalysis.Emit;
using Radon.Runtime.Memory;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal abstract class RuntimeObject
{
    public abstract RuntimeType Type { get; }
    public abstract int Size { get; }
    public abstract nuint Pointer { get; }
    
    public bool IsReference => this is ManagedReference;
    public bool IsValueType => Type.TypeInfo.IsValueType;
    public bool IsArray => Type.TypeInfo.IsArray;
    
    public abstract RuntimeObject ComputeOperation(OpCode operation, RuntimeObject? other, StackFrame stackFrame);
    public abstract override string ToString();

    public bool IsDeadObject()
    {
        var roots = ManagedRuntime.GetRoots();
        foreach (var root in roots)
        {
            if (root == this)
            {
                return false;
            }
            
            if (root.Search(this))
            {
                return false;
            }
        }
        
        return true;
    }

    private bool Search(RuntimeObject obj)
    {
        if (this == obj)
        {
            return true;
        }

        switch (this)
        {
            case ManagedObject managedObject:
            {
                var fields = managedObject.Fields;
                foreach (var field in fields)
                {
                    if (field.Search(obj))
                    {
                        return true;
                    }
                }
                
                break;
            }
            case ManagedReference managedReference:
            {
                if (managedReference.Target == nuint.Zero)
                {
                    return false;
                }
                
                var heapObj = ManagedRuntime.HeapManager.GetObject(managedReference.Target);
                return heapObj.Search(obj);
            }
            case ManagedArray managedArray:
            {
                var elements = managedArray.Elements;
                foreach (var element in elements)
                {
                    if (element.Search(obj))
                    {
                        return true;
                    }
                }

                break;
            }
        }

        return false;
    }
}
using Radon.CodeAnalysis.Emit;
using Radon.Common;
using Radon.Runtime.Memory;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

public abstract class RuntimeObject
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
        Logger.Log("Retrieving object roots...", LogLevel.Info);
        var roots = ManagedRuntime.GetRoots();
        Logger.Log("Retrieved object roots.", LogLevel.Info);
        Logger.Log("Searching for object...", LogLevel.Info);
        foreach (var root in roots)
        {
            if (root.Search(this))
            {
                Logger.Log("Object is alive.", LogLevel.Info);
                return false;
            }
        }
        
        Logger.Log("Object is dead.", LogLevel.Info);
        return true;
    }

    private bool Search(RuntimeObject obj)
    {
        Logger.Log("Searching object graph...", LogLevel.Info);
        if (this == obj)
        {
            Logger.Log("Object found.", LogLevel.Info);
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
                        Logger.Log("Object found.", LogLevel.Info);
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
                        Logger.Log("Object found.", LogLevel.Info);
                        return true;
                    }
                }

                break;
            }
        }

        return false;
    }
}
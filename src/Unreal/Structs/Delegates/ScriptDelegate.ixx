export module Saturn.Delegates.ScriptDelegate;

import Saturn.Core.UObject;
import Saturn.Structs.Name;
import Saturn.Readers.FArchive;

export class FScriptDelegate {
    UObjectPtr Object;
    FName FunctionName;
public:
    friend class FArchive& operator<<(FArchive& Ar, FScriptDelegate& Delegate) {
        return Ar << Delegate.Object << Delegate.FunctionName;
    }

    __forceinline std::string GetFunctionName() {
        return FunctionName.GetText();
    }

    __forceinline UObjectPtr GetObject() {
        return Object;
    }
};
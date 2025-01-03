export module Saturn.Delegates.ScriptDelegate;

import Saturn.Core.UObject;
import Saturn.Structs.Name;
import Saturn.Readers.ZenPackageReader;

export class FScriptDelegate {
    UObjectPtr Object;
    FName FunctionName;
public:
    friend FZenPackageReader& operator<<(FZenPackageReader& Ar, FScriptDelegate& Delegate) {
        return Ar << Delegate.Object << Delegate.FunctionName;
    }

    __forceinline std::string GetFunctionName() {
        return FunctionName.GetString();
    }

    __forceinline UObjectPtr GetObject() {
        return Object;
    }
};
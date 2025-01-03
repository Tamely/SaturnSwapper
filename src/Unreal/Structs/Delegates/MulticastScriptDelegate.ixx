export module Saturn.Delegates.MulticastScriptDelegate;

import <vector>;

import Saturn.Readers.ZenPackageReader;
export import Saturn.Delegates.ScriptDelegate;

export class FMulticastScriptDelegate {
    __forceinline std::vector<FScriptDelegate> GetInvocationList() {
        return InvocationList;
    }

    friend FZenPackageReader& operator<<(FZenPackageReader& Ar, FMulticastScriptDelegate& Delegate) {
        return Ar << Delegate.InvocationList;
    }
protected:
    std::vector<FScriptDelegate> InvocationList;
};
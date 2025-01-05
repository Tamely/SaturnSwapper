module;

#include "Saturn/Log.h"
#include "Saturn/Defines.h"

export module Saturn.Properties.DelegateProperty;

import Saturn.Readers.ZenPackageReader;
export import Saturn.Reflection.FProperty;
import Saturn.Delegates.MulticastScriptDelegate;

export class FDelegateProperty : public FProperty {
public:
    struct Value : public IPropValue {
    public:
        FScriptDelegate Delegate;

        __forceinline bool IsAcceptableType(EPropertyType Type) override {
            return Type == EPropertyType::DelegateProperty;
        }

        __forceinline void PlaceValue(EPropertyType Type, void* OutBuffer) override {
            if (Type == EPropertyType::DelegateProperty) {
                memcpy(OutBuffer, &Delegate, sizeof(Delegate));
            }
            else if (Type == EPropertyType::StrProperty) {
                *((std::string*)OutBuffer) = Delegate.GetFunctionName();
            }
        }

        void Write(FZenPackageReader& Ar, ESerializationMode SerializationMode = ESerializationMode::Normal) override {}
    };

    TUniquePtr<class IPropValue> Serialize(FZenPackageReader& Ar) override {
        auto Ret = std::make_unique<Value>();
        Ar << Ret->Delegate;

        LOG_TRACE("Serialized DelegateProperty with object {0} and function {1}", Ret->Delegate.GetObjectPtr()->GetName(), Ret->Delegate.GetFunctionName());
        
        return std::move(Ret);
    }
};

export class FMulticastDelegateProperty : public FProperty {
public:
    struct Value : public IPropValue {
    public:
        FMulticastScriptDelegate Delegate;

        __forceinline bool IsAcceptableType(EPropertyType Type) override {
            return Type == EPropertyType::MulticastDelegateProperty;
        }

        __forceinline void PlaceValue(EPropertyType Type, void* OutBuffer) override {
            if (Type == EPropertyType::MulticastDelegateProperty) {
                *((FMulticastScriptDelegate*)OutBuffer) = Delegate;
            }
        }

        void Write(FZenPackageReader& Ar, ESerializationMode SerializationMode = ESerializationMode::Normal) override {}
    };

    TUniquePtr<class IPropValue> Serialize(FZenPackageReader& Ar) override {
        auto Ret = std::make_unique<Value>();
        Ar << Ret->Delegate;

        LOG_TRACE("Serialized MulticastDelegateProperty with function {0}", Ret->Delegate.GetInvocationList().size());
        
        return std::move(Ret);
    }
};
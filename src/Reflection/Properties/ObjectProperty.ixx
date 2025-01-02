module;

#include "Saturn/Defines.h"

export module Saturn.Properties.ObjectProperty;

import Saturn.Core.UObject;
import Saturn.Readers.ZenPackageReader;
export import Saturn.Reflection.FProperty;

export class FObjectProperty : public FProperty {
public:
    struct Value : public IPropValue {
    public:
        UObjectPtr Object;

        __forceinline bool IsAcceptableType(EPropertyType Type) override {
            return Type == EPropertyType::ObjectProperty;
        }

        __forceinline void PlaceValue(EPropertyType Type, void* OutBuffer) override {
            if (Type == EPropertyType::ObjectProperty) {
                *((UObjectPtr*)OutBuffer) = Object;
            }
        }

        void Write(FZenPackageReader& Ar, ESerializationMode SerializationMode = ESerializationMode::Normal) override {
            uint8_t val = Val == 1 ? 1 : 0;
            Ar >> val;
        }
    };

    TUniquePtr<class IPropValue> Serialize(FZenPackageReader& Ar) override {
        auto Ret = std::make_unique<Value>();
        Ar << Ret->Object;

        return std::move(Ret);
    }
};
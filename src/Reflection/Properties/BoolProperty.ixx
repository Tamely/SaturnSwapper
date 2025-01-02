module;

#include "Saturn/Defines.h"

export module Saturn.Properties.BoolProperty;

import Saturn.Readers.ZenPackageReader;
export import Saturn.Reflection.FProperty;

export class FBoolProperty : public FProperty {
public:
    struct Value : public IPropValue {
    public:
        bool Val = false;

        __forceinline bool IsAcceptableType(EPropertyType Type) override {
            return Type == EPropertyType::BoolProperty;
        }

        __forceinline void PlaceValue(EPropertyType Type, void* OutBuffer) override {
            memcpy(OutBuffer, &Val, sizeof(Val));
        }

        void Write(FZenPackageReader& Ar, ESerializationMode SerializationMode = ESerializationMode::Normal) override {
            uint8_t val = Val == 1 ? 1 : 0;
            Ar >> val;
        }
    };

    TUniquePtr<class IPropValue> Serialize(FZenPackageReader& Ar) override {
        auto Ret = std::make_unique<Value>();

        uint8_t ByteValue;
        Ar << ByteValue;

        Ret->Val = ByteValue;
        
        return std::move(Ret);
    }
};
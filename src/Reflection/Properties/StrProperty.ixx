module;

#include "Saturn/Defines.h"

export module Saturn.Properties.StrProperty;

import <string>;

import Saturn.Readers.ZenPackageReader;
export import Saturn.Reflection.FProperty;

export class FStrProperty : public FProperty {
public:
    struct Value : public IPropValue {
    public:
        std::string Str;

        __forceinline bool IsAcceptableType(EPropertyType Type) override {
            return Type == EPropertyType::StrProperty;
        }

        __forceinline void PlaceValue(EPropertyType Type, void* OutBuffer) override {
            if (Type == EPropertyType::StrProperty) {
                *(std::string*)OutBuffer = Str;
            }
        }

        void Write(FZenPackageReader& Ar, ESerializationMode SerializationMode = ESerializationMode::Normal) override {
            Ar >> Str;
        }
    };

    TUniquePtr<class IPropValue> Serialize(FZenPackageReader& Ar) override {
        auto Ret = std::make_unique<Value>();
        Ar << Ret->Str;
        
        return std::move(Ret);
    }
};
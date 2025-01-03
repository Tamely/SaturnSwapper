module;

#include "Saturn/Defines.h"

export module Saturn.Properties.SoftObjectProperty;

import Saturn.Paths.SoftObjectPath;
import Saturn.Readers.ZenPackageReader;
export import Saturn.Reflection.FProperty;

export class FSoftObjectProperty : public FProperty {
public:
    struct Value : public IPropValue {
    public:
        FSoftObjectPath Path;

        __forceinline bool IsAcceptableType(EPropertyType Type) override {
            return Type == EPropertyType::SoftObjectProperty or Type == EPropertyType::ObjectProperty;
        }

        __forceinline void PlaceValue(EPropertyType Type, void* OutBuffer) override {
            if (Type == EPropertyType::SoftObjectProperty) {
                *((FSoftObjectPath*)OutBuffer) = Path;
            }
            else if (Type == EPropertyType::ObjectProperty) {
                // TODO: This
            }
        }

        void Write(FZenPackageReader& Ar, ESerializationMode SerializationMode = ESerializationMode::Normal) override {
            Ar >> Path;
        }
    };

    TUniquePtr<class IPropValue> Serialize(FZenPackageReader& Ar) override {
        auto Ret = std::make_unique<Value>();
        Ar << Ret->Path;
        
        return std::move(Ret);
    }
};
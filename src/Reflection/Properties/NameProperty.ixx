module;

#include "Saturn/Defines.h"

export module Saturn.Properties.NameProperty;

import Saturn.Structs.Name;
import Saturn.Readers.ZenPackageReader;
export import Saturn.Reflection.FProperty;

export class FNameProperty : public FProperty {
    class Value : public IPropValue {
    public:
        FName Name;

        __forceinline bool IsAcceptableType(EPropertyType Type) override {
            return Type == EPropertyType::NameProperty or Type == EPropertyType::StrProperty;
        }

        __forceinline void PlaceValue(EPropertyType Type, void* OutBuffer) override {
            if (Type == EPropertyType::NameProperty) {
                *((FName*)OutBuffer) = Name;
            }
            else if (Type == EPropertyType::StrProperty) {
                ((std::string*)OutBuffer)->assign(Name.GetText());
            }
        }

        __forceinline void Write(FZenPackageReader& Ar, ESerializationMode SerializationMode = ESerializationMode::Normal) override {
            Ar >> Name;
        }
    };

    TUniquePtr<IPropValue> Serialize(FZenPackageReader& Ar) override {
        auto Ret = std::make_unique<Value>();
        Ar << Ret->Name;

        return std::move(Ret);
    }
};
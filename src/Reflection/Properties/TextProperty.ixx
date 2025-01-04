module;

#include "Saturn/Log.h"
#include "Saturn/Defines.h"

export module Saturn.Properties.TextProperty;

import Saturn.Localization.Text;
import Saturn.Readers.ZenPackageReader;
export import Saturn.Reflection.FProperty;

export class FTextProperty : public FProperty {
public:
    struct Value : public IPropValue {
    public:
        FText Text;

        __forceinline bool IsAcceptableType(EPropertyType Type) override {
            return false; // TODO: this
        }

        __forceinline void PlaceValue(EPropertyType Type, void* OutBuffer) override {
           
        }

        void Write(FZenPackageReader& Ar, ESerializationMode SerializationMode = ESerializationMode::Normal) override {
            Ar >> Text;
        }
    };

    TUniquePtr<class IPropValue> Serialize(FZenPackageReader& Ar) override {
        auto Ret = std::make_unique<Value>();
        Ar << Ret->Text;

        LOG_INFO(Ret->Text.ToString());
        
        return std::move(Ret);
    }
};
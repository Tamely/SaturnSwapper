module;

#include "Saturn/Log.h"
#include "Saturn/Defines.h"

export module Saturn.Properties.ObjectProperty;

import Saturn.Core.UObject;
import Saturn.Asset.PackageIndex;
import Saturn.Readers.ZenPackageReader;
export import Saturn.Reflection.FProperty;

export class FObjectProperty : public FProperty {
public:
    struct Value : public IPropValue {
    public:
        FPackageIndex Index;
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
            Ar >> Index.ForDebugging();
        }
    };

    TUniquePtr<class IPropValue> Serialize(FZenPackageReader& Ar) override {
        auto Ret = std::make_unique<Value>();
        Ar << Ret->Index;
        Ar.SeekCur(-1 * sizeof(FPackageIndex));
        Ar << Ret->Object;

        LOG_TRACE("Serialized ObjectProperty with index {0}", Ret->Index.ForDebugging());

        return std::move(Ret);
    }
};
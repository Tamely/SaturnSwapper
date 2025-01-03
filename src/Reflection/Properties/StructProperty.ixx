module;

#include "Saturn/Log.h"
#include "Saturn/Defines.h"

export module Saturn.Properties.StructProperty;

import Saturn.Core.UObject;
import Saturn.Readers.ZenPackageReader;
export import Saturn.Reflection.FProperty;

export class FStructProperty : public FProperty {
public:
    friend class FPropertyFactory;

    struct Value : public IPropValue {
    public:
        UObjectPtr StructObject;

        __forceinline bool IsAcceptableType(EPropertyType Type) override {
            return false; // TODO: This
        }

        __forceinline void PlaceValue(EPropertyType Type, void* OutBuffer) override {
            
        }

        void Write(FZenPackageReader& Ar, ESerializationMode SerializationMode = ESerializationMode::Normal) override {
            LOG_ERROR("Serializing structs like this has not been implemented!");
        }
    };

    template<typename StructType>
    struct NativeValue : IPropValue {
    public:
        StructType Value;

        __forceinline bool IsAcceptableType(EPropertyType Type) override {
            return false;
        }

        __forceinline void PlaceValue(EPropertyType Type, void* OutBuffer) override {

        }

        void Write(FZenPackageReader& Ar, ESerializationMode SerializationMode = ESerializationMode::Normal) override {
            Ar >> Value;
        }
    };

    TUniquePtr<class IPropValue> Serialize(FZenPackageReader& Ar) override {
        return Struct->SerializeItem(Ar);
    }

    template <typename StructType>
    static TUniquePtr<IPropValue> SerializeNativeStruct(class FZenPackageReader& Ar) {
        auto Ret = std::make_unique<FStructProperty::NativeValue<StructType>>();
        Ar << Ret->Value;

        return std::move(Ret);
    }
private:
    UStructPtr Struct;
};
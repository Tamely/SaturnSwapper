module;

#include "Saturn/Log.h"
#include "Saturn/Defines.h"

export module Saturn.Properties.ArrayProperty;

import <vector>;

import Saturn.Readers.ZenPackageReader;
export import Saturn.Reflection.FProperty;

export class FArrayProperty : public FProperty {
public:
    friend class FPropertyFactory;

    struct Value : public IPropValue {
    public:
        std::vector<TUniquePtr<class IPropValue>> Array;

        __forceinline bool IsAcceptableType(EPropertyType Type) override {
            return Type == EPropertyType::ArrayProperty;
        }

        __forceinline void PlaceValue(EPropertyType Type, void* OutBuffer) override {
            auto& ArrBytes = *(std::vector<uint8_t>*)OutBuffer;
            ArrBytes.resize(ValueTypeSize * Array.size());

            for (size_t i = 0; i < Array.size(); i++) {
                auto& Prop = Array[i];

                if (!Prop->IsAcceptableType(Type)) {
                    continue;
                }

                Prop->PlaceValue(Type, ArrBytes.data() + (i * ValueTypeSize));
            }
        }

        void Write(FZenPackageReader& Ar, ESerializationMode SerializationMode = ESerializationMode::Normal) override {
            Ar >> Array.size();

            for (size_t i = 0; i < Array.size(); i++) {
                auto& Prop = Array[i];

                Prop->Write(Ar);
            }
        }
    };

    TUniquePtr<class IPropValue> Serialize(FZenPackageReader& Ar, ESerializationMode SerializationMode = ESerializationMode::Normal) override {
        auto Ret = std::make_unique<FArrayProperty::Value>();

        int32_t ArrayCount;
        Ar << ArrayCount;

        Ret->Array.resize(ArrayCount);

        for (size_t i = 0; i < ArrayCount; i++) {
            Ret->Array[i] = ElementType->Serialize(Ar);
        }

        LOG_TRACE("Serialized ArrayProperty with length {0}", ArrayCount);

        return std::move(Ret);
    }
private:
    FProperty* ElementType;
public:
    __forceinline FProperty* GetElementType() {
        return ElementType;
    }
};
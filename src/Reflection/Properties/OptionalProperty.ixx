module;

#include "Saturn/Defines.h"
#include "tsl/ordered_set.h"

export module Saturn.Properties.OptionalProperty;

import Saturn.Readers.ZenPackageReader;
export import Saturn.Reflection.FProperty;

template <typename K>
using TOrderedSet = tsl::ordered_set<K>;

export class FOptionalProperty : public FProperty {
public:
    friend class FPropertyFactory;
private:
    FProperty* ElementType;
public:
    struct Value : public IPropValue {
    public:
        std::vector<TSharedPtr<class IPropValue>> ElementsToRemove;
        TOrderedSet<TSharedPtr<class IPropValue>> Value;

        __forceinline bool IsAcceptableType(EPropertyType Type) override {
            return Type == EPropertyType::OptionalProperty;
        }

        __forceinline void PlaceValue(EPropertyType Type, void* OutBuffer) override {
            
        }

        void Write(FZenPackageReader& Ar, ESerializationMode SerializationMode = ESerializationMode::Normal) override {
            Ar >> static_cast<uint32_t>(ElementsToRemove.size());
            for (TSharedPtr<IPropValue> value : ElementsToRemove) {
                value->Write(Ar);
            }

            Ar >> static_cast<uint32_t>(Value.size());
            for (auto& kvp : Value) {
                kvp->Write(Ar);
            }
        }
    };

    TUniquePtr<class IPropValue> Serialize(FZenPackageReader& Ar) override {
        auto Ret = std::make_unique<Value>();

        int32_t NumElementsToRemove = 0;
        Ar << NumElementsToRemove;

        for (; NumElementsToRemove; --NumElementsToRemove) {
            TUniquePtr<IPropValue> elementToRemove = ElementType->Serialize(Ar);
            Ret->ElementsToRemove.push_back(std::move(elementToRemove));
        }

        int32_t NumEntries = 0;
        Ar << NumEntries;

        for (; NumEntries; --NumEntries) {
            TUniquePtr<IPropValue> value = ElementType->Serialize(Ar);
            Ret->Value.insert(std::move(value));
        }

        return std::move(Ret);
    }

    __forceinline FProperty* GetElementType() {
        return ElementType;
    }
};
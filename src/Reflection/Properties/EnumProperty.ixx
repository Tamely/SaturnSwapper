module;

#include "Saturn/Defines.h"

export module Saturn.Properties.EnumProperty;

import <string>;
import <vector>;

import Saturn.Structs.Name;
import Saturn.Readers.ZenPackageReader;
export import Saturn.Reflection.FProperty;

export struct FReflectedEnum {
    std::vector<std::string> Enum;
    std::string EnumName;
};

export class FEnumProperty : public FProperty {
public:
    friend class FPropertyFactory;

    struct Value : public IPropValue {
    public:
        uint64_t BinaryValue = 0;
        std::string EnumName;

        __forceinline bool IsAcceptableType(EPropertyType Type) override {
            return Type == EPropertyType::StrProperty or Type == EPropertyType::NameProperty;
        }

        __forceinline void PlaceValue(EPropertyType Type, void* OutBuffer) override {
            if (Type == EPropertyType::NameProperty) {
                *((FName*)OutBuffer) = EnumName;
            }
            else if (Type == EPropertyType::StrProperty) {
                *((std::string*)OutBuffer) = EnumName;
            }
        }

        void Write(FZenPackageReader& Ar, ESerializationMode SerializationMode = ESerializationMode::Normal) override {
            uint8_t valAsByte = static_cast<uint8_t>(BinaryValue);
            Ar >> valAsByte;
        }
    };
private:
    TSharedPtr<FReflectedEnum> Enum;
    FProperty* UnderlyingProp;
public:
    __forceinline FProperty* GetUnderlying() {
        return UnderlyingProp;
    }

    __forceinline std::vector<std::string> GetValues() {
        return Enum->Enum;
    }

    __forceinline std::string GetEnumName() {
        return Enum->EnumName;
    }

    TUniquePtr<class IPropValue> Serialize(FZenPackageReader& Ar) override {
        auto Ret = std::make_unique<Value>();

        if (Enum and UnderlyingProp) {
            auto EnumVal = UnderlyingProp->Serialize(Ar);
            auto IntValOpt = EnumVal->TryGetValue<int64_t>();

            if (!IntValOpt.has_value()) {
                return nullptr;
            }

            auto EnumIndex = IntValOpt.value();

            if (EnumIndex >= Enum->Enum.size()) {
                return nullptr;
            }

            Ret->BinaryValue = EnumIndex;
            Ret->EnumName = Enum->Enum[EnumIndex];
        }

        return std::move(Ret);
    }
};
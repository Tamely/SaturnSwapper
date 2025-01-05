module;

#include "Saturn/Log.h"
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

    TUniquePtr<class IPropValue> Serialize(FZenPackageReader& Ar, ESerializationMode SerializationMode = ESerializationMode::Normal) override {
        auto Ret = std::make_unique<Value>();

        switch (SerializationMode) {
            case ESerializationMode::Zero: {
                Ret->BinaryValue = 0;

                LOG_TRACE("Serialized EnumProperty with name {0} and index {1}", Ret->EnumName, 0);

                break;
            }
            case ESerializationMode::Normal: {
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
                    Ret->EnumName = IndexToEnum(Enum, EnumIndex);

                    LOG_TRACE("Serialized EnumProperty with name {0} and index {1}", Ret->EnumName, EnumIndex);
                }
                else if (Enum) {
                    uint8_t EnumIndex;
                    Ar << EnumIndex;

                    if (EnumIndex >= Enum->Enum.size()) {
                        return nullptr;
                    }

                    Ret->BinaryValue = EnumIndex;
                    Ret->EnumName = IndexToEnum(Enum, EnumIndex);

                    LOG_TRACE("Serialized EnumProperty with name {0} and index {1}", Ret->EnumName, EnumIndex);
                }
                break;
            }
            default: {
                FName val;
                Ar << val;

                Ret->EnumName = val.ToString();

                LOG_TRACE("Serialized EnumProperty with name {0}", Ret->EnumName);
            }
        };
        return std::move(Ret);
    }

    std::string IndexToEnum(TSharedPtr<FReflectedEnum> Enum, int Index) {
        std::string EnumName = "";
        if (Enum) {
            EnumName = Enum->EnumName;
        }
        else if (!Enum || Enum->EnumName.empty()) {
            EnumName = std::to_string(Index);
            return EnumName;
        }

        if (Enum and Enum->Enum.size() > 0) {
            for (int i = 0; i < Enum->Enum.size(); i++) {
                auto& enumName = Enum->Enum[i];
                if (i == Index) {
                    return Enum->EnumName + "::" + Enum->Enum[i];
                }
            }
        }

        return Enum->EnumName + "::" + std::to_string(Index);
    }
};
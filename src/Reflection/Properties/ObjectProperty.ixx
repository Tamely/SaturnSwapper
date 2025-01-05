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

    TUniquePtr<class IPropValue> Serialize(FZenPackageReader& Ar, ESerializationMode SerializationMode = ESerializationMode::Normal) override {
        auto Ret = std::make_unique<Value>();
        switch (SerializationMode) {
            case ESerializationMode::Zero: {
                Ret->Index = FPackageIndex::FromExport(-1);
                if (Ret->Index.IsNull()) {
                    Ret->Object = UObjectPtr(nullptr);
                    break;
                }

                if (Ret->Index.IsExport()) {
                    int32_t ExportIndex = Ret->Index.ToExport();
                    if (ExportIndex < Ar.PackageData->Exports.size()) {
                        Ret->Object = Ar.PackageData->Exports[ExportIndex].Object;
                    }
                    else {
                        LOG_ERROR("FObjectProperty: Export index read is not a valid index.");
                    }

                    break;
                }

                auto& ImportMap = Ar.PackageData->Header.ImportMap;

                if (Ret->Index.IsImport() && Ret->Index.ToImport() < ImportMap.size()) {
                    Ret->Object = Ar.PackageData->Package->IndexToObject(Ar.PackageData->Header, Ar.PackageData->Exports, Ar.PackageData->Header.ImportMap[Ret->Index.ToImport()]);
                }
                else {
                    LOG_ERROR("FObjectProperty: Bad object import index.");
                }
            }
            default: {
                Ar << Ret->Index;
                Ar.SeekCur(-1 * sizeof(FPackageIndex));
                Ar << Ret->Object;
            }
        }

        LOG_TRACE("Serialized ObjectProperty with index {0}", Ret->Index.ForDebugging());

        return std::move(Ret);
    }
};
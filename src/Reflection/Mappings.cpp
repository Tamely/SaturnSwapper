import Saturn.Reflection.Mappings;

#include <Saturn/Defines.h>

import Saturn.Core.UObject;
import Saturn.Readers.FArchive;
import Saturn.Readers.FileReaderNoWrite;

import Saturn.Compression.Oodle;

import Saturn.Properties.PropertyTypes;

import <string>;
import <vector>;

#define USMAP_FILE_MAGIC = 0x30C4;

static std::string InvalidName{};

static __forceinline std::string& ReadName(FArchive& Ar, std::vector<std::string>& Names) {
    int32_t NameIdx;
    Ar << NameIdx;

    if (NameIdx == -1) {
        return InvalidName;
    }

    return Names[NameIdx];
}

template <typename T>
static TObjectPtr<T> GetOrCreateObject(std::string& ClassName, TMap<std::string, UObjectPtr>& ObjectArray) {
    if (ObjectArray.contains(ClassName)) {
        return ObjectArray[ClassName].As<T>();
    }

    TObjectPtr<T> Ret = std::make_shared<T>();
    Ret->SetName(ClassName);

    ObjectArray[ClassName] = Ret;

    return Ret;
}

enum class EUsmapVersion : uint8_t {
    Initial,
    PackageVersioning,
    LatestPlusOne,
    Latest = LatestPlusOne - 1
};

enum class EUsmapCompressionMethod : uint8_t {
    None,
    Oodle,
    Brotli,
    ZStandard,

    Unknown = 0xFF
};

class FPropertyFactory {
    TMap<std::string, UObjectPtr>& ObjectArray;
    TMap<std::string, TSharedPtr<FReflectedEnum>> Enums;
    std::vector<std::string>& Names;

    FProperty* SerializePropertyInternal(FArchive& Ar) {
        EPropertyType Type;
        Ar.Serialize(&Type, sizeof(Type));

        FProperty* Ret = nullptr;

        switch (Type) {
            case EPropertyType::EnumProperty: {
                auto Prop = new FEnumProperty;
                Prop->UnderlyingProp = SerializePropertyInternal(Ar);
                Prop->Enum = Enums[ReadName(Ar, Names)];
                Ret = Prop;
                break;
            }
            case EPropertyType::StructProperty: {
                //auto Prop = new FStructProperty;
            }
        }
    }
};
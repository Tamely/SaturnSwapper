import Saturn.Reflection.Mappings;

#include <Saturn/Log.h>
#include <Saturn/Defines.h>

import Saturn.Core.TObjectPtr;
import Saturn.Core.UObject;
import Saturn.Readers.FArchive;
import Saturn.Readers.MemoryReader;
import Saturn.Readers.FileReaderNoWrite;

import Saturn.Compression.Oodle;

import Saturn.Properties.PropertyTypes;

import <string>;
import <vector>;
import <unordered_map>;

#define USMAP_FILE_MAGIC 0x30C4

static std::string InvalidName{};

std::string& Mappings::ReadName(FArchive& Ar, std::vector<std::string>& Names) {
    int32_t NameIdx;
    Ar << NameIdx;

    if (NameIdx == -1) {
        return InvalidName;
    }

    return Names[NameIdx];
}

template <typename T>
TObjectPtr<T> Mappings::GetOrCreateObject(std::string& ClassName, std::unordered_map<std::string, UObjectPtr>& ObjectArray) {
    if (ObjectArray.contains(ClassName)) {
        return ObjectArray[ClassName].As<T>();
    }

    TObjectPtr<T> Ret = std::make_shared<T>();
    Ret->SetName(ClassName);

    ObjectArray.insert_or_assign(ClassName, Ret.As<UObject>());
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
    std::unordered_map<std::string, UObjectPtr>& ObjectArray;
    std::unordered_map<std::string, TSharedPtr<FReflectedEnum>> Enums;
    std::vector<std::string>& Names;

    FProperty* SerializePropertyInternal(FArchive& Ar) {
        EPropertyType Type;
        Ar.Serialize(&Type, sizeof(Type));

        FProperty* Ret = nullptr;

        switch (Type) {
            case EPropertyType::EnumProperty: {
                auto Prop = new FEnumProperty;
                Prop->UnderlyingProp = SerializePropertyInternal(Ar);
                Prop->Enum = Enums[Mappings::ReadName(Ar, Names)];
                Ret = Prop;
                break;
            }
            case EPropertyType::StructProperty: {
                auto Prop = new FStructProperty;
            }
        }

        return Ret;
    }
public:
    FPropertyFactory(std::vector<std::string>& InNames, std::unordered_map<std::string, UObjectPtr>& InObjectArray)
        : Names(InNames), ObjectArray(InObjectArray) {}

    void SerializeEnums(FArchive& Ar) {
        uint32_t EnumsCount;
        Ar << EnumsCount;

        Enums.reserve(EnumsCount);

        for (size_t i = 0; i < EnumsCount; i++) {
            auto& EnumName = Mappings::ReadName(Ar, Names);

            uint8_t EnumNamesCount;
            Ar << EnumNamesCount;

            auto Enum = std::make_shared<FReflectedEnum>();
            Enum->EnumName = EnumName;

            auto& EnumNames = Enum->Enum;
            EnumNames.resize(EnumNamesCount);

            for (size_t j = 0; j < EnumNamesCount; j++) {
                EnumNames[j] = Mappings::ReadName(Ar, Names);
            }

            Enums.insert_or_assign(EnumName, Enum);
        }
    }

    FProperty* SerializeProperty(FArchive& Ar) {
        uint16_t Index;
        uint8_t ArrayDim;
        Ar << Index << ArrayDim;

        std::string& Name = Mappings::ReadName(Ar, Names);

        auto Ret = SerializePropertyInternal(Ar);

        Ret->Name = Name;
        Ret->Index = Index;
        Ret->ArrayDim = ArrayDim;

        return Ret;
    }
};

bool Mappings::RegisterTypesFromUsmap(const std::string& Path, std::unordered_map<std::string, UObjectPtr>& ObjectArray) {
    FFileReaderNoWrite FileAr(Path.c_str());

    if (!FileAr.IsValid()) {
        LOG_ERROR("Could not open handle to usmap file or it does not exist.");
        return false;
    }

    uint16_t Magic;
    FileAr << Magic;

    if (Magic != USMAP_FILE_MAGIC) {
        LOG_ERROR("Invalid usmap file magic.");
        return false;
    }

    EUsmapVersion Ver;
    FileAr.Serialize(&Ver, sizeof(Ver));

    if (Ver < EUsmapVersion::Initial || Ver > EUsmapVersion::Latest) {
        LOG_ERROR("Invalid usmap file version.");
        return false;
    }

    EUsmapCompressionMethod CompressionMethod;
    FileAr.Serialize(&CompressionMethod, sizeof(EUsmapCompressionMethod));

    uint32_t CompressedSize, DecompressedSize;
    FileAr << CompressedSize << DecompressedSize;

    auto UsmapBuf = std::make_unique<uint8_t[]>(DecompressedSize);

    switch (CompressionMethod) {
        case EUsmapCompressionMethod::None: {
            if (CompressedSize != DecompressedSize) {
                LOG_ERROR("Usmap compression method is uncompressed but the compressed and decompressed size are different.");
                return false;
            }

            FileAr.Serialize(UsmapBuf.get(), DecompressedSize);
            break;
        }
        case EUsmapCompressionMethod::Oodle: {
            auto CompressedBuf = std::make_unique<uint8_t[]>(CompressedSize);
            FileAr.Serialize(CompressedBuf.get(), CompressedSize);

            Oodle::Decompress(CompressedBuf.get(), CompressedSize, UsmapBuf.get(), DecompressedSize);
            break;
        }
        // TODO: Brotli and ZStandard
        default: {
            LOG_ERROR("Invalid usmap compression method.");
            return false;
        }
    }

    auto Ar = FMemoryReader(UsmapBuf.get(), DecompressedSize);

    uint32_t NamesCount;
    Ar << NamesCount;

    std::vector<std::string> Names(NamesCount);

    for (size_t i = 0; i < NamesCount; i++) {
        auto& Str = Names[i];

        uint8_t Len;
        Ar << Len;

        Str.resize(Len);
        Ar.Serialize(&Str[0], Len);
    }

    FPropertyFactory Factory(Names, ObjectArray);
    
    Factory.SerializeEnums(Ar);

    uint32_t StructCount;
    Ar << StructCount;

    for (size_t i = 0; i < StructCount; i++) {
        auto& ClassName = ReadName(Ar, Names);

        auto Struct = GetOrCreateObject<UClass>(ClassName, ObjectArray);

        auto& SuperName = ReadName(Ar, Names);

        if (!SuperName.empty()) {
            if (ObjectArray.contains(SuperName)) {
                Struct->SetSuper(ObjectArray[SuperName].As<UStruct>());
            }
            else {
                UClassPtr Super = std::make_shared<UClass>();
                ObjectArray.insert_or_assign(SuperName, Super.As<UObject>());

                Struct->SetSuper(Super.As<UStruct>());
            }
        }

        uint16_t PropCount, SerializablePropCount;
        Ar << PropCount << SerializablePropCount;

        if (SerializablePropCount) {
            auto& Link = Struct->PropertyLink;
            FProperty* Previous = nullptr;

            Link = Previous = Factory.SerializeProperty(Ar);

            for (size_t i = 1; i < SerializablePropCount; i++) {
                auto Prop = Factory.SerializeProperty(Ar);

                Previous->Next = Prop;
                Previous = Prop;
            }
        }
    }

    return true;
}
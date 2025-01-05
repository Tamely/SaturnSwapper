import Saturn.Properties.StructProperty;

import <functional>;
import <unordered_map>;
import <optional>;

#include "Saturn/Log.h"
#include "Saturn/Defines.h"

import Saturn.Core.UObject;
import Saturn.Reflection.FProperty;
import Saturn.Readers.ZenPackageReader;

import Saturn.Math.Box;
import Saturn.Math.Vector;
import Saturn.Structs.Guid;
import Saturn.Tags.GameplayTag;
import Saturn.Paths.SoftObjectPath;
import Saturn.Structs.InstancedStruct;
import Saturn.Tags.GameplayTagContainer;

static phmap::flat_hash_map<std::string, std::function<TUniquePtr<IPropValue>(FZenPackageReader&)>> NativeStructs = {
    { "Box", [](FZenPackageReader& Ar) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FBox>(Ar); } },
    { "Box2D", [](FZenPackageReader& Ar) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FBox2D>(Ar); } },
    { "Guid", [](FZenPackageReader& Ar) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FGuid>(Ar); } },
    //{ "GameplayTag", [](FZenPackageReader& Ar) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FGameplayTag>(Ar); } },
    { "GameplayTagContainer", [](FZenPackageReader& Ar) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FGameplayTagContainer>(Ar); } },
    { "InstancedStruct", [](FZenPackageReader& Ar) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FInstancedStruct>(Ar); } },
    { "SoftObjectPath", [](FZenPackageReader& Ar) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FSoftObjectPath>(Ar); } },
    { "Vector", [](FZenPackageReader& Ar) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FVector>(Ar); } },
    { "Vector2D", [](FZenPackageReader& Ar) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FVector2D>(Ar); } },
    { "Vector4", [](FZenPackageReader& Ar) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FVector4>(Ar); } }
};

TUniquePtr<IPropValue> UStruct::SerializeItem(FZenPackageReader& Ar) {
    auto StructName = GetName();

    if (NativeStructs.contains(StructName)) {
        return std::move(NativeStructs[StructName](Ar));
    }
     
    LOG_TRACE("Native structs did not contain {0}", StructName);

    auto Ret = std::make_unique<FStructProperty::Value>();
    auto ThisClass = This<UClass>();

    Ret->StructObject = std::make_shared<UObject>();
    Ret->StructObject->SetClass(ThisClass);

    SerializeScriptProperties(Ar, Ret->StructObject);

    return std::move(Ret);
}
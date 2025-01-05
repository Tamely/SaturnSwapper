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
import Saturn.Math.Color;
import Saturn.Math.Vector;
import Saturn.Structs.Guid;
import Saturn.Misc.DateTime;
import Saturn.Math.TIntVector;
import Saturn.Math.PerPlatform;
import Saturn.Curves.RichCurve;
import Saturn.Engine.SmartName;
import Saturn.Tags.GameplayTag;
import Saturn.Math.LinearColor;
import Saturn.Curves.SimpleCurve;
import Saturn.Paths.SoftObjectPath;
import Saturn.Structs.InstancedStruct;
import Saturn.Engine.NavAgentSelector;
import Saturn.Tags.GameplayTagContainer;
import Saturn.Engine.MaterialExpression;

static phmap::flat_hash_map<std::string, std::function<TUniquePtr<IPropValue>(FZenPackageReader&, ESerializationMode)>> NativeStructs = {
    { "Box", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FBox>(Ar, SerializationMode); } },
    { "Box2D", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FBox2D>(Ar, SerializationMode); } },
    { "Box2f", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<TBox2<float>>(Ar, SerializationMode); } },
    { "Color", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FColor>(Ar, SerializationMode); } },
    { "ColorMaterialInput", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FMaterialInput<FColor>>(Ar, SerializationMode); } },
    { "DateTime", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FDateTime>(Ar, SerializationMode); } },
    { "ExpressionInput", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FExpressionInput>(Ar, SerializationMode); } },
    { "Guid", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FGuid>(Ar, SerializationMode); } },
    { "NavAgentSelector", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FNavAgentSelector>(Ar, SerializationMode); } },
    { "SmartName", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FSmartName>(Ar, SerializationMode); } },
    { "RichCurveKey", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FRichCurveKey>(Ar, SerializationMode); } },
    { "SimpleCurveKey", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FSimpleCurveKey>(Ar, SerializationMode); } },
    { "ScalarMaterialInput", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FMaterialInput<float>>(Ar, SerializationMode); } },
    { "VectorMaterialInput", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FMaterialInputVector>(Ar, SerializationMode); } },
    { "Vector2MaterialInput", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FMaterialInputVector2D>(Ar, SerializationMode); } },
    { "MaterialAttributesInput", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FExpressionInput>(Ar, SerializationMode); } },
    { "PerPlatformBool", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FPerPlatformBool>(Ar, SerializationMode); } },
    { "PerPlatformInt", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FPerPlatformInt>(Ar, SerializationMode); } },
    { "PerPlatformFrameRate", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FPerPlatformFrameRate>(Ar, SerializationMode); } },
    { "PerPlatformFString", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FPerPlatformFString>(Ar, SerializationMode); } },
    { "LinearColor", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FLinearColor>(Ar, SerializationMode); } },
    { "GameplayTagContainer", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FGameplayTagContainer>(Ar, SerializationMode); } },
    { "InstancedStruct", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FInstancedStruct>(Ar, SerializationMode); } },
    { "SoftObjectPath", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FSoftObjectPath>(Ar, SerializationMode); } },
    { "Vector", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FVector>(Ar, SerializationMode); } },
    { "Vector2D", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FVector2D>(Ar, SerializationMode); } },
    { "Vector4", [](FZenPackageReader& Ar, ESerializationMode SerializationMode) -> TUniquePtr<IPropValue> { return FStructProperty::SerializeNativeStruct<FVector4>(Ar, SerializationMode); } }
};

TUniquePtr<IPropValue> UStruct::SerializeItem(FZenPackageReader& Ar, ESerializationMode SerializationMode) {
    auto StructName = GetName();

    if (NativeStructs.contains(StructName)) {
        return std::move(NativeStructs[StructName](Ar, SerializationMode));
    }

    LOG_TRACE("Struct with name {0} is not found in Native Structs map.", StructName);

    auto Ret = std::make_unique<FStructProperty::Value>();
    auto ThisClass = This<UClass>();

    Ret->StructObject = std::make_shared<UObject>();
    Ret->StructObject->SetClass(ThisClass);

    SerializeScriptProperties(Ar, Ret->StructObject);

    return std::move(Ret);
}
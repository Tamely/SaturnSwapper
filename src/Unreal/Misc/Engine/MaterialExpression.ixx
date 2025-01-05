export module Saturn.Engine.MaterialExpression;

import <cstdint>;

import Saturn.Math.Vector;
import Saturn.Core.UObject;
import Saturn.Structs.Name;
import Saturn.Readers.ZenPackageReader;

export class UMaterialExpression : public UObject {
public:
    void Serialize(FZenPackageReader& Ar) override {
        UObject::Serialize(Ar);
    }
};

FZenPackageReader& operator<<(FZenPackageReader& Ar, TObjectPtr<UMaterialExpression>& Expression) {
    return Ar << reinterpret_cast<UObjectPtr&>(Expression);
}

FZenPackageReader& operator>>(FZenPackageReader& Ar, TObjectPtr<UMaterialExpression>& Expression) {
    return Ar >> reinterpret_cast<UObjectPtr&>(Expression);
}

export struct FExpressionInput {
    TObjectPtr<UMaterialExpression> Expression;

    int32_t OutputIndex;
    FName InputName;

    int32_t Mask;
    int32_t MaskR;
    int32_t MaskG;
    int32_t MaskB;
    int32_t MaskA;

    FName ExpressionName;

    friend FZenPackageReader& operator<<(FZenPackageReader& Ar, FExpressionInput& Input) {
        Ar << Input.Expression;
        Ar << Input.OutputIndex;
        Ar << Input.InputName;
        Ar << Input.Mask;
        Ar << Input.MaskR;
        Ar << Input.MaskG;
        Ar << Input.MaskB;
        Ar << Input.MaskA;

        Input.ExpressionName = Input.Expression->GetName();

        return Ar;
    }

    friend FZenPackageReader& operator>>(FZenPackageReader& Ar, FExpressionInput& Input) {
        Ar >> Input.Expression;
        Ar >> Input.OutputIndex;
        Ar >> Input.InputName;
        Ar >> Input.Mask;
        Ar >> Input.MaskR;
        Ar >> Input.MaskG;
        Ar >> Input.MaskB;
        Ar >> Input.MaskA;

        return Ar;
    }
};

export template <class InputType>
struct FMaterialInput : FExpressionInput {
    bool UseConstant;
    InputType Constant;

    FMaterialInput() {
        UseConstant = false;
        Constant = InputType();
    }

    friend FZenPackageReader& operator<<(FZenPackageReader& Ar, FMaterialInput& Input) {
        Ar << reinterpret_cast<FExpressionInput&>(Input);

        Ar << Input.UseConstant;
        Ar << Input.Constant;

        return Ar;
    }

    friend FZenPackageReader& operator>>(FZenPackageReader& Ar, FMaterialInput& Input) {
        Ar >> reinterpret_cast<FExpressionInput&>(Input);

        Ar >> Input.UseConstant;
        Ar >> Input.Constant;

        return Ar;
    }
};

export struct FMaterialInputVector : FExpressionInput {
    bool UseConstant;
    FVector Constant;

    FMaterialInputVector() {
        UseConstant = false;
        Constant = FVector::ZeroVector;
    }

    friend FZenPackageReader& operator<<(FZenPackageReader& Ar, FMaterialInputVector& Input) {
        Ar << reinterpret_cast<FExpressionInput&>(Input);

        Ar << Input.UseConstant;
        Ar << Input.Constant;

        return Ar;
    }

    friend FZenPackageReader& operator>>(FZenPackageReader& Ar, FMaterialInputVector& Input) {
        Ar >> reinterpret_cast<FExpressionInput&>(Input);

        Ar >> Input.UseConstant;
        Ar >> Input.Constant;

        return Ar;
    }
};

export struct FMaterialInputVector2D : FExpressionInput {
    bool UseConstant;
    FVector2D Constant;

    FMaterialInputVector2D() {
        UseConstant = false;
        Constant = FVector2D::ZeroVector;
    }

    friend FZenPackageReader& operator<<(FZenPackageReader& Ar, FMaterialInputVector2D& Input) {
        Ar << reinterpret_cast<FExpressionInput&>(Input);

        Ar << Input.UseConstant;
        Ar << Input.Constant;

        return Ar;
    }

    friend FZenPackageReader& operator>>(FZenPackageReader& Ar, FMaterialInputVector2D& Input) {
        Ar >> reinterpret_cast<FExpressionInput&>(Input);

        Ar >> Input.UseConstant;
        Ar >> Input.Constant;

        return Ar;
    }
};
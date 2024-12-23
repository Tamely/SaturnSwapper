export module Saturn.Reflection.PropertyIterator;

import Saturn.Core.UObject;
export import Saturn.Reflection.FProperty;

export class FPropertyIterator {
    static constexpr int Invalid = -1;
public:
    __forceinline FPropertyIterator(UStructPtr InStruct)
        : Struct(InStruct), Link(InStruct->GetPropertyLink()) {}

    void Next() {
        if (ArrayIndex + 1 == Link->GetArrayDim()) {
            Link = Link->GetNext();
            ArrayIndex = 0;

            while (!Link) {
                Struct = Struct->GetSuper();

                if (!Struct) {
                    break;
                }

                Link = Struct->GetPropertyLink();
            }
        }
        else {
            ++ArrayIndex;
        }
    }

    __forceinline void operator++() {
        Next();
    }

    __forceinline void operator+=(int Num) {
        while (Num--) {
            Next();
        }
    }

    __forceinline operator bool() const {
        return Link;
    }

    __forceinline FProperty* operator*() {
        return Link;
    }
private:
    UStructPtr Struct;
    FProperty* Link = nullptr;
    __int32 ArrayIndex = 0;
};
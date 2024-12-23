module;

#include "Saturn/Defines.h"

export module Saturn.Core.TObjectPtr;

export template<typename ObjectType>
class TObjectPtr {
    TSharedPtr<ObjectType> Val;
public:
    TObjectPtr() : Val(nullptr) {}
    TObjectPtr(std::nullptr_t Null) : Val(nullptr) {}

    TObjectPtr(TSharedPtr<ObjectType> InObject) : Val(InObject) {}
    TObjectPtr(TSharedPtr<ObjectType>& InObject) : Val(InObject) {}

    __forceinline TObjectPtr operator=(TSharedPtr<ObjectType> Other) {
        Val = Other;
        return *this;
    }

    template <typename T>
    __forceinline bool operator==(TObjectPtr<T>& Other) {
        return Val == Other.Val;
    }

    template <typename T>
    __forceinline bool operator!=(TObjectPtr<T>& Other) {
        return !operator==(Other);
    }

    __forceinline ObjectType* operator->() {
        return Val.get();
    }

    __forceinline operator bool() const {
        return Val.operator bool();
    }

    __forceinline ObjectType* Get() {
        return Val.get();
    }

    __forceinline ObjectType& operator*() {
        return *Get();
    }

    template <typename T>
    __forceinline bool IsA() {
        return std::dynamic_pointer_cast<T>(Val);
    }

    template <typename T>
    __forceinline TSharedPtr<T> As() {
        return std::dynamic_pointer_cast<T>(Val);
    }

    template <typename T>
    __forceinline TObjectPtr<T> As() const {
        return TObjectPtr<T>(std::dynamic_pointer_cast<T>(Val));
    }

    template <typename T>
    __forceinline TObjectPtr<T> As() {
        return TObjectPtr<T>(std::dynamic_pointer_cast<T>(Val));
    }
};
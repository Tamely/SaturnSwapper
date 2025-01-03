module;

#include "Saturn/Defines.h"

export module Saturn.Core.TObjectPtr;

export template<typename ObjectType>
class TObjectPtr {
    TSharedPtr<ObjectType> Val;
    template <typename T> friend class TObjectPtr;
public:
    TObjectPtr() : Val(nullptr) {}
    TObjectPtr(std::nullptr_t Null) : Val(nullptr) {}

    TObjectPtr(TSharedPtr<ObjectType> InObject) : Val(InObject) {}
    TObjectPtr(TSharedPtr<ObjectType>& InObject) : Val(InObject) {}

    __forceinline TObjectPtr operator=(TSharedPtr<ObjectType> Other) {
        Val = Other;
        return *this;
    }

    __forceinline TObjectPtr operator=(TObjectPtr<ObjectType> Other) {
        Val = Other.Val;
        return *this;
    }

    template <typename OtherType>
    __forceinline TObjectPtr operator=(TObjectPtr<OtherType> Other) {
        Val = std::static_pointer_cast<ObjectType>(Other.Val);
        return *this;
    }

    __forceinline const TSharedPtr<ObjectType>& GetSharedPtr() const {
        return Val;
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
    __forceinline TObjectPtr<T> As() const {
        return TObjectPtr<T>(std::dynamic_pointer_cast<T>(Val));
    }

    template <typename T>
    __forceinline TObjectPtr<T> As() {
        return TObjectPtr<T>(std::dynamic_pointer_cast<T>(Val));
    }
};
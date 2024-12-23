module;

#include "Saturn/Defines.h"

export module Saturn.Reflection.FProperty;

import <string>;
export import Saturn.Reflection.PropertyValue;

export class FProperty {
public:
    friend class FPropertyFactory;
    //friend struct FUnversionedSerializer;
    friend class Mappings;

    virtual ~FProperty() = default;
protected:
    std::string Name;
    uint16_t Index;
    uint8_t ArrayDim;
    EPropertyType Type;
    FProperty* Next = nullptr;
public:
    __forceinline std::string GetName() { return Name; }
    __forceinline uint16_t GetIndex() { return Index; }
    __forceinline uint8_t GetArrayDim() { return ArrayDim; }
    __forceinline FProperty* GetNext() { return Next; }

    virtual TUniquePtr<class IPropValue> Serialize(class FArchive& Ar) {
        return nullptr;
    }

    virtual void Write(class FArchive& Ar) {}
};
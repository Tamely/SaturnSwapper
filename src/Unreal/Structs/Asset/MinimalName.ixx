export module Saturn.Asset.MinimalName;

import <cstdint>;

import Saturn.Structs.Name;

/** Externally, the instance number to represent no instance number is NAME_NO_NUMBER, 
    but internally, we add 1 to indices, so we use this #define internally for 
	zero'd memory initialization will still make NAME_None as expected */
#define NAME_NO_NUMBER_INTERNAL	0

export struct FMinimalName {
    friend FName;

    FMinimalName() {}

    FMinimalName(FName N)
        : Index(FNameEntryId::FromEName(N)) {}

    explicit FMinimalName(const FName& Name);
    bool IsNone() const;
    bool operator<(FMinimalName& Rhs) const;
private:
    /** Index into the Names aarray (used to find String portion of the string/number pair) */
    FNameEntryId Index;
    /** Number portion of the string/number pair (stored internally as 1 more than actual, so zero'd memory will be the default, no-instance case) */
    int32_t Number = NAME_NO_NUMBER_INTERNAL;

    friend __forceinline bool operator==(FMinimalName& Lhs, FMinimalName Rhs) {
        return Lhs.Index == Rhs.Index && Lhs.Number == Rhs.Number;
    }

    friend __forceinline uint32_t GetTypeHash(FMinimalName Name) {
        return GetTypeHash(Name.Index) + Name.Number;
    }
    
    friend __forceinline bool operator!=(FMinimalName Lhs, FMinimalName Rhs) {
        return !operator==(Lhs, Rhs);
    }
};
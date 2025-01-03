module;

#include "Saturn/Defines.h"

export module Saturn.Reflection.Mappings;

import <string>;

import Saturn.Core.UObject;

export class Mappings {
public:
    static std::string& ReadName(class FArchive& Ar, std::vector<std::string>& Names);
    template <typename T>
    static TObjectPtr<T> GetOrCreateObject(std::string& ClassName, TMap<std::string, UObjectPtr>& ObjectArray);
    static bool RegisterTypesFromUsmap(const std::string& Path, TMap<std::string, UObjectPtr>& ObjectArray);
};
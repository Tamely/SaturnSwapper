module;

#include "Saturn/Defines.h"

export module Saturn.Reflection.Mappings;

import <string>;

import Saturn.Core.UObject;

export class Mappings {
public:
    static bool RegisterTypesFromUsmap(const std::string& Path, TMap<std::string, UObjectPtr>& ObjectArray);
};
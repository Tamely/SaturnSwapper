module;

#include "Saturn/Defines.h"

export module Saturn.Core.GlobalContext;

import <string>;

import Saturn.Core.UObject;
import Saturn.IoStore.GlobalToc;

export class GlobalContext {
public:
    TSharedPtr<FGlobalTocData> GlobalToc;
    TMap<std::string, UObjectPtr> ObjectArray;
};
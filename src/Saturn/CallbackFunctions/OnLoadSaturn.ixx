module;

#include <Crypt/skCrypter.h>

export module Saturn.CallbackFunctions.OnLoadSaturn;

import <AppCore/AppCore.h>;
import <string>;

export class FOnLoadSaturn {
public:
	static JSValueRef OnLoadSaturn(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception);
public:
	static const char* GetName() {
		return "OnLoadSaturn";
	}
}; 

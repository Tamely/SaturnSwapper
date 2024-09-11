module;

#include <windows.h>
#include <shellapi.h>

export module Saturn.CallbackFunctions.OnOpenKey;

import Saturn.Config;

import <AppCore/AppCore.h>;
import <string>;

export class FOnOpenKey {
public:
	static JSValueRef OnOpenKey(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		ShellExecute(NULL, NULL, "https://loot-link.com/s?c0354435", NULL, NULL, SW_SHOW);

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnOpenKey";
	}
};
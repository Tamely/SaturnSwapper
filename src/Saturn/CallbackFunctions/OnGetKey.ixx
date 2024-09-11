export module Saturn.CallbackFunctions.OnGetKey;

import Saturn.Config;

import <AppCore/AppCore.h>;
import <string>;

export class FOnGetKey {
public:
	static JSValueRef OnGetKey(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		FConfig::Load();

		return JSValueMakeString(ctx, JSStringCreateWithUTF8CString(FConfig::Key.c_str()));
	}
public:
	static const char* GetName() {
		return "OnGetKey";
	}
};
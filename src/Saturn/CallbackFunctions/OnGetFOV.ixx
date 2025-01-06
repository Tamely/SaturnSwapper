export module Saturn.CallbackFunctions.OnGetFOV;

import Saturn.Config;

import <AppCore/AppCore.h>;
import <string>;

export class FOnGetFOV {
public:
	static JSValueRef OnGetFOV(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		FConfig::Load();

		return JSValueMakeBoolean(ctx, false);
	}
public:
	static const char* GetName() {
		return "OnGetFOV";
	}
};
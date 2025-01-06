export module Saturn.CallbackFunctions.OnIsItemConverted;

import Saturn.Config;

import <AppCore/AppCore.h>;
import <string>;

export class FOnIsItemConverted {
public:
	static JSValueRef OnIsItemConverted(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		FConfig::Load();

		return JSValueMakeBoolean(ctx, FConfig::bHasSwappedSkin);
	}
public:
	static const char* GetName() {
		return "OnIsItemConverted";
	}
};
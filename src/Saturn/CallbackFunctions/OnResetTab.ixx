export module Saturn.CallbackFunctions.OnResetTab;

import Saturn.Context;

import <AppCore/AppCore.h>;
import <string>;

export class FOnResetTab {
public:
	static JSValueRef OnResetTab(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		JSStringRef script = JSStringCreateWithUTF8CString("saturn.itemManager.clearItems()");
		JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
		JSStringRelease(script);

		FContext::Tab = 0;

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnResetTab";
	}
};
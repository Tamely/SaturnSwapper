export module Saturn.CallbackFunctions.OnLaunchClick;

import Saturn.Items.ItemModel;
import Saturn.Generators.SkinGenerator;

import <AppCore/AppCore.h>;
import <string>;

export class FOnLaunchClick {
public:
	static JSValueRef OnLaunchClick(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		//FortniteFunctionLibrary::LaunchFortnite();

		JSStringRef script = JSStringCreateWithUTF8CString("saturn.modalManager.hideModal('launch')");
		JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
		JSStringRelease(script);

		script = JSStringCreateWithUTF8CString("saturn.modalManager.showModal('item')");
		JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
		JSStringRelease(script);

		script = JSStringCreateWithUTF8CString("saturn.itemManager.clearItems()");
		JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
		JSStringRelease(script);

		script = JSStringCreateWithUTF8CString("OnGenerateSkins()");
		JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
		JSStringRelease(script);

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnLaunchClick";
	}
};
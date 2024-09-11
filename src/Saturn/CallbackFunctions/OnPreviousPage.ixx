export module Saturn.CallbackFunctions.OnPreviousPage;

import Saturn.Context;
import Saturn.Items.ItemModel;

import <AppCore/AppCore.h>;
import <string>;

export class FOnPreviousPage {
public:
	static JSValueRef OnPreviousPage(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		if (FContext::Tab > 0) {
			FContext::Tab--;

			JSStringRef script;
			switch (FContext::CosmeticState) {
			case ECosmeticState::Skin:
				script = JSStringCreateWithUTF8CString("OnGenerateSkins()");
				break;
			case ECosmeticState::Backbling:
				script = JSStringCreateWithUTF8CString("OnGenerateBackblings()");
				break;
			case ECosmeticState::Pickaxe:
				script = JSStringCreateWithUTF8CString("OnGeneratePickaxes()");
				break;
			case ECosmeticState::Emote:
				script = JSStringCreateWithUTF8CString("OnGenerateEmotes()");
				break;
			}

			JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
			JSStringRelease(script);

			script = JSStringCreateWithUTF8CString("OnDisplayItems()");
			JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
			JSStringRelease(script);
		}

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnPreviousPage";
	}
};
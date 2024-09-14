export module Saturn.CallbackFunctions.OnNextPage;

import Saturn.Context;
import Saturn.Items.ItemModel;
import Saturn.Generators.BaseGenerator;

import <AppCore/AppCore.h>;
import <string>;

export class FOnNextPage {
public:
	static JSValueRef OnNextPage(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		FContext::Tab++;

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

		if (FBaseGenerator::ItemsToDisplay.empty()) {
			FContext::Tab--;
			return JSValueMakeNull(ctx);
		}

		script = JSStringCreateWithUTF8CString("OnDisplayItems()");
		JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
		JSStringRelease(script);

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnNextPage";
	}
};
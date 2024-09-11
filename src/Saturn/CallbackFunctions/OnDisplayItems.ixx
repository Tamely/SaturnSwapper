export module Saturn.CallbackFunctions.OnDisplayItems;

import Saturn.Context;
import Saturn.Items.ItemModel;
import Saturn.Generators.BaseGenerator;

import <AppCore/AppCore.h>;
import <string>;

export class FOnDisplayItems {
public:
	static JSValueRef OnDisplayItems(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		JSStringRef script = JSStringCreateWithUTF8CString("saturn.itemManager.clearItems()");
		JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
		JSStringRelease(script);

		for (auto& item : FBaseGenerator::ItemsToDisplay) {
			script = JSStringCreateWithUTF8CString(std::string(std::string("saturn.itemManager.addItem(\"") + item.Name + std::string("\", \"") + item.Id + std::string("\")")).c_str());
			JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
			JSStringRelease(script);
		}

		FBaseGenerator::ItemsToDisplay.clear();

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnDisplayItems";
	}
};
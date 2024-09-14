export module Saturn.CallbackFunctions.OnSearch;

import Saturn.Context;
import Saturn.Items.ItemModel;
import Saturn.Items.LoadoutModel;

import Saturn.Generators.BaseGenerator;
import Saturn.Generators.SkinGenerator;
import Saturn.Generators.EmoteGenerator;
import Saturn.Generators.PickaxeGenerator;
import Saturn.Generators.BackblingGenerator;

import <AppCore/AppCore.h>;
import <string>;

export class FOnSearch {
public:
	static JSValueRef OnSearch(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		if (argumentCount < 1) {
			return JSValueMakeNull(ctx);
		}

		JSStringRef searchString = JSValueToStringCopy(ctx, arguments[0], nullptr);

		size_t bufferSize = JSStringGetMaximumUTF8CStringSize(searchString);
		char* buffer = new char[bufferSize];
		JSStringGetUTF8CString(searchString, buffer, bufferSize);

		std::string search = std::string(buffer);

		switch (FContext::CosmeticState) {
		case ECosmeticState::Skin:
			FBaseGenerator::ItemsToDisplay = FSkinGenerator::FilterItems(search);
			break;
		case ECosmeticState::Backbling:
			FBaseGenerator::ItemsToDisplay = FBackblingGenerator::FilterItems(search);
			break;
		case ECosmeticState::Emote:
			FBaseGenerator::ItemsToDisplay = FEmoteGenerator::FilterItems(search);
			break;
		case ECosmeticState::Pickaxe:
			FBaseGenerator::ItemsToDisplay = FPickaxeGenerator::FilterItems(search);
			break;
		}

		JSStringRelease(searchString);
		delete[] buffer;

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnSearch";
	}
};
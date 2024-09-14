export module Saturn.CallbackFunctions.OnApplyLoadout;

import Saturn.Context;
import Saturn.Items.ItemModel;
import Saturn.Items.LoadoutModel;

import Saturn.Generators.SkinGenerator;
import Saturn.Generators.PickaxeGenerator;
import Saturn.Generators.BackblingGenerator;

import <AppCore/AppCore.h>;
import <string>;

export class FOnApplyLoadout {
public:
	static JSValueRef OnApplyLoadout(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		if (argumentCount < 3) {
			return JSValueMakeNull(ctx);
		}

		JSStringRef skinIdString = JSValueToStringCopy(ctx, arguments[0], nullptr);
		JSStringRef pickaxeIdString = JSValueToStringCopy(ctx, arguments[1], nullptr);
		JSStringRef backblingIdString = JSValueToStringCopy(ctx, arguments[2], nullptr);

		size_t bufferSize = JSStringGetMaximumUTF8CStringSize(skinIdString);
		char* skinIdBuffer = new char[bufferSize];
		JSStringGetUTF8CString(skinIdString, skinIdBuffer, bufferSize);

		std::string skinId = std::string(skinIdBuffer);

		bufferSize = JSStringGetMaximumUTF8CStringSize(pickaxeIdString);
		char* pickaxeIdBuffer = new char[bufferSize];
		JSStringGetUTF8CString(pickaxeIdString, pickaxeIdBuffer, bufferSize);

		std::string pickaxeId = std::string(pickaxeIdBuffer);

		bufferSize = JSStringGetMaximumUTF8CStringSize(backblingIdString);
		char* backblingIdBuffer = new char[bufferSize];
		JSStringGetUTF8CString(backblingIdString, backblingIdBuffer, bufferSize);

		std::string backblingId = std::string(backblingIdBuffer);

		FContext::Loadout.Skin = FSkinGenerator::GetItemById(skinId);
		FContext::Loadout.Pickaxe = FPickaxeGenerator::GetItemById(pickaxeId);
		FContext::Loadout.Backbling = FBackblingGenerator::GetItemById(backblingId);

		JSStringRelease(skinIdString);
		JSStringRelease(pickaxeIdString);
		JSStringRelease(backblingIdString);
		delete[] skinIdBuffer;
		delete[] pickaxeIdBuffer;
		delete[] backblingIdBuffer;

		FLoadout::WriteToSaveGame(FContext::Loadout);

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnApplyLoadout";
	}
};
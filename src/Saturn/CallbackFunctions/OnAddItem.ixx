export module Saturn.CallbackFunctions.OnAddItem;

import Saturn.Config;
import Saturn.Context;
import Saturn.Items.ItemModel;
import Saturn.Items.LoadoutModel;

import Saturn.Generators.SkinGenerator;
import Saturn.Generators.EmoteGenerator;
import Saturn.Generators.PickaxeGenerator;
import Saturn.Generators.BackblingGenerator;

import <AppCore/AppCore.h>;
import <string>;

export class FOnAddItem {
public:
	static JSValueRef OnAddItem(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		if (argumentCount < 1) {
			return JSValueMakeNull(ctx);
		}
		
		JSValueRef idArg = arguments[0];
		JSStringRef idString = JSValueToStringCopy(ctx, idArg, nullptr);

		size_t bufferSize = JSStringGetMaximumUTF8CStringSize(idString);
		char* buffer = new char[bufferSize];
		JSStringGetUTF8CString(idString, buffer, bufferSize);

		std::string id = std::string(buffer);

		if (id == FContext::Loadout.Skin.Id) {
			FContext::Loadout.Skin = FItem();
			FConfig::Save();
			FLoadout::WriteToSaveGame(FContext::Loadout);

			JSStringRelease(idString);
			delete[] buffer;

			return JSValueMakeNull(ctx);
		}

		if (id == FContext::Loadout.Backbling.Id) {
			FContext::Loadout.Backbling = FItem();
			FConfig::Save();
			FLoadout::WriteToSaveGame(FContext::Loadout);

			JSStringRelease(idString);
			delete[] buffer;

			return JSValueMakeNull(ctx);
		}

		if (id == FContext::Loadout.Pickaxe.Id) {
			FContext::Loadout.Pickaxe = FItem();
			FConfig::Save();
			FLoadout::WriteToSaveGame(FContext::Loadout);

			JSStringRelease(idString);
			delete[] buffer;

			return JSValueMakeNull(ctx);
		}

		switch (FContext::CosmeticState) {
			case ECosmeticState::Skin:
				FContext::Loadout.Skin = FSkinGenerator::GetItemById(id);
				FLoadout::WriteToSaveGame(FContext::Loadout);
				FConfig::Save();
				break;
			case ECosmeticState::Backbling:
				FContext::Loadout.Backbling = FBackblingGenerator::GetItemById(id);
				FLoadout::WriteToSaveGame(FContext::Loadout);
				FConfig::Save();
				break;
			case ECosmeticState::Pickaxe:
				FContext::Loadout.Pickaxe = FPickaxeGenerator::GetItemById(id);
				FLoadout::WriteToSaveGame(FContext::Loadout);
				FConfig::Save();
				break;
			case ECosmeticState::Emote:
				FLoadout::WriteEmote(FEmoteGenerator::GetItemById(id));
				break;
		}

		JSStringRelease(idString);
		delete[] buffer;

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnAddItem";
	}
};
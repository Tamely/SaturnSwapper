export module Saturn.CallbackFunctions.OnLoadLoadouts;

import Saturn.Context;
import Saturn.Items.ItemModel;
import Saturn.Items.LoadoutModel;

import Saturn.Discord.RichPresence;

import <AppCore/AppCore.h>;
import <string>;

export class FOnLoadLoadouts {
public:
	static JSValueRef OnLoadLoadouts(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		FRichPresence::UpdateDiscord("Loadouts");

		for (FLoadout& loadout : FContext::Loadouts) {
			std::string Id = "";
			std::string skinName = "";
			std::string skinId = "";
			std::string backblingName = "";
			std::string backblingId = "";
			std::string pickaxeName = "";
			std::string pickaxeId = "";

			if (loadout.Pickaxe.IsValid()) {
				Id = loadout.Pickaxe.Id;
				pickaxeId = loadout.Pickaxe.Id;
				pickaxeName = loadout.Pickaxe.Name;
			}

			if (loadout.Backbling.IsValid()) {
				Id = loadout.Backbling.Id;
				backblingId = loadout.Backbling.Id;
				backblingName = loadout.Backbling.Name;
			}

			if (loadout.Skin.IsValid()) {
				Id = loadout.Skin.Id;
				skinId = loadout.Skin.Id;
				skinName = loadout.Skin.Name;
			}

			JSStringRef script = JSStringCreateWithUTF8CString(std::string(std::string("saturn.loadoutManager.addLoadout(\"") + skinName + "\", \"" + skinId + "\", \"" + pickaxeName + "\", \"" + pickaxeId + "\", \"" + backblingName + "\", \"" + backblingId + "\", \"" + Id + "\")").c_str());
			JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
			JSStringRelease(script);
		}


		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnLoadLoadouts";
	}
};
export module Saturn.CallbackFunctions.OnAddLoadout;

import Saturn.Context;
import Saturn.Items.ItemModel;

import <AppCore/AppCore.h>;
import <string>;

export class FOnAddLoadout {
public:
	static JSValueRef OnAddLoadout(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		JSStringRef script = JSStringCreateWithUTF8CString("saturn.loadoutManager.clearLoadouts()");
		JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
		JSStringRelease(script);

		FContext::Loadouts.push_back(FContext::Loadout);

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

			script = JSStringCreateWithUTF8CString(std::string(std::string("saturn.loadoutManager.addLoadout(\"") + skinId + "\", \"" + pickaxeId + "\", \"" + backblingId + "\", \"" + Id + "\")").c_str());
			JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
			JSStringRelease(script);
		}

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnAddLoadout";
	}
};
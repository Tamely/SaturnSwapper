export module Saturn.CallbackFunctions.OnGeneratePickaxes;

import Saturn.Context;
import Saturn.Items.ItemModel;
import Saturn.Generators.BaseGenerator;
import Saturn.Generators.PickaxeGenerator;

import Saturn.Discord.RichPresence;

import <AppCore/AppCore.h>;
import <string>;

export class FOnGeneratePickaxes {
public:
	static JSValueRef OnGeneratePickaxes(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		FContext::CosmeticState = ECosmeticState::Pickaxe;

		FRichPresence::UpdateDiscord("Pickaxes");

		FBaseGenerator::ItemsToDisplay = FPickaxeGenerator::GetItems();

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnGeneratePickaxes";
	}
};
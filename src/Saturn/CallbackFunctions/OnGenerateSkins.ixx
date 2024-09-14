export module Saturn.CallbackFunctions.OnGenerateSkins;

import Saturn.Context;
import Saturn.Items.ItemModel;
import Saturn.Generators.BaseGenerator;
import Saturn.Generators.SkinGenerator;

import Saturn.Discord.RichPresence;

import <AppCore/AppCore.h>;
import <string>;

export class FOnGenerateSkins {
public:
	static JSValueRef OnGenerateSkins(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		FContext::CosmeticState = ECosmeticState::Skin;

		FRichPresence::UpdateDiscord("Skins");

		FBaseGenerator::ItemsToDisplay = FSkinGenerator::GetItems();

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnGenerateSkins";
	}
};
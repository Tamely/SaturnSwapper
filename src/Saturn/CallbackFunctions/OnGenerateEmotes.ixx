export module Saturn.CallbackFunctions.OnGenerateEmotes;

import Saturn.Context;
import Saturn.Items.ItemModel;
import Saturn.Generators.BaseGenerator;
import Saturn.Generators.EmoteGenerator;

import Saturn.Discord.RichPresence;

import <AppCore/AppCore.h>;
import <string>;

export class FOnGenerateEmotes {
public:
	static JSValueRef OnGenerateEmotes(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		FContext::CosmeticState = ECosmeticState::Emote;

		FRichPresence::UpdateDiscord("Emotes");

		FBaseGenerator::ItemsToDisplay = FEmoteGenerator::GetItems();

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnGenerateEmotes";
	}
};
export module Saturn.CallbackFunctions.OnGenerateBackblings;

import Saturn.Context;
import Saturn.Items.ItemModel;
import Saturn.Generators.BaseGenerator;
import Saturn.Generators.BackblingGenerator;

import Saturn.Discord.RichPresence;

import <AppCore/AppCore.h>;
import <string>;

export class FOnGenerateBackblings {
public:
	static JSValueRef OnGenerateBackblings(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		FContext::CosmeticState = ECosmeticState::Backbling;

		FRichPresence::UpdateDiscord("Backblings");

		FBaseGenerator::ItemsToDisplay = FBackblingGenerator::GetItems();

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnGenerateBackblings";
	}
};
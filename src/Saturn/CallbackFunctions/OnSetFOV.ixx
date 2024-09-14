export module Saturn.CallbackFunctions.OnSetFOV;

import Saturn.Config;
import Saturn.Items.LoadoutModel;

import <AppCore/AppCore.h>;
import <string>;

export class FOnSetFOV {
public:
	static JSValueRef OnSetFOV(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		if (argumentCount < 1) {
			return JSValueMakeNull(ctx);
		}

		bool fovEnabled = JSValueToBoolean(ctx, arguments[0]);
		FLoadout::WriteFOV(fovEnabled);

		FConfig::bFOVEnabled = fovEnabled;
		FConfig::Save();

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnSetFOV";
	}
};
export module Saturn.CallbackFunctions.OnLaunchClick;

import Saturn.WindowsFunctionLibrary;

import <AppCore/AppCore.h>;
import <string>;

export class FOnLaunchClick {
public:
	static JSValueRef OnLaunchClick(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		std::wstring LauncherPathW = WindowsFunctionLibrary::GetSaturnLocalPath() + L"\\Externals\\Saturn.Launcher.exe";
		std::string LauncherPath = std::string(LauncherPathW.begin(), LauncherPathW.end());

		WindowsFunctionLibrary::LaunchExe(LauncherPath);

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnLaunchClick";
	}
};
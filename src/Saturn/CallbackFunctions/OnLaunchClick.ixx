export module Saturn.CallbackFunctions.OnLaunchClick;

import Saturn.WindowsFunctionLibrary;

import <AppCore/AppCore.h>;
import <string>;

#include "Saturn/Log.h"

export class FOnLaunchClick {
public:
	static JSValueRef OnLaunchClick(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		std::wstring LauncherPathW = WindowsFunctionLibrary::GetSaturnLocalPath() + L"\\Externals\\Saturn.Launcher.exe";
		std::string LauncherPath = std::string(LauncherPathW.begin(), LauncherPathW.end());

		if (WindowsFunctionLibrary::FileExists(LauncherPath)) {
			WindowsFunctionLibrary::LaunchExe(LauncherPath);
		}
		else {
			LOG_ERROR("Failed to launch Fortnite. The launcher does not exist!");
		}

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnLaunchClick";
	}
};
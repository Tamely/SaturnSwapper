module;

#include <Crypt/skCrypter.h>
#include <DiscordSDK/rapidjson/document.h>
#include <DiscordSDK/rapidjson/stringbuffer.h>
#include <DiscordSDK/rapidjson/writer.h>

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

export module Saturn.CallbackFunctions.OnLoadPluginTab;

import Saturn.Context;
import Saturn.Items.ItemModel;
import Saturn.Items.PluginModel;
import Saturn.Items.LoadoutModel;

import Saturn.Encryption.AES;
import Saturn.Compression.Oodle;
import Saturn.Generators.BaseGenerator;

import Saturn.WindowsFunctionLibrary;
import Saturn.FortniteFunctionLibrary;

import Saturn.Discord.RichPresence;

import <AppCore/AppCore.h>;
import <string>;
import <tuple>;
import <time.h>;
import <fstream>;
import <filesystem>;

export class FOnLoadPluginTab {
public:
	static JSValueRef OnLoadPluginTab(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		JSStringRef script = JSStringCreateWithUTF8CString(std::string(std::string("saturn.pluginManager.onSetPlugin(`") + FContext::SelectedPlugin.Name + "`, `" + FContext::SelectedPlugin.Author + "`, `" + FContext::SelectedPlugin.Message + "`, `" + FContext::SelectedPlugin.BannerURL + "`, `" + FContext::SelectedPlugin.Plugin + "`)").c_str());
		JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
		JSStringRelease(script);

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnLoadPluginTab";
	}
};

module;

#include <Crypt/skCrypter.h>
#include <DiscordSDK/rapidjson/document.h>
#include <DiscordSDK/rapidjson/stringbuffer.h>
#include <DiscordSDK/rapidjson/writer.h>

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

export module Saturn.CallbackFunctions.OnDownloadPlugin;

import Saturn.Context;
import Saturn.Items.ItemModel;
import Saturn.Items.PluginModel;
import Saturn.Items.LoadoutModel;

import Saturn.Encryption.AES;
import Saturn.Compression.Oodle;
import Saturn.Generators.BaseGenerator;

import Saturn.WindowsFunctionLibrary;
import Saturn.FortniteFunctionLibrary;
import Saturn.Scripts.ScriptWrapper;

import Saturn.Discord.RichPresence;

import <AppCore/AppCore.h>;
import <string>;
import <tuple>;
import <time.h>;
import <fstream>;
import <filesystem>;

export class FOnDownloadPlugin {
public:
	static JSValueRef OnDownloadPlugin(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		const std::wstring pluginsPathW = WindowsFunctionLibrary::GetSaturnLocalPath() + L"\\Plugins\\";
		const std::string pluginsPath = std::string(pluginsPathW.begin(), pluginsPathW.end());

		FContext::SelectedPlugin.Id;
		std::string plugin = WindowsFunctionLibrary::Decode(FContext::SelectedPlugin.Plugin);

		//std::ifstream plugin(std::string(pluginsPath + FContext::SelectedPlugin.Id + ".json").c_str());

		FScriptWrapper::Eval(plugin);

		WindowsFunctionLibrary::GetRequestSaturn(_("https://tamelyapi.azurewebsites.net/api/v1/Saturn/DownloadPlugin?id=") + FContext::SelectedPlugin.Id);

		return JSValueMakeBoolean(ctx, true);
	}
public:
	static const char* GetName() {
		return "OnDownloadPlugin";
	}
};

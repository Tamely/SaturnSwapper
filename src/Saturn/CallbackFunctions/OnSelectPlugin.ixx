module;

#include <Crypt/skCrypter.h>
#include <DiscordSDK/rapidjson/document.h>
#include <DiscordSDK/rapidjson/stringbuffer.h>
#include <DiscordSDK/rapidjson/writer.h>

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

export module Saturn.CallbackFunctions.OnSelectPlugin;

import Saturn.Context;
import Saturn.Items.ItemModel;
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
import Saturn.Items.PluginModel;

export class FOnSelectPlugin {
public:
	static JSValueRef OnSelectPlugin(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		if (argumentCount < 1) {
			return JSValueMakeNull(ctx);
		}

		JSValueRef idArg = arguments[0];
		JSStringRef idString = JSValueToStringCopy(ctx, idArg, nullptr);

		size_t bufferSize = JSStringGetMaximumUTF8CStringSize(idString);
		char* buffer = new char[bufferSize];
		JSStringGetUTF8CString(idString, buffer, bufferSize);

		std::string id = std::string(buffer);

		std::tuple<long, std::string> searchContent = WindowsFunctionLibrary::GetRequestSaturn(_("https://tamelyapi.azurewebsites.net/api/v1/Saturn/PluginSearch?id=") + id);

		if (std::get<long>(searchContent) != 200) {
			MessageBoxW(nullptr, L"Error fetching plugin info! Please contact support then retry later!", L"Error fetching plugin info", MB_OK);
			return JSValueMakeNull(ctx);
		}

		rapidjson::Document doc;
		doc.Parse(std::get<std::string>(searchContent).c_str());

		std::string name = doc["name"].GetString();
		std::string author = doc["author"].GetString();
		std::string banner = doc["bannerURL"].GetString();
		std::string plugin = doc["plugin"].GetString();

		std::string message = "";
		std::string content = "";

		if (!doc["contentURL"].IsNull()) {
			content = doc["contentURL"].GetString();
			message = "This plugin uses UEFN files which means you have to do the following bypass to not get kicked! 1.) Go into creative 2.) Do a jam track and wait for it to play music (doesn't matter which one) 3.) Leave Creative 4.) Go back into creative and do the same jam track again";
		}

		FPluginModel model;

		model.Id = id;
		model.Name = name;
		model.BannerURL = banner;
		model.Author = author;
		model.Plugin = plugin;
		model.Message = message;
		model.Content = content;

		FContext::SelectedPlugin = model;

		JSStringRef script = JSStringCreateWithUTF8CString("window.location.pathname = '/pages/plugin.html'");
		JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
		JSStringRelease(script);

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnSelectPlugin";
	}
};

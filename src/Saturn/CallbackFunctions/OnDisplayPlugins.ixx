module;

#include <Crypt/skCrypter.h>
#include <DiscordSDK/rapidjson/document.h>
#include <DiscordSDK/rapidjson/stringbuffer.h>
#include <DiscordSDK/rapidjson/writer.h>

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

export module Saturn.CallbackFunctions.OnDisplayPlugins;

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
import <vector>;
import <filesystem>;

struct PluginData {
	std::string image;
	std::string name;
	std::string id;
	int downloads;

	PluginData(const std::string& img, const std::string& n, const std::string& i, int d)
		: image(img), name(n), id(i), downloads(d) {}
};

export class FOnDisplayPlugins {
public:
	static JSValueRef OnDisplayPlugins(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		std::tuple<long, std::string> content = WindowsFunctionLibrary::GetRequestSaturn(_("https://tamelyapi.azurewebsites.net/api/v1/Saturn/PluginMarketplace"));

		if (std::get<long>(content) != 200) {
			MessageBoxW(nullptr, L"Failed to fetch plugins! Please report this to staff then try again later or use a VPN!", L"Error retrieving plugins", MB_OK);
			return JSValueMakeNull(ctx);
		}

		rapidjson::Document doc;
		doc.Parse(std::get<std::string>(content).c_str());

		JSStringRef script = JSStringCreateWithUTF8CString("saturn.pluginManager.clearPlugins()");
		JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
		JSStringRelease(script);

		std::vector<PluginData> plugins;
		for (const rapidjson::Value& iteration : doc.GetArray()) {
			std::string image = iteration["image"].GetString();
			std::string name = iteration["name"].GetString();
			std::string id = iteration["_id"].GetString();
			int downloads = iteration["downloads"].GetInt();

			plugins.emplace_back(image, name, id, downloads);
		}

		// Sort the plugins by downloads in descending order
		std::sort(plugins.begin(), plugins.end(), [](const PluginData& a, const PluginData& b) {
			return a.downloads > b.downloads;
		});

		// Add the plugins in sorted order
		for (const PluginData& plugin : plugins) {
			script = JSStringCreateWithUTF8CString(
				("saturn.pluginManager.addPlugin(`" + plugin.id + "`, `" + plugin.name + "`, `" + plugin.image + "`)").c_str());
			JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
			JSStringRelease(script);
		}

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnDisplayPlugins";
	}
};

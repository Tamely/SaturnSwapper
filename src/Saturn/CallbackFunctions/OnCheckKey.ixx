module;

#include <Crypt/skCrypter.h>
#include <DiscordSDK/rapidjson/document.h>

export module Saturn.CallbackFunctions.OnCheckKey;

import Saturn.Config;
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

export class FOnCheckKey {
public:
	static JSValueRef OnCheckKey(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		if (argumentCount < 1) {
			return JSValueMakeBoolean(ctx, false);
		}

		static std::tuple<long, std::string> versionData = WindowsFunctionLibrary::GetRequestSaturn(_("https://tamelyapi.azurewebsites.net/"));
		if (std::get<long>(versionData) == 200) {
			rapidjson::Document version;
			version.Parse(std::get<std::string>(versionData).c_str());

			if (FContext::VERSION != version["swapperVersion"].GetString()) {
				JSStringRef script = JSStringCreateWithUTF8CString("saturn.modalManager.showModal('version')");
				JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
				JSStringRelease(script);

				return JSValueMakeBoolean(ctx, false);
			}
		}

		JSStringRef key = JSValueToStringCopy(ctx, arguments[0], exception);
		char* buffer = new char[JSStringGetMaximumUTF8CStringSize(key)];
		JSStringGetUTF8CString(key, buffer, JSStringGetMaximumUTF8CStringSize(key));

		std::string keyString = std::string(buffer);

		if (keyString.length() != 66 || !keyString.starts_with("0x")) {
			delete[] buffer;
			JSStringRelease(key);

			return JSValueMakeBoolean(ctx, false);
		}

		std::tuple<long, std::string> keyData = WindowsFunctionLibrary::GetRequestSaturn(_("https://tamelyapi.azurewebsites.net/api/v1/Saturn/ReturnKeyExists?key=") + keyString);
		if (std::get<long>(keyData) != 200) {
			delete[] buffer;
			JSStringRelease(key);

			return JSValueMakeBoolean(ctx, false);
		}

		rapidjson::Document json;
		json.Parse(std::get<std::string>(keyData).c_str());

		if (!json["found"].GetBool()) {
			delete[] buffer;
			JSStringRelease(key);

			return JSValueMakeBoolean(ctx, false);
		}

		if (!std::string(json["hwid"].GetString()).empty() && json["hwid"].GetString() != WindowsFunctionLibrary::GetHWID()) {
			delete[] buffer;
			JSStringRelease(key);

			return JSValueMakeBoolean(ctx, false);
		}

		if (!json["mmid"].IsNull() && !std::string(json["mmid"].GetString()).empty()) {
			FContext::Paid = true;
		}
		else {
			FContext::Paid = false;
		}

		if (std::string(json["hwid"].GetString()).empty()) {
			std::tuple<long, std::string> hwidData = WindowsFunctionLibrary::GetRequestSaturn(_("https://tamelyapi.azurewebsites.net/api/v1/Saturn/SetHWID?key=") + keyString + _("&hwid=") + WindowsFunctionLibrary::GetHWID());
			if (std::get<long>(hwidData) != 200) {
				delete[] buffer;
				JSStringRelease(key);

				return JSValueMakeBoolean(ctx, false);
			}
		}

		FConfig::Key = keyString;
		FConfig::RuntimeKey = keyString;
		FConfig::Save();

		delete[] buffer;
		JSStringRelease(key);

		JSStringRef script = JSStringCreateWithUTF8CString("SendToURL('/pages/home.html')");
		JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
		JSStringRelease(script);
		
		return JSValueMakeBoolean(ctx, true);
	}
public:
	static const char* GetName() {
		return "OnCheckKey";
	}
};

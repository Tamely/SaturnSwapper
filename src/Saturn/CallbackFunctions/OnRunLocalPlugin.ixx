module;

#include <Crypt/skCrypter.h>
#include <DiscordSDK/rapidjson/document.h>
#include <DiscordSDK/rapidjson/stringbuffer.h>
#include <DiscordSDK/rapidjson/writer.h>

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

export module Saturn.CallbackFunctions.OnRunLocalPlugin;

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

export class FOnRunLocalPlugin {
public:
	static JSValueRef OnRunLocalPlugin(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		if (WindowsFunctionLibrary::FileExists("plugin.js")) {
			FScriptWrapper::Eval(WindowsFunctionLibrary::ReadAllText("plugin.js"));
		}

		MessageBoxW(nullptr, L"Successfully completed executing local plugin!", L"Success!", MB_OK);

		return JSValueMakeNull(ctx);
	}
public:
	static const char* GetName() {
		return "OnRunLocalPlugin";
	}
};

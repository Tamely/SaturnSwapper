module;

#include <Crypt/skCrypter.h>
#include <DiscordSDK/rapidjson/document.h>
#include <DiscordSDK/rapidjson/stringbuffer.h>
#include <DiscordSDK/rapidjson/writer.h>

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

export module Saturn.CallbackFunctions.OnLoadSaturn;

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
import Saturn.Unreal.IoStoreReader;

export class FOnLoadSaturn {
public:
	static JSValueRef OnLoadSaturn(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
		FRichPresence::Initialize();

		WindowsFunctionLibrary::MakeDirectory(WindowsFunctionLibrary::GetSaturnLocalPath());
		WindowsFunctionLibrary::MakeDirectory(WindowsFunctionLibrary::GetSaturnLocalPath() + L"\\Plugins\\");

		std::wstring externalsPathW = WindowsFunctionLibrary::GetSaturnLocalPath() + L"\\Externals\\";
		std::string externalsPath = std::string(externalsPathW.begin(), externalsPathW.end());

		WindowsFunctionLibrary::MakeDirectory(externalsPathW);

		std::tuple<long, std::string> depContent = WindowsFunctionLibrary::GetRequestSaturn(_("https://tamelyapi.azurewebsites.net/api/v1/Saturn/Dependencies"));

		if (std::get<long>(depContent) != 200) {
			MessageBoxW(nullptr, L"Failed to fetch dependency modules! Please report this to staff then try again later or use a VPN!", L"Error retrieving dependencies", MB_OK);
			return JSValueMakeBoolean(ctx, false);
		}

		if (FortniteFunctionLibrary::GetFortniteInstallationPath() == "NOTFOUND") {
			MessageBoxW(nullptr, L"Couldn't find a Fortnite installation path! Please verify Fortnite in Epic Games Launcher! If you use Legendary, make sure the \"LauncherInstalled.dat\" file exists as specified in #FAQ!", L"Error getting Fortnite path", MB_OK);
			return JSValueMakeBoolean(ctx, false);
		}

		rapidjson::Document doc;
		doc.Parse(std::get<std::string>(depContent).c_str());

		for (rapidjson::Value& iteration : doc.GetArray()) {
			std::string name = iteration["name"].GetString();
			std::string link = iteration["link"].GetString();

			if (!WindowsFunctionLibrary::FileExists(externalsPath + name)) {
				WindowsFunctionLibrary::DownloadFile(externalsPath + name, link);
			}
		}

		std::wstring oodlePathW = WindowsFunctionLibrary::GetSaturnLocalPath() + L"\\Externals\\oo2core_5_win64.dll";
		std::string oodlePath = std::string(oodlePathW.begin(), oodlePathW.end());
		Oodle::LoadDLL(oodlePath.c_str());

		std::string mainKey = FortniteFunctionLibrary::GetFortniteAESKey();

		if (mainKey == "ERROR") {
			JSStringRef script = JSStringCreateWithUTF8CString("saturn.modalManager.showModal('error')");
			JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
			JSStringRelease(script);

			return JSValueMakeBoolean(ctx, false);
		}

		FAESKey key(mainKey.c_str());
		std::string path = FortniteFunctionLibrary::GetFortniteInstallationPath() + _("pakchunk0-WindowsClient.pak");

		FBaseGenerator::InitializeAssetRegistry(path, key);

		return JSValueMakeBoolean(ctx, true);
	}
public:
	static const char* GetName() {
		return "OnLoadSaturn";
	}
}; 

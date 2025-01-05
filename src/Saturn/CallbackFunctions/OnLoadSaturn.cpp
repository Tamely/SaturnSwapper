#include <Crypt/skCrypter.h>
#include <DiscordSDK/rapidjson/document.h>
#include <DiscordSDK/rapidjson/stringbuffer.h>
#include <DiscordSDK/rapidjson/writer.h>

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <Saturn/Log.h>
import Saturn.CallbackFunctions.OnLoadSaturn;

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


import Saturn.Structs.Guid;
import Saturn.Files.FileProvider;

JSValueRef FOnLoadSaturn::OnLoadSaturn(JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception) {
	FRichPresence::Initialize();
	LOG_INFO("Initialized Discord RPC");

	std::wstring externalsPathW = WindowsFunctionLibrary::GetSaturnLocalPath() + L"\\Externals\\";
	std::string externalsPath = std::string(externalsPathW.begin(), externalsPathW.end());

	WindowsFunctionLibrary::MakeDirectory(externalsPathW);
	LOG_INFO("Created externals directory");

	std::tuple<long, std::string> depContent = WindowsFunctionLibrary::GetRequestSaturn(_("https://tamelyapi.azurewebsites.net/api/v1/Saturn/Dependencies"));
	LOG_INFO("Got Dependency endpoint with status {0}", std::get<long>(depContent));

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
			LOG_INFO("Downloading dependency '{0}' from '{1}'", name, link);
			WindowsFunctionLibrary::DownloadFile(externalsPath + name, link);
		}
	}

	LOG_INFO("Loading Oodle dll");
	std::wstring oodlePathW = WindowsFunctionLibrary::GetSaturnLocalPath() + L"\\Externals\\oo2core_9_win64.dll";
	std::string oodlePath = std::string(oodlePathW.begin(), oodlePathW.end());
	Oodle::LoadDLL(oodlePath.c_str());
	LOG_INFO("Loaded Oodle dll");

	LOG_INFO("Fetching main AES key");
	std::string mainKey = FortniteFunctionLibrary::GetFortniteAESKey();
	LOG_INFO("Fetched key {0}", mainKey);

	if (mainKey == "ERROR") {
		JSStringRef script = JSStringCreateWithUTF8CString("saturn.modalManager.showModal('error')");
		JSEvaluateScript(ctx, script, NULL, NULL, NULL, nullptr);
		JSStringRelease(script);

		return JSValueMakeBoolean(ctx, false);
	}

	FGuid defaultGUID;
	FAESKey defaultAES(mainKey.c_str());
	LOG_INFO("Created AES info");

	LOG_INFO("Loading Asset Registry");
	std::string path = FortniteFunctionLibrary::GetFortniteInstallationPath() + _("pakchunk0-WindowsClient.pak");
	FBaseGenerator::InitializeAssetRegistry(path, defaultAES);
	LOG_INFO("Loaded Asset Registry");

	FFileProvider provider(FortniteFunctionLibrary::GetFortniteInstallationPath(), "D:\\++Fortnite+Release-33.11-CL-38773622-Windows_oo.usmap");
	LOG_INFO("Created provider");
	provider.SubmitKey(defaultGUID, defaultAES);
	LOG_INFO("Submited default key");
	provider.MountAsync();
	LOG_INFO("Mounted");

	return JSValueMakeBoolean(ctx, true);
}
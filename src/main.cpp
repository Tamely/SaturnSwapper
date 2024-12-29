#include "Saturn/Scripts/WebServer.h"
#include "SaturnApp.h"

#include "Saturn/Log.h"

import Saturn.Context;

import Saturn.Scripts.ScriptWrapper;

import Saturn.Structs.Guid;
import Saturn.Encryption.AES;
import Saturn.Files.FileProvider;
import Saturn.VFS.FileSystem;

int main(int argc, char* argv[]) {
	Log::Init();
	LOG_INFO("Started Saturn");
	FScriptWrapper::InitBindings();
	LOG_INFO("Init bindings");
	FWebServer::CreateWebServerThread();
	LOG_INFO("Create Web Server");

	FGuid guid(0, 0, 0, 0);
	FAESKey aes("0x62450FF9261CCC2EE50C217A2D9EE97F05F09203CF6E395B7CAB9D8892B714CE");
	LOG_INFO("Created AES info");

	FFileProvider provider("D:\\Fortnite Builds\\Fortnite\\FortniteGame\\Content\\Paks");
	LOG_INFO("Created provider");
	provider.SubmitKey(guid, aes);
	LOG_INFO("Submit key");
	provider.MountAsync();
	LOG_INFO("Mounted");

	//VirtualFileSystem::PrintRegisteredFiles();

	if (argc >= 3) {
		FContext::Channel = argv[1]; // channel
		FContext::Variant = argv[2]; // variant
	}

	//SaturnApp app;
	//app.Run();

	return 0;
}

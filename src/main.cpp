#include "Saturn/Scripts/WebServer.h"
#include "SaturnApp.h"

#include "Saturn/Log.h"

import Saturn.Context;

import Saturn.Scripts.ScriptWrapper;

import Saturn.Structs.Guid;
import Saturn.Encryption.AES;
import Saturn.Files.FileProvider;
import Saturn.VFS.FileSystem;

import Saturn.Core.IoStatus;
import Saturn.Misc.IoBuffer;
import Saturn.Compression.Oodle;
import Saturn.Readers.ZenPackageReader;
import Saturn.Structs.IoStoreTocChunkInfo;

import <optional>;

int main(int argc, char* argv[]) {
	Log::Init();
	LOG_INFO("Started Saturn");
	FScriptWrapper::InitBindings();
	LOG_INFO("Init bindings");
	FWebServer::CreateWebServerThread();
	LOG_INFO("Create Web Server");
	Oodle::LoadDLL("oo2core_5_win64.dll");

	FGuid defaultGUID;
	FAESKey defaultAES("0x62450FF9261CCC2EE50C217A2D9EE97F05F09203CF6E395B7CAB9D8892B714CE");
	LOG_INFO("Created AES info");

	FGuid guid1001("02A94B6E1D64352BBF332D801395069C");
	FAESKey aes1001("0x106B870C4F18C617510178913431943D52829093805F4AC151716207F1D5478B");

	FFileProvider provider("D:\\Fortnite Builds\\Fortnite\\FortniteGame\\Content\\Paks");
	LOG_INFO("Created provider");
	provider.SubmitKey(defaultGUID, defaultAES);
	provider.SubmitKey(guid1001, aes1001);
	LOG_INFO("Submit key");
	provider.MountAsync();
	LOG_INFO("Mounted");

	TIoStatusOr<FIoBuffer> bufferStatus = VirtualFileSystem::GetBufferByPathAndExtension("/BRCosmetics/Athena/Items/Cosmetics/Characters/CID_028_Athena_Commando_F.uasset");
	if (!bufferStatus.IsOk()) {
		LOG_WARN(bufferStatus.Status().ToString());
		return 0;
	}
	FIoBuffer buffer = bufferStatus.ConsumeValueOrDie();
	LOG_INFO("Read succeeded! Buffer size: {0}", buffer.GetSize());

	FZenPackageReader Ar(buffer);
	if (!Ar.IsOk()) {
		LOG_WARN(Ar.GetStatus().ToString());
		return 0;
	}
	LOG_INFO("Package Name: {0}", std::string(Ar.GetPackageName().begin(), Ar.GetPackageName().end()));

	/*
	Ar.LoadProperties();
	if (!Ar.IsOk()) {
		LOG_WARN(Ar.GetStatus().ToString());
		return 0;
	}
	LOG_INFO("Fragment Count: {0}", Ar.GetFragments().size());
	*/

	if (argc >= 3) {
		FContext::Channel = argv[1]; // channel
		FContext::Variant = argv[2]; // variant
	}

	//SaturnApp app;
	//app.Run();

	return 0;
}

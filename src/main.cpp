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

import Saturn.Core.UObject;

import <optional>;

int main(int argc, char* argv[]) {
	Log::Init();
	LOG_INFO("Started Saturn");
	FScriptWrapper::InitBindings();
	LOG_INFO("Init bindings");
	FWebServer::CreateWebServerThread();

	SaturnApp app;
	app.Run();

	return 0;
}

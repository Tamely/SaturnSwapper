#include "Saturn/Scripts/WebServer.h"
#include "SaturnApp.h"

#include "Saturn/Log.h"

import Saturn.Context;

import Saturn.Scripts.ScriptWrapper;

int main(int argc, char* argv[]) {
	Log::Init();
	LOG_INFO("Started Saturn");
	//FScriptWrapper::InitBindings();
	//LOG_INFO("Init bindings");
	//FWebServer::CreateWebServerThread();

	SaturnApp app;
	app.Run();

	return 0;
}

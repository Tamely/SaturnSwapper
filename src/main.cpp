#include "Saturn/Scripts/WebServer.h"
#include "SaturnApp.h"

import Saturn.Context;

import Saturn.Scripts.ScriptWrapper;

int main(int argc, char* argv[]) {
	FScriptWrapper::InitBindings();
	//FWebServer::CreateWebServerThread();

	if (argc >= 3) {
		FContext::Channel = argv[1]; // channel
		FContext::Variant = argv[2]; // variant
	}

	SaturnApp app;
	app.Run();


	return 0;
}

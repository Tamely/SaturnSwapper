#include "Saturn/Scripts/WebServer.h"
#include "SaturnApp.h"

import Saturn.Context;

import Saturn.Scripts.ScriptWrapper;
import Saturn.IoStore.IoStoreReader;

import Saturn.Structs.Guid;
import Saturn.Encryption.AES;

int main(int argc, char* argv[]) {
	FScriptWrapper::InitBindings();
	FWebServer::CreateWebServerThread();

	FGuid guid(0, 0, 0, 0);
	FAESKey aes("0x62450FF9261CCC2EE50C217A2D9EE97F05F09203CF6E395B7CAB9D8892B714CE");

	std::unordered_map<FGuid, FAESKey> decryptionKeys;
	decryptionKeys.insert({ guid, aes });

	FIoStoreReader reader;
	reader.Initialize("D:\\Fortnite Builds\\Fortnite\\FortniteGame\\Content\\Paks\\pakchunk0-WindowsClient", decryptionKeys);

	if (argc >= 3) {
		FContext::Channel = argv[1]; // channel
		FContext::Variant = argv[2]; // variant
	}

	SaturnApp app;
	app.Run();


	return 0;
}

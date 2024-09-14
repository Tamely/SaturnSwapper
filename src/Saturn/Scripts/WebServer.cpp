#include "WebServer.h"

#include <HTTPLib/httplib.h>
#include <Crypt/skCrypter.h>

import Saturn.Context;
import Saturn.WindowsFunctionLibrary;
import Saturn.Functions.DoNothing;

void FWebServer::CreateWebServerThread() {
	CreateThread(nullptr, 0, InitWebServer, nullptr, 0, nullptr);
}

DWORD WINAPI FWebServer::InitWebServer(LPVOID) {
	try {
		httplib::Server svr;

		svr.Get("/", [](const httplib::Request&, httplib::Response& res) {
			res.set_content("{ \"note\":\"Instruction listener is up and running!\"", "application/json");
			});

		svr.Get("/finished", [](const httplib::Request&, httplib::Response& res) {
			FDoNothing::encode();

			static std::vector<uint8_t> data = WindowsFunctionLibrary::EncodeToBuffer("0");
			res.set_content(reinterpret_cast<const char*>(data.data()), data.size(), "image/png");
			});

		svr.listen(_("127.0.0.1"), 1337);
	}
	catch (std::exception e) {
		std::string exception = e.what();
		std::wstring exceptionW = std::wstring(exception.begin(), exception.end());

		MessageBoxW(nullptr, exceptionW.c_str(), L"Issue starting plugin listener", MB_OK);
	}

	return 0;
}
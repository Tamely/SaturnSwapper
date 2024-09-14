#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

class FWebServer {
public:
	static void CreateWebServerThread();
	static DWORD WINAPI InitWebServer(LPVOID);
};
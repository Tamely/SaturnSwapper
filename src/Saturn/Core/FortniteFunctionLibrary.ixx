export module Saturn.FortniteFunctionLibrary;

import <string>;

export class FortniteFunctionLibrary {
public:
	static std::string GetFortniteInstallationPath();
	static std::string GetFortniteAESKey();
	static std::wstring GetFortniteLocalPath();
	static bool PatchEpicGames();
	static bool PatchFortnite();
	static void LaunchFortnite();
	static void KillEpicProcesses();
private:
	static void KillProcessByName(const char* procName);
};
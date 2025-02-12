export module Saturn.FortniteFunctionLibrary;

import <tuple>;
import <string>;
import <vector>;

import Saturn.Items.LoadoutModel;

export class FortniteFunctionLibrary {
public:
	static std::string GetFortniteInstallationPath();
	static std::string GetFortniteAESKey();
	static std::vector<std::tuple<std::string, std::string>>& GetFortniteDynamicAESKeys();
	static std::tuple<std::string, std::string> GetFortniteMappingsURL();
	static std::wstring GetFortniteLocalPath();
	static bool PatchEpicGames();
	static bool PatchFortnite(const FLoadout& Loadout);
	static void LaunchFortnite();
	static void KillEpicProcesses();
private:
	static void KillProcessByName(const char* procName);
};
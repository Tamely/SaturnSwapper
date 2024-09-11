export module Saturn.Config;

import <string>;
import <vector>;
import <unordered_map>;

import Saturn.Items.LoadoutModel;

export struct FConfig {
	static std::string Key;
	static bool bFOVEnabled;
	static std::vector<FLoadout> Loadouts;

	static std::string UcasPath;
	static int64_t UcasSize;
	static std::unordered_map<std::string, std::string> UtocChanges;

	static void Load();
	static void Save();

	static void ClearLoadout();
};
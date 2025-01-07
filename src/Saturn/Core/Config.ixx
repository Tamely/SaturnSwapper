export module Saturn.Config;

import <string>;
import <vector>;
import <unordered_map>;

import Saturn.Items.LoadoutModel;

export struct FConfig {
	static std::string Key;
	static std::string RuntimeKey;
	
	static bool bHasSwappedSkin;

	static std::unordered_map<std::string, std::string> Dependencies;

	static void Load();
	static void Save();

	static void ClearLoadout();
};
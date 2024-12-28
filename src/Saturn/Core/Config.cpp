#include <DiscordSDK/rapidjson/document.h>
#include <DiscordSDK/rapidjson/stringbuffer.h>
#include <DiscordSDK/rapidjson/writer.h>

import Saturn.Config;

import <string>;
import <vector>;
import <fstream>;

import Saturn.Items.LoadoutModel;
import Saturn.Items.ItemModel;
import Saturn.WindowsFunctionLibrary;
import Saturn.Context;

std::string FConfig::Key = "";

bool FConfig::bFOVEnabled = false;
std::vector<FLoadout> FConfig::Loadouts = {};

std::string FConfig::UcasPath = "";
int64_t FConfig::UcasSize = INT64_MAX;
std::unordered_map<std::string, std::string> FConfig::UtocChanges = {};

std::wstring localPathW = WindowsFunctionLibrary::GetSaturnLocalPath();
std::string localPath = std::string(localPathW.begin(), localPathW.end());

void FConfig::Load() {
	static bool bLoaded = false;
	if (bLoaded) {
		return;
	}

	std::ifstream ifs(localPath + "\\Config.json");

	if (!ifs.good()) {
		return;
	}

	std::string content((std::istreambuf_iterator<char>(ifs)), (std::istreambuf_iterator<char>()));
	ifs.close();

	rapidjson::Document doc;
	doc.Parse(content.c_str());

	// This prevents it from erroring with the old config
	if (doc["SwappedSkin"].IsNull()) {
		doc.Clear();

		remove(std::string(localPath + "\\Config.json").c_str());
		Load();

		return;
	}

	bLoaded = true;

	FConfig::Key = doc["Key"].GetString();
	FConfig::bFOVEnabled = doc["FOVEnabled"].GetBool();

	std::string itemString = doc["SwappedSkin"].GetString();
	if (itemString != "") {
		std::vector<std::string> parts = WindowsFunctionLibrary::Split(itemString, "  ");
		FItem item;
		item.PackagePath = parts[0];
		item.Id = parts[1];
		item.Name = parts[2];

		FContext::Loadout.Skin = item;
	}

	itemString = doc["SwappedBackbling"].GetString();
	if (itemString != "") {
		std::vector<std::string> parts = WindowsFunctionLibrary::Split(itemString, "  ");
		FItem item;
		item.PackagePath = parts[0];
		item.Id = parts[1];
		item.Name = parts[2];

		FContext::Loadout.Backbling = item;
	}

	itemString = doc["SwappedPickaxe"].GetString();
	if (itemString != "") {
		std::vector<std::string> parts = WindowsFunctionLibrary::Split(itemString, "  ");
		FItem item;
		item.PackagePath = parts[0];
		item.Id = parts[1];
		item.Name = parts[2];

		FContext::Loadout.Pickaxe = item;
	}

	FConfig::UcasPath = doc["UcasPath"].GetString();
	FConfig::UcasSize = doc["UcasSize"].GetInt64();

	rapidjson::Value& changes = doc["UtocChanges"];
	for (rapidjson::Value::ConstMemberIterator itr = changes.MemberBegin(); itr != changes.MemberEnd(); ++itr) {
		std::string key = itr->name.GetString();
		std::string val = itr->value.GetString();
		FConfig::UtocChanges[key] = val;
	}
}

	

void FConfig::Save() {
	rapidjson::Document doc;
	doc.SetObject();

	rapidjson::Document::AllocatorType& allocator = doc.GetAllocator();

	doc.AddMember("Key", rapidjson::Value(FConfig::Key.c_str(), allocator).Move(), allocator);
	doc.AddMember("FOVEnabled", FConfig::bFOVEnabled, allocator);

	doc.AddMember("SwappedSkin", rapidjson::Value(std::string(FContext::Loadout.Skin.PackagePath + "  " + FContext::Loadout.Skin.Id + "  " + FContext::Loadout.Skin.Name).c_str(), allocator).Move(), allocator);
	doc.AddMember("SwappedBackbling", rapidjson::Value(std::string(FContext::Loadout.Backbling.PackagePath + "  " + FContext::Loadout.Backbling.Id + "  " + FContext::Loadout.Backbling.Name).c_str(), allocator).Move(), allocator);
	doc.AddMember("SwappedPickaxe", rapidjson::Value(std::string(FContext::Loadout.Pickaxe.PackagePath + "  " + FContext::Loadout.Pickaxe.Id + "  " + FContext::Loadout.Pickaxe.Name).c_str(), allocator).Move(), allocator);

	doc.AddMember("UcasPath", rapidjson::Value(FConfig::UcasPath.c_str(), allocator).Move(), allocator);
	doc.AddMember("UcasSize", FConfig::UcasSize, allocator);

	rapidjson::Value changes(rapidjson::kObjectType);

	for (const auto& [k, v] : FConfig::UtocChanges) {
		changes.AddMember(rapidjson::Value(k.c_str(), allocator).Move(), rapidjson::Value(v.c_str(), allocator).Move(), allocator);
	}

	doc.AddMember("UtocChanges", changes, allocator);

	rapidjson::StringBuffer buffer;
	rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);
	doc.Accept(writer);

	std::ofstream ofs(localPath + "\\Config.json");
	ofs << buffer.GetString();
	ofs.close();
}

void FConfig::ClearLoadout() {
	FContext::Loadout = FLoadout();
	FConfig::Save();
}
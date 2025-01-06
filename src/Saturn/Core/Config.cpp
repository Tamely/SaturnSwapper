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
std::string FConfig::RuntimeKey = "";

bool FConfig::bHasSwappedSkin = false;

std::unordered_map<std::string, int64_t> FConfig::UcasSizes = {};

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

	bLoaded = true;

	FConfig::Key = doc["Key"].GetString();
	FConfig::bHasSwappedSkin = doc["bHasSwappedSkin"].GetBool();

	rapidjson::Value& changes = doc["UcasSizes"];
	for (rapidjson::Value::ConstMemberIterator itr = changes.MemberBegin(); itr != changes.MemberEnd(); ++itr) {
		std::string key = itr->name.GetString();
		int64_t val = itr->value.GetInt64();
		FConfig::UcasSizes[key] = val;
	}
}

void FConfig::Save() {
	rapidjson::Document doc;
	doc.SetObject();

	rapidjson::Document::AllocatorType& allocator = doc.GetAllocator();

	doc.AddMember("Key", rapidjson::Value(FConfig::Key.c_str(), allocator).Move(), allocator);
	doc.AddMember("bHasSwappedSkin", FConfig::bHasSwappedSkin, allocator);

	rapidjson::Value changes(rapidjson::kObjectType);

	for (const auto& [k, v] : FConfig::UcasSizes) {
		changes.AddMember(rapidjson::Value(k.c_str(), allocator).Move(), v, allocator);
	}

	doc.AddMember("UcasSizes", changes, allocator);

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
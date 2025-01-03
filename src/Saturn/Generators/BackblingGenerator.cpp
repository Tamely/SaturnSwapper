#include "DiscordSDK/rapidjson/document.h"

#include <algorithm>

import Saturn.Generators.BackblingGenerator;

import Saturn.Context;
import Saturn.Items.ItemModel;
import Saturn.Generators.BaseGenerator;
import Saturn.WindowsFunctionLibrary;

import <vector>;
import <string>;
import <tuple>;

using namespace rapidjson;

Document FBackblingGenerator::json;

std::vector<FItem> FBackblingGenerator::GetItems() {
	std::vector<FItem> items;

	static std::tuple<int, std::string> stringData = WindowsFunctionLibrary::GetRequest("https://fortnite-api.com/v2/cosmetics/br/search/all?backendType=AthenaBackpack");

	static bool initialized = false;
	if (!initialized) {
		json.Parse(std::get<std::string>(stringData).c_str());
		initialized = true;
	}

	int skipNum = FContext::Tab * 254;
	for (auto& buffer : AssetRegistryState.PreallocatedAssetDataBuffers) {
		if (buffer.AssetClass.ToString() != ClassName) {
			continue;
		}

		if (skipNum > 0) {
			skipNum--;
			continue;
		}

		FItem item;

		item.PackagePath = buffer.PackageName.ToString();
		item.Id = buffer.AssetName.ToString();

		item.Name = "Unknown";
		if (std::get<int>(stringData) == 200) {
			for (Value& iteration : json["data"].GetArray()) {
				if (iteration["id"].GetString() == item.Id) {
					item.Name = iteration["name"].GetString();
					break;
				}
			}
		}

		if (item.Name == "null" || item.Name == "Unknown") {
			continue;
		}

		if (items.size() >= 254) {
			break;
		}

		if (item.Name == "TBD") {
			item.Name = item.Id;
		}

		items.push_back(item);
	}

	return items;
}

std::vector<FItem> FBackblingGenerator::FilterItems(const std::string& filter) {
	if (filter.empty()) {
		return GetItems();
	}

	std::vector<FItem> items;
	std::string filterCopy = filter;
	std::transform(filterCopy.begin(), filterCopy.end(), filterCopy.begin(), ::tolower);
	filterCopy.erase(std::remove_if(filterCopy.begin(), filterCopy.end(), [](auto const& c) -> bool { return !std::isalpha(c); }), filterCopy.end());

	for (auto& buffer : AssetRegistryState.PreallocatedAssetDataBuffers) {
		if (buffer.AssetClass.ToString() != ClassName) {
			continue;
		}

		FItem item;

		item.PackagePath = buffer.PackageName.ToString();
		item.Id = buffer.AssetName.ToString();

		item.Name = "Unknown";

		for (Value& iteration : json["data"].GetArray()) {
			if (iteration["id"].GetString() == item.Id) {
				item.Name = iteration["name"].GetString();
				break;
			}
		}

		if (item.Name == "null" || item.Name == "Unknown") {
			continue;
		}

		if (item.Name == "TBD") {
			item.Name = item.Id;
		}

		std::string name = item.Name;
		std::transform(name.begin(), name.end(), name.begin(), ::tolower);
		name.erase(std::remove_if(name.begin(), name.end(), [](auto const& c) -> bool { return !std::isalpha(c); }), name.end());

		if (name.contains(filterCopy)) {
			items.push_back(item);
		}
	}

	return items;
}

FItem FBackblingGenerator::GetItemById(const std::string& id) {
	FItem item;

	for (auto& buffer : AssetRegistryState.PreallocatedAssetDataBuffers) {
		if (buffer.AssetClass.ToString() != ClassName) {
			continue;
		}

		if (buffer.AssetName.ToString() == id) {
			item.PackagePath = buffer.PackageName.ToString();
			item.Id = buffer.AssetName.ToString();

			item.Name = "Unknown";
			for (Value& iteration : json["data"].GetArray()) {
				if (iteration["id"].GetString() == item.Id) {
					item.Name = iteration["name"].GetString();
					break;
				}
			}

			if (item.Name == "null" || item.Name == "Unknown") {
				continue;
			}

			if (item.Name == "TBD") {
				item.Name = item.Id;
			}

			break;
		}
	}

	return item;
}
export module Saturn.Items.ItemModel;

import <string>;

export struct FItem {
	std::string PackagePath;
	std::string Name = "Unnamed";
	std::string Id;

	bool IsValid() {
		return !PackagePath.empty() && !Name.empty() && Name != "Unnamed" && !Id.empty();
	}
};
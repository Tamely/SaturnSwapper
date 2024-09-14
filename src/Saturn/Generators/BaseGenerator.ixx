export module Saturn.Generators.BaseGenerator;

import Saturn.Encryption.AES;
import Saturn.Items.ItemModel;
import Saturn.AssetRegistry.AssetRegistryState;

import <string>;
import <vector>;

export class FBaseGenerator {
public:
	static void InitializeAssetRegistry(const std::string& pakPath, const FAESKey& encryptionKey);
protected:
	static bool bIsInitialized;
	static FAssetRegistryState AssetRegistryState;
public:
	static std::vector<FItem> ItemsToDisplay;
};
#include <Crypt/skCrypter.h>

import Saturn.Generators.BaseGenerator;

import Saturn.Pak.Pak;
import Saturn.Pak.PakEntry;
import Saturn.Encryption.AES;
import Saturn.Readers.MemoryReader;
import Saturn.AssetRegistry.AssetRegistryState;

import <string>;

FAssetRegistryState FBaseGenerator::AssetRegistryState;
bool FBaseGenerator::bIsInitialized = false;
std::vector<FItem> FBaseGenerator::ItemsToDisplay = {};

void FBaseGenerator::InitializeAssetRegistry(const std::string& pakPath, const FAESKey& encryptionKey) {
	if (bIsInitialized) {
		return;
	}

	FPak pak = FPak(pakPath, encryptionKey);
	FPakEntry entry = pak.GetEntries()[_("FortniteGame/AssetRegistry.bin")];
	FBufferReader reader(entry.Read(pakPath, pak.GetVersion(), pak.GetCompressionMethods(), encryptionKey));
	AssetRegistryState = FAssetRegistryState(reader);

	std::erase_if(AssetRegistryState.PreallocatedAssetDataBuffers, [](FAssetData& data) { return data.AssetClass.GetText() != "AthenaCharacterItemDefinition" 
																							  && data.AssetClass.GetText() != "AthenaBackpackItemDefinition"
																							  && data.AssetClass.GetText() != "AthenaPickaxeItemDefinition"
																							  && data.AssetClass.GetText() != "AthenaDanceItemDefinition"
		; });

	bIsInitialized = true;
}
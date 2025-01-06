#include <DiscordSDK/rapidjson/document.h>
#include <Crypt/skCrypter.h>

#include "Saturn/Log.h"
#include "Saturn/Defines.h"

#include <windows.h>
#include <process.h>
#include <Tlhelp32.h>
#include <winbase.h>
#include <ShlObj_core.h>

import Saturn.FortniteFunctionLibrary;

import <string>;
import <vector>;
import <fstream>;
import <sstream>;
import <filesystem>;

import Saturn.Config;
import Saturn.Context;
import Saturn.Structs.FileInfo;
import Saturn.Compression.Oodle;
import Saturn.Readers.FileReader;
import Saturn.Items.LoadoutModel;
import Saturn.Paths.SoftObjectPath;
import Saturn.IoStore.IoStoreReader;
import Saturn.WindowsFunctionLibrary;
import Saturn.Structs.IoOffsetLength;
import Saturn.Toc.IoContainerSettings;
import Saturn.Readers.ZenPackageReader;
import Saturn.Structs.IoStoreTocResource;

std::string FortniteFunctionLibrary::GetFortniteInstallationPath() {
	static const std::string DRIVES[] = { "A:\\", "B:\\", "C:\\", "D:\\", "E:\\", "F:\\", "G:\\", "H:\\", "I:\\", "J:\\", "K:\\", "L:\\", "M:\\" };

	std::string filePath;
	for (auto& drive : DRIVES) {
		filePath = drive + "ProgramData\\Epic\\UnrealEngineLauncher\\LauncherInstalled.dat";

		if (std::filesystem::exists(filePath)) {
			break;
		}
	}

	std::ifstream ifs(filePath);
	std::string content((std::istreambuf_iterator<char>(ifs)), (std::istreambuf_iterator<char>()));
	ifs.close();

	rapidjson::Document json;
	json.Parse(content.c_str());

	for (rapidjson::Value& iteration : json["InstallationList"].GetArray()) {
		std::string appName = iteration["AppName"].GetString();
		if (appName == "Fortnite") {
			std::string installLocation = iteration["InstallLocation"].GetString();
			return installLocation + "\\FortniteGame\\Content\\Paks\\";
		}
	}

	return "NOTFOUND";
}

std::string FortniteFunctionLibrary::GetFortniteAESKey() {
	static std::tuple<int, std::string> stringData = WindowsFunctionLibrary::GetRequest("https://fortnite-api.com/v2/aes");

	if (std::get<0>(stringData) != 200) {
		return "ERROR";
	}

	rapidjson::Document json;
	json.Parse(std::get<1>(stringData).c_str());

	try {
		std::string mainKey = json["data"]["mainKey"].GetString();
		if (mainKey.empty() || mainKey == "null") {
			return "ERROR";
		}

		return mainKey;
	}
	catch (std::exception e) {
		return "ERROR";
	}
}

std::tuple<std::string, std::string> FortniteFunctionLibrary::GetFortniteMappingsURL()
{
	static std::tuple<int, std::string> stringData = WindowsFunctionLibrary::GetRequest("https://fortnitecentral.genxgames.gg/api/v1/mappings");

	if (std::get<0>(stringData) != 200) {
		return { "ERROR", "ERROR" };
	}

	rapidjson::Document json;
	json.Parse(std::get<1>(stringData).c_str());

	if (json.HasParseError() || !json.IsArray()) {
		return { "ERROR", "ERROR" };
	}

	for (const auto& item : json.GetArray()) {
		if (item.HasMember("url") && item.HasMember("fileName") && item.HasMember("meta") && item["meta"].HasMember("compressionMethod")) {
			std::string compressionMethod = item["meta"]["compressionMethod"].GetString();

			if (compressionMethod == "Oodle" || compressionMethod == "None") {
				return {
					item["url"].GetString(),
					item["fileName"].GetString()
				};
			}
		}
	}

	return { "ERROR", "ERROR" };
}

std::wstring FortniteFunctionLibrary::GetFortniteLocalPath() {
	std::wstringstream ss;

	wchar_t* localAppData = 0;
	SHGetKnownFolderPath(FOLDERID_LocalAppData, 0, NULL, &localAppData);

	ss << localAppData << L"\\FortniteGame\\Saved\\";

	CoTaskMemFree(static_cast<void*>(localAppData));

	return ss.str();
}

bool FortniteFunctionLibrary::PatchEpicGames() {
	return false;
}

bool FortniteFunctionLibrary::PatchFortnite(const FLoadout& Loadout) {
	FortniteFunctionLibrary::KillEpicProcesses();

	if (FConfig::bHasSwappedSkin) {
		LOG_INFO("Creating backup directory");
		std::wstring BackupDirectoryW = WindowsFunctionLibrary::GetSaturnLocalPath() + L"\\TocBackups\\";
		std::string BackupDirectory = std::string(BackupDirectoryW.begin(), BackupDirectoryW.end());

		WindowsFunctionLibrary::MakeDirectory(BackupDirectoryW);


		LOG_INFO("Getting all TOCs from backup directory");

		std::vector<std::string> backedUpTOCs = WindowsFunctionLibrary::GetFilesInDirectory(BackupDirectory);
		for (std::string& toc : backedUpTOCs) {
			WindowsFunctionLibrary::RenameFile(BackupDirectory + toc, GetFortniteInstallationPath() + toc);
			LOG_INFO("Moved TOC from '{0}' to '{1}'.", BackupDirectory + toc, GetFortniteInstallationPath() + toc);
		}

		for (auto& [path, size] : FConfig::UcasSizes) {
			FFileReader reader(path.c_str());
			reader.TrimToSize(size, true);
			reader.Close();
		}

		LOG_INFO("Reverted!");
		FConfig::bHasSwappedSkin = false;
		FConfig::UcasSizes.clear();
		FConfig::Save();

		return true;
	}

	uint8_t* asset = std::move(ASSET_DATA);
	int UsedCharacterParts = 0;

	static const std::vector<std::wstring> CharacterPartPathsToSearch = {
		L"/Game/00000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
		L"/Game/11111111111111111111111111111111111111111111111111111111111111111111111111111111111111",
		L"/Game/22222222222222222222222222222222222222222222222222222222222222222222222222222222222222",
		L"/Game/33333333333333333333333333333333333333333333333333333333333333333333333333333333333333",
		L"/Game/44444444444444444444444444444444444444444444444444444444444444444444444444444444444444"
	};

	static const std::vector<std::wstring> CharacterPartNamesToSearch = {
		L"000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
		L"111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111",
		L"222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222",
		L"333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333",
		L"444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444"
	};

	FZenPackageReader assetReader(asset, ASSET_LENGTH);
	if (!assetReader.IsOk()) {
		LOG_ERROR("Error patching Fortnite. Failed to load DefaultGameDataCosmetics. Error: {0}", assetReader.GetStatus().ToString());
		return false;
	}

	if (FContext::Loadout.Skin.IsValid()) {
		LOG_INFO("Loading skin '{0}'", Loadout.Skin.Name);
		UPackagePtr package = FContext::Provider->LoadPackage(Loadout.Skin.PackagePath + ".uasset");
		LOG_INFO("Getting first export");
		UObjectPtr firstExport = package->GetFirstExport();
		LOG_INFO("Got export");

		std::vector<FSoftObjectPath> characterParts = firstExport->GetProperty<std::vector<FSoftObjectPath>>("BaseCharacterParts");
		LOG_INFO("Skin '{0}' has {1} character parts!", Loadout.Skin.Name, characterParts.size());

		for (FSoftObjectPath& charPart : characterParts) {
			if (UsedCharacterParts > CharacterPartNamesToSearch.size() - 1) {
				LOG_ERROR("Ran out of useable character part searches!");
				break;
			}

			std::string packageName = charPart.GetAssetPath().GetPackageName();
			std::string assetName = charPart.GetAssetPath().GetAssetName();

			std::wstring packageNameW = std::wstring(packageName.begin(), packageName.end());
			std::wstring assetNameW = std::wstring(assetName.begin(), assetName.end());

			assetReader.GetNameMap().SetName(CharacterPartPathsToSearch[UsedCharacterParts], packageNameW);
			assetReader.GetNameMap().SetName(CharacterPartNamesToSearch[UsedCharacterParts++], assetNameW);
		}

		uint8_t* originalAsset = std::move(ASSET_DATA);
		std::vector<uint8_t> originalBuffer(originalAsset, originalAsset + ASSET_LENGTH);
		std::vector<uint8_t> bufferToWrite = assetReader.SerializeAsByteArray(originalBuffer);

		FIoStoreReader* reader = FContext::Provider->GetReaderByPathAndExtension("/Game/Balance/DefaultGameDataCosmetics.uasset");
		uint32_t TocEntryIndex = FContext::Provider->GetTocEntryIndexByPathAndExtension("/Game/Balance/DefaultGameDataCosmetics.uasset");

		FIoStoreTocResource& toc = reader->GetTocResource();

		TIoStatusOr<FIoStoreTocChunkInfo> chunkStatus = reader->GetChunkInfo(TocEntryIndex);
		if (!chunkStatus.IsOk()) {
			LOG_ERROR("Failed to load DefaultGameDataCosmetics");
			return false;
		}

		FIoStoreTocChunkInfo chunkInfo = chunkStatus.ConsumeValueOrDie();
		FIoChunkId chunkId = chunkInfo.Id;

		FIoOffsetAndLength* offsetAndLength = reader->GetOffsetAndLength(chunkId);

		const int32_t FirstBlockIndex = int32_t(offsetAndLength->GetOffset() / toc.Header.CompressionBlockSize);

		int32_t PartitionIndex = toc.CompressionBlocks[FirstBlockIndex].GetOffset() / toc.Header.PartitionSize;
		int64_t PartitionOffset = toc.CompressionBlocks[FirstBlockIndex].GetOffset() % toc.Header.PartitionSize;

		int32_t newBlockCount = (bufferToWrite.size() - 1) / toc.Header.CompressionBlockSize + 1;

		std::vector<std::string> containerPaths;
		reader->GetContainerFilePaths(containerPaths);

		std::string ContainerPath = containerPaths[PartitionIndex];

		FFileReader Ar(ContainerPath.c_str());
		Ar.Seek(PartitionOffset);
		LOG_INFO("Opened container '{0}' at offset {1}", ContainerPath, Ar.Tell());

		for (int i = 0; i < newBlockCount; i++) {
			int32_t blockBufferLen = bufferToWrite.size() - i * toc.Header.CompressionBlockSize > toc.Header.CompressionBlockSize
				? toc.Header.CompressionBlockSize
				: bufferToWrite.size() - i * toc.Header.CompressionBlockSize;

			uint8_t* blockBuffer = new uint8_t[blockBufferLen];
			memcpy(blockBuffer, bufferToWrite.data() + (i * toc.Header.CompressionBlockSize), blockBufferLen);

			uint32_t MaxCompressionSize = Oodle::GetMaximumCompressedSize(blockBufferLen);
			int32_t CompressedSize = 0;
			uint8_t* compressedBlockBufferWithLargeLen = new uint8_t[MaxCompressionSize];
			Oodle::Compress(compressedBlockBufferWithLargeLen, CompressedSize, bufferToWrite.data() + (i * toc.Header.CompressionBlockSize), blockBufferLen);

			uint8_t* CompressedBlockBuffer = new uint8_t[CompressedSize];
			memcpy(CompressedBlockBuffer, compressedBlockBufferWithLargeLen, CompressedSize);

			int64_t Offset = Ar.Tell();

			toc.CompressionBlocks[FirstBlockIndex + i] = FIoStoreTocCompressedBlockEntry();
			toc.CompressionBlocks[FirstBlockIndex + i].SetOffset(Offset + ((toc.Header.PartitionCount - 1) * toc.Header.PartitionSize));
			toc.CompressionBlocks[FirstBlockIndex + i].SetCompressedSize(CompressedSize);
			toc.CompressionBlocks[FirstBlockIndex + i].SetUncompressedSize(blockBufferLen);
			toc.CompressionBlocks[FirstBlockIndex + i].SetCompressionMethodIndex(1);

			if (!Ar.WriteBuffer(blockBuffer, blockBufferLen)) {
				LOG_ERROR("Failed to write block buffer");
				return false;
			}

			uint64_t Padding = 0;
			Ar.WriteBuffer(&Padding, sizeof(uint64_t));

			delete[] blockBuffer;
		}

		Ar.Close();

		std::string containerName = reader->GetContainerName();
		toc.ChunkOffsetAndLengths[TocEntryIndex].SetLength(bufferToWrite.size());

		FIoContainerSettings containerSettings;
		containerSettings.ContainerId = toc.Header.ContainerId;
		containerSettings.ContainerFlags = toc.Header.ContainerFlags;
		containerSettings.EncryptionKeyGuid = toc.Header.EncryptionKeyGuid;

		uint64_t size = 0;
		FIoStatus status = FIoStoreTocResource::Write(GetFortniteInstallationPath() + containerName + ".utoc.tamely", toc, toc.Header.CompressionBlockSize, toc.Header.PartitionSize, containerSettings, size);
		if (!status.IsOk()) {
			LOG_ERROR("Failed to make toc. Error {0}", status.ToString());
			return false;
		}
		else {
			LOG_INFO("Creating backup directory");
			std::wstring BackupDirectoryW = WindowsFunctionLibrary::GetSaturnLocalPath() + L"\\TocBackups\\";
			std::string BackupDirectory = std::string(BackupDirectoryW.begin(), BackupDirectoryW.end());

			WindowsFunctionLibrary::MakeDirectory(BackupDirectoryW);
			LOG_INFO("Moving TOC to backup directory");
			WindowsFunctionLibrary::RenameFile(GetFortniteInstallationPath() + containerName + ".utoc", BackupDirectory + containerName + ".utoc");
			LOG_INFO("Moving new TOC to old position");
			WindowsFunctionLibrary::RenameFile(GetFortniteInstallationPath() + containerName + ".utoc.tamely", GetFortniteInstallationPath() + containerName + ".utoc");
			LOG_INFO("Added fallback parts!");
		}

		reader = FContext::Provider->GetReaderByPathAndExtension("/BRCosmetics/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_Prime.uasset");
		TocEntryIndex = FContext::Provider->GetTocEntryIndexByPathAndExtension("/BRCosmetics/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_Prime.uasset");
		chunkStatus = reader->GetChunkInfo(TocEntryIndex);
		if (!chunkStatus.IsOk()) {
			LOG_ERROR("Failed to load CP_Athena_Body_F_Prime");
			return false;
		}

		chunkInfo = chunkStatus.ConsumeValueOrDie();
		FIoChunkId defaultChunkId = chunkInfo.Id;

		toc = reader->GetTocResource();

		for (auto& tocChunkId : toc.ChunkIds) {
			if (tocChunkId == defaultChunkId) {
				tocChunkId = CreateIoChunkId(0, tocChunkId.GetChunkIndex(), tocChunkId.GetChunkType());
				break;
			}
		}

		containerSettings.ContainerId = toc.Header.ContainerId;
		containerSettings.ContainerFlags = toc.Header.ContainerFlags;
		containerSettings.EncryptionKeyGuid = toc.Header.EncryptionKeyGuid;

		size = 0;
		status = FIoStoreTocResource::Write(GetFortniteInstallationPath() + reader->GetContainerName() + ".utoc.tamely", toc, toc.Header.CompressionBlockSize, toc.Header.PartitionSize, containerSettings, size);
		if (!status.IsOk()) {
			LOG_ERROR("Failed to make toc. Error {0}", status.ToString());
			return false;
		}
		else {
			LOG_INFO("Creating backup directory");
			std::wstring BackupDirectoryW = WindowsFunctionLibrary::GetSaturnLocalPath() + L"\\TocBackups\\";
			std::string BackupDirectory = std::string(BackupDirectoryW.begin(), BackupDirectoryW.end());

			WindowsFunctionLibrary::MakeDirectory(BackupDirectoryW);
			LOG_INFO("Moving TOC to backup directory");
			WindowsFunctionLibrary::RenameFile(GetFortniteInstallationPath() + reader->GetContainerName() + ".utoc", BackupDirectory + reader->GetContainerName() + ".utoc");
			LOG_INFO("Moving new TOC to old position");
			WindowsFunctionLibrary::RenameFile(GetFortniteInstallationPath() + reader->GetContainerName() + ".utoc.tamely", GetFortniteInstallationPath() + reader->GetContainerName() + ".utoc");
			LOG_INFO("Invalidated default");
			LOG_INFO("Converted!");

			FConfig::bHasSwappedSkin = true;
		}
	}

	FConfig::Save();
	return true;
}

void FortniteFunctionLibrary::LaunchFortnite() {
	system("cmd.exe /C start com.epicgames.launcher://apps/Fortnite?action=launch");
}

void FortniteFunctionLibrary::KillEpicProcesses() {
	static std::string Processes[] = { "EpicGamesLauncher.exe", "FortniteLauncher.exe", "FortniteClient-Win64-Shipping.exe", "FortniteClient-Win64-Shipping_BE.exe", "FortniteClient-Win64-Shipping_EAC.exe", "FortniteClient-Win64-Shipping_EAC_EOS.exe", "CrashReportClient.exe", "EpicGamesLauncher", "FortniteLauncher", "FortniteClient-Win64-Shipping", "FortniteClient-Win64-Shipping_BE", "FortniteClient-Win64-Shipping_EAC", "FortniteClient-Win64-Shipping_EAC_EOS", "CrashReportClient" };
	for (auto& proc : Processes) {
		FortniteFunctionLibrary::KillProcessByName(proc.c_str());
	}
}

void FortniteFunctionLibrary::KillProcessByName(const char* procName) {
	HANDLE hSnapShot = CreateToolhelp32Snapshot(TH32CS_SNAPALL, NULL);
	PROCESSENTRY32 pEntry;
	pEntry.dwSize = sizeof(pEntry);
	BOOL hRes = Process32First(hSnapShot, &pEntry);
	while (hRes)
	{
		if (strcmp(pEntry.szExeFile, procName) == 0)
		{
			HANDLE hProcess = OpenProcess(PROCESS_TERMINATE, 0,
				(DWORD)pEntry.th32ProcessID);
			if (hProcess != NULL)
			{
				TerminateProcess(hProcess, 9);
				CloseHandle(hProcess);
			}
		}
		hRes = Process32Next(hSnapShot, &pEntry);
	}
	CloseHandle(hSnapShot);
}
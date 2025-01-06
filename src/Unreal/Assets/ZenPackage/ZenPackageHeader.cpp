import Saturn.ZenPackage.ZenPackageHeader;

#include "Saturn/Log.h"

import <string>;
import <vector>;
import <cstdint>;

import Saturn.Asset.NameMap;
import Saturn.Structs.MappedName;
import Saturn.Readers.MemoryReader;
import Saturn.Asset.ExportMapEntry;
import Saturn.Asset.BulkDataMapEntry;
import Saturn.Asset.ExportBundleEntry;
import Saturn.Asset.PackageObjectIndex;
import Saturn.Asset.DependencyBundleEntry;
import Saturn.Asset.DependencyBundleHeader;
import Saturn.ZenPackage.ZenPackageSummary;
import Saturn.ZenPackage.ZenPackageImportedPackageNamesContainer;

FZenPackageHeader FZenPackageHeader::MakeView(std::vector<uint8_t>& Memory) {
    std::string Error;
    FZenPackageHeader Result = MakeView(Memory, Error);
    if (!Error.empty()) {
        LOG_ERROR(Error);
    }
    return Result;
}

FZenPackageHeader FZenPackageHeader::MakeView(std::vector<uint8_t>& Memory, std::string& OutError) {
    OutError.clear();

    FZenPackageHeader PackageHeader;
    uint8_t* PackageHeaderDataPtr = reinterpret_cast<uint8_t*>(Memory.data());
    PackageHeader.PackageSummary = std::move(reinterpret_cast<FZenPackageSummary*>(PackageHeaderDataPtr));

    std::vector<uint8_t> PackageHeaderDataView(PackageHeaderDataPtr + sizeof(FZenPackageSummary), PackageHeaderDataPtr + PackageHeader.PackageSummary->HeaderSize - sizeof(FZenPackageSummary));
    FMemoryReader PackageHeaderDataReader(PackageHeaderDataView);
    if (PackageHeader.PackageSummary->bHasVersioningInfo) {
        // We actually do not have this done yet bc fortnite doesn't use it
    }

    {
        PackageHeader.NameMap.Load(PackageHeaderDataReader, FMappedName::EType::Package);
    }
    PackageHeader.PackageName = PackageHeader.NameMap.GetName(PackageHeader.PackageSummary->Name);

    int64_t BulkDataMapSize = 0;
    uint64_t BulkDataPad = 0;
    PackageHeaderDataReader << BulkDataPad;
    uint8_t PadBytes[sizeof(uint64_t)] = {};
    PackageHeaderDataReader.Serialize(PadBytes, BulkDataPad);
    PackageHeaderDataReader << BulkDataMapSize;
    uint8_t* BulkDataMapData = PackageHeaderDataPtr + sizeof(FZenPackageSummary) + PackageHeaderDataReader.Tell();
    PackageHeader.BulkDataMap = std::vector<FBulkDataMapEntry>(reinterpret_cast<FBulkDataMapEntry*>(BulkDataMapData), reinterpret_cast<FBulkDataMapEntry*>(BulkDataMapData + BulkDataMapSize));

    PackageHeader.CookedHeaderSize = PackageHeader.PackageSummary->CookedHeaderSize;
    PackageHeader.ImportedPublicExportHashes = std::vector<uint64_t>(reinterpret_cast<uint64_t*>(PackageHeaderDataPtr + PackageHeader.PackageSummary->ImportedPublicExportHashesOffset), reinterpret_cast<uint64_t*>(PackageHeaderDataPtr + PackageHeader.PackageSummary->ImportMapOffset));
    PackageHeader.ImportMap = std::vector<FPackageObjectIndex>(reinterpret_cast<FPackageObjectIndex*>(PackageHeaderDataPtr + PackageHeader.PackageSummary->ImportMapOffset), reinterpret_cast<FPackageObjectIndex*>(PackageHeaderDataPtr + PackageHeader.PackageSummary->ExportMapOffset));
    PackageHeader.ExportMap = std::vector<FExportMapEntry>(reinterpret_cast<FExportMapEntry*>(PackageHeaderDataPtr + PackageHeader.PackageSummary->ExportMapOffset), reinterpret_cast<FExportMapEntry*>(PackageHeaderDataPtr + PackageHeader.PackageSummary->ExportBundleEntriesOffset));
    PackageHeader.ExportCount = PackageHeader.ExportMap.size();

    const uint64_t ExportBundleEntriesSize = PackageHeader.PackageSummary->DependencyBundleHeadersOffset - PackageHeader.PackageSummary->ExportBundleEntriesOffset;
    const int32_t ExportBundleEntriesCount = static_cast<int32_t>(ExportBundleEntriesSize / sizeof(FExportBundleEntry));

    if (ExportBundleEntriesCount != PackageHeader.ExportCount * FExportBundleEntry::ExportCommandType_Count) {
        OutError = "Corrupt Zen header in package " + std::string(PackageHeader.PackageName.begin(), PackageHeader.PackageName.end());
        return PackageHeader;
    }

    PackageHeader.ExportBundleEntries = std::vector<FExportBundleEntry>(reinterpret_cast<FExportBundleEntry*>(PackageHeaderDataPtr + PackageHeader.PackageSummary->ExportBundleEntriesOffset), reinterpret_cast<FExportBundleEntry*>(PackageHeaderDataPtr + PackageHeader.PackageSummary->ExportBundleEntriesOffset + (sizeof(FExportBundleEntry) * ExportBundleEntriesCount)));
    PackageHeader.DependencyBundleHeaders = std::vector<FDependencyBundleHeader>(reinterpret_cast<FDependencyBundleHeader*>(PackageHeaderDataPtr + PackageHeader.PackageSummary->DependencyBundleHeadersOffset), reinterpret_cast<FDependencyBundleHeader*>(PackageHeaderDataPtr + PackageHeader.PackageSummary->DependencyBundleEntriesOffset));
    PackageHeader.DependencyBundleEntries = std::vector<FDependencyBundleEntry>(reinterpret_cast<FDependencyBundleEntry*>(PackageHeaderDataPtr + PackageHeader.PackageSummary->DependencyBundleEntriesOffset), reinterpret_cast<FDependencyBundleEntry*>(PackageHeaderDataPtr + PackageHeader.PackageSummary->ImportedPackageNamesOffset));

    std::vector<uint8_t> ImportedPackageNamesDataView = std::vector<uint8_t>(reinterpret_cast<uint8_t*>(PackageHeaderDataPtr + PackageHeader.PackageSummary->ImportedPackageNamesOffset), reinterpret_cast<uint8_t*>(PackageHeaderDataPtr + PackageHeader.PackageSummary->ImportedPackageNamesOffset + PackageHeader.PackageSummary->HeaderSize - PackageHeader.PackageSummary->ImportedPackageNamesOffset));
    FMemoryReader ImportedPackageNamesDataReader(ImportedPackageNamesDataView);
    FZenPackageImportedPackageNamesContainer Container;
    ImportedPackageNamesDataReader << Container;
    PackageHeader.ImportedPackageNames = std::move(Container.Names);

    PackageHeader.ExportOffset = PackageHeader.PackageSummary->HeaderSize;

    for (std::wstring& nameW : PackageHeader.ImportedPackageNames) {
        std::string name(nameW.begin(), nameW.end());
        PackageHeader.ImportedPackageIds.push_back(FPackageId::FromName(name));
    }

    return PackageHeader;
}

void FZenPackageHeader::Reset() {
    PackageSummary = nullptr;
    ImportedPublicExportHashes = std::vector<uint64_t>();
    ImportMap = std::vector<FPackageObjectIndex>();
    ExportMap = std::vector<FExportMapEntry>();
    BulkDataMap = std::vector<FBulkDataMapEntry>();
    ExportBundleEntries = std::vector<FExportBundleEntry>();
    DependencyBundleHeaders = std::vector<FDependencyBundleHeader>();
    DependencyBundleEntries = std::vector<FDependencyBundleEntry>();
}

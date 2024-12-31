export module Saturn.Asset.ExportFilterFlags;

import <cstdint>;

export enum class EExportFilterFlags : uint8_t {
    None,
    NotForClient,
    NotForServer
};
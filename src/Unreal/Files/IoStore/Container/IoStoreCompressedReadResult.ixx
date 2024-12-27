export module Saturn.Container.IoStoreCompressedReadResult;

import Saturn.Misc.IoBuffer;

import <string>;
import <vector>;
import <cstdint>;

export struct FIoStoreCompressedBlockInfo {
    // Name of the method used to compress the block
    std::string CompressionMethod;
    // The size of relevant data in the block (i.e. what you pass to decompress).
    uint32_t CompressedSize;
    // The size of the _block_ after decompression. This is not adjusted for any FIoReadOptions used.
    uint32_t UncompressedSize;
    // The size of the data this block takes in IoBuffer (i.e. after padding for decryption).
    uint32_t AlignedSize;
    // Where in IoBuffer this block starts.
    uint64_t OffsetInBuffer;
};

export struct FIoStoreCompressedReadResult {
    // The buffer containing the chunk
    FIoBuffer IoBuffer;

    // Info about the blocks that the chunk is split up into.
    std::vector<FIoStoreCompressedBlockInfo> Blocks;
    // There is where the data starts in IoBuffer (for when you pass in a data range via FIoReadOptions)
    uint64_t UncompressedOffset = 0;
    // This is the total size requested via FIoReadOptions. Notably, if you requested a narrow rannge, you could
    // add up all the block uncompressed sizes and it would be larger than this.
    uint64_t UncompressedSize = 0;
    // This is the total size of compressed data, which is less than IoBuffer size due to padding for decryption.
    uint64_t TotalCompressedSize = 0;
};
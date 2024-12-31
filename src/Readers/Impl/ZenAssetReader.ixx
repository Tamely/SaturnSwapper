export module Saturn.Readers.ZenAssetReader;

import Saturn.Readers.MemoryReader;

import Saturn.Misc.IoBuffer;
import Saturn.IoStore.IoStoreReader;

import <cstdint>;
import <vector>;

export class FZenAssetReader : public FMemoryReader {
public:
    FZenAssetReader(FIoBuffer& IoBuffer) : FMemoryReader(IoBuffer.GetData(), IoBuffer.GetSize()) {

    }
private:
    FIoStoreReader* GlobalData = nullptr;
    std::vector<uint64_t> ImportedPublicExportHashes;

};
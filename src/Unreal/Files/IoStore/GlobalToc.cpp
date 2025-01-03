import Saturn.IoStore.GlobalToc;

#include "Saturn/Log.h"
#include "Saturn/Defines.h"

import Saturn.Asset.NameMap;
import Saturn.Structs.MappedName;
import Saturn.Asset.PackageObjectIndex;

import Saturn.Core.IoStatus;
import Saturn.Misc.IoBuffer;
import Saturn.Structs.IoChunkId;
import Saturn.Readers.MemoryReader;
import Saturn.IoStore.IoStoreReader;
import Saturn.Structs.IoStoreTocChunkInfo;

void FGlobalTocData::Serialize(FIoStoreReader* Reader) {
    FIoChunkId ChunkId(0, 0, EIoChunkType::ScriptObjects);

    TIoStatusOr<FIoStoreTocChunkInfo> ChunkStatus = Reader->GetChunkInfo(ChunkId);
    if (!ChunkStatus.IsOk()) return;

    FIoStoreTocChunkInfo ChunkInfo = ChunkStatus.ConsumeValueOrDie();

    TIoStatusOr<FIoBuffer> ScriptObjectsBufferStatus = Reader->Read(ChunkId, FIoReadOptions(0, ChunkInfo.Size));
    if (!ScriptObjectsBufferStatus.IsOk()) return;

    FIoBuffer ScriptObjectsBuffer = ScriptObjectsBufferStatus.ConsumeValueOrDie();

    FMemoryReader ScriptObjectsReader(ScriptObjectsBuffer.GetData(), ScriptObjectsBuffer.GetSize());

    NameMap.Load(ScriptObjectsReader, FMappedName::EType::Global);

    int32_t NumScriptObjects = 0;
    ScriptObjectsReader << NumScriptObjects;

    FScriptObjectEntry* ScriptObjectEntries = reinterpret_cast<FScriptObjectEntry*>(ScriptObjectsBuffer.GetData() + ScriptObjectsReader.Tell());

    ScriptObjectByGlobalIdMap.reserve(NumScriptObjects);
    for (size_t i = 0; i < NumScriptObjects; i++) {
        auto& ScriptObjectEntry = ScriptObjectEntries[i];
        ScriptObjectByGlobalIdMap.insert_or_assign(ScriptObjectEntry.GlobalIndex, ScriptObjectEntry);
    }
}
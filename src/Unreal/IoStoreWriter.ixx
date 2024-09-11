export module Saturn.Unreal.IoStoreWriter;

import Saturn.Compression;
import Saturn.Asset.ZenAsset;
import Saturn.Structs.FileInfo;
import Saturn.Structs.IoChunkId;
import Saturn.Readers.FileReader;
import Saturn.Unreal.IoStoreReader;
import Saturn.Structs.IoStoreTocCompressedBlockEntry;

import <memory>;
import <unordered_map>;
import <filesystem>;
import <fstream>;
import <iostream>;
import <tuple>;

import <fstream>;

import "xxhash/xxhash.h";
import Saturn.Config;
import Saturn.WindowsFunctionLibrary;

export class IoStoreWriter {
public:
	__forceinline IoStoreWriter(std::shared_ptr<IoStoreReader> reader) : m_Reader(reader) {
		for (int i = 0; i < m_Reader->PartitionCount; i++) {
			std::string ucasPath = m_Reader->m_FilePath.substr(0, m_Reader->m_FilePath.length() - 5) +
				(i > 0 ? ("_s" + std::to_string(i) + ".ucas") : ".ucas");

			m_Containers[i] = std::make_shared<FFileReader>(ucasPath.c_str());
		}
	}

    __forceinline static void Revert() {
        std::filesystem::resize_file(FConfig::UcasPath, FConfig::UcasSize);
        
        for (auto& [k, v] : FConfig::UtocChanges) {
            std::string utocPath = WindowsFunctionLibrary::Split(k, "  ")[0];
            FFileReader utoc(utocPath.c_str());

            std::string utocOffset = WindowsFunctionLibrary::Split(k, "  ")[1];
            utoc.Seek(std::stoll(utocOffset));

            std::string lengthString = WindowsFunctionLibrary::Split(v, "  ")[0];
            size_t len = std::stoi(lengthString);

            std::string dataString = WindowsFunctionLibrary::Split(v, "  ")[1];
            utoc.WriteBuffer(WindowsFunctionLibrary::Decode(dataString, len), len);
            utoc.Close();
        }


        FConfig::ClearLoadout();
        FConfig::UcasPath = "";
        FConfig::UcasSize = INT64_MAX;
        FConfig::UtocChanges = {};
        FConfig::Save();
    }

    void Close() {
        for (auto& c : m_Containers) {
            c.second->Close();
        }
    }

    __forceinline void InvalidateFile(const std::string& path) {
        VFileInfo pathInfo = m_Reader->GetFile(path);

        m_Reader->m_Reader.Seek(pathInfo.ChunkId.GetPosition());

        uint8_t* data = new uint8_t[12];
        m_Reader->m_Reader.Serialize(data, 12);
        m_Reader->m_Reader.SeekCur(-1 * 12);

        FConfig::UtocChanges[m_Reader->m_FilePath + "  " + std::to_string(m_Reader->m_Reader.Tell())] = std::to_string(12)
            + "  " + WindowsFunctionLibrary::Encode(data, 12);

        FIoChunkId chunk = pathInfo.ChunkId;
        chunk.Invalidate();

        m_Reader->m_Reader >> chunk;

        FConfig::Save();
    }

    __forceinline void OverwriteFileWithBouncer(const std::string& filePath, const std::string& bouncerPath, uint8_t buffer[], size_t bufferLen) {
        if (FConfig::UcasSize == INT64_MAX) return OverwriteFileWithBouncerComplete(filePath, bouncerPath, buffer, bufferLen);

        VFileInfo info = m_Reader->GetFile(filePath);

        int64_t currentPos = INT64_MAX;

        int32_t newBlockCount = (bufferLen - 1) / m_Reader->COMPRESSION_BLOCK_SIZE + 1;

        for (int i = 0; i < newBlockCount; i++) {
            int32_t blockBufferLen = bufferLen - i * m_Reader->COMPRESSION_BLOCK_SIZE > m_Reader->COMPRESSION_BLOCK_SIZE
                ? m_Reader->COMPRESSION_BLOCK_SIZE
                : bufferLen - i * m_Reader->COMPRESSION_BLOCK_SIZE;

            uint8_t* blockBuffer = new uint8_t[blockBufferLen];
            memcpy(blockBuffer, buffer + (i * m_Reader->COMPRESSION_BLOCK_SIZE), blockBufferLen);

            if (currentPos == INT64_MAX) {
                currentPos = FConfig::UcasSize;
            }

            m_Containers[m_Reader->PartitionCount - 1]->Seek(currentPos);
            m_Containers[m_Reader->PartitionCount - 1]->WriteBuffer(blockBuffer, blockBufferLen);

            currentPos += blockBufferLen + 8;

            delete[] blockBuffer;
        }
    }

	__forceinline void OverwriteFile(const std::string& filePath, uint8_t buffer[], size_t bufferLen) {
        if (FConfig::UcasSize == INT64_MAX) return OverwriteFileComplete(filePath, buffer, bufferLen);

        VFileInfo info = m_Reader->GetFile(filePath);

        int64_t currentPos = INT64_MAX;

        int32_t newBlockCount = (bufferLen - 1) / m_Reader->COMPRESSION_BLOCK_SIZE + 1;

        for (int i = 0; i < newBlockCount; i++) {
            int32_t blockBufferLen = bufferLen - i * m_Reader->COMPRESSION_BLOCK_SIZE > m_Reader->COMPRESSION_BLOCK_SIZE
                                   ? m_Reader->COMPRESSION_BLOCK_SIZE
                                   : bufferLen - i * m_Reader->COMPRESSION_BLOCK_SIZE;

            uint8_t* blockBuffer = new uint8_t[blockBufferLen];
            memcpy(blockBuffer, buffer + (i * m_Reader->COMPRESSION_BLOCK_SIZE), blockBufferLen);

            if (currentPos == INT64_MAX) {
                currentPos = FConfig::UcasSize;
            }

            m_Containers[m_Reader->PartitionCount - 1]->Seek(currentPos);
            m_Containers[m_Reader->PartitionCount - 1]->WriteBuffer(blockBuffer, blockBufferLen);

            currentPos += blockBufferLen + 8;

            delete[] blockBuffer;
        }
	}
private:
    __forceinline void OverwriteFileWithBouncerComplete(const std::string& filePath, const std::string& bouncerPath, uint8_t buffer[], size_t bufferLen) {
        VFileInfo info = m_Reader->GetFile(filePath);
        VFileInfo bouncerInfo = m_Reader->GetFile(bouncerPath);

        int64_t currentPos = INT64_MAX;

        int32_t newBlockCount = (bufferLen - 1) / m_Reader->COMPRESSION_BLOCK_SIZE + 1;

        for (int i = 0; i < newBlockCount; i++) {
            int32_t blockBufferLen = bufferLen - i * m_Reader->COMPRESSION_BLOCK_SIZE > m_Reader->COMPRESSION_BLOCK_SIZE
                ? m_Reader->COMPRESSION_BLOCK_SIZE
                : bufferLen - i * m_Reader->COMPRESSION_BLOCK_SIZE;

            uint8_t* blockBuffer = new uint8_t[blockBufferLen];
            memcpy(blockBuffer, buffer + (i * m_Reader->COMPRESSION_BLOCK_SIZE), blockBufferLen);

            m_Reader->m_Reader.Seek(m_Reader->m_CompressionBlockPosition + 12 * (bouncerInfo.FirstBlockIndex + i));

            FIoStoreTocCompressedBlockEntry block;
            m_Reader->m_Reader << block;

            if (FConfig::UcasSize == INT64_MAX) {
                FConfig::UcasPath = m_Reader->m_FilePath.substr(0, m_Reader->m_FilePath.length() - 5) +
                    (m_Reader->PartitionCount - 1 > 0 ? ("_s" + std::to_string(m_Reader->PartitionCount - 1) + ".ucas") : ".ucas");
                FConfig::UcasSize = m_Containers[m_Reader->PartitionCount - 1]->TotalSize();
            }

            if (currentPos == INT64_MAX) {
                currentPos = FConfig::UcasSize;
            }

            m_Containers[m_Reader->PartitionCount - 1]->Seek(currentPos);
            m_Containers[m_Reader->PartitionCount - 1]->WriteBuffer(blockBuffer, blockBufferLen);

            m_Reader->m_Reader.SeekCur(-1 * sizeof(FIoStoreTocCompressedBlockEntry));
            block.SetOffset(currentPos + ((m_Reader->PartitionCount - 1) * m_Reader->PartitionSize));

            currentPos = m_Containers[m_Reader->PartitionCount - 1]->Tell() + 8;

            block.SetCompressedSize(blockBufferLen);
            block.SetUncompressedSize(blockBufferLen);
            block.SetCompressionMethodIndex(0);

            uint8_t* data = new uint8_t[sizeof(FIoStoreTocCompressedBlockEntry)];
            m_Reader->m_Reader.Serialize(data, sizeof(FIoStoreTocCompressedBlockEntry));
            m_Reader->m_Reader.SeekCur(-1 * sizeof(FIoStoreTocCompressedBlockEntry));

            FConfig::UtocChanges[m_Reader->m_FilePath + "  " + std::to_string(m_Reader->m_Reader.Tell())] = std::to_string(sizeof(FIoStoreTocCompressedBlockEntry))
                + "  " + WindowsFunctionLibrary::Encode(data, sizeof(FIoStoreTocCompressedBlockEntry));

            m_Reader->m_Reader >> block;

            delete[] blockBuffer;
        }

        m_Reader->m_Reader.Seek(144 + m_Reader->TocEntryCount * 12 + bouncerInfo.TocEntryIndex * 10);
        FIoOffsetAndLength bouncerOffsetAndLength;
        m_Reader->m_Reader << bouncerOffsetAndLength;

        m_Reader->m_Reader.Seek(144 + m_Reader->TocEntryCount * 12 + info.TocEntryIndex * 10);

        bouncerOffsetAndLength.SetLength(bufferLen);

        uint8_t* data = new uint8_t[sizeof(FIoOffsetAndLength)];
        m_Reader->m_Reader.Serialize(data, sizeof(FIoOffsetAndLength));
        m_Reader->m_Reader.SeekCur(-1 * sizeof(FIoOffsetAndLength));

        FConfig::UtocChanges[m_Reader->m_FilePath + "  " + std::to_string(m_Reader->m_Reader.Tell())] = std::to_string(sizeof(FIoOffsetAndLength))
            + "  " + WindowsFunctionLibrary::Encode(data, sizeof(FIoOffsetAndLength));

        m_Reader->m_Reader >> bouncerOffsetAndLength;

        FConfig::Save();
    }

    __forceinline void OverwriteFileComplete(const std::string& filePath, uint8_t buffer[], size_t bufferLen) {
        VFileInfo info = m_Reader->GetFile(filePath);

        int64_t currentPos = INT64_MAX;

        int32_t newBlockCount = (bufferLen - 1) / m_Reader->COMPRESSION_BLOCK_SIZE + 1;

        for (int i = 0; i < newBlockCount; i++) {
            int32_t blockBufferLen = bufferLen - i * m_Reader->COMPRESSION_BLOCK_SIZE > m_Reader->COMPRESSION_BLOCK_SIZE
                ? m_Reader->COMPRESSION_BLOCK_SIZE
                : bufferLen - i * m_Reader->COMPRESSION_BLOCK_SIZE;

            uint8_t* blockBuffer = new uint8_t[blockBufferLen];
            memcpy(blockBuffer, buffer + (i * m_Reader->COMPRESSION_BLOCK_SIZE), blockBufferLen);

            m_Reader->m_Reader.Seek(m_Reader->m_CompressionBlockPosition + 12 * (info.FirstBlockIndex + i));

            FIoStoreTocCompressedBlockEntry block;
            m_Reader->m_Reader << block;

            if (FConfig::UcasSize == INT64_MAX) {
                FConfig::UcasPath = m_Reader->m_FilePath.substr(0, m_Reader->m_FilePath.length() - 5) +
                    (m_Reader->PartitionCount - 1 > 0 ? ("_s" + std::to_string(m_Reader->PartitionCount - 1) + ".ucas") : ".ucas");
                FConfig::UcasSize = m_Containers[m_Reader->PartitionCount - 1]->TotalSize();
            }

            if (currentPos == INT64_MAX) {
                currentPos = FConfig::UcasSize;
            }

            m_Containers[m_Reader->PartitionCount - 1]->Seek(currentPos);
            m_Containers[m_Reader->PartitionCount - 1]->WriteBuffer(blockBuffer, blockBufferLen);

            m_Reader->m_Reader.SeekCur(-1 * sizeof(FIoStoreTocCompressedBlockEntry));
            block.SetOffset(currentPos + ((m_Reader->PartitionCount - 1) * m_Reader->PartitionSize));

            currentPos = m_Containers[m_Reader->PartitionCount - 1]->Tell() + 8;

            block.SetCompressedSize(blockBufferLen);
            block.SetUncompressedSize(blockBufferLen);
            block.SetCompressionMethodIndex(0);

            uint8_t* data = new uint8_t[sizeof(FIoStoreTocCompressedBlockEntry)];
            m_Reader->m_Reader.Serialize(data, sizeof(FIoStoreTocCompressedBlockEntry));
            m_Reader->m_Reader.SeekCur(-1 * sizeof(FIoStoreTocCompressedBlockEntry));

            FConfig::UtocChanges[m_Reader->m_FilePath + "  " + std::to_string(m_Reader->m_Reader.Tell())] = std::to_string(sizeof(FIoStoreTocCompressedBlockEntry))
                + "  " + WindowsFunctionLibrary::Encode(data, sizeof(FIoStoreTocCompressedBlockEntry));

            m_Reader->m_Reader >> block;

            delete[] blockBuffer;
        }

        m_Reader->m_Reader.Seek(144 + m_Reader->TocEntryCount * 12 + info.TocEntryIndex * 10);
        FIoOffsetAndLength offsetAndLength;
        m_Reader->m_Reader << offsetAndLength;

        m_Reader->m_Reader.SeekCur(-1 * sizeof(FIoOffsetAndLength));

        offsetAndLength.SetLength(bufferLen);

        uint8_t* data = new uint8_t[sizeof(FIoOffsetAndLength)];
        m_Reader->m_Reader.Serialize(data, sizeof(FIoOffsetAndLength));
        m_Reader->m_Reader.SeekCur(-1 * sizeof(FIoOffsetAndLength));

        FConfig::UtocChanges[m_Reader->m_FilePath + "  " + std::to_string(m_Reader->m_Reader.Tell())] = std::to_string(sizeof(FIoOffsetAndLength))
            + "  " + WindowsFunctionLibrary::Encode(data, sizeof(FIoOffsetAndLength));

        m_Reader->m_Reader >> offsetAndLength;

        FConfig::Save();
    }
private:
	std::unordered_map<int32_t, std::shared_ptr<FFileReader>> m_Containers;
	std::shared_ptr<IoStoreReader> m_Reader;
};
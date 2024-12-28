export module Saturn.Readers.FileReaderNoWrite;

export import Saturn.Readers.FArchive;
import <fstream>;

export class FFileReaderNoWrite : public FArchive
{
public:
	FFileReaderNoWrite() {}
	FFileReaderNoWrite(const char* InFilename) : FileStream(InFilename, std::ios::binary | std::ios::in)
	{
		FileStream.imbue(std::locale::classic());
	}

	~FFileReaderNoWrite()
	{
		FileStream.close();
	}

	void Seek(int64_t InPos)
	{
		FileStream.seekg(InPos, FileStream._Seekbeg);
	}

	int64_t Tell()
	{
		return FileStream.tellg();
	}

	int64_t TotalSize()
	{
		auto Pos = FileStream.tellg();
		FileStream.seekg(0, FileStream._Seekend);

		auto Ret = FileStream.tellg();
		FileStream.seekg(Pos, FileStream._Seekbeg);

		return Ret;
	}

	bool Close()
	{
		FileStream.close();

		return !FileStream.is_open();
	}

	bool Serialize(void* V, int64_t Length)
	{
		if (FileStream.read(static_cast<char*>(V), Length)) {
			return true;
		}
		return false;
	}

	bool WriteBuffer(void* V, int64_t Length) {
		if (FileStream.write(static_cast<char*>(V), Length)) {
			return true;
		}
		return false;
	}

	bool IsValid()
	{
		return !!FileStream;
	}

protected:
	friend class FortniteFunctionLibrary;
	std::fstream FileStream;
};
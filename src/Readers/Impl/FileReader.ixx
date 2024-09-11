export module Saturn.Readers.FileReader;

export import Saturn.Readers.FArchive;
import <fstream>;

export class FFileReader : public FArchive
{
public:
	FFileReader() {}
	FFileReader(const char* InFilename) : FileStream(InFilename, std::ios::binary | std::ios::in | std::ios::out)
	{
	}

	~FFileReader()
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

	void Serialize(void* V, int64_t Length)
	{
		FileStream.read(static_cast<char*>(V), Length);
	}

	void WriteBuffer(void* V, int64_t Length) {
		FileStream.write(static_cast<char*>(V), Length);
	}

	bool IsValid()
	{
		return !!FileStream;
	}

protected:
	friend class FortniteFunctionLibrary;
	std::fstream FileStream;
};
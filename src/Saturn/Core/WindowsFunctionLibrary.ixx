export module Saturn.WindowsFunctionLibrary;

import <tuple>;
import <string>;
import <vector>;

export class WindowsFunctionLibrary {
public:
	static std::tuple<long, std::string> GetRequest(const std::string& url);
	static std::tuple<long, std::string> GetRequestSaturn(const std::string& url);
	static std::string ws2s(const std::wstring& wstr);
	static std::wstring s2ws(const std::string& str);
	static unsigned int ByteToImage(const std::string& filePath, uint8_t byte);
	static unsigned int StringToImage(const std::string& filePath, const std::string& inputData);
	static std::vector<uint8_t> StringToImage(const std::string& String);
	static std::vector<uint8_t> EncodeToBuffer(const std::string& input);
	static std::wstring GetSaturnLocalPath();
	static std::string GetHWID();
	static size_t FindArrayInFile(std::fstream* file, const std::vector<uint8_t>& byteArray);
	static uint8_t* FindSubArray(uint8_t* mainArray, size_t arrayLen, uint8_t* subArray, size_t subArrayLength);
	static std::string Encode(uint8_t const* bytes_to_encode, size_t in_len);
	static uint8_t* Decode(std::string const& encoded_string, size_t& out_len);
	static std::string Decode(const std::string& input);
	static std::vector<std::string> Split(const std::string& s, const std::string& delimiter);
	static std::string ReadAllText(const std::string& path);
	static bool FileExists(const std::string& path);
	static std::string CreateTemporaryDirectory();
	static std::string FindFileByExtension(const std::string& directory, const std::string& extension);
	static void MakeDirectory(const std::wstring& directory);
	static void DownloadFile(const std::string& directory, const std::string& url);
	static int FindSmallestFileSizeIndex(const std::vector<std::string>& FilePaths);
	static void RenameFile(const std::string& OldPath, const std::string& NewPath);
	static std::vector<std::string> GetFilesInDirectory(const std::string& Path);
	static void TrimFileToSize(const std::string& Path, int64_t Length);
};
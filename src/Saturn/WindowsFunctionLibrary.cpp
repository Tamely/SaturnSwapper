#include <curl/curl.h>
#include <LodePNG/lodepng.h>
#include <Crypt/skCrypter.h>

#include <map>
#include <sstream>
#include <clocale>
#include <ShlObj_core.h>
#include <lmcons.h>
#include <filesystem>
#include <random>

import Saturn.WindowsFunctionLibrary;
import Saturn.Context;

import <tuple>;
import <string>;
import <vector>;
import <fstream>;

size_t WriteFunction(void* ptr, size_t size, size_t nmemb, std::string* data) {
	data->append((char*)ptr, size * nmemb);
	return size * nmemb;
}

size_t WriteFileFunction(void* contents, size_t size, size_t nmemb, FILE* stream) {
	size_t written = fwrite(contents, size, nmemb, stream);
	return written;
}

std::tuple<long, std::string> WindowsFunctionLibrary::GetRequest(const std::string& url) {
	if (!FContext::HasInitializedCurl) {
		FContext::HasInitializedCurl = true;
		curl_global_init(CURL_GLOBAL_ALL);
	}

	auto curl = curl_easy_init();
	if (curl) {
		curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
		curl_easy_setopt(curl, CURLOPT_NOPROGRESS, 1L);
		curl_easy_setopt(curl, CURLOPT_USERAGENT, "Saturn/3.0.0");
		curl_easy_setopt(curl, CURLOPT_MAXREDIRS, 50L);
		curl_easy_setopt(curl, CURLOPT_TCP_KEEPALIVE, 1L);

		std::string response_string;
		std::string header_string;
		curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteFunction);
		curl_easy_setopt(curl, CURLOPT_WRITEDATA, &response_string);
		curl_easy_setopt(curl, CURLOPT_HEADERDATA, &header_string);

		curl_easy_perform(curl);

		char* url;
		long response_code;
		double elapsed;
		curl_easy_getinfo(curl, CURLINFO_RESPONSE_CODE, &response_code);
		curl_easy_getinfo(curl, CURLINFO_TOTAL_TIME, &elapsed);
		curl_easy_getinfo(curl, CURLINFO_EFFECTIVE_URL, &url);

		curl_easy_cleanup(curl);
		curl = 0;

		return { response_code, response_string };
	}

	return { -1, "" };
}

std::tuple<long, std::string> WindowsFunctionLibrary::GetRequestSaturn(const std::string& url) {
	if (!FContext::HasInitializedCurl) {
		FContext::HasInitializedCurl = true;
		curl_global_init(CURL_GLOBAL_ALL);
	}

	auto curl = curl_easy_init();
	if (curl) {
		curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
		curl_easy_setopt(curl, CURLOPT_NOPROGRESS, 1L);
		curl_easy_setopt(curl, CURLOPT_USERAGENT, "Saturn/3.0.0");
		curl_easy_setopt(curl, CURLOPT_MAXREDIRS, 50L);
		curl_easy_setopt(curl, CURLOPT_TCP_KEEPALIVE, 1L);

		std::string response_string;
		std::string header_string;
		curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteFunction);
		curl_easy_setopt(curl, CURLOPT_WRITEDATA, &response_string);
		curl_easy_setopt(curl, CURLOPT_HEADERDATA, &header_string);

		struct curl_slist* headers = NULL;
		headers = curl_slist_append(headers, skCrypt("ApiKey: 85F50B1D31"));
		curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);

		curl_easy_perform(curl);

		char* url;
		long response_code;
		double elapsed;
		curl_easy_getinfo(curl, CURLINFO_RESPONSE_CODE, &response_code);
		curl_easy_getinfo(curl, CURLINFO_TOTAL_TIME, &elapsed);
		curl_easy_getinfo(curl, CURLINFO_EFFECTIVE_URL, &url);

		curl_slist_free_all(headers);
		curl_easy_cleanup(curl);
		curl = 0;

		return { response_code, response_string };
	}

	return { -1, "" };
}

std::string WindowsFunctionLibrary::ws2s(const std::wstring& wstr) {
	if (wstr.empty()) {
		return std::string();
	}
	unsigned len = wstr.size() * 4;
	setlocale(LC_CTYPE, "");
	char* p = new char[len];
	size_t i;
	wcstombs_s(&i, p, len, wstr.c_str(), len);
	std::string str(p);
	delete[] p;
	return str;
}

std::wstring WindowsFunctionLibrary::s2ws(const std::string& str) {
	if (str.empty()) {
		return std::wstring();
	}
	unsigned len = str.size() * 2;
	setlocale(LC_CTYPE, "");
	wchar_t* p = new wchar_t[len];
	size_t i;
	mbstowcs_s(&i, p, len, str.c_str(), len);
	std::wstring wstr(p);
	delete[] p;
	return wstr;
}

unsigned int WindowsFunctionLibrary::ByteToImage(const std::string& filePath, uint8_t byte) {
	std::vector<uint8_t> output = { byte, 0, 0 };
	return lodepng::encode(filePath, output.data(), 1, 1, LCT_RGB);
}

unsigned int WindowsFunctionLibrary::StringToImage(const std::string& filePath, const std::string& inputData) {
	std::vector<uint8_t> output = StringToImage(inputData);
	return lodepng::encode(filePath, output.data(), (int)(output.size() / 3), 1, LCT_RGB);
}

std::vector<uint8_t> WindowsFunctionLibrary::EncodeToBuffer(const std::string& input) {
	std::vector<uint8_t> output = StringToImage(input);

	std::vector<uint8_t> buffer;
	unsigned width = static_cast<unsigned>(output.size() / 3);
	unsigned height = 1;
	unsigned error = lodepng::encode(buffer, output.data(), width, height, LCT_RGB);

	if (error) {
		throw std::runtime_error(lodepng_error_text(error));
	}

	return buffer;
}

std::vector<uint8_t> WindowsFunctionLibrary::StringToImage(const std::string& String) {
	static std::map<char, uint8_t> CharToByte{
		std::pair<char, uint8_t>('0',1),
		std::pair<char, uint8_t>('1',2),
		std::pair<char, uint8_t>('2',3),
		std::pair<char, uint8_t>('3',4),
		std::pair<char, uint8_t>('4',5),
		std::pair<char, uint8_t>('5',6),
		std::pair<char, uint8_t>('6',7),
		std::pair<char, uint8_t>('7',8),
		std::pair<char, uint8_t>('8',9),
		std::pair<char, uint8_t>('9',10),
		std::pair<char, uint8_t>('a',11),
		std::pair<char, uint8_t>('b',12),
		std::pair<char, uint8_t>('c',13),
		std::pair<char, uint8_t>('d',14),
		std::pair<char, uint8_t>('e',15),
		std::pair<char, uint8_t>('f',16),
		std::pair<char, uint8_t>('g',17),
		std::pair<char, uint8_t>('h',18),
		std::pair<char, uint8_t>('i',19),
		std::pair<char, uint8_t>('j',20),
		std::pair<char, uint8_t>('k',21),
		std::pair<char, uint8_t>('l',22),
		std::pair<char, uint8_t>('m',23),
		std::pair<char, uint8_t>('n',24),
		std::pair<char, uint8_t>('o',25),
		std::pair<char, uint8_t>('p',26),
		std::pair<char, uint8_t>('q',27),
		std::pair<char, uint8_t>('r',28),
		std::pair<char, uint8_t>('s',29),
		std::pair<char, uint8_t>('t',30),
		std::pair<char, uint8_t>('u',31),
		std::pair<char, uint8_t>('v',32),
		std::pair<char, uint8_t>('w',33),
		std::pair<char, uint8_t>('x',34),
		std::pair<char, uint8_t>('y',35),
		std::pair<char, uint8_t>('z',36),
		std::pair<char, uint8_t>('A',37),
		std::pair<char, uint8_t>('B',38),
		std::pair<char, uint8_t>('C',39),
		std::pair<char, uint8_t>('D',40),
		std::pair<char, uint8_t>('E',41),
		std::pair<char, uint8_t>('F',42),
		std::pair<char, uint8_t>('G',43),
		std::pair<char, uint8_t>('H',44),
		std::pair<char, uint8_t>('I',45),
		std::pair<char, uint8_t>('J',46),
		std::pair<char, uint8_t>('K',47),
		std::pair<char, uint8_t>('L',48),
		std::pair<char, uint8_t>('M',49),
		std::pair<char, uint8_t>('N',50),
		std::pair<char, uint8_t>('O',51),
		std::pair<char, uint8_t>('P',52),
		std::pair<char, uint8_t>('Q',53),
		std::pair<char, uint8_t>('R',54),
		std::pair<char, uint8_t>('S',55),
		std::pair<char, uint8_t>('T',56),
		std::pair<char, uint8_t>('U',57),
		std::pair<char, uint8_t>('V',58),
		std::pair<char, uint8_t>('W',59),
		std::pair<char, uint8_t>('X',60),
		std::pair<char, uint8_t>('Y',61),
		std::pair<char, uint8_t>('Z',62),
		std::pair<char, uint8_t>('/',63),
		std::pair<char, uint8_t>('.',64),
		std::pair<char, uint8_t>(' ',65),
		std::pair<char, uint8_t>('_',66),
		std::pair<char, uint8_t>(',',67),
		std::pair<char, uint8_t>('?',68),
		std::pair<char, uint8_t>('&',69),
		std::pair<char, uint8_t>('=',70),
		std::pair<char, uint8_t>('-',71),
		std::pair<char, uint8_t>('+',72),
		std::pair<char, uint8_t>('[',73),
		std::pair<char, uint8_t>(']',74),
		std::pair<char, uint8_t>('\\',75),
		std::pair<char, uint8_t>('|',76),
		std::pair<char, uint8_t>('\'',77),
		std::pair<char, uint8_t>('\"',78),
		std::pair<char, uint8_t>('>',79),
		std::pair<char, uint8_t>('<',80),
		std::pair<char, uint8_t>(':',81),
		std::pair<char, uint8_t>(';',82),
		std::pair<char, uint8_t>('!',83),
		std::pair<char, uint8_t>('@',84),
		std::pair<char, uint8_t>('$',85),
		std::pair<char, uint8_t>('%',86),
		std::pair<char, uint8_t>('^',87),
		std::pair<char, uint8_t>('*',88),
		std::pair<char, uint8_t>('(',89),
		std::pair<char, uint8_t>(')',90),
		std::pair<char, uint8_t>('`',91)
	};

	std::vector<uint8_t> arr;

	for (uint8_t c : String) {
		arr.push_back(CharToByte[c]);
	}

	while (arr.size() % 3 != 0) {
		arr.push_back(0);
	}

	return arr;
}

std::wstring WindowsFunctionLibrary::GetSaturnLocalPath() {
	std::wstringstream ss;

	wchar_t* localAppData = 0;
	SHGetKnownFolderPath(FOLDERID_LocalAppData, 0, NULL, &localAppData);

	ss << localAppData << L"\\Saturn\\";

	CoTaskMemFree(static_cast<void*>(localAppData));

	return ss.str();
}

std::string WindowsFunctionLibrary::GetHWID() {
	HW_PROFILE_INFO hwProfileInfo;
	if (GetCurrentHwProfile(&hwProfileInfo) != NULL) {
		return hwProfileInfo.szHwProfileGuid;
	}

	return "";
}

size_t WindowsFunctionLibrary::FindArrayInFile(std::fstream* file, const std::vector<uint8_t>& byteArray) {
	file->clear();
	file->seekg(0);

	// Get the length of the byte array to search for
	std::streamsize byteArrayLength = static_cast<std::streamsize>(byteArray.size());

	// Create a buffer to read the file in chunks
	const std::streamsize bufferSize = 4096;  // Adjust the buffer size as needed
	std::vector<uint8_t> buffer(bufferSize);

	// Read the file in chunks
	while (file->read(reinterpret_cast<char*>(buffer.data()), bufferSize)) {
		// Search for the byte array in the buffer
		for (std::streamsize i = 0; i < bufferSize - byteArrayLength + 1; ++i) {
			if (memcmp(buffer.data() + i, byteArray.data(), byteArrayLength) == 0) {
				// Byte array found, calculate and return its offset
				return file->tellg() - bufferSize + i;
			}
		}
	}

	// Search for the byte array in the remaining characters
	file->clear();  // Clear the end-of-file flag
	file->seekg(-byteArrayLength, std::ios::end);  // Move to the last part of the file
	std::vector<uint8_t> lastBuffer(byteArrayLength);
	file->read(reinterpret_cast<char*>(lastBuffer.data()), byteArrayLength);

	// Search for the byte array in the last buffer
	for (std::streamsize i = 0; i < byteArrayLength; ++i) {
		if (memcmp(lastBuffer.data() + i, byteArray.data(), byteArrayLength - i) == 0) {
			// Byte array found, calculate and return its offset
			return file->tellg() + i;
		}
	}

	// Byte array not found in the file
	return -1;
}

uint8_t* WindowsFunctionLibrary::FindSubArray(uint8_t* mainArray, size_t arrayLen, uint8_t* subArray, size_t subArrayLength) {
	if (subArrayLength == 0 || arrayLen < subArrayLength) {
		return nullptr;
	}

	for (size_t i = 0; i <= arrayLen - subArrayLength; ++i) {
		if (std::memcmp(mainArray + i, subArray, subArrayLength) == 0) {
			return mainArray + i;
		}
	}

	return nullptr;
}

const std::string base64_chars =
	"ABCDEFGHIJKLMNOPQRSTUVWXYZ"
	"abcdefghijklmnopqrstuvwxyz"
	"0123456789+/";

inline bool is_base64(uint8_t c) {
	return (isalnum(c) || (c == '+') || (c == '/'));
}

std::string WindowsFunctionLibrary::Encode(uint8_t const* bytes_to_encode, size_t in_len) {
	std::string ret;
	int i = 0;
	int j = 0;
	uint8_t char_array_3[3];
	uint8_t char_array_4[4];

	while (in_len--) {
		char_array_3[i++] = *(bytes_to_encode++);
		if (i == 3) {
			char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
			char_array_4[1] = ((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4);
			char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6);
			char_array_4[3] = char_array_3[2] & 0x3f;

			for (i = 0; (i < 4); i++)
				ret += base64_chars[char_array_4[i]];
			i = 0;
		}
	}

	if (i) {
		for (j = i; j < 3; j++)
			char_array_3[j] = '\0';

		char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
		char_array_4[1] = ((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4);
		char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6);
		char_array_4[3] = char_array_3[2] & 0x3f;

		for (j = 0; (j < i + 1); j++)
			ret += base64_chars[char_array_4[j]];

		while ((i++ < 3))
			ret += '=';
	}

	return ret;
}

uint8_t* WindowsFunctionLibrary::Decode(std::string const& encoded_string, size_t& out_len) {
	size_t in_len = encoded_string.size();
	if (in_len % 4 != 0) {
		throw std::invalid_argument("Invalid Base64 input length");
	}

	size_t i = 0;
	size_t j = 0;
	size_t in_ = 0;
	uint8_t char_array_4[4], char_array_3[3];
	out_len = in_len / 4 * 3;
	if (encoded_string[in_len - 1] == '=') out_len--;
	if (encoded_string[in_len - 2] == '=') out_len--;

	uint8_t* ret = new uint8_t[out_len];
	size_t ret_idx = 0;

	while (in_len-- && (encoded_string[in_] != '=') && is_base64(encoded_string[in_])) {
		char_array_4[i++] = encoded_string[in_]; in_++;
		if (i == 4) {
			for (i = 0; i < 4; i++)
				char_array_4[i] = base64_chars.find(char_array_4[i]);

			char_array_3[0] = (char_array_4[0] << 2) + ((char_array_4[1] & 0x30) >> 4);
			char_array_3[1] = ((char_array_4[1] & 0xf) << 4) + ((char_array_4[2] & 0x3c) >> 2);
			char_array_3[2] = ((char_array_4[2] & 0x3) << 6) + char_array_4[3];

			for (i = 0; (i < 3); i++)
				ret[ret_idx++] = char_array_3[i];
			i = 0;
		}
	}

	if (i) {
		for (j = i; j < 4; j++)
			char_array_4[j] = 0;

		for (j = 0; j < 4; j++)
			char_array_4[j] = base64_chars.find(char_array_4[j]);

		char_array_3[0] = (char_array_4[0] << 2) + ((char_array_4[1] & 0x30) >> 4);
		char_array_3[1] = ((char_array_4[1] & 0xf) << 4) + ((char_array_4[2] & 0x3c) >> 2);
		char_array_3[2] = ((char_array_4[2] & 0x3) << 6) + char_array_4[3];

		for (j = 0; (j < i - 1); j++) ret[ret_idx++] = char_array_3[j];
	}

	return ret;
}

int base64_char_to_value(char c) {
	if ('A' <= c && c <= 'Z') return c - 'A';
	if ('a' <= c && c <= 'z') return c - 'a' + 26;
	if ('0' <= c && c <= '9') return c - '0' + 52;
	if (c == '+') return 62;
	if (c == '/') return 63;
	throw std::invalid_argument("Invalid Base64 character");
}

std::string WindowsFunctionLibrary::Decode(const std::string& input) {
	if (input.size() % 4 != 0) {
		throw std::invalid_argument("Invalid Base64 input");
	}

	std::string output;
	std::vector<int> temp(4);

	for (size_t i = 0; i < input.size(); i += 4) {
		for (int j = 0; j < 4; ++j) {
			temp[j] = input[i + j] == '=' ? 0 : base64_char_to_value(input[i + j]);
		}

		output.push_back((temp[0] << 2) + ((temp[1] & 0x30) >> 4));
		if (input[i + 2] != '=') {
			output.push_back(((temp[1] & 0xf) << 4) + ((temp[2] & 0x3c) >> 2));
		}
		if (input[i + 3] != '=') {
			output.push_back(((temp[2] & 0x3) << 6) + temp[3]);
		}
	}

	return output;
}

std::vector<std::string> WindowsFunctionLibrary::Split(const std::string& s, const std::string& delimiter) {
	std::vector<std::string> tokens;
	size_t start = 0;
	size_t end = s.find(delimiter);

	while (end != std::string::npos) {
		tokens.push_back(s.substr(start, end - start));
		start = end + delimiter.length();
		end = s.find(delimiter, start);
	}
	tokens.push_back(s.substr(start, end));

	return tokens;
}

std::string WindowsFunctionLibrary::ReadAllText(const std::string& path) {
	std::ifstream inputFile(path);
	if (!inputFile.is_open()) return std::string();

	return std::string((std::istreambuf_iterator<char>(inputFile)), std::istreambuf_iterator<char>());
}

bool WindowsFunctionLibrary::FileExists(const std::string& path) {
	if (FILE* file = fopen(path.c_str(), "r")) {
		fclose(file);
		return true;
	}
	else {
		return false;
	}
}

std::string WindowsFunctionLibrary::CreateTemporaryDirectory() {
	int maxTries = 1000;
	auto tempDir = std::filesystem::temp_directory_path();
	
	int i = 0;
	std::random_device dev;
	std::mt19937 prng(dev());
	std::uniform_int_distribution<uint64_t> rand(0);
	std::filesystem::path path;

	while (true) {
		std::stringstream ss;
		ss << std::hex << rand(prng);
		path = tempDir / ss.str();

		if (std::filesystem::create_directory(path)) {
			break;
		}
		if (i == maxTries) {
			throw std::runtime_error("Could not find non-existing directory");
		}
		i++;
	}

	return path.string() + "\\";

}

std::string WindowsFunctionLibrary::FindFileByExtension(const std::string& directory, const std::string& extension) {
	for (const auto& entry : std::filesystem::directory_iterator(directory)) {
		if (entry.is_regular_file() && entry.path().extension() == extension) {
			return entry.path().string();
		}
	}

	return "";
}

void WindowsFunctionLibrary::MakeDirectory(const std::wstring& directory) {
	CreateDirectoryW(directory.c_str(), NULL);
}

void WindowsFunctionLibrary::DownloadFile(const std::string& directory, const std::string& url) {
	if (!url.empty()) {
		if (!FContext::HasInitializedCurl) {
			FContext::HasInitializedCurl = true;
			curl_global_init(CURL_GLOBAL_ALL);
		}

		CURL* curl;
		curl = curl_easy_init();

		if (curl) {
			FILE* fp = fopen(directory.c_str(), "wb");

			curl_easy_setopt(curl, CURLOPT_URL, url.c_str());

			curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteFileFunction);
			curl_easy_setopt(curl, CURLOPT_WRITEDATA, fp);

			CURLcode res = curl_easy_perform(curl);

			if (res != CURLE_OK) {
				MessageBoxW(nullptr, L"Request failed!", L"WindowsFunctionLibrary::DownloadFile", NULL);
				return;
			}

			fclose(fp);
			curl_easy_cleanup(curl);
		}
		else {
			MessageBoxW(nullptr, L"Error initializing cURL!", L"WindowsFunctionLibrary::DownloadFile", NULL);
			return;
		}
	}
	else {
		MessageBoxW(nullptr, L"URL cannot be empty!", L"WindowsFunctionLibrary::DownloadFile", NULL);
		return;
	}
}

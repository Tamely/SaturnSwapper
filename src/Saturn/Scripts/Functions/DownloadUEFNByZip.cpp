import Saturn.Functions.DownloadUEFNByZip;

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#include <curl/curl.h>
#include <duktape/duktape.h>
#include <miniz/zip_file.hpp>

#include <filesystem>

#include <Crypt/skCrypter.h>

import Saturn.Context;
import Saturn.FortniteFunctionLibrary;
import Saturn.WindowsFunctionLibrary;

import <unordered_map>;
import <string>;

// UDownloadUEFNByZip("URL");
duk_ret_t FDownloadUEFNByZip::dukDownloadUEFNByZip(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 1) {
		MessageBoxW(nullptr, L"This function takes 1 argument!", L"WebClient", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	std::string url = duk_get_string(ctx, 0);

	FILE* fp = fopen("temp.zip", "wb");

	if (!url.empty()) {
		if (!FContext::HasInitializedCurl) {
			FContext::HasInitializedCurl = true;
			curl_global_init(CURL_GLOBAL_ALL);
		}

		CURL* curl;
		curl = curl_easy_init();

		if (curl) {
			curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
			curl_easy_setopt(curl, CURLOPT_NOPROGRESS, 1L);
			curl_easy_setopt(curl, CURLOPT_USERAGENT, "Saturn/3.0.0");
			curl_easy_setopt(curl, CURLOPT_MAXREDIRS, 50L);
			curl_easy_setopt(curl, CURLOPT_TCP_KEEPALIVE, 1L);

			curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
			curl_easy_setopt(curl, CURLOPT_WRITEDATA, fp);

			CURLcode res = curl_easy_perform(curl);
			if (res != CURLE_OK) {
				MessageBoxW(nullptr, L"Request failed!", L"DownloadUEFNByZip", NULL);
				return DUK_RET_TYPE_ERROR;
			}

			fclose(fp);
			curl_easy_cleanup(curl);

			std::string tempDir = WindowsFunctionLibrary::CreateTemporaryDirectory();

			miniz_cpp::zip_file file("temp.zip");
			file.extractall(tempDir);
			file.~zip_file();

			remove("temp.zip");

			std::string ucas = WindowsFunctionLibrary::FindFileByExtension(tempDir, "ucas");
			std::string utoc = WindowsFunctionLibrary::FindFileByExtension(tempDir, "utoc");
			std::string pak = WindowsFunctionLibrary::FindFileByExtension(tempDir, "pak");
			std::string sig = WindowsFunctionLibrary::FindFileByExtension(tempDir, "sig");

			if (ucas.empty()) {
				std::cout << "UCAS file could not be downloaded!" << std::endl;
			}

			if (utoc.empty()) {
				std::cout << "UTOC file could not be downloaded!" << std::endl;
			}

			if (pak.empty()) {
				std::cout << "PAK file could not be downloaded!" << std::endl;
			}

			if (sig.empty()) {
				std::cout << "SIG file could not be downloaded!" << std::endl;
			}

			// pakchunk2optional-WindowsClient
			std::filesystem::remove(FortniteFunctionLibrary::GetFortniteInstallationPath() + _("//plugin.ucas"));
			std::filesystem::remove(FortniteFunctionLibrary::GetFortniteInstallationPath() + _("//plugin.utoc"));
			std::filesystem::remove(FortniteFunctionLibrary::GetFortniteInstallationPath() + _("//plugin.pak"));
			std::filesystem::remove(FortniteFunctionLibrary::GetFortniteInstallationPath() + _("//plugin.sig"));

			std::filesystem::copy(ucas, FortniteFunctionLibrary::GetFortniteInstallationPath() + _("//plugin.ucas"));
			std::filesystem::copy(utoc, FortniteFunctionLibrary::GetFortniteInstallationPath() + _("//plugin.utoc"));
			std::filesystem::copy(pak, FortniteFunctionLibrary::GetFortniteInstallationPath() + _("//plugin.pak"));
			std::filesystem::copy(sig, FortniteFunctionLibrary::GetFortniteInstallationPath() + _("//plugin.sig"));

			std::filesystem::remove(tempDir);
		}
		else {
			MessageBoxW(nullptr, L"Error initializing cURL!", L"DownloadUEFNByZip", NULL);
			return DUK_RET_TYPE_ERROR;
		}
	}
	else {
		MessageBoxW(nullptr, L"URL cannot be empty!", L"DownloadUEFNByZip", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	return 0;
}

size_t FDownloadUEFNByZip::WriteCallback(void* contents, size_t size, size_t nmemb, FILE* stream) {
	size_t written = fwrite(contents, size, nmemb, stream);
	return written;
}
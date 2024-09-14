import Saturn.Functions.WebClient;

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#include <curl/curl.h>;
#include <duktape/duktape.h>;

import Saturn.Context;

import <unordered_map>;
import <string>;

// WebClientPointer UWebClient("Host");
duk_ret_t FWebClient::dukWebClient(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 1) {
		MessageBoxW(nullptr, L"This function takes 1 argument!", L"WebClient", NULL);
		return DUK_RET_TYPE_ERROR;
	}
	
	std::string host = duk_get_string(ctx, 0);
	
	if (!host.empty()) {
		if (!FContext::HasInitializedCurl) {
			FContext::HasInitializedCurl = true;
			curl_global_init(CURL_GLOBAL_ALL);
		}

		CURL* curl;
		curl = curl_easy_init();

		if (curl) {
			curl_easy_setopt(curl, CURLOPT_URL, host.c_str());
			curl_easy_setopt(curl, CURLOPT_NOPROGRESS, 1L);
			curl_easy_setopt(curl, CURLOPT_USERAGENT, "Saturn/3.0.0");
			curl_easy_setopt(curl, CURLOPT_MAXREDIRS, 50L);
			curl_easy_setopt(curl, CURLOPT_TCP_KEEPALIVE, 1L);

			curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
		}
		else {
			MessageBoxW(nullptr, L"Error initializing cURL!", L"WebClient", NULL);
			return DUK_RET_TYPE_ERROR;
		}

		CreatedClients[curl] = host;

		duk_push_pointer(ctx, curl);
	}
	else {
		MessageBoxW(nullptr, L"Host name cannot be empty!", L"WebClient", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	return 1;
}

size_t FWebClient::WriteCallback(void* contents, size_t size, size_t nmemb, void* userp) {
	((std::string*)userp)->append((char*)contents, size * nmemb);
	return size * nmemb;
}

std::unordered_map<void*, std::string> FWebClient::CreatedClients = {};
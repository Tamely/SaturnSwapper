import Saturn.Functions.WebClientGet;
import Saturn.Functions.WebClient;

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#include <curl/curl.h>;
#include <duktape/duktape.h>;

import Saturn.Context;
import <string>;

//String UWebClientGet(Client, "Path")
duk_ret_t FWebClientGet::dukWebClientGet(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 2) {
		MessageBoxW(nullptr, L"This function takes 2 arguments!", L"WebClientGet", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	CURL* curl = static_cast<CURL*>(duk_get_pointer(ctx, 0));
	if (!curl) {
		MessageBoxW(nullptr, L"Client is invalid!", L"WebClientGet", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	std::string path = duk_get_string(ctx, 1);
	std::string hostName = FWebClient::CreatedClients[curl];

	if (!path.empty()) {
		curl_easy_setopt(curl, CURLOPT_URL, std::string(hostName + path).c_str());

		std::string responseBody;
		curl_easy_setopt(curl, CURLOPT_WRITEDATA, &responseBody);

		CURLcode res = curl_easy_perform(curl);

		if (res != CURLE_OK) {
			MessageBoxW(nullptr, L"Request timed out!", L"WebClientGet", NULL);
			return DUK_RET_TYPE_ERROR;
		}

		duk_push_string(ctx, responseBody.c_str());
	}
	else {
		MessageBoxW(nullptr, L"Path cannot be empty!", L"WebClientGet", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	return 1;
}
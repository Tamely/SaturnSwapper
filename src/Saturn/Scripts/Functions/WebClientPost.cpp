import Saturn.Functions.WebClientPost;
import Saturn.Functions.WebClient;

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#include <curl/curl.h>;
#include <duktape/duktape.h>;

import Saturn.Context;
import <string>;

//String UWebClientPost(Client, "Path", "Body", "Content-Type")
duk_ret_t FWebClientPost::dukWebClientPost(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 4) {
		MessageBoxW(nullptr, L"This function takes 4 arguments!", L"WebClientPost", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	CURL* curl = static_cast<CURL*>(duk_get_pointer(ctx, 0));
	if (!curl) {
		MessageBoxW(nullptr, L"Client is invalid!", L"WebClientPost", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	std::string path = duk_get_string(ctx, 1);
	std::string body = duk_get_string(ctx, 2);
	std::string content_type = duk_get_string(ctx, 3);

	std::string hostName = FWebClient::CreatedClients[curl];

	if (!path.empty()) {
		curl_easy_setopt(curl, CURLOPT_URL, std::string(hostName + path).c_str());

		curl_easy_setopt(curl, CURLOPT_POST, 1L);
		curl_easy_setopt(curl, CURLOPT_POSTFIELDS, body.c_str());
		
		struct curl_slist* headers = nullptr;
		headers = curl_slist_append(headers, std::string("Content-Type: " + content_type).c_str());
		curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
		
		std::string responseBody;
		curl_easy_setopt(curl, CURLOPT_WRITEDATA, &responseBody);

		CURLcode res = curl_easy_perform(curl);

		if (res != CURLE_OK) {
			MessageBoxW(nullptr, L"Request timed out!", L"WebClientPost", NULL);
			return DUK_RET_TYPE_ERROR;
		}

		curl_slist_free_all(headers);

		duk_push_string(ctx, responseBody.c_str());
	}
	else {
		MessageBoxW(nullptr, L"Path cannot be empty!", L"WebClientPost", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	return 1;
}
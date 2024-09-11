export module Saturn.Functions.WebClient;

import <duktape/duktape.h>;

import <unordered_map>;
import <string>;

export struct FWebClient {
	// WebClientPointer UWebClient("Host");
	static duk_ret_t dukWebClient(duk_context* ctx);

	static size_t WriteCallback(void* contents, size_t size, size_t nmemb, void* userp);
	static std::unordered_map<void*, std::string> CreatedClients;
};
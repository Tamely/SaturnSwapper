export module Saturn.Functions.DownloadUEFNByZip;

import <duktape/duktape.h>;

export struct FDownloadUEFNByZip {
	// UDownloadUEFNByZip("URL");
	static duk_ret_t dukDownloadUEFNByZip(duk_context* ctx);

	static size_t WriteCallback(void* contents, size_t size, size_t nmemb, FILE* stream);
};
export module Saturn.Functions.FindABPByPath;

import <duktape/duktape.h>;
import <string>;

export struct FFindABPByPath {
	const static int ABP_SIG = 0xAAAC;
	static int ABPCount;

	static void encode(const std::string& path);
	// UBlueprint UFindABPByPath("Path");
	static duk_ret_t dukFindABPByPath(duk_context* ctx);
};
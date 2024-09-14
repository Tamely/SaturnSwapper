export module Saturn.Functions.FindMontageByPath;

import <duktape/duktape.h>;
import <string>;

export struct FFindMontageByPath {
	const static int MONTAGE_SIG = 0xAAAD;
	static int MontageCount;

	static void encode(const std::string& path);
	// UMontage UFindMontageByPath("Path");
	static duk_ret_t dukFindMontageByPath(duk_context* ctx);
};
export module Saturn.Functions.FindTextureByPath;

import <duktape/duktape.h>;
import <string>;

export struct FFindTextureByPath {
	static void encode(const std::string& path);
	// UTexture2D UFindTextureByPath("Path");
	static duk_ret_t dukFindTextureByPath(duk_context* ctx);
};
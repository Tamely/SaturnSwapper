export module Saturn.Functions.FindTextureByURL;

import <duktape/duktape.h>;
import <string>;

export struct FFindTextureByURL {
	const static int TEXTURE_SIG = 0xAAAA;
	static int TextureCount;

	static void encode(const std::string& url);
	// UTexture2D UFindTextureByURL("URL");
	static duk_ret_t dukFindTextureByURL(duk_context* ctx);
};
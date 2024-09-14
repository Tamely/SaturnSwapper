export module Saturn.Functions.WebClientPost;

import <duktape/duktape.h>;

export struct FWebClientPost {
	//String UWebClientPost(Client, "Path", "Body", "Content-Type")
	static duk_ret_t dukWebClientPost(duk_context* ctx);
};
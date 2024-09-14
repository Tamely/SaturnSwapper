export module Saturn.Functions.WebClientGet;

import <duktape/duktape.h>;

export struct FWebClientGet {
	//String UWebClientGet(Client, "Path")
	static duk_ret_t dukWebClientGet(duk_context* ctx);
};
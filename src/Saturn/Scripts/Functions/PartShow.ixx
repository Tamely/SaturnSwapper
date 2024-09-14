export module Saturn.Functions.PartShow;

import <duktape/duktape.h>;
import <string>;

export struct FPartShow {
	static void encode(int part);
	// UPartShow(Part);
	static duk_ret_t dukPartShow(duk_context* ctx);
};
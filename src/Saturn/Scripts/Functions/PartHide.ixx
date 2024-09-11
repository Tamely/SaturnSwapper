export module Saturn.Functions.PartHide;

import <duktape/duktape.h>;
import <string>;

export struct FPartHide {
	static void encode(int part);
	// UPartHide(Part);
	static duk_ret_t dukPartHide(duk_context* ctx);
};